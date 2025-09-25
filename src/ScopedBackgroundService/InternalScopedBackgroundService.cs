using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace ScopedHostedService.ScopedBackgroundService;

/// <summary>
///     Actual BackgroundService used for execution. Customization is not currently supported.
/// </summary>
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