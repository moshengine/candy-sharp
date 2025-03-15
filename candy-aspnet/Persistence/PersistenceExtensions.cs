using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Candy.AspNet
{
    public static class PersistenceExtensions
    {
        public static IServiceCollection AddMongoDbContext(
            this IServiceCollection services,
            string connectionString,
            string databaseName
        )
        {
            return services.AddScoped<IMongoDatabase>(sp =>
            {
                var client = new MongoClient(connectionString);
                return client.GetDatabase(databaseName);
            });
            //return services.AddSingleton(_ => new MongoDbContext(connectionString, databaseName));
        }
    }
}
