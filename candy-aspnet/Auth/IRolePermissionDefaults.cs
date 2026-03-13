namespace Candy.AspNet.Auth;

public interface IRolePermissionDefaults
{
    IReadOnlySet<string> ForRole(string role);
    IReadOnlyList<Role> GetAllRoles();
    Role? GetDefaultRole();
    Task ReloadAsync();
}
