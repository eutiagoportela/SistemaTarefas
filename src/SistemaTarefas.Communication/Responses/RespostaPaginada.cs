namespace SistemaTarefas.Communication.Responses;

public class RespostaPaginada<T>
{
    public IReadOnlyList<T> Itens { get; set; } = [];
    public int Pagina { get; set; }
    public int Tamanho { get; set; }
    public int Total { get; set; }
    public int TotalPaginas => Tamanho == 0 ? 0 : (int)Math.Ceiling(Total / (double)Tamanho);
}
