using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScopedHostedService;
using ScopedHostedService.ScopedBackgroundService;
using Xunit;

namespace ScopedHostedService.Tests
{
    public class OrderProcessorTests
    {
        public class TestRunner : IScopedBackgroundRunner
        {
            public bool Executed { get; private set; } = false;

            public Task DoWorkAsync(CancellationToken stoppingToken)
            {
                Executed = true;
                return Task.CompletedTask;
            }

            // Nested BackgroundService-style class
            public class Service : ScopedBackgroundService<TestRunner>
            {
                public override Task ExecuteInScopeAsync(TestRunner runner, CancellationToken stoppingToken)
                    => runner.DoWorkAsync(stoppingToken);
            }
        }

        [Fact]
        public async Task Runner_Is_Resolved_And_Executed()
        {
            // Arrange DI container
            var services = new ServiceCollection();
            services.AddBackgroundServiceScoped<TestRunner>();
            var provider = services.BuildAndValidateServiceProvider();

            // Act
            var hostedService = provider.GetRequiredService<IHostedService>();
            await hostedService.StartAsync(CancellationToken.None);

            // Assert
            var runner = provider.GetRequiredService<TestRunner>();
            Assert.True(runner.Executed);
        }

        [Fact]
        public async Task Runner_Stops_When_CancellationToken_Cancelled()
        {
            var services = new ServiceCollection();
            services.AddBackgroundServiceScoped<TestRunner>();
            var provider = services.BuildAndValidateServiceProvider();

            var hostedService = provider.GetRequiredService<IHostedService>();

            using var cts = new CancellationTokenSource();
            cts.Cancel(); // cancel immediately

            await hostedService.StartAsync(cts.Token);

            // Should complete gracefully
            await hostedService.StopAsync(CancellationToken.None);
        }

        // Test missing nested Service class
        public class MissingServiceRunner : IScopedBackgroundRunner;

        [Fact]
        public void Registration_Throws_If_No_Nested_Service()
        {
            var services = new ServiceCollection();
            var ex = Assert.Throws<InvalidOperationException>(() =>
                services.AddBackgroundServiceScoped<MissingServiceRunner>());

            Assert.Contains("must contain a nested class", ex.Message);
        }
    }
}
