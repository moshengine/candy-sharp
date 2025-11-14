using Discord;
using Microsoft.Extensions.Logging;

namespace Candy.Discord;

public class ModerationService
{
    private readonly ILogger<ModerationService> _logger;

    public ModerationService(ILogger<ModerationService> logger)
    {
        _logger = logger;
    }

    public async Task<int> ClearMessagesAsync(ITextChannel channel, int count)
    {
        var messages = await channel.GetMessagesAsync(count).FlattenAsync();
        var messageList = messages.ToList();

        if (!messageList.Any())
        {
            return 0;
        }

        var twoWeeksAgo = DateTimeOffset.UtcNow - TimeSpan.FromDays(14);
        var recentMessages = messageList.Where(m => m.Timestamp > twoWeeksAgo).ToList();
        var oldMessages = messageList.Where(m => m.Timestamp <= twoWeeksAgo).ToList();

        var deletedCount = 0;

        if (recentMessages.Count > 0)
        {
            await channel.DeleteMessagesAsync(recentMessages);
            deletedCount += recentMessages.Count;
            _logger.LogDebug("Bulk deleted {Count} recent messages from channel {ChannelId}", recentMessages.Count, channel.Id);
        }

        if (oldMessages.Count > 0)
        {
            foreach (var message in oldMessages)
            {
                await message.DeleteAsync();
                deletedCount++;
            }
            _logger.LogDebug("Individually deleted {Count} old messages from channel {ChannelId}", oldMessages.Count, channel.Id);
        }

        _logger.LogInformation("Deleted {Count} total messages from channel {ChannelId}", deletedCount, channel.Id);
        return deletedCount;
    }
}

