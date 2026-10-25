namespace SistemaTarefas.Domain.Repositorios;

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
