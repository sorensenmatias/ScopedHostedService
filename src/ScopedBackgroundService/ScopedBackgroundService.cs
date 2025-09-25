using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ScopedHostedService.ScopedBackgroundService;

public class ScopedBackgroundService<TRunner>
    where TRunner : class, IScopedBackgroundRunner
{
    public class InternalBackgroundService(IServiceProvider serviceProvider) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            await using var scope = scopeFactory.CreateAsyncScope();
            var runner = scope.ServiceProvider.GetRequiredService<TRunner>();
            await runner.ExecuteInScopeAsync(cancellationToken);
        }
    }
}