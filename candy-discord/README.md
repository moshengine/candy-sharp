# Candy.Discord

Shared Discord.Net utilities, services, and extensions for Discord bot projects.

## Installation

Add project reference to your bot's `.csproj`:

```xml
<ItemGroup>
    <ProjectReference Include="..\candy-discord\Candy.Discord.csproj" />
</ItemGroup>
```

## Service Registration

Register services in your `Program.cs`:

```csharp
builder.Services.AddSingleton<MoshEngine.Candy.Discord.ModerationService>();
```

## Usage

### ModerationService

```csharp
public class ModerationModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ModerationService _moderationService;

    public ModerationModule(ModerationService moderationService)
    {
        _moderationService = moderationService;
    }

    public async Task ClearAsync(ITextChannel channel, int count)
    {
        var deletedCount = await _moderationService.ClearMessagesAsync(channel, count);
        await ReplyAsync($"Deleted {deletedCount} messages");
    }
}
```

### EmbedBuilder Extensions

```csharp
var embed = new EmbedBuilder()
    .WithTitle("Success")
    .WithSuccessColor()
    .Build();
```

### Message Formatter

```csharp
var message = $"{MessageFormatter.Emoji.Success} {MessageFormatter.Bold("Task completed")}";
var timestamp = MessageFormatter.Timestamp(DateTimeOffset.Now, TimestampStyle.Relative);
```

## Extension Points

- `Services/` - Reusable Discord services
- `Extensions/` - Extension methods for Discord.Net types
- `Utilities/` - Helper classes and constants

