namespace Candy.AspNet.Auth;

/// <summary>
///   No-op role defaults for apps using the simple per-user permission system.
/// </summary>
public sealed class EmptyRolePermissionDefaults : IRolePermissionDefaults
{
    public IReadOnlySet<string> ForRole(string role) =>
        new HashSet<string>(StringComparer.Ordinal);

    public IReadOnlyList<Role> GetAllRoles() => [];

    public Role? GetDefaultRole() => null;

    public Task ReloadAsync() => Task.CompletedTask;
}
