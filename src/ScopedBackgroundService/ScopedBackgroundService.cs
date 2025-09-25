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
            var scope = CreateScope();
            try
            {
                await ExecuteRunner(scope, cancellationToken);
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

        protected virtual IServiceScope CreateScope()
        {
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            return scopeFactory.CreateAsyncScope();
        }

        protected virtual async Task ExecuteRunner(IServiceScope scope, CancellationToken cancellationToken)
        {
            var runner = scope.ServiceProvider.GetRequiredService<TRunner>();
            await runner.ExecuteInScopeAsync(cancellationToken);
        }
    }
}