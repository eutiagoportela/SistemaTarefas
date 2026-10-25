using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Application.Mapeamentos;
using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Tarefas;

public interface IObterTarefaUseCase
{
    Task<Resultado<TarefaResponse>> Execute(Guid id, CancellationToken cancellationToken);
}

public sealed class ObterTarefaUseCase : IObterTarefaUseCase
{
    private readonly ITarefaRepository _tarefas;
    private readonly IUsuarioAtual _usuario;
    private readonly TimeProvider _relogio;

    public ObterTarefaUseCase(ITarefaRepository tarefas, IUsuarioAtual usuario, TimeProvider relogio)
    {
        _tarefas = tarefas;
        _usuario = usuario;
        _relogio = relogio;
    }

    public async Task<Resultado<TarefaResponse>> Execute(Guid id, CancellationToken cancellationToken)
    {
        var tarefa = await _tarefas.ObterPorIdAsync(id, cancellationToken);
        if (tarefa is null || (!_usuario.IsAdmin && tarefa.UsuarioId != _usuario.Id))
        {
            return Resultado<TarefaResponse>.NaoEncontrado("Tarefa não encontrada.");
        }

        return Resultado<TarefaResponse>.Ok(tarefa.ParaResponse(_relogio));
    }
}
