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
    private readonly MySqlContainer _mysql = new MySqlBuilder() // cria container MySQL para testes de integração
        .WithImage("mysql:8.0")
        .WithUsername("tarefas")
        .WithPassword("tarefas123")
        .WithDatabase("sistematarefas")
        .Build();

    private WebApplicationFactory<Program>? _factory;
    private HttpClient _client = null!;

    public async Task InitializeAsync() // inicializa o container MySQL e cria a fábrica de aplicação para testes
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
    public async Task Login_E_CriarTarefa_DeveFuncionar() //testa o fluxo completo de login e criação de tarefa
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login", new { nomeUsuario = "admin", senha = "Admin123" });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        using var doc = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("dados").GetProperty("token").GetString();
        token.Should().NotBeNullOrEmpty();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!);
        var criar = await _client.PostAsJsonAsync("/api/v1/tarefas", new
        {
            titulo = "Tarefa de integração",
            descricao = "Validar o fluxo completo",
            dataVencimento = DateTime.UtcNow.AddDays(1)
        });

        criar.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AtualizarTarefa_ComIfMatch_DeveRetornarOk() // testa a atualização de uma tarefa usando o cabeçalho If-Match para controle de versão
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login", new { nomeUsuario = "admin", senha = "Admin123" });
        login.EnsureSuccessStatusCode();
        using var loginDoc = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var token = loginDoc.RootElement.GetProperty("dados").GetProperty("token").GetString();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!);

        var criar = await _client.PostAsJsonAsync("/api/v1/tarefas", new
        {
            titulo = "Antes do PUT",
            descricao = "Teste If-Match",
            dataVencimento = DateTime.UtcNow.AddDays(2)
        });
        criar.EnsureSuccessStatusCode();
        using var criarDoc = JsonDocument.Parse(await criar.Content.ReadAsStringAsync());
        var dados = criarDoc.RootElement.GetProperty("dados");
        var id = dados.GetProperty("id").GetGuid();
        var versao = dados.GetProperty("versao").GetUInt32();

        using var put = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/tarefas/{id}");
        put.Headers.TryAddWithoutValidation("If-Match", versao.ToString());
        put.Content = JsonContent.Create(new
        {
            titulo = "Depois do PUT",
            descricao = "Atualizado",
            dataVencimento = DateTime.UtcNow.AddDays(3)
        });

        var resposta = await _client.SendAsync(put);
        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
