using Microsoft.EntityFrameworkCore;
using SistemaTarefas.Domain.Entidades;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Infrastructure.Persistencia;

namespace SistemaTarefas.Infrastructure.Repositorios;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly TarefasDbContext _contexto;

    public UsuarioRepository(TarefasDbContext contexto) => _contexto = contexto;

    public async Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default)
        => await _contexto.Usuarios.AddAsync(usuario, cancellationToken);

    public async Task AdicionarRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        var usuario = _contexto.ChangeTracker.Entries<Usuario>()
            .FirstOrDefault(e => e.Entity.Id == token.UsuarioId);
        if (usuario is not null && usuario.State != EntityState.Detached)
        {
            usuario.State = EntityState.Unchanged;
        }

        await _contexto.RefreshTokens.AddAsync(token, cancellationToken);
    }

    public Task<Usuario?> ObterPorNomeAsync(string nomeUsuario, CancellationToken cancellationToken = default)
        => _contexto.Usuarios
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.NomeUsuario == nomeUsuario, cancellationToken);

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _contexto.Usuarios
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<RefreshToken?> ObterRefreshTokenAtivoAsync(string tokenHash, CancellationToken cancellationToken = default)
        => _contexto.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.RevogadoEm == null, cancellationToken);
}
