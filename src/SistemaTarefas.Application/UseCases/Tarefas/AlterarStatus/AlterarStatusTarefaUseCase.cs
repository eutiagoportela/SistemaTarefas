using Microsoft.Extensions.Logging;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Application.Mapeamentos;
using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Entidades;
using SistemaTarefas.Domain.Excecoes;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Tarefas.AlterarStatus;

public sealed class AlterarStatusTarefaUseCase : IAlterarStatusTarefaUseCase
{
    private readonly ITarefaRepository _tarefas;
    private readonly IUnitOfWork _uow;
    private readonly IUsuarioAtual _usuario;
    private readonly TimeProvider _relogio;
    private readonly ILogger<AlterarStatusTarefaUseCase> _logger;

    public AlterarStatusTarefaUseCase(
        ITarefaRepository tarefas,
        IUnitOfWork uow,
        IUsuarioAtual usuario,
        TimeProvider relogio,
        ILogger<AlterarStatusTarefaUseCase> logger)
    {
        _tarefas = tarefas;
        _uow = uow;
        _usuario = usuario;
        _relogio = relogio;
        _logger = logger;
    }

    public Task<Resultado<TarefaResponse>> Iniciar(
        Guid id,
        uint versaoEsperada,
        CancellationToken cancellationToken)
        => Executar(
            id,
            versaoEsperada,
            t => t.Iniciar(_relogio),
            cancellationToken);

    public Task<Resultado<TarefaResponse>> Concluir(
        Guid id,
        uint versaoEsperada,
        CancellationToken cancellationToken)
        => Executar(
            id,
            versaoEsperada,
            t => t.Concluir(_relogio),
            cancellationToken);

    public Task<Resultado<TarefaResponse>> Reabrir(
        Guid id,
        uint versaoEsperada,
        CancellationToken cancellationToken)
        => Executar(
            id,
            versaoEsperada,
            t => t.Reabrir(_relogio),
            cancellationToken);

    private async Task<Resultado<TarefaResponse>> Executar(
        Guid id,
        uint versaoEsperada,
        Action<Tarefa> comando,
        CancellationToken cancellationToken)
    {
        var tarefa = await ObterAutorizada(id, cancellationToken);

        if (tarefa is null)
        {
            return Resultado<TarefaResponse>.NaoEncontrado(
                "Tarefa não encontrada.");
        }

        if (tarefa.Versao != versaoEsperada)
        {
            _logger.LogWarning(
                "Conflito de versão na tarefa {TarefaId}: esperada {VersaoEsperada}, atual {VersaoAtual}",
                id,
                versaoEsperada,
                tarefa.Versao);

            return Resultado<TarefaResponse>.Conflito(
                "A tarefa foi alterada por outra requisição. Recarregue e tente novamente.");
        }

        try
        {
            comando(tarefa);

            await _uow.CommitAsync(cancellationToken);

            return Resultado<TarefaResponse>.Ok(
                tarefa.ParaResponse(_relogio));
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(
                "Regra de domínio na tarefa {TarefaId}: {Mensagem}",
                id,
                ex.Message);

            return Resultado<TarefaResponse>.Validacao(ex.Message);
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
