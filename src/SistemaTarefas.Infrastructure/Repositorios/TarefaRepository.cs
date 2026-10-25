using Microsoft.EntityFrameworkCore;
using SistemaTarefas.Domain.Entidades;
using SistemaTarefas.Domain.Enums;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Infrastructure.Persistencia;

namespace SistemaTarefas.Infrastructure.Repositorios;

public sealed class TarefaRepository : ITarefaRepository
{
    private readonly TarefasDbContext _contexto;

    public TarefaRepository(TarefasDbContext contexto) => _contexto = contexto;

    public async Task AdicionarAsync(Tarefa tarefa, CancellationToken cancellationToken = default)
        => await _contexto.Tarefas.AddAsync(tarefa, cancellationToken);

    public Task<Tarefa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _contexto.Tarefas.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Tarefa> Itens, int Total)> ListarAsync(
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
        CancellationToken cancellationToken = default)
    {
        var query = _contexto.Tarefas.AsQueryable();
        if (!admin && usuarioId.HasValue)
        {
            query = query.Where(t => t.UsuarioId == usuarioId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (vencimentoDe.HasValue)
        {
            query = query.Where(t => t.DataVencimento >= vencimentoDe.Value);
        }

        if (vencimentoAte.HasValue)
        {
            query = query.Where(t => t.DataVencimento <= vencimentoAte.Value);
        }

        if (atrasadas == true)
        {
            var agora = relogio.GetUtcNow().UtcDateTime;
            query = query.Where(t => t.Status != StatusTarefa.Concluido && t.DataVencimento < agora);
        }

        if (!string.IsNullOrWhiteSpace(busca))
        {
            query = query.Where(t => t.Titulo.Valor.Contains(busca) || t.Descricao.Valor.Contains(busca));
        }

        var descendente = string.Equals(direcao, "desc", StringComparison.OrdinalIgnoreCase);
        query = ordenarPor?.ToLowerInvariant() switch
        {
            "titulo" => descendente ? query.OrderByDescending(t => t.Titulo.Valor) : query.OrderBy(t => t.Titulo.Valor),
            "status" => descendente ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "criacao" => descendente ? query.OrderByDescending(t => t.DataCriacao) : query.OrderBy(t => t.DataCriacao),
            _ => descendente ? query.OrderByDescending(t => t.DataVencimento) : query.OrderBy(t => t.DataVencimento)
        };

        var total = await query.CountAsync(cancellationToken);
        var itens = await query.Skip((pagina - 1) * tamanho).Take(tamanho).ToListAsync(cancellationToken);
        return (itens, total);
    }
}
