# Candy.Logging

Spring Boot-inspired Serilog console logging for .NET.

```csharp
using Candy.Logging;

await Host.CreateDefaultBuilder(args)
    .UseCandyLogging("MyProject")
    .Build()
    .RunAsync();
```

```
12:34:56 INF MyApp.Services.UserServ     User authentication successful
12:34:57 ERR MyApp.Payment.Processor      Payment processing failed
```

Pass a `loggerConfig` callback to `UseCandyLogging` to add sinks or level overrides. `Debug.Log` / `Debug.LogWarning` / `Debug.LogError` are available as a static wrapper.
