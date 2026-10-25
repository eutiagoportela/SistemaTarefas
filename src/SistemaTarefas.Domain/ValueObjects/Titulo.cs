using SistemaTarefas.Domain.Excecoes;

namespace SistemaTarefas.Domain.ValueObjects;

public sealed record Titulo
{
    public const int TamanhoMinimo = 3;
    public const int TamanhoMaximo = 120;

    public string Valor { get; }

    public Titulo(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new DomainException("O título é obrigatório.");
        }

        var normalizado = valor.Trim();
        if (normalizado.Length < TamanhoMinimo || normalizado.Length > TamanhoMaximo)
        {
            throw new DomainException($"O título deve ter entre {TamanhoMinimo} e {TamanhoMaximo} caracteres.");
        }

        Valor = normalizado;
    }

    public override string ToString() => Valor;
}
