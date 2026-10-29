using SistemaTarefas.Communication.Requests;
using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Tarefas.Atualizar;

public interface IAtualizarTarefaUseCase
{
    Task<Resultado<TarefaResponse>> Execute(
        Guid id,
        uint versaoEsperada,
        RequestAtualizarTarefa request,
        CancellationToken cancellationToken);
}
