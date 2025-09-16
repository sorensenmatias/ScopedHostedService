# ScopedHostedService

A lightweight .NET library for creating **hosted services with scoped dependencies**.  
ScopedHostedService simplifies building `IHostedService` implementations that need scoped services (like `DbContext`) without boilerplate or manual scope management.

---

## ✅ Why Use ScopedHostedService?

- **Scoped Dependency Support** – Automatically creates an `IServiceScope` for each execution, so you can safely use `DbContext` or other scoped services.  
- **ServiceProvider Validation** – Ensures your services are properly registered in DI at startup, catching common misconfigurations early.  
- **Cancellation Token Support** – Integrates cleanly with `IHostedService` patterns, passing `CancellationToken` to your work method.  
- **Minimal Boilerplate** – Focus on your business logic; no need to manually create scopes or manage lifetimes.  
- **Safe Registration** – Outer runner classes cannot be accidentally registered as `IHostedService`.  
- **Flexible Execution** – Works for loops, one-shot tasks, or any scoped execution logic.

---

## 📦 Installation

```bash
dotnet add package ScopedHostedService
```

Example usage
```c#
// 1️⃣ Create a runner class
public class HelloRunner : IScopedHostedRunner
{
    private readonly ILogger<HelloRunner> _logger;

    public HelloRunner(ILogger<HelloRunner> logger)
    {
        _logger = logger;
    }

    public Task SayHelloAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Hello from ScopedHostedService!");
        return Task.CompletedTask;
    }

    // 2️⃣ Nested hosted service
    public class Service : ScopedHostedService<HelloRunner>
    {
        public override Task ExecuteInScopeAsync(HelloRunner runner, CancellationToken stoppingToken)
            => runner.SayHelloAsync(stoppingToken);
    }
}

// 3️⃣ Register and run
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddLogging(config => config.AddConsole());
        services.AddScopedHostedService<HelloRunner>();
    })
    .Build();

await host.RunAsync();
```
