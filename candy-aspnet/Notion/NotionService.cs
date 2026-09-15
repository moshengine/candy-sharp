using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Notion.Client;

namespace MoshEngine.Candy.AspNet
{
    /// <summary>
    ///   Wraps the Notion client. Supports two usage styles:
    ///   <list type="bullet">
    ///     <item>Single-tenant: construct with one API key and call the methods
    ///       without an <c>apiKey</c> argument (used by mosh-engine).</item>
    ///     <item>Multi-tenant: construct via <see cref="Disabled"/> (no default
    ///       key) and pass an <c>apiKey</c> per call, so different users/workspaces
    ///       use their own integration (used by Tomoni's per-config sync). Clients
    ///       are cached per key.</item>
    ///   </list>
    /// </summary>
    public class NotionService
    {
        private readonly NotionClient? _defaultClient;
        private readonly ConcurrentDictionary<string, NotionClient> _clients = new();

        public NotionService(string apiKey)
            : this(apiKey, createClient: true) { }

        /// <summary>
        ///   An instance with no default client — safe to register when no global
        ///   API key is configured. The parameterless method overloads throw; the
        ///   <c>apiKey</c> overloads work by creating a per-key client on demand.
        /// </summary>
        public static NotionService Disabled() => new(apiKey: null, createClient: false);

        protected NotionService(string? apiKey, bool createClient)
        {
            if (!createClient)
            {
                _defaultClient = null;
                return;
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("Notion API key cannot be null or empty.", nameof(apiKey));
            }

            _defaultClient = NotionClientFactory.Create(new ClientOptions { AuthToken = apiKey });
        }

        /// <summary>
        ///   Resolves the client for a call: a cached per-key client when
        ///   <paramref name="apiKey"/> is provided, otherwise the default client
        ///   (throws when neither is available).
        /// </summary>
        private NotionClient ResolveClient(string? apiKey)
        {
            if (!string.IsNullOrEmpty(apiKey))
                return _clients.GetOrAdd(apiKey, key => NotionClientFactory.Create(new ClientOptions { AuthToken = key }));

            return _defaultClient
                ?? throw new InvalidOperationException(
                    "Notion service has no default API key configured — pass an apiKey to this call.");
        }

        // --- Default-client methods (single-tenant, backward compatible) --------

        public Task<Page> CreatePageAsync(PagesCreateParameters parameters) =>
            CreatePageAsync(null, parameters);

        /// <summary>
        /// Query a Notion database with automatic pagination support.
        /// Fetches all matching pages across multiple API calls if needed.
        /// </summary>
        public Task<List<Page>> QueryDatabaseAsync(string databaseId, DatabasesQueryParameters? queryParams = null) =>
            QueryDatabaseAsync(null, databaseId, queryParams);

        public Task<Page> UpdatePageAsync(string pageId, PagesUpdateParameters parameters) =>
            UpdatePageAsync(null, pageId, parameters);

        public Task<Database> GetDatabaseAsync(string databaseId) =>
            GetDatabaseAsync(null, databaseId);

        // --- Per-key methods (multi-tenant) -------------------------------------

        public async Task<Page> CreatePageAsync(string? apiKey, PagesCreateParameters parameters)
        {
            return await ResolveClient(apiKey).Pages.CreateAsync(parameters);
        }

        /// <summary>
        /// Query a Notion database (using the given key's client) with automatic
        /// pagination support.
        /// </summary>
        public async Task<List<Page>> QueryDatabaseAsync(
            string? apiKey,
            string databaseId,
            DatabasesQueryParameters? queryParams = null)
        {
            if (string.IsNullOrEmpty(databaseId))
            {
                throw new ArgumentException("Database ID cannot be null or empty.", nameof(databaseId));
            }

            var client = ResolveClient(apiKey);
            queryParams ??= new DatabasesQueryParameters();
            var allPages = new List<Page>();
            string? cursor = null;

            do
            {
                queryParams.StartCursor = cursor;
                var response = await client.Databases.QueryAsync(databaseId, queryParams);
                allPages.AddRange(response.Results);
                cursor = response.HasMore ? response.NextCursor : null;
            } while (cursor != null);

            return allPages;
        }

        public async Task<Page> UpdatePageAsync(string? apiKey, string pageId, PagesUpdateParameters parameters)
        {
            if (string.IsNullOrEmpty(pageId))
            {
                throw new ArgumentException("Page ID cannot be null or empty.", nameof(pageId));
            }

            return await ResolveClient(apiKey).Pages.UpdateAsync(pageId, parameters);
        }

        public async Task<Database> GetDatabaseAsync(string? apiKey, string databaseId)
        {
            if (string.IsNullOrEmpty(databaseId))
            {
                throw new ArgumentException("Database ID cannot be null or empty.", nameof(databaseId));
            }

            return await ResolveClient(apiKey).Databases.RetrieveAsync(databaseId);
        }
    }
}
