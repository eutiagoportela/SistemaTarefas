using SistemaTarefas.Communication.Requests;
using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Auth.RefreshToken;

public interface IRefreshTokenUseCase
{
    Task<Resultado<LoginResponse>> Execute(
        RequestRefresh request,
        CancellationToken cancellationToken);
}
