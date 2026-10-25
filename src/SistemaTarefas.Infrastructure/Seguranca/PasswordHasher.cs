using SistemaTarefas.Application.Abstracoes;

namespace SistemaTarefas.Infrastructure.Seguranca;

public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);

    public bool Verificar(string senha, string hash) => BCrypt.Net.BCrypt.Verify(senha, hash);
}
