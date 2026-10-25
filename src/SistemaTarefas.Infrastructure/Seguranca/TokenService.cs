using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Domain.Entidades;

namespace SistemaTarefas.Infrastructure.Seguranca;

public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _relogio;

    public TokenService(IOptions<JwtOptions> options, TimeProvider relogio)
    {
        _options = options.Value;
        _relogio = relogio;
    }

    public (string Token, DateTime ExpiraEm) GerarAccessToken(Usuario usuario)
    {
        var expira = _relogio.GetUtcNow().UtcDateTime.AddMinutes(_options.ExpiracaoMinutos);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, usuario.NomeUsuario),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NomeUsuario),
            new Claim(ClaimTypes.Role, usuario.Papel.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var jwt = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            expires: expira,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expira);
    }

    public string GerarRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes);
    }
}
