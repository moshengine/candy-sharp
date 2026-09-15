using Microsoft.AspNetCore.Authorization;

namespace MoshEngine.Candy.AspNet.Auth;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
