using Microsoft.Extensions.Logging;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Domain.Entidades;
using SistemaTarefas.Domain.Excecoes;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Tarefas.Remover;

public sealed class RemoverTarefaUseCase : IRemoverTarefaUseCase
{
    private readonly ITarefaRepository _tarefas;
    private readonly IUnitOfWork _uow;
    private readonly IUsuarioAtual _usuario;
    private readonly TimeProvider _relogio;
    private readonly ILogger<RemoverTarefaUseCase> _logger;

    public RemoverTarefaUseCase(
        ITarefaRepository tarefas,
        IUnitOfWork uow,
        IUsuarioAtual usuario,
        TimeProvider relogio,
        ILogger<RemoverTarefaUseCase> logger)
    {
        _tarefas = tarefas;
        _uow = uow;
        _usuario = usuario;
        _relogio = relogio;
        _logger = logger;
    }

    public async Task<Resultado> Execute(
        Guid id,
        uint versaoEsperada,
        CancellationToken cancellationToken)
    {
        var tarefa = await ObterAutorizada(id, cancellationToken);

        if (tarefa is null)
        {
            return Resultado.NaoEncontrado(
                "Tarefa não encontrada.");
        }

        if (tarefa.Versao != versaoEsperada)
        {
            _logger.LogWarning(
                "Conflito de versão na tarefa {TarefaId}: esperada {VersaoEsperada}, atual {VersaoAtual}",
                id,
                versaoEsperada,
                tarefa.Versao);

            return Resultado.Conflito(
                "A tarefa foi alterada por outra requisição. Recarregue e tente novamente.");
        }

        try
        {
            tarefa.Remover(_relogio);

            await _uow.CommitAsync(cancellationToken);

            return Resultado.Ok();
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(
                "Regra de domínio na tarefa {TarefaId}: {Mensagem}",
                id,
                ex.Message);

            return Resultado.Validacao(ex.Message);
        }
    }

    private async Task<Tarefa?> ObterAutorizada(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tarefa = await _tarefas.ObterPorIdAsync(
            id,
            cancellationToken);

        if (tarefa is null)
        {
            _logger.LogWarning(
                "Tarefa não encontrada: {TarefaId}",
                id);

            return null;
        }

        if (!_usuario.IsAdmin && tarefa.UsuarioId != _usuario.Id)
        {
            _logger.LogWarning(
                "Acesso negado à tarefa {TarefaId} (usuário {UsuarioId})",
                id,
                _usuario.Id);

            return null;
        }

        return tarefa;
    }
}
