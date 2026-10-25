using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Domain.Entidades;
using SistemaTarefas.Domain.Enums;

namespace SistemaTarefas.Infrastructure.Persistencia;

public sealed class InicializadorBanco
{
    private readonly TarefasDbContext _contexto;
    private readonly IPasswordHasher _hasher;
    private readonly IConfiguration _configuracao;
    private readonly ILogger<InicializadorBanco> _logger;
    private readonly TimeProvider _relogio;

    public InicializadorBanco(
        TarefasDbContext contexto,
        IPasswordHasher hasher,
        IConfiguration configuracao,
        ILogger<InicializadorBanco> logger,
        TimeProvider relogio)
    {
        _contexto = contexto;
        _hasher = hasher;
        _configuracao = configuracao;
        _logger = logger;
        _relogio = relogio;
    }

    public async Task ExecutarAsync(CancellationToken cancellationToken = default)
    {
        var aplicar = _configuracao["Banco:AplicarMigrationsNoStartup"];
        if (!string.Equals(aplicar, "false", StringComparison.OrdinalIgnoreCase))
        {
            for (var tentativa = 1; tentativa <= 10; tentativa++)
            {
                try
                {
                    await _contexto.Database.MigrateAsync(cancellationToken);
                    break;
                }
                catch (Exception ex) when (tentativa < 10)
                {
                    _logger.LogWarning(ex, "Aguardando MySQL (tentativa {Tentativa})", tentativa);
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
                }
            }
        }

        var nome = _configuracao["Seed:Admin:Usuario"] ?? "admin";
        if (await _contexto.Usuarios.AnyAsync(u => u.NomeUsuario == nome, cancellationToken))
        {
            return;
        }

        var senha = _configuracao["Seed:Admin:Senha"] ?? "Admin123";
        var admin = Usuario.Criar(nome, _hasher.Hash(senha), PapelUsuario.Admin, _relogio);
        await _contexto.Usuarios.AddAsync(admin, cancellationToken);
        await _contexto.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Usuário admin inicial criado");
    }
}
