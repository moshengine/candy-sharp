using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace Candy.AspNet
{
    /// <summary>
    ///   Thin wrapper over Octokit for the pieces the Tomoni GitHub issue sync
    ///   needs: listing a repository's issues (with filters) and flipping an issue
    ///   open/closed. Multi-tenant — a token is passed per call so each user/config
    ///   uses their own credential. Clients are cached per token.
    /// </summary>
    public class GitHubService
    {
        private readonly ConcurrentDictionary<string, GitHubClient> _clients = new();

        /// <summary>Resolves (and caches) an Octokit client for the given token.</summary>
        private GitHubClient ClientFor(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("GitHub token cannot be null or empty.", nameof(token));

            return _clients.GetOrAdd(token, t => new GitHubClient(new ProductHeaderValue("Tomoni"))
            {
                Credentials = new Credentials(t),
            });
        }

        /// <summary>
        ///   List every issue in a repository matching <paramref name="request"/>.
        ///   Octokit paginates through all result pages automatically. The result
        ///   includes pull requests (GitHub models PRs as issues) — callers filter
        ///   those out via <see cref="Issue.PullRequest"/> when unwanted.
        /// </summary>
        public async Task<IReadOnlyList<Issue>> GetIssuesForRepositoryAsync(
            string token,
            string owner,
            string repo,
            RepositoryIssueRequest request)
        {
            if (string.IsNullOrEmpty(owner))
                throw new ArgumentException("Owner cannot be null or empty.", nameof(owner));
            if (string.IsNullOrEmpty(repo))
                throw new ArgumentException("Repo cannot be null or empty.", nameof(repo));

            return await ClientFor(token).Issue.GetAllForRepository(owner, repo, request);
        }

        /// <summary>Set an issue's state (open/closed). Used for completion write-back.</summary>
        public async Task UpdateIssueStateAsync(string token, string owner, string repo, int number, ItemState state)
        {
            if (string.IsNullOrEmpty(owner))
                throw new ArgumentException("Owner cannot be null or empty.", nameof(owner));
            if (string.IsNullOrEmpty(repo))
                throw new ArgumentException("Repo cannot be null or empty.", nameof(repo));

            await ClientFor(token).Issue.Update(owner, repo, number, new IssueUpdate { State = state });
        }
    }
}
