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

public interface IAtualizarTarefaUseCase
{
    Task<Resultado<TarefaResponse>> Execute(Guid id, uint versaoEsperada, RequestAtualizarTarefa request, CancellationToken cancellationToken);
}

public interface IAlterarStatusTarefaUseCase
{
    Task<Resultado<TarefaResponse>> Iniciar(Guid id, uint versaoEsperada, CancellationToken cancellationToken);
    Task<Resultado<TarefaResponse>> Concluir(Guid id, uint versaoEsperada, CancellationToken cancellationToken);
    Task<Resultado<TarefaResponse>> Reabrir(Guid id, uint versaoEsperada, CancellationToken cancellationToken);
}

public interface IRemoverTarefaUseCase
{
    Task<Resultado> Execute(Guid id, uint versaoEsperada, CancellationToken cancellationToken);
}

public interface IObterHistoricoTarefaUseCase
{
    Task<Resultado<IReadOnlyList<HistoricoResponse>>> Execute(Guid id, CancellationToken cancellationToken);
}

public sealed class ComandosTarefaUseCase :
    IAtualizarTarefaUseCase,
    IAlterarStatusTarefaUseCase,
    IRemoverTarefaUseCase,
    IObterHistoricoTarefaUseCase
{
    private readonly ITarefaRepository _tarefas;
    private readonly IUnitOfWork _uow;
    private readonly IUsuarioAtual _usuario;
    private readonly IAuditoriaConsulta _auditoria;
    private readonly IValidator<RequestAtualizarTarefa> _validator;
    private readonly TimeProvider _relogio;

    public ComandosTarefaUseCase(
        ITarefaRepository tarefas,
        IUnitOfWork uow,
        IUsuarioAtual usuario,
        IAuditoriaConsulta auditoria,
        IValidator<RequestAtualizarTarefa> validator,
        TimeProvider relogio)
    {
        _tarefas = tarefas;
        _uow = uow;
        _usuario = usuario;
        _auditoria = auditoria;
        _validator = validator;
        _relogio = relogio;
    }

    public async Task<Resultado<TarefaResponse>> Execute(
        Guid id,
        uint versaoEsperada,
        RequestAtualizarTarefa request,
        CancellationToken cancellationToken)
    {
        var validacao = await _validator.ValidateAsync(request, cancellationToken);
        if (!validacao.IsValid)
        {
            return Resultado<TarefaResponse>.Validacao("Erros de validação", validacao.Errors.Select(e => e.ErrorMessage).ToList());
        }

        return await Executar(id, versaoEsperada, t => t.AtualizarDetalhes(request.Titulo, request.Descricao, request.DataVencimento, _relogio), cancellationToken);
    }

    public Task<Resultado<TarefaResponse>> Iniciar(Guid id, uint versao, CancellationToken ct)
        => Executar(id, versao, t => t.Iniciar(_relogio), ct);

    public Task<Resultado<TarefaResponse>> Concluir(Guid id, uint versao, CancellationToken ct)
        => Executar(id, versao, t => t.Concluir(_relogio), ct);

    public Task<Resultado<TarefaResponse>> Reabrir(Guid id, uint versao, CancellationToken ct)
        => Executar(id, versao, t => t.Reabrir(_relogio), ct);

    public async Task<Resultado> Execute(Guid id, uint versaoEsperada, CancellationToken cancellationToken)
    {
        var resultado = await Executar(id, versaoEsperada, t => t.Remover(_relogio), cancellationToken);
        return resultado.Sucesso ? Resultado.Ok() : Resultado.Falha(resultado.Erro!);
    }

    public async Task<Resultado<IReadOnlyList<HistoricoResponse>>> Execute(Guid id, CancellationToken cancellationToken)
    {
        var tarefa = await ObterAutorizada(id, cancellationToken);
        if (tarefa is null)
        {
            return Resultado<IReadOnlyList<HistoricoResponse>>.NaoEncontrado("Tarefa não encontrada.");
        }

        var historico = await _auditoria.ListarPorTarefaAsync(id, cancellationToken);
        return Resultado<IReadOnlyList<HistoricoResponse>>.Ok(historico);
    }

    private async Task<Resultado<TarefaResponse>> Executar(Guid id, uint versao, Action<Tarefa> comando, CancellationToken cancellationToken)
    {
        var tarefa = await ObterAutorizada(id, cancellationToken);
        if (tarefa is null)
        {
            return Resultado<TarefaResponse>.NaoEncontrado("Tarefa não encontrada.");
        }

        if (tarefa.Versao != versao)
        {
            return Resultado<TarefaResponse>.Conflito("A tarefa foi alterada por outra requisição. Recarregue e tente novamente.");
        }

        try
        {
            comando(tarefa);
            await _uow.CommitAsync(cancellationToken);
            return Resultado<TarefaResponse>.Ok(tarefa.ParaResponse(_relogio));
        }
        catch (DomainException ex)
        {
            return Resultado<TarefaResponse>.Validacao(ex.Message);
        }
    }

    private async Task<Tarefa?> ObterAutorizada(Guid id, CancellationToken cancellationToken)
    {
        var tarefa = await _tarefas.ObterPorIdAsync(id, cancellationToken);
        if (tarefa is null || (!_usuario.IsAdmin && tarefa.UsuarioId != _usuario.Id))
        {
            return null;
        }

        return tarefa;
    }
}
