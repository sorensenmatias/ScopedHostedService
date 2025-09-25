using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ScopedHostedService.ScopedBackgroundService;

/// <summary>
///     Orchestrates how the scoped background runner is executed.
/// </summary>
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