# candy-sharp

Personal C# libraries and Unity packages.

## Libraries

| Name | Target | Description |
|------|--------|-------------|
| candy-common | netstandard2.1 | Shared utilities for all projects |
| candy-aspnet | net9.0 | ASP.NET Core utilities (MongoDB, Notion, Security) |
| candy-blazor | net9.0 | Blazor WebAssembly utilities |
| candy-discord | net9.0 | Discord.Net services and extensions |
| candy-logging | net9.0 | Spring Boot-inspired console logging (Serilog) |
| candy-unity | Unity package | Editor tools and runtime utilities |

## Installation

Add project references in your `.csproj`:

```xml
<ItemGroup>
    <ProjectReference Include="../candy-sharp/candy-aspnet/Candy.AspNet.csproj" />
</ItemGroup>
```

For Unity packages, see individual package.json files.

