namespace SistemaTarefas.Application.Abstracoes;

public interface IIdempotenciaStore
{
    Task<string?> ObterRespostaAsync(Guid usuarioId, string chave, string hashCorpo, CancellationToken cancellationToken);
    Task SalvarAsync(Guid usuarioId, string chave, string hashCorpo, string respostaJson, CancellationToken cancellationToken);
}
