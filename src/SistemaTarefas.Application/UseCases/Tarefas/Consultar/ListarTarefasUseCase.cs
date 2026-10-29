using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Application.Mapeamentos;
using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Enums;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Tarefas.Consultar;

public sealed record FiltroTarefas(
    StatusTarefa? Status,
    DateTime? VencimentoDe,
    DateTime? VencimentoAte,
    bool? Atrasadas,
    string? Busca,
    int Pagina,
    int Tamanho,
    string OrdenarPor,
    string Direcao);

public sealed class ListarTarefasUseCase : IListarTarefasUseCase
{
    private readonly ITarefaRepository _tarefas;
    private readonly IUsuarioAtual _usuario;
    private readonly TimeProvider _relogio;

    public ListarTarefasUseCase(ITarefaRepository tarefas, IUsuarioAtual usuario, TimeProvider relogio)
    {
        _tarefas = tarefas;
        _usuario = usuario;
        _relogio = relogio;
    }

    public async Task<Resultado<RespostaPaginada<TarefaResponse>>> Execute(FiltroTarefas filtro, CancellationToken cancellationToken)
    {
        var pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
        var tamanho = filtro.Tamanho is < 1 or > 100 ? 20 : filtro.Tamanho;

        var (itens, total) = await _tarefas.ListarAsync(
            _usuario.Id,
            _usuario.IsAdmin,
            filtro.Status,
            filtro.VencimentoDe,
            filtro.VencimentoAte,
            filtro.Atrasadas,
            filtro.Busca,
            pagina,
            tamanho,
            filtro.OrdenarPor,
            filtro.Direcao,
            _relogio,
            cancellationToken);

        return Resultado<RespostaPaginada<TarefaResponse>>.Ok(new RespostaPaginada<TarefaResponse>
        {
            Itens = itens.Select(t => t.ParaResponse(_relogio)).ToList(),
            Pagina = pagina,
            Tamanho = tamanho,
            Total = total
        });
    }
}
