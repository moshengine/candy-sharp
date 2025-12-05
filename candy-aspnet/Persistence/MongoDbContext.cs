using MongoDB.Driver;

namespace Candy.AspNet
{
    /// <summary>
    /// For interacting with a MongoDB database.
    /// </summary>
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbContext"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string to use for connecting to the MongoDB server.</param>
        /// <param name="databaseName">The name of the database to connect to.</param>
        public MongoDbContext(string connectionString, string databaseName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException(
                    "Connection string cannot be null or empty.",
                    nameof(connectionString)
                );
            }

            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentException(
                    "Database name cannot be null or empty.",
                    nameof(databaseName)
                );
            }

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        /// <summary>
        /// Gets a MongoDB collection of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of documents in the collection.</typeparam>
        /// <param name="name">The name of the collection.</param>
        /// <returns>An <see cref="IMongoCollection{T}"/> representing the requested collection.</returns>
        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }
    }
}
