using Microsoft.AspNetCore.Mvc;
using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Api.Extensoes;

public static class ResultadoHttp
{
    public static IActionResult ParaHttp<T>(this Resultado<T> resultado, HttpContext http, string mensagemSucesso, int statusSucesso = StatusCodes.Status200OK)
    {
        var trace = http.TraceIdentifier;
        if (!resultado.Sucesso)
        {
            return Falha(resultado.Erro!, trace);
        }

        return new ObjectResult(RespostaPadrao<T>.ComSucesso(resultado.Valor, mensagemSucesso, trace))
        {
            StatusCode = statusSucesso
        };
    }

    public static IActionResult ParaHttp(this Resultado resultado, HttpContext http, string mensagemSucesso)
    {
        var trace = http.TraceIdentifier;
        if (!resultado.Sucesso)
        {
            return Falha(resultado.Erro!, trace);
        }

        return new OkObjectResult(RespostaPadrao<object>.ComSucesso(null, mensagemSucesso, trace));
    }

    private static ObjectResult Falha(ErroAplicacao erro, string trace)
    {
        var status = erro.Tipo switch
        {
            TipoErro.Validacao => StatusCodes.Status400BadRequest,
            TipoErro.NaoEncontrado => StatusCodes.Status404NotFound,
            TipoErro.Conflito => StatusCodes.Status409Conflict,
            TipoErro.Proibido => StatusCodes.Status403Forbidden,
            TipoErro.NaoAutorizado => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest
        };

        var corpo = erro.Detalhes is { Count: > 0 }
            ? RespostaPadrao<object>.ComErros(erro.Detalhes, erro.Mensagem, trace)
            : RespostaPadrao<object>.ComErro(erro.Mensagem, "Erro na operação", trace);

        return new ObjectResult(corpo) { StatusCode = status };
    }
}
