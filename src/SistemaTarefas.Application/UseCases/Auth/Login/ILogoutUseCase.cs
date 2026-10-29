using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Auth.Logout;

public interface ILogoutUseCase
{
    Task<Resultado> Execute(
        string refreshToken,
        CancellationToken cancellationToken);
}
