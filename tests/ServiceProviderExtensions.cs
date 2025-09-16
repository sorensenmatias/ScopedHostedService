using Microsoft.Extensions.DependencyInjection;

namespace ScopedHostedService.Tests;

public static class ServiceProviderExtensions
{
    public static ServiceProvider BuildAndValidateServiceProvider(this ServiceCollection services)
    {
        return services.BuildServiceProvider(new ServiceProviderOptions()
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });
    }
}