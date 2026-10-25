using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SistemaTarefas.Api.Swagger;

/// <summary>
/// O ApiExplorer do ASP.NET Core não expõe alguns parâmetros [FromHeader] no OpenAPI;
/// este filtro garante If-Match e Idempotency-Key visíveis no Swagger UI.
/// </summary>
public sealed class SwaggerHeadersOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath ?? string.Empty;
        var method = context.ApiDescription.HttpMethod ?? string.Empty;

        if (method.Equals("POST", StringComparison.OrdinalIgnoreCase)
            && path.Equals("api/v1/tarefas", StringComparison.OrdinalIgnoreCase))
        {
            AdicionarHeader(
                operation,
                "Idempotency-Key",
                required: false,
                "Chave opcional para idempotência na criação (UUID recomendado).");
        }

        if (RequerIfMatch(method, path))
        {
            AdicionarHeader(
                operation,
                "If-Match",
                required: true,
                "Número inteiro do campo JSON `versao` (ex.: 1) ou ETag do GET por id (ex.: \"1\"). Não use `traceId`.");
        }
    }

    private static bool RequerIfMatch(string method, string path)
    {
        if (!path.StartsWith("api/v1/tarefas/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (method.Equals("PUT", StringComparison.OrdinalIgnoreCase)
            && !path.Contains("/historico", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (method.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (method.Equals("POST", StringComparison.OrdinalIgnoreCase)
            && (path.EndsWith("/iniciar", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith("/concluir", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith("/reabrir", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return false;
    }

    private static void AdicionarHeader(OpenApiOperation operation, string nome, bool required, string descricao)
    {
        if (operation.Parameters.Any(p =>
                p.Name.Equals(nome, StringComparison.OrdinalIgnoreCase)
                && p.In == ParameterLocation.Header))
        {
            return;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = nome,
            In = ParameterLocation.Header,
            Required = required,
            Description = descricao,
            Schema = new OpenApiSchema { Type = "string", Example = nome == "If-Match" ? new Microsoft.OpenApi.Any.OpenApiString("1") : null }
        });
    }
}
