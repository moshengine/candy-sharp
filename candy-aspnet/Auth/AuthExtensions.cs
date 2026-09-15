using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace Candy.AspNet.Auth;

public static class AuthExtensions
{
    public static IServiceCollection AddCandyAuth(
        this IServiceCollection services,
        string jwtSecret,
        string? apiKey = null,
        PermissionSystem permissionSystem = PermissionSystem.RoleBased,
        bool enableApiKey = true)
    {
        var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        var authBuilder = services.AddAuthentication(options =>
        {
            if (enableApiKey)
            {
                options.DefaultScheme = "MultiAuth";
                options.DefaultChallengeScheme = "MultiAuth";
            }
            else
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = jwtKey,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
            };
        });

        if (enableApiKey)
        {
            authBuilder
                .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>(ApiKeyAuthHandler.SchemeName, null)
                .AddPolicyScheme("MultiAuth", "JWT or API Key", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        if (context.Request.Headers.ContainsKey("X-Api-Key"))
                            return ApiKeyAuthHandler.SchemeName;
                        return JwtBearerDefaults.AuthenticationScheme;
                    };
                });
        }

        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        if (permissionSystem == PermissionSystem.RoleBased)
        {
            services.AddSingleton<IRolePermissionDefaults>(sp =>
                new DatabaseRolePermissionDefaults(sp.GetRequiredService<IMongoDatabase>()));
        }
        else
        {
            services.AddSingleton<IRolePermissionDefaults, EmptyRolePermissionDefaults>();
        }

        return services;
    }

    public static IServiceCollection AddCandyAuthSimple(this IServiceCollection services, string jwtSecret) =>
        services.AddCandyAuth(
            jwtSecret,
            permissionSystem: PermissionSystem.Simple,
            enableApiKey: false);
}
