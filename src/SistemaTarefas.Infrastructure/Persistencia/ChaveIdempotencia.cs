namespace SistemaTarefas.Infrastructure.Persistencia;

public sealed class ChaveIdempotencia
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Chave { get; set; } = string.Empty;
    public string HashCorpo { get; set; } = string.Empty;
    public string RespostaJson { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public DateTime ExpiraEm { get; set; }
}
