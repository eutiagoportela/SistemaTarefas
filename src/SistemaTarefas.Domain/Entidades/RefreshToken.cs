namespace SistemaTarefas.Domain.Entidades;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiraEm { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? RevogadoEm { get; private set; }

    private RefreshToken()
    {
    }

    public static RefreshToken Criar(Guid usuarioId, string tokenHash, DateTime expiraEm, TimeProvider relogio)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            TokenHash = tokenHash,
            ExpiraEm = expiraEm,
            CriadoEm = relogio.GetUtcNow().UtcDateTime
        };
    }

    public bool EstaAtivo(TimeProvider relogio)
        => RevogadoEm is null && ExpiraEm > relogio.GetUtcNow().UtcDateTime;

    public void Revogar(TimeProvider relogio)
        => RevogadoEm = relogio.GetUtcNow().UtcDateTime;
}
