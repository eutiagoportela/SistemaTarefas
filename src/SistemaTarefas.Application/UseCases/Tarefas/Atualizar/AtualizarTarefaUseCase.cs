using FluentValidation;
using Microsoft.Extensions.Logging;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Application.Mapeamentos;
using SistemaTarefas.Communication.Requests;
using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Entidades;
using SistemaTarefas.Domain.Excecoes;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Tarefas.Atualizar;

public sealed class AtualizarTarefaUseCase : IAtualizarTarefaUseCase
{
    private readonly ITarefaRepository _tarefas;
    private readonly IUnitOfWork _uow;
    private readonly IUsuarioAtual _usuario;
    private readonly IValidator<RequestAtualizarTarefa> _validator;
    private readonly TimeProvider _relogio;
    private readonly ILogger<AtualizarTarefaUseCase> _logger;

    public AtualizarTarefaUseCase(
        ITarefaRepository tarefas,
        IUnitOfWork uow,
        IUsuarioAtual usuario,
        IValidator<RequestAtualizarTarefa> validator,
        TimeProvider relogio,
        ILogger<AtualizarTarefaUseCase> logger)
    {
        _tarefas = tarefas;
        _uow = uow;
        _usuario = usuario;
        _validator = validator;
        _relogio = relogio;
        _logger = logger;
    }

    public async Task<Resultado<TarefaResponse>> Execute(
        Guid id,
        uint versaoEsperada,
        RequestAtualizarTarefa request,
        CancellationToken cancellationToken)
    {
        var validacao = await _validator.ValidateAsync(
            request,
            cancellationToken);

        if (!validacao.IsValid)
        {
            var erros = validacao.Errors
                .Select(e => e.ErrorMessage)
                .ToList();

            _logger.LogWarning(
                "Dados inválidos ao atualizar tarefa {TarefaId}: {Erros}",
                id,
                string.Join(", ", erros));

            return Resultado<TarefaResponse>.Validacao(
                "Erros de validação",
                erros);
        }

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
            tarefa.AtualizarDetalhes(
                request.Titulo,
                request.Descricao,
                request.DataVencimento,
                _relogio);

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
