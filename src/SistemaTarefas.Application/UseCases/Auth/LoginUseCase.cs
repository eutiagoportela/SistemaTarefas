using FluentValidation;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Communication.Requests;
using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Entidades;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Auth;

public interface ILoginUseCase
{
    Task<Resultado<LoginResponse>> Execute(RequestLogin request, CancellationToken cancellationToken);
}

public sealed class LoginUseCase : ILoginUseCase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    private readonly IValidator<RequestLogin> _validator;
    private readonly TimeProvider _relogio;

    public LoginUseCase(
        IUsuarioRepository usuarios,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        ITokenService tokens,
        IValidator<RequestLogin> validator,
        TimeProvider relogio)
    {
        _usuarios = usuarios;
        _uow = uow;
        _hasher = hasher;
        _tokens = tokens;
        _validator = validator;
        _relogio = relogio;
    }

    public async Task<Resultado<LoginResponse>> Execute(RequestLogin request, CancellationToken cancellationToken)
    {
        var validacao = await _validator.ValidateAsync(request, cancellationToken);
        if (!validacao.IsValid)
        {
            return Resultado<LoginResponse>.Validacao("Erros de validação", validacao.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var usuario = await _usuarios.ObterPorNomeAsync(request.NomeUsuario, cancellationToken);
        if (usuario is null || !_hasher.Verificar(request.Senha, usuario.SenhaHash))
        {
            return Resultado<LoginResponse>.NaoAutorizado("Credenciais inválidas");
        }

        var (token, expira) = _tokens.GerarAccessToken(usuario);
        var refresh = _tokens.GerarRefreshToken();
        await _usuarios.AdicionarRefreshTokenAsync(
            RefreshToken.Criar(usuario.Id, _tokens.HashRefreshToken(refresh), expira.AddDays(7), _relogio),
            cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return Resultado<LoginResponse>.Ok(new LoginResponse
        {
            Token = token,
            RefreshToken = refresh,
            ExpiraEm = expira
        });
    }
}
