namespace SistemaTarefas.Communication.Responses;

public class TarefaResponse
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DataVencimento { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataConclusao { get; set; }
    public bool EstaAtrasada { get; set; }
    public uint Versao { get; set; }
}
