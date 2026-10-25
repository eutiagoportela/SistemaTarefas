using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace SistemaTarefas.Architecture.Tests;

public class DependenciasTests
{
    [Fact]
    public void Domain_NaoDeveDependerDeOutrasCamadas()
    {
        var resultado = Types.InAssembly(typeof(Domain.Entidades.Tarefa).Assembly)
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
    public void Application_NaoDeveDependerDeInfrastructureNemApi()
    {
        var resultado = Types.InAssembly(typeof(Application.Extensions.DependencyInjectionExtension).Assembly)
            .Should()
            .NotHaveDependencyOnAny("SistemaTarefas.Infrastructure", "SistemaTarefas.Api")
            .GetResult();

        resultado.IsSuccessful.Should().BeTrue();
    }
}
