using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using SistemaTarefas.Api.Middlewares;
using SistemaTarefas.Api.Seguranca;
using SistemaTarefas.Api.Swagger;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Application.Extensions;
using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Infrastructure.Persistencia;
using SistemaTarefas.Infrastructure.Seguranca;

var builder = WebApplication.CreateBuilder(args);

// Configura o Serilog para registrar logs no console usando as configurações da aplicação.
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration).Enrich.FromLogContext().WriteTo.Console());

// Registra os controllers e configura o JSON para usar camelCase e enums como texto.
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Padroniza as respostas HTTP 400 quando a validação automática do modelo encontra erros.
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var erros = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
            RespostaPadrao<object>.ComErros(erros, "Erros de validação", context.HttpContext.TraceIdentifier));
    };
});

// Permite acessar o contexto HTTP e registra o usuário atual como Scoped para cada requisição.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioAtual, UsuarioAtual>();

// Configura a autenticação JWT usando as configurações e o ambiente da aplicação.
builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);

// Configura a conexão e os serviços de persistência com MySQL. (PEGA DADOS EXTERNOS COMO NO RENDER VARIAVEL DE AMBIENTE)
builder.Services.AddMySqlConfiguration(builder.Configuration);

// Registra os serviços da camada Application por meio de um método de extensão.
builder.Services.AddApplication();

// Registra verificações de saúde da aplicação e da conexão com o banco de dados.
builder.Services.AddHealthChecks().AddDbContextCheck<TarefasDbContext>("mysql");

// Obtém as origens permitidas pelo CORS ou usa localhost:3000 como padrão.
var origens = builder.Configuration.GetSection("Cors:Origens").Get<string[]>() ?? ["http://localhost:3000"];


// Configura a política CORS para permitir métodos e cabeçalhos das origens autorizadas.
builder.Services.AddCors(o => o.AddPolicy("padrao", p => p.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod()));

// Define um limite de 10 requisições por minuto, por IP, para a política chamada "login".
builder.Services.AddRateLimiter(o =>
{
    o.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "anon",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1)
        }));
});

// Configura rastreamento e métricas da API e do Entity Framework Core, enviando os dados ao console.
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("SistemaTarefas"))
    .WithTracing(t => t.AddAspNetCoreInstrumentation().AddEntityFrameworkCoreInstrumentation().AddConsoleExporter())
    .WithMetrics(m => m.AddAspNetCoreInstrumentation().AddConsoleExporter());

// Registra e configura a documentação da API somente quando o Swagger está habilitado.
var swaggerLigado = builder.Configuration.GetValue("Swagger:Habilitado", builder.Environment.IsDevelopment());
if (swaggerLigado)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Sistema Tarefas API",
            Version = "v1",
            Description = "API de gestão de tarefas (ToDo) — .NET 8"
        });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme // Configura o esquema Bearer para informar o JWT no cabeçalho Authorization.
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Insira o token JWT no formato: Bearer {seu token}"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement // Permite que o Swagger represente a autenticação Bearer nos endpoints documentados.
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });

        // Localiza o arquivo XML de documentação gerado na compilação.
        var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
        // Inclui os comentários XML na documentação se o arquivo existir.
        if (File.Exists(xml))
        {
            c.IncludeXmlComments(xml);
        }

        c.OperationFilter<SwaggerHeadersOperationFilter>();
    });
}

// Constrói a aplicação com os serviços e as configurações registrados.
var app = builder.Build();

// Cria um escopo de DI para obter e executar o inicializador do banco de dados.
using (var escopo = app.Services.CreateScope())
{
    var init = escopo.ServiceProvider.GetRequiredService<InicializadorBanco>();
    await init.ExecutarAsync();
}

// Registra o middleware responsável por tratar exceções não tratadas.
app.UseMiddleware<ExceptionHandlingMiddleware>();
// Registra informações sobre as requisições HTTP usando Serilog.
app.UseSerilogRequestLogging();
// Aplica a política CORS configurada anteriormente.
app.UseCors("padrao");
// Ativa o middleware de limitação de requisições.
app.UseRateLimiter();

// Habilita a interface e a documentação Swagger quando a configuração permitir.
if (swaggerLigado)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema Tarefas v1");
        c.RoutePrefix = string.Empty;
        c.DocumentTitle = "Sistema Tarefas API";
    });
}

app.UseAuthentication();// Valida o token e identifica o usuário da requisição.
app.UseAuthorization();// Verifica se o usuário autenticado tem permissão para acessar o recurso.

app.MapControllers();// Disponibiliza as rotas definidas nos controllers.

// Disponibiliza a verificação de saúde sem exigir autenticação.
app.MapHealthChecks("/health").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();

// Exibe mensagens de inicialização apenas no ambiente de desenvolvimento.
if (app.Environment.IsDevelopment())
{
    Console.WriteLine("API SistemaTarefas em http://localhost:5000");
    Console.WriteLine("Swagger na raiz. Login: POST /api/v1/auth/login");
}

app.Run();

// Permite que testes automatizados acessem o tipo Program.
public partial class Program;
