namespace SistemaTarefas.Infrastructure.Persistencia;

public sealed class AuditoriaRegistro
{
    public Guid Id { get; set; }
    public Guid? TarefaId { get; set; }
    public Guid? UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public string Acao { get; set; } = string.Empty;
    public string? Detalhe { get; set; }
    public DateTime OcorridoEm { get; set; }
}
