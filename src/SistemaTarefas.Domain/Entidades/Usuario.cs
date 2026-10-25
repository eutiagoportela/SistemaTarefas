using SistemaTarefas.Domain.Enums;
using SistemaTarefas.Domain.Excecoes;

namespace SistemaTarefas.Domain.Entidades;

public sealed class Usuario
{
    private readonly List<RefreshToken> _refreshTokens = [];

    public Guid Id { get; private set; }
    public string NomeUsuario { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public PapelUsuario Papel { get; private set; }
    public DateTime DataCriacao { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private Usuario()
    {
    }

    public static Usuario Criar(string nomeUsuario, string senhaHash, PapelUsuario papel, TimeProvider relogio)
    {
        if (string.IsNullOrWhiteSpace(nomeUsuario))
        {
            throw new DomainException("O nome de usuário é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(senhaHash))
        {
            throw new DomainException("O hash da senha é obrigatório.");
        }

        return new Usuario
        {
            Id = Guid.NewGuid(),
            NomeUsuario = nomeUsuario.Trim(),
            SenhaHash = senhaHash,
            Papel = papel,
            DataCriacao = relogio.GetUtcNow().UtcDateTime
        };
    }

    public void AdicionarRefreshToken(RefreshToken token) => _refreshTokens.Add(token);
}
