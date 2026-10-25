using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Domain.Entidades;
using SistemaTarefas.Domain.Eventos;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Infrastructure.Persistencia;

namespace SistemaTarefas.Infrastructure.Repositorios;

public sealed class UnitOfWork : IUnitOfWork
{
    private static readonly ResiliencePipeline Pipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(200),
            BackoffType = DelayBackoffType.Exponential,
            ShouldHandle = new PredicateBuilder()
                .Handle<DbUpdateException>(ex => ex is not DbUpdateConcurrencyException)
        })
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = 8,
            SamplingDuration = TimeSpan.FromSeconds(30),
            BreakDuration = TimeSpan.FromSeconds(15)
        })
        .Build();

    private readonly TarefasDbContext _contexto;
    private readonly IUsuarioAtual _usuarioAtual;
    private readonly TimeProvider _relogio;

    public UnitOfWork(TarefasDbContext contexto, IUsuarioAtual usuarioAtual, TimeProvider relogio)
    {
        _contexto = contexto;
        _usuarioAtual = usuarioAtual;
        _relogio = relogio;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        var tarefas = _contexto.ChangeTracker.Entries<Tarefa>()
            .Select(e => e.Entity)
            .Where(t => t.Eventos.Count > 0)
            .ToList();

        foreach (var tarefa in tarefas)
        {
            foreach (var evento in tarefa.Eventos)
            {
                _contexto.Auditorias.Add(new AuditoriaRegistro
                {
                    Id = Guid.NewGuid(),
                    TarefaId = tarefa.Id,
                    UsuarioId = _usuarioAtual.EstaAutenticado ? _usuarioAtual.Id : tarefa.UsuarioId,
                    UsuarioNome = _usuarioAtual.EstaAutenticado ? _usuarioAtual.Nome : "sistema",
                    Acao = evento.GetType().Name,
                    Detalhe = evento switch
                    {
                        TarefaCriada => "Tarefa criada",
                        TarefaConcluida => "Tarefa concluída",
                        TarefaReaberta => "Tarefa reaberta",
                        _ => evento.GetType().Name
                    },
                    OcorridoEm = evento.OcorridoEm
                });
            }
        }

        await Pipeline.ExecuteAsync(async ct => await _contexto.SaveChangesAsync(ct), cancellationToken);

        foreach (var tarefa in tarefas)
        {
            tarefa.LimparEventos();
        }
    }
}
