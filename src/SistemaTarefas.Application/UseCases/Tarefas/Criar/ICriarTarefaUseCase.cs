using SistemaTarefas.Communication.Requests;
using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Tarefas.Criar;

public interface ICriarTarefaUseCase
{
    Task<Resultado<TarefaResponse>> Execute(
        RequestCriarTarefa request,
        CancellationToken cancellationToken);
}
