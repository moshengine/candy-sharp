using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Candy.AspNet.Auth;

public class ApiKeyAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ApiKey";
    private const string HeaderName = "X-Api-Key";

    private readonly string _apiKey;
    private readonly IRolePermissionDefaults _roleDefaults;

    public ApiKeyAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration,
        IRolePermissionDefaults roleDefaults)
        : base(options, logger, encoder)
    {
        _apiKey = new[] {
            Environment.GetEnvironmentVariable("API_KEY"),
            configuration["Auth:ApiKey"],
        }.FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? "";
        _roleDefaults = roleDefaults;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var headerValue))
            return Task.FromResult(AuthenticateResult.NoResult());

        if (string.IsNullOrEmpty(_apiKey) || headerValue != _apiKey)
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));

        var allRoles = _roleDefaults.GetAllRoles();
        var allPermissions = allRoles
            .SelectMany(r => r.Permissions)
            .Distinct(StringComparer.Ordinal);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "service"),
            new(ClaimTypes.Role, allRoles.FirstOrDefault(r => r.SortOrder == 0)?.Name ?? "Owner"),
            new("auth_type", "apikey"),
        };
        claims.AddRange(
            allPermissions.Select(x => new Claim(PermissionClaimTypes.Permission, x))
        );

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
