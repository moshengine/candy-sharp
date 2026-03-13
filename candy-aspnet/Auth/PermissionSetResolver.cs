namespace Candy.AspNet.Auth;

public static class PermissionSetResolver
{
    public static IReadOnlySet<string> ResolveEffective(
        string role,
        IEnumerable<string>? grants,
        IEnumerable<string>? denies,
        IRolePermissionDefaults roleDefaults)
    {
        var effective = new HashSet<string>(roleDefaults.ForRole(role), StringComparer.Ordinal);

        foreach (var permission in Normalize(grants))
            effective.Add(permission);

        foreach (var permission in Normalize(denies))
            effective.Remove(permission);

        return effective;
    }

    public static IReadOnlySet<string> ResolveEffective(IUser user, IRolePermissionDefaults roleDefaults)
    {
        return ResolveEffective(user.Role, user.PermissionGrants, user.PermissionDenies, roleDefaults);
    }

    private static IEnumerable<string> Normalize(IEnumerable<string>? permissions)
    {
        if (permissions == null)
            yield break;

        foreach (var raw in permissions)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            yield return raw.Trim().ToLowerInvariant();
        }
    }
}
