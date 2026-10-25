using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SistemaTarefas.Application.UseCases.Auth;
using SistemaTarefas.Application.UseCases.Tarefas;
using SistemaTarefas.Application.Validadores;

namespace SistemaTarefas.Application.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddValidatorsFromAssemblyContaining<RequestLoginValidator>();

        services.AddScoped<IRegistrarUsuarioUseCase, RegistrarUsuarioUseCase>();
        services.AddScoped<ILoginUseCase, LoginUseCase>();
        services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();

        services.AddScoped<ICriarTarefaUseCase, CriarTarefaUseCase>();
        services.AddScoped<IObterTarefaUseCase, ObterTarefaUseCase>();
        services.AddScoped<IListarTarefasUseCase, ListarTarefasUseCase>();
        services.AddScoped<ComandosTarefaUseCase>();
        services.AddScoped<IAtualizarTarefaUseCase>(sp => sp.GetRequiredService<ComandosTarefaUseCase>());
        services.AddScoped<IAlterarStatusTarefaUseCase>(sp => sp.GetRequiredService<ComandosTarefaUseCase>());
        services.AddScoped<IRemoverTarefaUseCase>(sp => sp.GetRequiredService<ComandosTarefaUseCase>());
        services.AddScoped<IObterHistoricoTarefaUseCase>(sp => sp.GetRequiredService<ComandosTarefaUseCase>());

        return services;
    }
}
