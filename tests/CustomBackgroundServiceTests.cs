using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ScopedHostedService.ScopedBackgroundService;
using Xunit;

namespace ScopedHostedService.Tests;

public class CustomBackgroundServiceTests
{
    public CustomBackgroundServiceTests()
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
            .ConfigureServices(sc =>
            {
                sc.AddBackgroundServiceScoped<MyScopedBackgroundServiceOrchestrator, TestRunner>();
            })
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

    private class TestRunner : IScopedBackgroundRunner
    {
        public Task ExecuteInScopeAsync(CancellationToken cancellationToken)
        {
            TestRunnerExecuted = true;
            return Task.CompletedTask;
        }
    }

    private class MyScopedBackgroundServiceOrchestrator(IServiceProvider serviceProvider) : 
        ScopedBackgroundServiceOrchestrator<TestRunner>(serviceProvider)
    {
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var scope = CreateScope();
            await ExecuteScopedRunner(scope, cancellationToken);
        }
    }
}