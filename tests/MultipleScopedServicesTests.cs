using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScopedHostedService.ScopedBackgroundService;
using Xunit;

namespace ScopedHostedService.Tests;

public class MultipleScopedServicesTests
{
    public MultipleScopedServicesTests()
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
                sc.AddScoped<ScopedService1>();
                sc.AddScoped<ScopedService2>();
                sc.AddSingleton<SingletonService>();
                sc.AddBackgroundServiceScoped<TestRunner>();
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

    private class TestRunner(
        ScopedService1 scopedService1,
        ScopedService2 scopedService2,
        SingletonService singletonService)
        : IScopedBackgroundRunner
    {
#pragma warning disable IDE0051
        // ReSharper disable once NotAccessedField.Local
        private readonly ScopedService1 _scopedService1 = scopedService1;

        // ReSharper disable once NotAccessedField.Local
        private readonly ScopedService2 _scopedService2 = scopedService2;
        // ReSharper disable once NotAccessedField.Local
        private readonly SingletonService _singletonService = singletonService;
#pragma warning restore IDE0051

        public Task ExecuteInScopeAsync(CancellationToken cancellationToken)
        {
            TestRunnerExecuted = true;
            return Task.CompletedTask;
        }
    }

    private class ScopedService1;

    private class ScopedService2;

    private class SingletonService;
}