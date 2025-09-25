using Microsoft.Extensions.DependencyInjection;
using ScopedHostedService.ScopedBackgroundService;

namespace ScopedHostedService;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundServiceScoped<TRunner>(
        this IServiceCollection services)
        where TRunner : class, IScopedBackgroundRunner
    {
        services.AddScoped<TRunner>();

        services.AddSingleton<ScopedBackgroundServiceOrchestrator<TRunner>>();
        
        services.AddHostedService<InternalScopedBackgroundService<ScopedBackgroundServiceOrchestrator<TRunner>, TRunner>>();

        return services;
    }

    public static IServiceCollection AddBackgroundServiceScoped<TScopedBackgroundServiceOrchestrator, TRunner>(
        this IServiceCollection services)
        where TScopedBackgroundServiceOrchestrator : ScopedBackgroundServiceOrchestrator<TRunner>
        where TRunner : class, IScopedBackgroundRunner
    {
        services.AddScoped<TRunner>();

        services.AddSingleton<TScopedBackgroundServiceOrchestrator>();

        services.AddHostedService<InternalScopedBackgroundService<TScopedBackgroundServiceOrchestrator, TRunner>>();

        return services;
    }
}