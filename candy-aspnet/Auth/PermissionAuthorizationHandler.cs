using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Candy.AspNet.Auth;

public sealed class PermissionAuthorizationHandler(
    IRolePermissionDefaults roleDefaults,
    ILogger<PermissionAuthorizationHandler> logger)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var caller = CallerIdentity.FromPrincipal(context.User, roleDefaults);
        if (caller.HasPermission(requirement.Permission))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        logger.LogWarning(
            "Permission denied for user {UserId}: {Permission}",
            userId,
            requirement.Permission
        );

        return Task.CompletedTask;
    }
}
