using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SistemaTarefas.Application.UseCases.Auth.Login;
using SistemaTarefas.Application.UseCases.Auth.Logout;
using SistemaTarefas.Application.UseCases.Auth.RefreshToken;
using SistemaTarefas.Application.UseCases.Auth.RegistrarUsuario;
using SistemaTarefas.Application.UseCases.Tarefas.AlterarStatus;
using SistemaTarefas.Application.UseCases.Tarefas.Atualizar;
using SistemaTarefas.Application.UseCases.Tarefas.Consultar;
using SistemaTarefas.Application.UseCases.Tarefas.Criar;
using SistemaTarefas.Application.UseCases.Tarefas.Remover;
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

        services.AddScoped<IAtualizarTarefaUseCase, AtualizarTarefaUseCase>();
        services.AddScoped<IAlterarStatusTarefaUseCase, AlterarStatusTarefaUseCase>();
        services.AddScoped<IRemoverTarefaUseCase, RemoverTarefaUseCase>();
        services.AddScoped<IObterHistoricoTarefaUseCase, ObterHistoricoTarefaUseCase>();
        services.AddScoped<ILogoutUseCase, LogoutUseCase>();

        return services;
    }
}
