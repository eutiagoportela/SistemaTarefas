using FluentValidation;
using Microsoft.Extensions.Logging;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Communication.Requests;
using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Domain.Resultados;
using EntidadeRefreshToken = SistemaTarefas.Domain.Entidades.RefreshToken;

namespace SistemaTarefas.Application.UseCases.Auth.Login;

public sealed class LoginUseCase : ILoginUseCase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    private readonly IValidator<RequestLogin> _validator;
    private readonly TimeProvider _relogio;
    private readonly ILogger<LoginUseCase> _logger;

    public LoginUseCase(
        IUsuarioRepository usuarios,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        ITokenService tokens,
        IValidator<RequestLogin> validator,
        TimeProvider relogio,
        ILogger<LoginUseCase> logger)
    {
        _usuarios = usuarios;
        _uow = uow;
        _hasher = hasher;
        _tokens = tokens;
        _validator = validator;
        _relogio = relogio;
        _logger = logger;
    }

    public async Task<Resultado<LoginResponse>> Execute(
        RequestLogin request,
        CancellationToken cancellationToken)
    {
        var validacao = await _validator.ValidateAsync(
            request,
            cancellationToken);

        if (!validacao.IsValid)
        {
            var erros = validacao.Errors
                .Select(e => e.ErrorMessage)
                .ToList();

            _logger.LogWarning(
                "Dados inválidos ao realizar login: {Erros}",
                string.Join(", ", erros));

            return Resultado<LoginResponse>.Validacao(
                "Erros de validação",
                erros);
        }

        _logger.LogInformation(
            "Tentativa de login: {NomeUsuario}",
            request.NomeUsuario);

        var usuario = await _usuarios.ObterPorNomeAsync(
            request.NomeUsuario,
            cancellationToken);

        if (usuario is null ||
            !_hasher.Verificar(request.Senha, usuario.SenhaHash))
        {
            _logger.LogWarning(
                "Credenciais inválidas para usuário {NomeUsuario}",
                request.NomeUsuario);

            return Resultado<LoginResponse>.NaoAutorizado(
                "Credenciais inválidas");
        }

        var (token, expira) = _tokens.GerarAccessToken(usuario);
        var refresh = _tokens.GerarRefreshToken();

        await _usuarios.AdicionarRefreshTokenAsync(
            EntidadeRefreshToken.Criar(
                usuario.Id,
                _tokens.HashRefreshToken(refresh),
                expira.AddDays(7),
                _relogio),
            cancellationToken);

        await _uow.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Login realizado com sucesso: {NomeUsuario}",
            request.NomeUsuario);

        return Resultado<LoginResponse>.Ok(new LoginResponse
        {
            Token = token,
            RefreshToken = refresh,
            ExpiraEm = expira
        });
    }
}
