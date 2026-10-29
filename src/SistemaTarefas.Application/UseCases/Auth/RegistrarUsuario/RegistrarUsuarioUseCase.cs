using FluentValidation;
using Microsoft.Extensions.Logging;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Communication.Requests;
using SistemaTarefas.Domain.Entidades;
using SistemaTarefas.Domain.Enums;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Auth.RegistrarUsuario;

public sealed class RegistrarUsuarioUseCase : IRegistrarUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IValidator<RequestRegistrar> _validator;
    private readonly TimeProvider _relogio;
    private readonly ILogger<RegistrarUsuarioUseCase> _logger;

    public RegistrarUsuarioUseCase(
        IUsuarioRepository usuarios,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        IValidator<RequestRegistrar> validator,
        TimeProvider relogio,
        ILogger<RegistrarUsuarioUseCase> logger)
    {
        _usuarios = usuarios;
        _uow = uow;
        _hasher = hasher;
        _validator = validator;
        _relogio = relogio;
        _logger = logger;
    }

    public async Task<Resultado> Execute(
        RequestRegistrar request,
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
                "Dados inválidos ao registrar usuário: {Erros}",
                string.Join(", ", erros));

            return Resultado.Validacao("Erros de validação", erros);
        }

        var existente = await _usuarios.ObterPorNomeAsync(
            request.NomeUsuario,
            cancellationToken);

        if (existente is not null)
        {
            _logger.LogWarning(
                "Tentativa de registro com usuário já existente: {NomeUsuario}",
                request.NomeUsuario);

            return Resultado.Conflito(
                $"O usuário '{request.NomeUsuario}' já está cadastrado.");
        }

        var usuario = Usuario.Criar(
            request.NomeUsuario,
            _hasher.Hash(request.Senha),
            PapelUsuario.Usuario,
            _relogio);

        await _usuarios.AdicionarAsync(usuario, cancellationToken);

        await _uow.CommitAsync(cancellationToken);

        return Resultado.Ok();
    }
}
