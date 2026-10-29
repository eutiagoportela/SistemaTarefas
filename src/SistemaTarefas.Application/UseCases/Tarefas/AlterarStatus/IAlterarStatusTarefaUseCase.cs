using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Tarefas.AlterarStatus;

public interface IAlterarStatusTarefaUseCase
{
    Task<Resultado<TarefaResponse>> Iniciar(
        Guid id,
        uint versaoEsperada,
        CancellationToken cancellationToken);

    Task<Resultado<TarefaResponse>> Concluir(
        Guid id,
        uint versaoEsperada,
        CancellationToken cancellationToken);

    Task<Resultado<TarefaResponse>> Reabrir(
        Guid id,
        uint versaoEsperada,
        CancellationToken cancellationToken);
}
