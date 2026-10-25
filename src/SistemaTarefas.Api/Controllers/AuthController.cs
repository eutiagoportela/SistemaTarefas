using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SistemaTarefas.Api.Extensoes;
using SistemaTarefas.Application.UseCases.Auth;
using SistemaTarefas.Communication.Requests;
using SistemaTarefas.Communication.Responses;

namespace SistemaTarefas.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    /// <summary>Registra um novo usuário.</summary>
    [AllowAnonymous]
    [HttpPost("registrar")]
    [ProducesResponseType(typeof(RespostaPadrao<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Registrar(
        [FromServices] IRegistrarUsuarioUseCase useCase,
        [FromBody] RequestRegistrar request,
        CancellationToken cancellationToken)
    {
        var resultado = await useCase.Execute(request, cancellationToken);
        return resultado.ParaHttp(HttpContext, "Usuário registrado com sucesso");
    }

    /// <summary>Realiza login e retorna um token JWT.</summary>
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    [ProducesResponseType(typeof(RespostaPadrao<LoginResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(
        [FromServices] ILoginUseCase useCase,
        [FromBody] RequestLogin request,
        CancellationToken cancellationToken)
    {
        var resultado = await useCase.Execute(request, cancellationToken);
        return resultado.ParaHttp(HttpContext, "Login realizado com sucesso");
    }

    /// <summary>Renova o access token.</summary>
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromServices] IRefreshTokenUseCase useCase,
        [FromBody] RequestRefresh request,
        CancellationToken cancellationToken)
    {
        var resultado = await useCase.Execute(request, cancellationToken);
        return resultado.ParaHttp(HttpContext, "Token renovado");
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
        => Ok(RespostaPadrao<object>.ComSucesso(null, "Logout realizado. Descarte o token no cliente.", HttpContext.TraceIdentifier));
}
