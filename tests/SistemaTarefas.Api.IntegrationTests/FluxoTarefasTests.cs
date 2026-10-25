using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MySql;
using Xunit;

namespace SistemaTarefas.Api.IntegrationTests;

[Trait("Category", "Integration")]
public class FluxoTarefasTests : IAsyncLifetime
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
            builder.UseSetting("Swagger:Habilitado", "false");
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
    public async Task Login_E_CriarTarefa_DeveFuncionar()
    {
        var login = await _client!.PostAsJsonAsync("/api/v1/auth/login", new { nomeUsuario = "admin", senha = "Admin123" });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        using var doc = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("dados").GetProperty("token").GetString();
        token.Should().NotBeNullOrEmpty();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var criar = await _client.PostAsJsonAsync("/api/v1/tarefas", new
        {
            titulo = "Tarefa de integração",
            descricao = "Validar o fluxo completo",
            dataVencimento = DateTime.UtcNow.AddDays(1)
        });

        criar.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
