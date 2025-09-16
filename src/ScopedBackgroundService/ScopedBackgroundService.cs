using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ScopedHostedService.ScopedBackgroundService;

public abstract class ScopedBackgroundService<TRunner>
    where TRunner : class, IScopedBackgroundRunner
{
    public abstract Task ExecuteInScopeAsync(TRunner runner, CancellationToken stoppingToken);

    internal sealed class Wrapper : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ScopedBackgroundService<TRunner> _service;

        public Wrapper(IServiceScopeFactory scopeFactory, ScopedBackgroundService<TRunner> service)
        {
            _scopeFactory = scopeFactory;
            _service = service;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var runner = scope.ServiceProvider.GetRequiredService<TRunner>();
            await _service.ExecuteInScopeAsync(runner, stoppingToken);
        }
    }
}