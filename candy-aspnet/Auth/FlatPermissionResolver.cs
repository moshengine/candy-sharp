namespace MoshEngine.Candy.AspNet.Auth;

public static class FlatPermissionResolver
{
    public static IReadOnlySet<string> ResolveEffective(IUser user)
    {
        return new HashSet<string>(Normalize(user.Permissions), StringComparer.Ordinal);
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
