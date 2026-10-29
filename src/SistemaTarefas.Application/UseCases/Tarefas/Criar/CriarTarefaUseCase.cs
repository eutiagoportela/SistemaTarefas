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

namespace SistemaTarefas.Application.UseCases.Tarefas.Criar;

public sealed class CriarTarefaUseCase : ICriarTarefaUseCase
{
    private readonly ITarefaRepository _tarefas;
    private readonly IUnitOfWork _uow;
    private readonly IUsuarioAtual _usuario;
    private readonly IValidator<RequestCriarTarefa> _validator;
    private readonly TimeProvider _relogio;
    private readonly ILogger<CriarTarefaUseCase> _logger;

    public CriarTarefaUseCase(
        ITarefaRepository tarefas,
        IUnitOfWork uow,
        IUsuarioAtual usuario,
        IValidator<RequestCriarTarefa> validator,
        TimeProvider relogio,
        ILogger<CriarTarefaUseCase> logger)
    {
        _tarefas = tarefas;
        _uow = uow;
        _usuario = usuario;
        _validator = validator;
        _relogio = relogio;
        _logger = logger;
    }

    public async Task<Resultado<TarefaResponse>> Execute(RequestCriarTarefa request, CancellationToken cancellationToken)
    {
        var validacao = await _validator.ValidateAsync(request, cancellationToken);
        if (!validacao.IsValid)
        {
            var erros = validacao.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Dados inválidos ao criar tarefa: {Erros}", string.Join(", ", erros));
            return Resultado<TarefaResponse>.Validacao("Erros de validação", erros);
        }

        try
        {
            var tarefa = Tarefa.Criar(request.Titulo, request.Descricao, request.DataVencimento, _usuario.Id, _relogio);
            await _tarefas.AdicionarAsync(tarefa, cancellationToken);
            await _uow.CommitAsync(cancellationToken);
            return Resultado<TarefaResponse>.Ok(tarefa.ParaResponse(_relogio));
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Regra de domínio ao criar tarefa: {Mensagem}", ex.Message);
            return Resultado<TarefaResponse>.Validacao(ex.Message);
        }
    }
}
