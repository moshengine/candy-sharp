using Microsoft.Extensions.DependencyInjection;

namespace MoshEngine.Candy.AspNet
{
    public static class NotionExtensions
    {
        public static IServiceCollection AddNotionService(
            this IServiceCollection services,
            string apiKey
        )
        {
            return services.AddSingleton(_ => new NotionService(apiKey));
        }
    }
}
