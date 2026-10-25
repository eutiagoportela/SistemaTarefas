using FluentValidation;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Application.Mapeamentos;
using SistemaTarefas.Communication.Requests;
using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Entidades;
using SistemaTarefas.Domain.Excecoes;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Tarefas;

public interface ICriarTarefaUseCase
{
    Task<Resultado<TarefaResponse>> Execute(RequestCriarTarefa request, CancellationToken cancellationToken);
}

public sealed class CriarTarefaUseCase : ICriarTarefaUseCase
{
    private readonly ITarefaRepository _tarefas;
    private readonly IUnitOfWork _uow;
    private readonly IUsuarioAtual _usuario;
    private readonly IValidator<RequestCriarTarefa> _validator;
    private readonly TimeProvider _relogio;

    public CriarTarefaUseCase(
        ITarefaRepository tarefas,
        IUnitOfWork uow,
        IUsuarioAtual usuario,
        IValidator<RequestCriarTarefa> validator,
        TimeProvider relogio)
    {
        _tarefas = tarefas;
        _uow = uow;
        _usuario = usuario;
        _validator = validator;
        _relogio = relogio;
    }

    public async Task<Resultado<TarefaResponse>> Execute(RequestCriarTarefa request, CancellationToken cancellationToken)
    {
        var validacao = await _validator.ValidateAsync(request, cancellationToken);
        if (!validacao.IsValid)
        {
            return Resultado<TarefaResponse>.Validacao("Erros de validação", validacao.Errors.Select(e => e.ErrorMessage).ToList());
        }

        try
        {
            var tarefa = Tarefa.Criar(request.Titulo, request.Descricao, request.DataVencimento, _usuario.Id, _relogio);
            await _tarefas.AdicionarAsync(tarefa, cancellationToken);
            await _uow.CommitAsync(cancellationToken);
            return Resultado<TarefaResponse>.Ok(tarefa.ParaResponse(_relogio));
        }
        catch (DomainException ex)
        {
            return Resultado<TarefaResponse>.Validacao(ex.Message);
        }
    }
}
