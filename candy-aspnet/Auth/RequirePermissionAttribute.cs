using Microsoft.AspNetCore.Authorization;

namespace Candy.AspNet.Auth;

public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    internal const string PolicyPrefix = "perm:";

    public string Permission { get; }

    public RequirePermissionAttribute(string permission)
    {
        Permission = permission;
        Policy = $"{PolicyPrefix}{permission}";
    }
}
