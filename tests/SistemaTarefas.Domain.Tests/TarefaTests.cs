using FluentAssertions;
using SistemaTarefas.Domain.Entidades;
using SistemaTarefas.Domain.Enums;
using SistemaTarefas.Domain.Eventos;
using SistemaTarefas.Domain.Excecoes;
using SistemaTarefas.Domain.ValueObjects;
using Xunit;

namespace SistemaTarefas.Domain.Tests;

public class TarefaTests
{
    private static readonly DateTime AgoraFixo = new(2026, 10, 24, 18, 0, 0, DateTimeKind.Utc);
    private readonly TimeProvider _relogio = new RelogioFixo(AgoraFixo);

    [Fact]
    public void Criar_DeveFalhar_QuandoTituloForCurto()
    {
        var act = () => Tarefa.Criar("ab", "Descrição válida", AgoraFixo.AddDays(1), Guid.NewGuid(), _relogio);
        act.Should().Throw<DomainException>().WithMessage("*entre 3 e 120*");
    }

    [Fact]
    public void Criar_DeveFalhar_QuandoVencimentoNaoForFuturo()
    {
        var act = () => Tarefa.Criar("Título ok", "Descrição", AgoraFixo, Guid.NewGuid(), _relogio);
        act.Should().Throw<DomainException>().WithMessage("*vencimento deve ser futura*");
    }

    [Fact]
    public void Criar_DeveNascerPendente_ComEvento()
    {
        var usuario = Guid.NewGuid();
        var tarefa = Tarefa.Criar("Enviar relatório", "Relatório semanal", AgoraFixo.AddDays(2), usuario, _relogio);

        tarefa.Status.Should().Be(StatusTarefa.Pendente);
        tarefa.Titulo.Valor.Should().Be("Enviar relatório");
        tarefa.Eventos.Should().ContainSingle(e => e is TarefaCriada);
        tarefa.UsuarioId.Should().Be(usuario);
    }

    [Fact]
    public void Iniciar_DeveFalhar_QuandoNaoEstiverPendente()
    {
        var tarefa = CriarValida();
        tarefa.Iniciar(_relogio);
        var act = () => tarefa.Iniciar(_relogio);
        act.Should().Throw<DomainException>().WithMessage("*pendentes*");
    }

    [Fact]
    public void Concluir_DeveFalhar_QuandoNaoEstiverEmAndamento()
    {
        var tarefa = CriarValida();
        var act = () => tarefa.Concluir(_relogio);
        act.Should().Throw<DomainException>().WithMessage("*em andamento*");
    }

    [Fact]
    public void FluxoFeliz_IniciarConcluirReabrir()
    {
        var tarefa = CriarValida();
        tarefa.Iniciar(_relogio);
        tarefa.Status.Should().Be(StatusTarefa.EmAndamento);

        tarefa.Concluir(_relogio);
        tarefa.Status.Should().Be(StatusTarefa.Concluido);
        tarefa.DataConclusao.Should().NotBeNull();
        tarefa.Eventos.Should().Contain(e => e is TarefaConcluida);

        var actEditar = () => tarefa.AtualizarDetalhes("Novo título", "Nova", AgoraFixo.AddDays(3), _relogio);
        actEditar.Should().Throw<DomainException>();

        tarefa.Reabrir(_relogio);
        tarefa.Status.Should().Be(StatusTarefa.Pendente);
        tarefa.DataConclusao.Should().BeNull();
        tarefa.Eventos.Should().Contain(e => e is TarefaReaberta);
    }

    [Fact]
    public void Titulo_DeveFalhar_QuandoVazio()
    {
        var act = () => new Titulo("  ");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Descricao_DeveFalhar_QuandoPassarDoLimite()
    {
        var act = () => new Descricao(new string('a', 1001));
        act.Should().Throw<DomainException>();
    }

    private Tarefa CriarValida()
        => Tarefa.Criar("Tarefa válida", "Descrição", AgoraFixo.AddHours(8), Guid.NewGuid(), _relogio);
}

file sealed class RelogioFixo : TimeProvider
{
    private readonly DateTimeOffset _agora;

    public RelogioFixo(DateTime utcNow) => _agora = new DateTimeOffset(utcNow, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _agora;
}
