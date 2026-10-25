using SistemaTarefas.Domain.Excecoes;

namespace SistemaTarefas.Domain.ValueObjects;

public sealed record Descricao
{
    public const int TamanhoMaximo = 1000;

    public string Valor { get; }

    public Descricao(string? valor)
    {
        var normalizado = valor?.Trim() ?? string.Empty;
        if (normalizado.Length > TamanhoMaximo)
        {
            throw new DomainException($"A descrição deve ter no máximo {TamanhoMaximo} caracteres.");
        }

        Valor = normalizado;
    }

    public override string ToString() => Valor;
}
