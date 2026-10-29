using SistemaTarefas.Communication.Requests;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Auth.RegistrarUsuario;

public interface IRegistrarUsuarioUseCase
{
    Task<Resultado> Execute(
        RequestRegistrar request,
        CancellationToken cancellationToken);
}
