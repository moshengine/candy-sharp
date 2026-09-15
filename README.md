# candy-sharp

Personal C# libraries and Unity packages. Licensed under MIT.

There is no backwards-compatibility guarantee. Pin a git tag if you need a stable snapshot.

## Libraries

| Name | Target | Description |
|------|--------|-------------|
| candy-common | netstandard2.1 + UPM | Shared utilities |
| candy-aspnet | net9.0 | ASP.NET Core utilities (auth, MongoDB, Notion, GitHub) |
| candy-discord | net9.0 | Discord.Net services and extensions |
| candy-logging | net9.0 | Spring Boot-inspired Serilog console logging |
| candy-unity | UPM | Editor tools and runtime utilities |

## Unity

Add a git URL to `Packages/manifest.json`. Append `#v0.1.0` (or any later tag) to pin:

```json
"com.danielkreitsch.candy-unity": "https://github.com/glowdragon/candy-sharp.git?path=/candy-unity#v0.1.0",
"com.danielkreitsch.candy-common": "https://github.com/glowdragon/candy-sharp.git?path=/candy-common/unity#v0.1.0"
```

## .NET

```xml
<ItemGroup>
    <ProjectReference Include="../candy-sharp/candy-aspnet/Candy.AspNet.csproj" />
</ItemGroup>
```

## Versioning

Packages share a lockstep version. The first public baseline is `0.1.0` (`v0.1.0`).
