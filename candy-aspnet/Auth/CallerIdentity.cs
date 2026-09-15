using System.Security.Claims;

namespace MoshEngine.Candy.AspNet.Auth;

public record CallerIdentity(string UserId, string Role, bool IsService, IReadOnlySet<string> Permissions)
{
    public bool HasPermission(string permission)
    {
        if (Permissions.Contains(PermissionClaimTypes.Admin))
            return true;

        return Permissions.Contains(permission);
    }

    public static CallerIdentity FromPrincipal(ClaimsPrincipal principal, IRolePermissionDefaults roleDefaults)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var isService = principal.FindFirstValue("auth_type") == "apikey";
        var role = principal.FindFirstValue(ClaimTypes.Role) ?? "";
        var claimPermissions = principal.FindAll(PermissionClaimTypes.Permission)
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLowerInvariant())
            .ToHashSet(StringComparer.Ordinal);
        var permissions = claimPermissions.Count > 0
            ? claimPermissions
            : new HashSet<string>(roleDefaults.ForRole(role), StringComparer.Ordinal);

        return new CallerIdentity(userId, role, isService, permissions);
    }
}
