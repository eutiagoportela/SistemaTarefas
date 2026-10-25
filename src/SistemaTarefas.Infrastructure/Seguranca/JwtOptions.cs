using System.ComponentModel.DataAnnotations;

namespace SistemaTarefas.Infrastructure.Seguranca;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(32)]
    public string Secret { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = "SistemaTarefas";

    [Required]
    public string Audience { get; set; } = "SistemaTarefas";

    [Range(5, 240)]
    public int ExpiracaoMinutos { get; set; } = 60;
}
