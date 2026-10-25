using Microsoft.EntityFrameworkCore;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Communication.Responses;

namespace SistemaTarefas.Infrastructure.Persistencia;

public sealed class AuditoriaConsulta : IAuditoriaConsulta
{
    private readonly TarefasDbContext _contexto;

    public AuditoriaConsulta(TarefasDbContext contexto) => _contexto = contexto;

    public async Task<IReadOnlyList<HistoricoResponse>> ListarPorTarefaAsync(Guid tarefaId, CancellationToken cancellationToken)
        => await _contexto.Auditorias.AsNoTracking()
            .Where(a => a.TarefaId == tarefaId)
            .OrderByDescending(a => a.OcorridoEm)
            .Select(a => new HistoricoResponse
            {
                Id = a.Id,
                Acao = a.Acao,
                Detalhe = a.Detalhe,
                OcorridoEm = a.OcorridoEm,
                Usuario = a.UsuarioNome
            })
            .ToListAsync(cancellationToken);
}
