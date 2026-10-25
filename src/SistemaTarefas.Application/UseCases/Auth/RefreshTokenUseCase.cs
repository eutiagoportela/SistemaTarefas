using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Communication.Requests;
using SistemaTarefas.Communication.Responses;
using SistemaTarefas.Domain.Entidades;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Domain.Resultados;

namespace SistemaTarefas.Application.UseCases.Auth;

public interface IRefreshTokenUseCase
{
    Task<Resultado<LoginResponse>> Execute(RequestRefresh request, CancellationToken cancellationToken);
}

public sealed class RefreshTokenUseCase : IRefreshTokenUseCase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokens;
    private readonly TimeProvider _relogio;

    public RefreshTokenUseCase(IUsuarioRepository usuarios, IUnitOfWork uow, ITokenService tokens, TimeProvider relogio)
    {
        _usuarios = usuarios;
        _uow = uow;
        _tokens = tokens;
        _relogio = relogio;
    }

    public async Task<Resultado<LoginResponse>> Execute(RequestRefresh request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Resultado<LoginResponse>.Validacao("O refresh token é obrigatório.");
        }

        var hash = _tokens.HashRefreshToken(request.RefreshToken);
        var atual = await _usuarios.ObterRefreshTokenAtivoAsync(hash, cancellationToken);
        if (atual is null || !atual.EstaAtivo(_relogio))
        {
            return Resultado<LoginResponse>.NaoAutorizado("Refresh token inválido ou expirado.");
        }

        var usuario = await _usuarios.ObterPorIdAsync(atual.UsuarioId, cancellationToken);
        if (usuario is null)
        {
            return Resultado<LoginResponse>.NaoAutorizado("Refresh token inválido ou expirado.");
        }

        atual.Revogar(_relogio);
        var (token, expira) = _tokens.GerarAccessToken(usuario);
        var novoRefresh = _tokens.GerarRefreshToken();
        await _usuarios.AdicionarRefreshTokenAsync(
            RefreshToken.Criar(usuario.Id, _tokens.HashRefreshToken(novoRefresh), expira.AddDays(7), _relogio),
            cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return Resultado<LoginResponse>.Ok(new LoginResponse
        {
            Token = token,
            RefreshToken = novoRefresh,
            ExpiraEm = expira
        });
    }
}
