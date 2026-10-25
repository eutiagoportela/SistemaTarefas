using System.Security.Claims;
using SistemaTarefas.Application.Abstracoes;

namespace SistemaTarefas.Api.Seguranca;

public sealed class UsuarioAtual : IUsuarioAtual
{
    public UsuarioAtual(IHttpContextAccessor accessor)
    {
        var user = accessor.HttpContext?.User;
        EstaAutenticado = user?.Identity?.IsAuthenticated == true;
        if (!EstaAutenticado || user is null)
        {
            return;
        }

        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(id, out var guid))
        {
            Id = guid;
        }

        Nome = user.Identity?.Name ?? user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        IsAdmin = user.IsInRole("Admin");
    }

    public Guid Id { get; }
    public string Nome { get; } = string.Empty;
    public bool IsAdmin { get; }
    public bool EstaAutenticado { get; }
}
