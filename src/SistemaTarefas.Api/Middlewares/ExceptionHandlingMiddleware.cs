using System.Net;
using Microsoft.EntityFrameworkCore;
using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Excecoes;

namespace SistemaTarefas.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _ambiente;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment ambiente)
    {
        _next = next;
        _logger = logger;
        _ambiente = ambiente;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            await Escrever(context, 422, ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            await Escrever(context, 412, "O registro foi alterado por outra requisição.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado");
            var mensagem = _ambiente.IsDevelopment() ? "Erro interno do servidor." : "Erro interno do servidor.";
            await Escrever(context, 500, mensagem);
        }
    }

    private static async Task Escrever(HttpContext context, int status, string erro)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(
            RespostaPadrao<object>.ComErro(erro, status >= 500 ? "Erro na operação" : "Erro na operação", context.TraceIdentifier));
    }
}
