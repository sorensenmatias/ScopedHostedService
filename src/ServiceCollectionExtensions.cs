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
        // Register the runner itself
        services.AddScoped<TRunner>();

        // Look for nested class named "Service"
        var nestedServiceType = typeof(TRunner).GetNestedType("Service");

        if (nestedServiceType is null)
        {
            throw new InvalidOperationException(
                $"The type {typeof(TRunner).FullName} must contain a nested class named 'Service'.");
        }

        // Validate inheritance
        if (!typeof(ScopedBackgroundService<TRunner>).IsAssignableFrom(nestedServiceType))
        {
            throw new InvalidOperationException(
                $"The nested 'Service' type in {typeof(TRunner).Name} must inherit from ScopedBackgroundService<{typeof(TRunner).Name}>.");
        }

        // Register user service and wrapper
        services.AddSingleton(nestedServiceType);
        services.AddHostedService(sp =>
        {
            var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            var service = (ScopedBackgroundService<TRunner>)sp.GetRequiredService(nestedServiceType);
            return new ScopedBackgroundService<TRunner>.Wrapper(scopeFactory, service);
        });

        return services;
    }
}