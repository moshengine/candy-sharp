using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Notion.Client;

namespace Candy.AspNet
{
    public class NotionService
    {
        private readonly string _apiKey;
        private readonly NotionClient _client;

        public NotionService(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException(
                    "Notion API key cannot be null or empty.",
                    nameof(apiKey)
                );
            }

            _apiKey = apiKey;
            _client = NotionClientFactory.Create(new ClientOptions { AuthToken = _apiKey });

            Debug.Log($"Notion service initialized with apiKey: {_apiKey}");
        }

        public async Task<Page> CreatePageAsync(PagesCreateParameters parameters)
        {
            return await _client.Pages.CreateAsync(parameters);
        }

        public async Task<List<Page>> QueryDatabaseAsync(
            string databaseId,
            DatabasesQueryParameters queryParams = null
        )
        {
            if (string.IsNullOrEmpty(databaseId))
            {
                throw new ArgumentException(
                    "Database ID cannot be null or empty.",
                    nameof(databaseId)
                );
            }

            queryParams ??= new DatabasesQueryParameters();
            var pages = await _client.Databases.QueryAsync(databaseId, queryParams);
            return pages.Results;
        }
    }
}
