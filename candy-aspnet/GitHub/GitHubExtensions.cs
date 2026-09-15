using Microsoft.Extensions.DependencyInjection;

namespace MoshEngine.Candy.AspNet
{
    public static class GitHubExtensions
    {
        /// <summary>
        ///   Registers the multi-tenant <see cref="GitHubService"/> (tokens are
        ///   passed per call, so no global token is needed here).
        /// </summary>
        public static IServiceCollection AddGitHubService(this IServiceCollection services)
        {
            return services.AddSingleton<GitHubService>();
        }
    }
}
