namespace SistemaTarefas.Communication.Requests;

public class RequestCriarTarefa
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime DataVencimento { get; set; }
}
