using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace SistemaTarefas.Architecture.Tests;

public class DependenciasTests
{
    [Fact]
    public void Domain_NaoDeveDependerDeOutrasCamadas() // testa se a camada de domínio não possui dependências para outras camadas do sistema
    {
        var resultado = Types.InAssembly(typeof(Domain.Entidades.Tarefa).Assembly) // aqui verifica se a camada de domínio não possui dependências para outras camadas do sistema, ela deve ser independente e não depender de outras camadas, como Application, Infrastructure, Api ou Communication.
            .Should()
            .NotHaveDependencyOnAny(
                "SistemaTarefas.Application",
                "SistemaTarefas.Infrastructure",
                "SistemaTarefas.Api",
                "SistemaTarefas.Communication")
            .GetResult();

        resultado.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_NaoDeveDependerDeInfrastructureNemApi() // testa se a camada de aplicação não possui dependências para as camadas de infraestrutura e API, ela deve depender apenas da camada de domínio e de abstrações
    {
        var resultado = Types.InAssembly(typeof(Application.Extensions.DependencyInjectionExtension).Assembly)
            .Should()
            .NotHaveDependencyOnAny("SistemaTarefas.Infrastructure", "SistemaTarefas.Api")
            .GetResult();

        resultado.IsSuccessful.Should().BeTrue();
    }
}
