using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Tarefas.Consultar;
public interface IObterTarefaUseCase
{
    Task<Resultado<TarefaResponse>> Execute(
        Guid id,
        CancellationToken cancellationToken);
}
