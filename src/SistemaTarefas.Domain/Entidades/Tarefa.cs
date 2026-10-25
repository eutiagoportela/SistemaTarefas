using SistemaTarefas.Domain.Enums;
using SistemaTarefas.Domain.Eventos;
using SistemaTarefas.Domain.Excecoes;
using SistemaTarefas.Domain.ValueObjects;

namespace SistemaTarefas.Domain.Entidades;

public sealed class Tarefa
{
    private readonly List<IDomainEvent> _eventos = [];

    public Guid Id { get; private set; }
    public Titulo Titulo { get; private set; } = null!;
    public Descricao Descricao { get; private set; } = null!;
    public StatusTarefa Status { get; private set; }
    public DateTime DataVencimento { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }
    public DateTime? DataConclusao { get; private set; }
    public Guid UsuarioId { get; private set; }
    public uint Versao { get; private set; }
    public bool IsDeleted { get; private set; }

    public IReadOnlyCollection<IDomainEvent> Eventos => _eventos.AsReadOnly();

    private Tarefa()
    {
    }

    public static Tarefa Criar(
        string titulo,
        string? descricao,
        DateTime dataVencimento,
        Guid usuarioId,
        TimeProvider relogio)
    {
        ArgumentNullException.ThrowIfNull(relogio);
        if (usuarioId == Guid.Empty)
        {
            throw new DomainException("O usuário da tarefa é obrigatório.");
        }

        var agora = relogio.GetUtcNow().UtcDateTime;
        ValidarVencimento(dataVencimento, agora);

        var tarefa = new Tarefa
        {
            Id = Guid.NewGuid(),
            Titulo = new Titulo(titulo),
            Descricao = new Descricao(descricao),
            Status = StatusTarefa.Pendente,
            DataVencimento = dataVencimento.ToUniversalTime(),
            DataCriacao = agora,
            UsuarioId = usuarioId,
            Versao = 1
        };

        tarefa._eventos.Add(new TarefaCriada(tarefa.Id, usuarioId, agora));
        return tarefa;
    }

    public void AtualizarDetalhes(string titulo, string? descricao, DateTime dataVencimento, TimeProvider relogio)
    {
        GarantirAtiva();
        if (Status == StatusTarefa.Concluido)
        {
            throw new DomainException("Uma tarefa concluída não pode ter os detalhes alterados. Reabra-a antes.");
        }

        var agora = Agora(relogio);
        ValidarVencimento(dataVencimento, agora);

        Titulo = new Titulo(titulo);
        Descricao = new Descricao(descricao);
        DataVencimento = dataVencimento.ToUniversalTime();
        DataAtualizacao = agora;
        IncrementarVersao();
    }

    public void Iniciar(TimeProvider relogio)
    {
        GarantirAtiva();
        if (Status != StatusTarefa.Pendente)
        {
            throw new DomainException("Somente tarefas pendentes podem ser iniciadas.");
        }

        Status = StatusTarefa.EmAndamento;
        DataAtualizacao = Agora(relogio);
        IncrementarVersao();
    }

    public void Concluir(TimeProvider relogio)
    {
        GarantirAtiva();
        if (Status != StatusTarefa.EmAndamento)
        {
            throw new DomainException("Somente tarefas em andamento podem ser concluídas.");
        }

        var agora = Agora(relogio);
        Status = StatusTarefa.Concluido;
        DataConclusao = agora;
        DataAtualizacao = agora;
        IncrementarVersao();
        _eventos.Add(new TarefaConcluida(Id, UsuarioId, agora));
    }

    public void Reabrir(TimeProvider relogio)
    {
        GarantirAtiva();
        if (Status != StatusTarefa.Concluido)
        {
            throw new DomainException("Somente tarefas concluídas podem ser reabertas.");
        }

        var agora = Agora(relogio);
        Status = StatusTarefa.Pendente;
        DataConclusao = null;
        DataAtualizacao = agora;
        IncrementarVersao();
        _eventos.Add(new TarefaReaberta(Id, UsuarioId, agora));
    }

    public void Remover(TimeProvider relogio)
    {
        GarantirAtiva();
        IsDeleted = true;
        DataAtualizacao = Agora(relogio);
        IncrementarVersao();
    }

    public bool EstaAtrasada(TimeProvider relogio)
        => Status != StatusTarefa.Concluido && DataVencimento < Agora(relogio);

    public void LimparEventos() => _eventos.Clear();

    private void GarantirAtiva()
    {
        if (IsDeleted)
        {
            throw new DomainException("A tarefa foi removida.");
        }
    }

    private static void ValidarVencimento(DateTime dataVencimento, DateTime agora)
    {
        if (dataVencimento.ToUniversalTime() <= agora)
        {
            throw new DomainException("A data de vencimento deve ser futura.");
        }
    }

    private static DateTime Agora(TimeProvider relogio)
    {
        ArgumentNullException.ThrowIfNull(relogio);
        return relogio.GetUtcNow().UtcDateTime;
    }

    private void IncrementarVersao() => Versao++;
}
