using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MySql;
using Xunit;

namespace SistemaTarefas.Api.IntegrationTests;

[Trait("Category", "Integration")]
public class SwaggerOpenApiTests : IAsyncLifetime
{
    private readonly MySqlContainer _mysql = new MySqlBuilder()
        .WithImage("mysql:8.0")
        .WithUsername("tarefas")
        .WithPassword("tarefas123")
        .WithDatabase("sistematarefas")
        .Build();

    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    public async Task InitializeAsync()
    {
        await _mysql.StartAsync();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Default", _mysql.GetConnectionString() + ";TreatTinyAsBoolean=true");
            builder.UseSetting("Jwt:Secret", "integration-secret-key-minimum-32-characters-xx");
            builder.UseSetting("Jwt:Issuer", "SistemaTarefas");
            builder.UseSetting("Jwt:Audience", "SistemaTarefas");
            builder.UseSetting("Swagger:Habilitado", "true");
            builder.UseSetting("Seed:Admin:Usuario", "admin");
            builder.UseSetting("Seed:Admin:Senha", "Admin123");
        });
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _mysql.DisposeAsync();
    }

    [Fact]
    public async Task OpenApi_PutTarefas_DeveIncluirHeaderIfMatch()
    {
        var json = await _client!.GetStringAsync("/swagger/v1/swagger.json");
        json.Should().Contain("\"If-Match\"");
        json.Should().Contain("api/v1/tarefas/{id}");
    }
}
