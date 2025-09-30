# Candy.Logging

Beautiful, Spring Boot-inspired console logging for .NET using Serilog.

## Features

- 🎨 **Colored & Aligned Output** - Fixed-width categories, all messages start at the same position
- 📏 **Smart Category Shortening** - Long namespaces automatically truncated
- 🔍 **Enhanced Exceptions** - Highlighted stack traces with syntax coloring
- ⚡ **Serilog Powered** - Full access to Serilog's ecosystem

## Quick Start

```csharp
using Candy.Logging;

await Host.CreateDefaultBuilder(args)
    .UseCandyLogging("MyProject")
    .Build()
    .RunAsync();
```

## Output Example

```
12:34:56 INF MyApp.Services.UserServ     User authentication successful
12:34:56 WRN MyApp.Data.Repository        Database query took 1500ms
12:34:57 ERR MyApp.Payment.Processor      Payment processing failed
 
  Exception: System.InvalidOperationException
  Message: Payment gateway timeout
 
  at MyApp.Payment.Processor.ProcessPayment() in /Payment/Processor.cs:line 42
```

## Usage

### Basic Setup

```csharp
await Host.CreateDefaultBuilder(args)
    .UseCandyLogging("MyProject", "MyNamespace")
    .Build()
    .RunAsync();
```

### With Additional Configuration

```csharp
await Host.CreateDefaultBuilder(args)
    .UseCandyLogging(
        loggerConfig => 
        {
            // Add file sink with same formatting
            loggerConfig.WriteTo.File(
                new CandySerilogFormatter(),
                "logs/app.log",
                rollingInterval: RollingInterval.Day
            );
            
            // Filter noisy logs
            loggerConfig.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
        },
        "MyProject"
    )
    .Build()
    .RunAsync();
```

### Logging

**Using Dependency Injection (Recommended)**

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;
    
    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }
    
    public void DoWork()
    {
        _logger.LogInformation("Processing {ItemCount} items", count);
        _logger.LogError(exception, "Failed to process item");
    }
}
```

**Using Static Debug Class**

```csharp
using Candy.Logging;

Debug.Log("Info message");
Debug.LogWarning("Warning message");
Debug.LogError("Error message");
```

## License

Personal library for my projects.