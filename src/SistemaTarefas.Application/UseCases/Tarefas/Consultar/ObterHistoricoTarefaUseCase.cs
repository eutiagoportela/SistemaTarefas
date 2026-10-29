
using Microsoft.Extensions.Logging;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Entidades;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Tarefas.Consultar;

public sealed class ObterHistoricoTarefaUseCase : IObterHistoricoTarefaUseCase
{
    private readonly ITarefaRepository _tarefas;
    private readonly IUsuarioAtual _usuario;
    private readonly IAuditoriaConsulta _auditoria;
    private readonly ILogger<ObterHistoricoTarefaUseCase> _logger;

    public ObterHistoricoTarefaUseCase(
        ITarefaRepository tarefas,
        IUsuarioAtual usuario,
        IAuditoriaConsulta auditoria,
        ILogger<ObterHistoricoTarefaUseCase> logger)
    {
        _tarefas = tarefas;
        _usuario = usuario;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<Resultado<IReadOnlyList<HistoricoResponse>>> Execute(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tarefa = await ObterAutorizada(id, cancellationToken);

        if (tarefa is null)
        {
            return Resultado<IReadOnlyList<HistoricoResponse>>.NaoEncontrado(
                "Tarefa não encontrada.");
        }

        var historico = await _auditoria.ListarPorTarefaAsync(
            id,
            cancellationToken);

        return Resultado<IReadOnlyList<HistoricoResponse>>.Ok(historico);
    }

    private async Task<Tarefa?> ObterAutorizada(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tarefa = await _tarefas.ObterPorIdAsync(
            id,
            cancellationToken);

        if (tarefa is null)
        {
            _logger.LogWarning(
                "Tarefa não encontrada: {TarefaId}",
                id);

            return null;
        }

        if (!_usuario.IsAdmin && tarefa.UsuarioId != _usuario.Id)
        {
            _logger.LogWarning(
                "Acesso negado à tarefa {TarefaId} (usuário {UsuarioId})",
                id,
                _usuario.Id);

            return null;
        }

        return tarefa;
    }
}
