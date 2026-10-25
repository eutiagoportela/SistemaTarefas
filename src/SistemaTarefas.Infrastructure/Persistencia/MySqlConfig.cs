using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaTarefas.Application.Abstracoes;
using SistemaTarefas.Domain.Repositorios;
using SistemaTarefas.Infrastructure.Repositorios;

namespace SistemaTarefas.Infrastructure.Persistencia;

public static class MySqlConfig
{
    public static IServiceCollection AddMySqlConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' não configurada.");

        services.AddDbContext<TarefasDbContext>(options =>
        {
            options.UseMySql(connectionString, ServerVersion.Parse("8.0.36-mysql"), mySql =>
            {
                mySql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                mySql.CommandTimeout(30);
                mySql.MigrationsAssembly(typeof(TarefasDbContext).Assembly.FullName);
            });
        });

        services.AddScoped<ITarefaRepository, TarefaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuditoriaConsulta, AuditoriaConsulta>();
        services.AddScoped<IIdempotenciaStore, IdempotenciaStore>();
        services.AddScoped<InicializadorBanco>();

        return services;
    }
}
