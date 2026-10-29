using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Tarefas.Consultar;

public interface IObterHistoricoTarefaUseCase
{
    Task<Resultado<IReadOnlyList<HistoricoResponse>>> Execute(
        Guid id,
        CancellationToken cancellationToken);
}
