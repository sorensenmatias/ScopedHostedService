using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ScopedHostedService.ScopedBackgroundService;
using Xunit;

namespace ScopedHostedService.Tests;

public class OrderProcessorTests
{
    public OrderProcessorTests()
    {
        TestRunnerExecuted = false;
    }

    private static bool TestRunnerExecuted { get; set; }

    private static IHost BuildHost()
    {
        var host = new HostBuilder()
            .UseDefaultServiceProvider(o =>
            {
                o.ValidateOnBuild = true;
                o.ValidateScopes = true;
            })
            .ConfigureServices(sc => { sc.AddBackgroundServiceScoped<TestRunner>(); })
            .Build();

        return host;
    }

    [Fact]
    public async Task Runner_Is_Resolved_And_Executed()
    {
        // Arrange
        using var host = BuildHost();

        // Act
        await host.StartAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(TestRunnerExecuted);

        await host.StartAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Runner_Stops_When_CancellationToken_Cancelled()
    {
        // Arrange
        using var host = BuildHost();

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // cancel immediately

        try
        {
            await host.StartAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
        }

        // Assert
        Assert.False(TestRunnerExecuted);
    }

    private class TestRunner : IScopedBackgroundRunner
    {
        public Task DoWorkAsync(CancellationToken cancellationToken)
        {
            TestRunnerExecuted = true;
            return Task.CompletedTask;
        }
    }
}