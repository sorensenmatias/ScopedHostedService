using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ScopedHostedService.ScopedBackgroundService;

public class ScopedBackgroundServiceOrchestrator<TScopedBackgroundRunner>(IServiceProvider serviceProvider)
    where TScopedBackgroundRunner : class, IScopedBackgroundRunner
{
    protected virtual IServiceScope CreateScope()
    {
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        return scopeFactory.CreateAsyncScope();
    }

    protected virtual async Task ExecuteScopedRunner(IServiceScope scope, CancellationToken cancellationToken)
    {
        var runner = scope.ServiceProvider.GetRequiredService<TScopedBackgroundRunner>();
        await runner.ExecuteInScopeAsync(cancellationToken);
    }

    public virtual async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var scope = CreateScope();
        try
        {
            await ExecuteScopedRunner(scope, cancellationToken);
        }
        finally
        {
            if (scope is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else
            {
                scope.Dispose();
            }
        }
    }
}
public class InternalScopedBackgroundService<TScopedBackgroundServiceOrchestrator, TScopedBackgroundRunner>(
    TScopedBackgroundServiceOrchestrator scopedBackgroundServiceOrchestrator) : BackgroundService 
    where TScopedBackgroundServiceOrchestrator : ScopedBackgroundServiceOrchestrator<TScopedBackgroundRunner>
    where TScopedBackgroundRunner : class, IScopedBackgroundRunner
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return scopedBackgroundServiceOrchestrator.ExecuteAsync(stoppingToken);
    }
}