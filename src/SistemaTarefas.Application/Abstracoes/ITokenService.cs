using SistemaTarefas.Domain.Entidades;

namespace SistemaTarefas.Application.Abstracoes;

public interface ITokenService
{
    (string Token, DateTime ExpiraEm) GerarAccessToken(Usuario usuario);
    string GerarRefreshToken();
    string HashRefreshToken(string refreshToken);
}
