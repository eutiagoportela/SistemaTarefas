using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Tarefas.Remover;

public interface IRemoverTarefaUseCase
{
    Task<Resultado> Execute(
        Guid id,
        uint versaoEsperada,
        CancellationToken cancellationToken);
}
