namespace SistemaTarefas.Communication.Requests;

public class RequestAtualizarTarefa
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime DataVencimento { get; set; }
}
