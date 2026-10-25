namespace SistemaTarefas.Application.Abstracoes;

public interface IUsuarioAtual
{
    Guid Id { get; }
    string Nome { get; }
    bool IsAdmin { get; }
    bool EstaAutenticado { get; }
}
