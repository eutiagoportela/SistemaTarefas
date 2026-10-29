using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Tarefas.Consultar;
public interface IListarTarefasUseCase
{
    Task<Resultado<RespostaPaginada<TarefaResponse>>> Execute(
        FiltroTarefas filtro,
        CancellationToken cancellationToken);
}
