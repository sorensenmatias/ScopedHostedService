using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ScopedHostedService.ScopedBackgroundService;

public class ScopedBackgroundService<TRunner>
    where TRunner : class, IScopedBackgroundRunner
{
    protected virtual Task ExecuteInScopeAsync(TRunner runner, CancellationToken stoppingToken)
    {
        return runner.DoWorkAsync(stoppingToken);
    }

    internal sealed class InternalBackgroundService(IServiceProvider serviceProvider) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            await using var scope = scopeFactory.CreateAsyncScope();
            var scopedBackgroundService = scope.ServiceProvider.GetRequiredService<ScopedBackgroundService<TRunner>>();
            var runner = scope.ServiceProvider.GetRequiredService<TRunner>();
            await scopedBackgroundService.ExecuteInScopeAsync(runner, stoppingToken);
        }
    }
}