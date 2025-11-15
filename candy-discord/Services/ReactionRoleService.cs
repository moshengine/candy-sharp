using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Candy.Discord;

public class ReactionRoleService
{
    private readonly IMongoCollection<ReactionRoleMapping> _reactionRoles;
    private readonly ILogger<ReactionRoleService> _logger;

    public ReactionRoleService(IMongoDatabase database, ILogger<ReactionRoleService> logger)
    {
        _reactionRoles = database.GetCollection<ReactionRoleMapping>("reactionRoles");
        _logger = logger;
    }

    public async Task RegisterReactionRoleAsync(
        ulong messageId,
        string emoji,
        ulong roleId,
        ulong guildId
    )
    {
        var mapping = new ReactionRoleMapping
        {
            MessageId = messageId,
            Emoji = emoji,
            RoleId = roleId,
            GuildId = guildId,
        };

        await _reactionRoles.InsertOneAsync(mapping);
        _logger.LogInformation(
            "Registered reaction role: {Emoji} → Role {RoleId} for message {MessageId}",
            emoji,
            roleId,
            messageId
        );
    }

    public async Task SyncReactionRolesAsync(SocketGuild guild)
    {
        try
        {
            _logger.LogInformation(
                "Starting reaction role sync for guild {GuildId}",
                guild.Id
            );

            var mappings = await _reactionRoles
                .Find(m => m.GuildId == guild.Id)
                .ToListAsync();

            if (!mappings.Any())
            {
                _logger.LogDebug("No reaction role mappings found for guild {GuildId}", guild.Id);
                return;
            }

            var syncedCount = 0;
            var errorCount = 0;

            foreach (var mapping in mappings)
            {
                try
                {
                    // Find the channel and message
                    IUserMessage? message = null;
                    foreach (var channel in guild.TextChannels)
                    {
                        try
                        {
                            message = await channel.GetMessageAsync(mapping.MessageId) as IUserMessage;
                            if (message != null)
                                break;
                        }
                        catch
                        {
                            // Message not in this channel, continue
                        }
                    }

                    if (message == null)
                    {
                        _logger.LogWarning(
                            "Message {MessageId} not found in guild {GuildId}, removing reaction role mapping",
                            mapping.MessageId,
                            guild.Id
                        );
                        await _reactionRoles.DeleteOneAsync(m => m.Id == mapping.Id);
                        continue;
                    }

                    var role = guild.GetRole(mapping.RoleId);
                    if (role == null)
                    {
                        _logger.LogWarning(
                            "Role {RoleId} not found in guild {GuildId}",
                            mapping.RoleId,
                            guild.Id
                        );
                        continue;
                    }

                    // Get all users who reacted with this emoji
                    var reactions = message.Reactions.FirstOrDefault(r => r.Key.Name == mapping.Emoji);
                    var reactedUserIds = new HashSet<ulong>();
                    
                    if (reactions.Value.ReactionCount > 0)
                    {
                        var users = await message.GetReactionUsersAsync(new Emoji(mapping.Emoji), reactions.Value.ReactionCount).FlattenAsync();
                        
                        foreach (var user in users.Where(u => !u.IsBot))
                        {
                            reactedUserIds.Add(user.Id);
                            var guildUser = guild.GetUser(user.Id);
                            if (guildUser == null)
                                continue;

                            if (!guildUser.Roles.Contains(role))
                            {
                                await guildUser.AddRoleAsync(role);
                                _logger.LogInformation(
                                    "Synced role {RoleName} ({RoleId}) to user {UserName} ({UserId}) (missing role for reaction {Emoji})",
                                    role.Name,
                                    mapping.RoleId,
                                    guildUser.DisplayName,
                                    user.Id,
                                    mapping.Emoji
                                );
                                syncedCount++;
                            }
                        }
                    }

                    // Check for users with the role but no reaction (excluding whitelisted users)
                    var whitelistedUserIds = new HashSet<ulong>(mapping.WhitelistedUserIds ?? new List<ulong>());
                    var usersWithRoleNoReaction = guild.Users
                        .Where(u => !u.IsBot 
                            && u.Roles.Contains(role) 
                            && !reactedUserIds.Contains(u.Id)
                            && !whitelistedUserIds.Contains(u.Id))
                        .ToList();

                    if (usersWithRoleNoReaction.Any())
                    {
                        _logger.LogWarning(
                            "Role {RoleName} ({RoleId}): {Count} user(s) have the role but haven't reacted with {Emoji}",
                            role.Name,
                            mapping.RoleId,
                            usersWithRoleNoReaction.Count,
                            mapping.Emoji
                        );
                        
                        foreach (var user in usersWithRoleNoReaction)
                        {
                            _logger.LogWarning("  - {UserName} ({UserId})", user.DisplayName, user.Id);
                        }
                        
                        var userIdsForCommand = string.Join(", ", usersWithRoleNoReaction.Select(u => u.Id));
                        _logger.LogWarning(
                            "To whitelist, run:\n```\ndb.reactionRoles.updateOne({{_id: ObjectId(\"{MappingId}\")}}, {{$push: {{whitelistedUserIds: {{$each: [{UserIds}]}}}}}})\n```",
                            mapping.Id,
                            userIdsForCommand
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error syncing reaction role for message {MessageId}",
                        mapping.MessageId
                    );
                    errorCount++;
                }
            }

            _logger.LogInformation(
                "Reaction role sync complete for guild {GuildId}: {SyncedCount} roles granted, {ErrorCount} errors",
                guild.Id,
                syncedCount,
                errorCount
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during reaction role sync for guild {GuildId}", guild.Id);
        }
    }

    public async Task HandleReactionAddedAsync(
        Cacheable<IUserMessage, ulong> cachedMessage,
        Cacheable<IMessageChannel, ulong> cachedChannel,
        SocketReaction reaction
    )
    {
        try
        {
            // Ignore bot reactions
            if (reaction.User.IsSpecified && reaction.User.Value.IsBot)
                return;

            var channel = await cachedChannel.GetOrDownloadAsync();
            if (channel is not SocketGuildChannel guildChannel)
                return;

            // Find mapping in database
            var emoji = reaction.Emote.Name;
            var mapping = await _reactionRoles
                .Find(m => m.MessageId == reaction.MessageId && m.Emoji == emoji)
                .FirstOrDefaultAsync();

            if (mapping == null)
                return;

            // Grant role
            var guildUser = reaction.User.IsSpecified && reaction.User.Value is SocketGuildUser specifiedUser
                ? specifiedUser
                : guildChannel.Guild.GetUser(reaction.UserId);

            if (guildUser != null)
            {
                var role = guildChannel.Guild.GetRole(mapping.RoleId);
                if (role != null)
                {
                    await guildUser.AddRoleAsync(role);
                    _logger.LogInformation(
                        "Granted role {RoleName} ({RoleId}) to user {UserName} ({UserId}) via reaction {Emoji}",
                        role.Name,
                        mapping.RoleId,
                        guildUser.DisplayName,
                        reaction.UserId,
                        emoji
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "Role {RoleId} not found in guild {GuildId}",
                        mapping.RoleId,
                        guildChannel.Guild.Id
                    );
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling reaction added");
        }
    }

    public async Task HandleReactionRemovedAsync(
        Cacheable<IUserMessage, ulong> cachedMessage,
        Cacheable<IMessageChannel, ulong> cachedChannel,
        SocketReaction reaction
    )
    {
        try
        {
            // Ignore bot reactions
            if (reaction.User.IsSpecified && reaction.User.Value.IsBot)
                return;

            var channel = await cachedChannel.GetOrDownloadAsync();
            if (channel is not SocketGuildChannel guildChannel)
                return;

            // Find mapping in database
            var emoji = reaction.Emote.Name;
            var mapping = await _reactionRoles
                .Find(m => m.MessageId == reaction.MessageId && m.Emoji == emoji)
                .FirstOrDefaultAsync();

            if (mapping == null)
                return;

            // Remove role
            var guildUser = reaction.User.IsSpecified && reaction.User.Value is SocketGuildUser specifiedUser
                ? specifiedUser
                : guildChannel.Guild.GetUser(reaction.UserId);

            if (guildUser != null)
            {
                var role = guildChannel.Guild.GetRole(mapping.RoleId);
                if (role != null)
                {
                    await guildUser.RemoveRoleAsync(role);
                    _logger.LogInformation(
                        "Removed role {RoleName} ({RoleId}) from user {UserName} ({UserId}) via reaction removal {Emoji}",
                        role.Name,
                        mapping.RoleId,
                        guildUser.DisplayName,
                        reaction.UserId,
                        emoji
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "Role {RoleId} not found in guild {GuildId}",
                        mapping.RoleId,
                        guildChannel.Guild.Id
                    );
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling reaction removed");
        }
    }
}

