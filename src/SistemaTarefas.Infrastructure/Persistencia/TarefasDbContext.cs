using Microsoft.EntityFrameworkCore;
using SistemaTarefas.Domain.Entidades;
using SistemaTarefas.Domain.ValueObjects;

namespace SistemaTarefas.Infrastructure.Persistencia;

public sealed class TarefasDbContext : DbContext
{
    public TarefasDbContext(DbContextOptions<TarefasDbContext> options) : base(options)
    {
    }

    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditoriaRegistro> Auditorias => Set<AuditoriaRegistro>();
    public DbSet<ChaveIdempotencia> ChavesIdempotencia => Set<ChaveIdempotencia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(x => x.Id);
            e.Property(x => x.NomeUsuario).HasMaxLength(80).IsRequired();
            e.HasIndex(x => x.NomeUsuario).IsUnique();
            e.Property(x => x.SenhaHash).IsRequired();
            e.Property(x => x.Papel).HasConversion<string>().HasMaxLength(20);
            e.HasMany(x => x.RefreshTokens).WithOne().HasForeignKey(x => x.UsuarioId);
            e.Navigation(x => x.RefreshTokens).HasField("_refreshTokens").UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.ToTable("RefreshTokens");
            e.HasKey(x => x.Id);
            e.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            e.HasIndex(x => x.TokenHash);
        });

        modelBuilder.Entity<Tarefa>(e =>
        {
            e.ToTable("Tarefas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo)
                .HasConversion(v => v.Valor, v => new Titulo(v))
                .HasMaxLength(120)
                .IsRequired();
            e.Property(x => x.Descricao)
                .HasConversion(v => v.Valor, v => new Descricao(v))
                .HasMaxLength(1000);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Versao).IsConcurrencyToken();
            e.HasIndex(x => new { x.UsuarioId, x.Status, x.DataVencimento });
            e.HasQueryFilter(x => !x.IsDeleted);
            e.Ignore(x => x.Eventos);
        });

        modelBuilder.Entity<AuditoriaRegistro>(e =>
        {
            e.ToTable("AuditoriaRegistros");
            e.HasKey(x => x.Id);
            e.Property(x => x.Acao).HasMaxLength(80).IsRequired();
            e.Property(x => x.UsuarioNome).HasMaxLength(80);
            e.HasIndex(x => x.TarefaId);
        });

        modelBuilder.Entity<ChaveIdempotencia>(e =>
        {
            e.ToTable("ChavesIdempotencia");
            e.HasKey(x => x.Id);
            e.Property(x => x.Chave).HasMaxLength(80).IsRequired();
            e.HasIndex(x => new { x.UsuarioId, x.Chave }).IsUnique();
        });
    }
}
