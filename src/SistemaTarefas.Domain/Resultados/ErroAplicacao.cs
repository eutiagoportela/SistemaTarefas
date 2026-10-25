namespace SistemaTarefas.Domain.Resultados;

public sealed record ErroAplicacao(
    TipoErro Tipo,
    string Mensagem,
    IReadOnlyList<string>? Detalhes = null);
