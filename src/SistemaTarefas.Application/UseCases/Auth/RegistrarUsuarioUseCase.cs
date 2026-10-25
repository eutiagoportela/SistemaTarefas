using FluentValidation;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Communication.Requests;
using SistemaTarefas.Domain.Entidades;
using SistemaTarefas.Domain.Enums;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Auth;

public interface IRegistrarUsuarioUseCase
{
    Task<Resultado> Execute(RequestRegistrar request, CancellationToken cancellationToken);
}

public sealed class RegistrarUsuarioUseCase : IRegistrarUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IValidator<RequestRegistrar> _validator;
    private readonly TimeProvider _relogio;

    public RegistrarUsuarioUseCase(
        IUsuarioRepository usuarios,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        IValidator<RequestRegistrar> validator,
        TimeProvider relogio)
    {
        _usuarios = usuarios;
        _uow = uow;
        _hasher = hasher;
        _validator = validator;
        _relogio = relogio;
    }

    public async Task<Resultado> Execute(RequestRegistrar request, CancellationToken cancellationToken)
    {
        var validacao = await _validator.ValidateAsync(request, cancellationToken);
        if (!validacao.IsValid)
        {
            return Resultado.Validacao("Erros de validação", validacao.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var existente = await _usuarios.ObterPorNomeAsync(request.NomeUsuario, cancellationToken);
        if (existente is not null)
        {
            return Resultado.Conflito($"O usuário '{request.NomeUsuario}' já está cadastrado.");
        }

        var usuario = Usuario.Criar(request.NomeUsuario, _hasher.Hash(request.Senha), PapelUsuario.Usuario, _relogio);
        await _usuarios.AdicionarAsync(usuario, cancellationToken);
        await _uow.CommitAsync(cancellationToken);
        return Resultado.Ok();
    }
}
