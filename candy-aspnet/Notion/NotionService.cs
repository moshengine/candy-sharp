using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Candy.Logging;
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

        /// <summary>
        /// Query a Notion database with automatic pagination support.
        /// Fetches all matching pages across multiple API calls if needed.
        /// </summary>
        public async Task<List<Page>> QueryDatabaseAsync(
            string databaseId,
            DatabasesQueryParameters? queryParams = null
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
            var allPages = new List<Page>();
            string? cursor = null;

            do
            {
                queryParams.StartCursor = cursor;
                var response = await _client.Databases.QueryAsync(databaseId, queryParams);
                allPages.AddRange(response.Results);
                cursor = response.HasMore ? response.NextCursor : null;
            } while (cursor != null);

            return allPages;
        }

        public async Task<Page> UpdatePageAsync(string pageId, PagesUpdateParameters parameters)
        {
            if (string.IsNullOrEmpty(pageId))
            {
                throw new ArgumentException(
                    "Page ID cannot be null or empty.",
                    nameof(pageId)
                );
            }

            return await _client.Pages.UpdateAsync(pageId, parameters);
        }

        public async Task<Database> GetDatabaseAsync(string databaseId)
        {
            if (string.IsNullOrEmpty(databaseId))
            {
                throw new ArgumentException(
                    "Database ID cannot be null or empty.",
                    nameof(databaseId)
                );
            }

            return await _client.Databases.RetrieveAsync(databaseId);
        }
    }
}
