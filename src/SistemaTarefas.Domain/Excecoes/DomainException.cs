namespace SistemaTarefas.Domain.Excecoes;

public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
