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

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration).Enrich.FromLogContext().WriteTo.Console());

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var erros = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
            RespostaPadrao<object>.ComErros(erros, "Erros de validação", context.HttpContext.TraceIdentifier));
    };
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioAtual, UsuarioAtual>();
builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddMySqlConfiguration(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddHealthChecks().AddDbContextCheck<TarefasDbContext>("mysql");

var origens = builder.Configuration.GetSection("Cors:Origens").Get<string[]>() ?? ["http://localhost:3000"];
builder.Services.AddCors(o => o.AddPolicy("padrao", p => p.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod()));

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

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("SistemaTarefas"))
    .WithTracing(t => t.AddAspNetCoreInstrumentation().AddEntityFrameworkCoreInstrumentation().AddConsoleExporter())
    .WithMetrics(m => m.AddAspNetCoreInstrumentation().AddConsoleExporter());

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
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Insira o token JWT no formato: Bearer {seu token}"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
        var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
        if (File.Exists(xml))
        {
            c.IncludeXmlComments(xml);
        }

        c.OperationFilter<SwaggerHeadersOperationFilter>();
    });
}

var app = builder.Build();

using (var escopo = app.Services.CreateScope())
{
    var init = escopo.ServiceProvider.GetRequiredService<InicializadorBanco>();
    await init.ExecutarAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();
app.UseCors("padrao");
app.UseRateLimiter();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();

if (app.Environment.IsDevelopment())
{
    Console.WriteLine("API SistemaTarefas em http://localhost:5000");
    Console.WriteLine("Swagger na raiz. Login: POST /api/v1/auth/login");
}

app.Run();

public partial class Program;
