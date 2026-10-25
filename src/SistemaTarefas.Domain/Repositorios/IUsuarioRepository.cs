using SistemaTarefas.Domain.Entidades;

namespace SistemaTarefas.Domain.Repositorios;

public interface IUsuarioRepository
{
    Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default);

    Task AdicionarRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default);

    Task<Usuario?> ObterPorNomeAsync(string nomeUsuario, CancellationToken cancellationToken = default);

    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<RefreshToken?> ObterRefreshTokenAtivoAsync(string tokenHash, CancellationToken cancellationToken = default);
}
