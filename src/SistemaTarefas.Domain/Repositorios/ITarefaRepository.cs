using SistemaTarefas.Domain.Entidades;
using SistemaTarefas.Domain.Enums;

namespace SistemaTarefas.Domain.Repositorios;

public interface ITarefaRepository
{
    Task AdicionarAsync(Tarefa tarefa, CancellationToken cancellationToken = default);

    Task<Tarefa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Tarefa> Itens, int Total)> ListarAsync(
        Guid? usuarioId,
        bool admin,
        StatusTarefa? status,
        DateTime? vencimentoDe,
        DateTime? vencimentoAte,
        bool? atrasadas,
        string? busca,
        int pagina,
        int tamanho,
        string ordenarPor,
        string direcao,
        TimeProvider relogio,
        CancellationToken cancellationToken = default);
}
