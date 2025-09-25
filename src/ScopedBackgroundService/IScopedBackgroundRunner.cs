using System.Threading;
using System.Threading.Tasks;

namespace ScopedHostedService.ScopedBackgroundService;

/// <summary>
///     Contains the code that should be executed in a scope.
/// </summary>
public interface IScopedBackgroundRunner
{
    Task ExecuteInScopeAsync(CancellationToken cancellationToken = default);
}