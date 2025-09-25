using System.Threading;
using System.Threading.Tasks;

namespace ScopedHostedService.ScopedBackgroundService;

public interface IScopedBackgroundRunner
{
    Task ExecuteInScopeAsync(CancellationToken cancellationToken);
}