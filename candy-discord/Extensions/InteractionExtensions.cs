using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Candy.Discord;

public static class InteractionExtensions
{
    public static async Task<bool> TryRespondAsync(
        this SocketInteraction interaction,
        string message,
        bool ephemeral = false,
        ILogger? logger = null)
    {
        try
        {
            await interaction.RespondAsync(message, ephemeral: ephemeral);
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to respond to interaction");
            return false;
        }
    }

    public static async Task<bool> TryFollowupAsync(
        this SocketInteraction interaction,
        string message,
        bool ephemeral = false,
        ILogger? logger = null)
    {
        try
        {
            await interaction.FollowupAsync(message, ephemeral: ephemeral);
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to send followup message");
            return false;
        }
    }

    public static async Task<bool> TryDeferAsync(
        this SocketInteraction interaction,
        bool ephemeral = false,
        ILogger? logger = null)
    {
        try
        {
            await interaction.DeferAsync(ephemeral: ephemeral);
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to defer interaction");
            return false;
        }
    }
}

