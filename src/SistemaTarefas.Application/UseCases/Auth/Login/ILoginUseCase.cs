using SistemaTarefas.Communication.Requests;
using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Auth.Login;

public interface ILoginUseCase
{
    Task<Resultado<LoginResponse>> Execute(
        RequestLogin request,
        CancellationToken cancellationToken);
}
