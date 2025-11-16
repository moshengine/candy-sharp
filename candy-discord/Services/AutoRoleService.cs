using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Candy.Discord;

public class AutoRoleService
{
    private readonly ILogger<AutoRoleService> _logger;

    public AutoRoleService(ILogger<AutoRoleService> logger)
    {
        _logger = logger;
    }

    public async Task GrantRoleToMemberAsync(SocketGuildUser user, ulong roleId)
    {
        try
        {
            if (user.Roles.Any(r => r.Id == roleId))
            {
                _logger.LogDebug("User {Username} already has role {RoleId}", user.Username, roleId);
                return;
            }

            var role = user.Guild.GetRole(roleId);
            if (role == null)
            {
                _logger.LogWarning("Role {RoleId} not found in guild {GuildId}", roleId, user.Guild.Id);
                return;
            }

            await user.AddRoleAsync(role);
            _logger.LogInformation("Granted role {RoleName} to user {Username}", role.Name, user.Username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to grant role {RoleId} to user {Username}", roleId, user.Username);
        }
    }

    public async Task SyncAutoRoleForGuildAsync(SocketGuild guild, ulong roleId)
    {
        try
        {
            _logger.LogInformation("Starting auto-role sync for guild {GuildName} ({GuildId})", guild.Name, guild.Id);

            var role = guild.GetRole(roleId);
            if (role == null)
            {
                _logger.LogWarning("Role {RoleId} not found in guild {GuildId}", roleId, guild.Id);
                return;
            }

            var membersWithoutRole = guild.Users.Where(u => !u.IsBot && !u.Roles.Any(r => r.Id == roleId)).ToList();

            if (membersWithoutRole.Count == 0)
            {
                _logger.LogInformation("All members already have the auto-role {RoleName}", role.Name);
                return;
            }

            _logger.LogInformation("Found {Count} members without role {RoleName}", membersWithoutRole.Count, role.Name);

            foreach (var member in membersWithoutRole)
            {
                await GrantRoleToMemberAsync(member, roleId);
                await Task.Delay(100); // Rate limit protection
            }

            _logger.LogInformation("Completed auto-role sync for guild {GuildName}", guild.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing auto-role for guild {GuildId}", guild.Id);
        }
    }
}

