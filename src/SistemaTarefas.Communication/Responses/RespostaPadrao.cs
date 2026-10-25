namespace SistemaTarefas.Communication.Responses;

public class RespostaPadrao<T>
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
    public T? Dados { get; set; }
    public List<string>? Erros { get; set; }
    public string? TraceId { get; set; }

    public static RespostaPadrao<T> ComSucesso(T? dados, string mensagem = "Operação realizada com sucesso", string? traceId = null)
        => new()
        {
            Sucesso = true,
            Mensagem = mensagem,
            Dados = dados,
            TraceId = traceId
        };

    public static RespostaPadrao<T> ComErro(string erro, string mensagem = "Erro na operação", string? traceId = null)
        => new()
        {
            Sucesso = false,
            Mensagem = mensagem,
            Erros = [erro],
            TraceId = traceId
        };

    public static RespostaPadrao<T> ComErros(IEnumerable<string> erros, string mensagem = "Erros de validação", string? traceId = null)
        => new()
        {
            Sucesso = false,
            Mensagem = mensagem,
            Erros = erros.ToList(),
            TraceId = traceId
        };
}
