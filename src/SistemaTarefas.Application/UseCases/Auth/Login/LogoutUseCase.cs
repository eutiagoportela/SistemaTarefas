using Microsoft.Extensions.Logging;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Auth.Logout;

public sealed class LogoutUseCase : ILogoutUseCase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokens;
    private readonly TimeProvider _relogio;
    private readonly ILogger<LogoutUseCase> _logger;

    public LogoutUseCase(
        IUsuarioRepository usuarios,
        IUnitOfWork uow,
        ITokenService tokens,
        TimeProvider relogio,
        ILogger<LogoutUseCase> logger)
    {
        _usuarios = usuarios;
        _uow = uow;
        _tokens = tokens;
        _relogio = relogio;
        _logger = logger;
    }

    public async Task<Resultado> Execute(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Resultado.Ok();
        }

        var hash = _tokens.HashRefreshToken(refreshToken);

        var atual = await _usuarios.ObterRefreshTokenAtivoAsync(
            hash,
            cancellationToken);

        if (atual is null)
        {
            return Resultado.Ok();
        }

        atual.Revogar(_relogio);

        await _uow.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Refresh token revogado com sucesso");

        return Resultado.Ok();
    }
}
