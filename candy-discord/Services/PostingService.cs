using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace MoshEngine.Candy.Discord;

public class PostingService
{
    private readonly ILogger<PostingService> _logger;

    public PostingService(ILogger<PostingService> logger)
    {
        _logger = logger;
    }

    public async Task<IUserMessage> PostContentAsync(
        ITextChannel channel,
        string content,
        bool replace,
        ulong botUserId
    )
    {
        if (replace)
        {
            var existingMessage = await FindBotLastMessageAsync(channel, botUserId);
            if (existingMessage != null)
            {
                await existingMessage.ModifyAsync(props => props.Content = content);
                _logger.LogInformation(
                    "Replaced message {MessageId} in channel {ChannelId}",
                    existingMessage.Id,
                    channel.Id
                );
                return existingMessage;
            }
        }

        var message = await channel.SendMessageAsync(content);
        _logger.LogInformation(
            "Posted content to channel {ChannelId} (replace: {Replace})",
            channel.Id,
            replace
        );

        return message;
    }

    public async Task<IUserMessage> PostContentAsync(
        ITextChannel channel,
        Embed embed,
        bool replace,
        ulong botUserId
    )
    {
        if (replace)
        {
            var existingMessage = await FindBotLastMessageAsync(channel, botUserId);
            if (existingMessage != null)
            {
                await existingMessage.ModifyAsync(props => props.Embed = embed);
                _logger.LogInformation(
                    "Replaced message {MessageId} in channel {ChannelId}",
                    existingMessage.Id,
                    channel.Id
                );
                return existingMessage;
            }
        }

        var message = await channel.SendMessageAsync(embed: embed);
        _logger.LogInformation(
            "Posted embed to channel {ChannelId} (replace: {Replace})",
            channel.Id,
            replace
        );

        return message;
    }

    public async Task AddReactionsAsync(IUserMessage message, IEnumerable<string> emojis)
    {
        foreach (var emoji in emojis)
        {
            try
            {
                IEmote emote = ParseEmote(emoji);
                await message.AddReactionAsync(emote);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to add reaction {Emoji} to message {MessageId}",
                    emoji,
                    message.Id
                );
            }
        }

        _logger.LogDebug(
            "Added {Count} reactions to message {MessageId}",
            emojis.Count(),
            message.Id
        );
    }

    private static IEmote ParseEmote(string emoji)
    {
        // Custom emote format: <:name:id> or <a:name:id> (animated)
        if (Emote.TryParse(emoji, out var emote))
            return emote;
        return new Emoji(emoji);
    }

    private async Task<IUserMessage?> FindBotLastMessageAsync(ITextChannel channel, ulong botUserId)
    {
        try
        {
            var messages = await channel.GetMessagesAsync(100).FlattenAsync();
            var botMessage = messages.FirstOrDefault(m => m.Author.Id == botUserId) as IUserMessage;
            
            if (botMessage != null)
            {
                _logger.LogDebug(
                    "Found bot's last message {MessageId} in channel {ChannelId}",
                    botMessage.Id,
                    channel.Id
                );
            }
            
            return botMessage;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to find bot's last message in channel {ChannelId}",
                channel.Id
            );
            return null;
        }
    }
}

