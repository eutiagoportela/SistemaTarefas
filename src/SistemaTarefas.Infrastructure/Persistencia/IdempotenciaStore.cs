using Microsoft.EntityFrameworkCore;
using SistemaTarefas.Application.Abstracoes;

namespace SistemaTarefas.Infrastructure.Persistencia;

public sealed class IdempotenciaStore : IIdempotenciaStore
{
    private readonly TarefasDbContext _contexto;
    private readonly TimeProvider _relogio;

    public IdempotenciaStore(TarefasDbContext contexto, TimeProvider relogio)
    {
        _contexto = contexto;
        _relogio = relogio;
    }

    public async Task<string?> ObterRespostaAsync(Guid usuarioId, string chave, string hashCorpo, CancellationToken cancellationToken)
    {
        var agora = _relogio.GetUtcNow().UtcDateTime;
        var registro = await _contexto.ChavesIdempotencia
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.Chave == chave && c.ExpiraEm > agora, cancellationToken);

        if (registro is null)
        {
            return null;
        }

        return registro.HashCorpo == hashCorpo ? registro.RespostaJson : null;
    }

    public async Task SalvarAsync(Guid usuarioId, string chave, string hashCorpo, string respostaJson, CancellationToken cancellationToken)
    {
        var agora = _relogio.GetUtcNow().UtcDateTime;
        _contexto.ChavesIdempotencia.Add(new ChaveIdempotencia
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Chave = chave,
            HashCorpo = hashCorpo,
            RespostaJson = respostaJson,
            CriadoEm = agora,
            ExpiraEm = agora.AddHours(24)
        });
        await _contexto.SaveChangesAsync(cancellationToken);
    }
}
