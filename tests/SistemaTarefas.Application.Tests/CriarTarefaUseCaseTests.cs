using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Application.UseCases.Tarefas.Criar;
using SistemaTarefas.Application.Validadores;
using SistemaTarefas.Communication.Requests;
using SistemaTarefas.Domain.Entidades;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Domain.Resultados;
using Xunit;

namespace SistemaTarefas.Application.Tests;

public class CriarTarefaUseCaseTests
{
    [Fact] // marca o método como um teste que será executado pelo xUnit
    public async Task Execute_DeveRetornarValidacao_QuandoTituloVazio() // testa se o caso de uso retorna um resultado de validação quando o título da tarefa é vazio
    {
        var useCase = CriarSut(out _);
        var resultado = await useCase.Execute(new RequestCriarTarefa
        {
            Titulo = "",
            DataVencimento = DateTime.UtcNow.AddDays(1)
        }, CancellationToken.None);

        resultado.Sucesso.Should().BeFalse();
        resultado.Erro!.Tipo.Should().Be(TipoErro.Validacao);
    }

    [Fact]
    public async Task Execute_DevePersistir_QuandoDadosValidos() // testa se o caso de uso persiste a tarefa quando os dados são válidos
    {
        var useCase = CriarSut(out var uow);
        var resultado = await useCase.Execute(new RequestCriarTarefa
        {
            Titulo = "Estudar DDD",
            Descricao = "Ler o capítulo 1",
            DataVencimento = DateTime.UtcNow.AddDays(2)
        }, CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.Titulo.Should().Be("Estudar DDD");
        uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CriarTarefaUseCase CriarSut(out Mock<IUnitOfWork> uow) // cria uma instância do caso de uso CriarTarefaUseCase com dependências simuladas para testes
    {
        var repo = new Mock<ITarefaRepository>();
        uow = new Mock<IUnitOfWork>();
        var usuario = new Mock<IUsuarioAtual>();
        usuario.SetupGet(x => x.Id).Returns(Guid.NewGuid());
        IValidator<RequestCriarTarefa> validator = new RequestCriarTarefaValidator();
        return new CriarTarefaUseCase(
            repo.Object,
            uow.Object,
            usuario.Object,
            validator,
            TimeProvider.System,
            NullLogger<CriarTarefaUseCase>.Instance);
    }
}
