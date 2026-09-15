using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Candy.AspNet;

public static class SecurityExtensions
{
    /// <summary>
    /// Registers a default CORS policy for browser SPAs and SignalR hubs.
    /// localhost / 127.0.0.1 (any port) are always allowed; set
    /// <paramref name="corsOriginsEnvironmentVariable"/> (comma-separated) for
    /// production web origins.
    /// </summary>
    public static IServiceCollection AddSimpleCors(
        this IServiceCollection services,
        string corsOriginsEnvironmentVariable = "CORS_ORIGINS")
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                ConfigureSimpleCorsPolicy(policy, corsOriginsEnvironmentVariable));
        });

        return services;
    }

    /// <summary>
    /// Applies the default CORS policy from <see cref="AddSimpleCors"/>.
    /// </summary>
    public static void UseSimpleCors(this WebApplication app) => app.UseCors();

    private static void ConfigureSimpleCorsPolicy(
        CorsPolicyBuilder policy,
        string corsOriginsEnvironmentVariable)
    {
        var explicitOrigins = (Environment.GetEnvironmentVariable(corsOriginsEnvironmentVariable) ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        policy.SetIsOriginAllowed(origin =>
            {
                if (Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                    && uri.Host is "localhost" or "127.0.0.1")
                {
                    return true;
                }

                return explicitOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    }
}
