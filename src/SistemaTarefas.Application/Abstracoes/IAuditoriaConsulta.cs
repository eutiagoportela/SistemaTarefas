using SistemaTarefas.Communication.Responses;

namespace SistemaTarefas.Application.Abstracoes;

public interface IAuditoriaConsulta
{
    Task<IReadOnlyList<HistoricoResponse>> ListarPorTarefaAsync(Guid tarefaId, CancellationToken cancellationToken);
}
