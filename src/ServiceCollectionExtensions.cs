using System;
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

        services.AddSingleton<ScopedBackgroundService<TRunner>>();
        
        services.AddHostedService(sp => new ScopedBackgroundService<TRunner>.InternalBackgroundService(sp));

        return services;
    }
}