namespace SistemaTarefas.Communication.Responses;

public class HistoricoResponse
{
    public Guid Id { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string? Detalhe { get; set; }
    public DateTime OcorridoEm { get; set; }
    public string Usuario { get; set; } = string.Empty;
}
