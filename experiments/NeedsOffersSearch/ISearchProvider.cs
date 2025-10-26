using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinksPlatform.NeedsOffersSearch
{
    /// <summary>
    /// Interface for search providers (e.g., Google Ads API, Yandex Ads API)
    /// Used to search for people who match specific needs or offers
    /// </summary>
    public interface ISearchProvider
    {
        /// <summary>
        /// Gets the name of the search provider
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Initiates a search for people matching the given query
        /// </summary>
        /// <param name="query">The search query (need or offer description)</param>
        /// <param name="maxResults">Maximum number of results to return</param>
        /// <returns>Collection of search results</returns>
        Task<IEnumerable<SearchResult>> SearchAsync(string query, int maxResults = 10);

        /// <summary>
        /// Subscribes to ongoing search results for a specific query
        /// This allows low-frequency, continuous monitoring
        /// </summary>
        /// <param name="query">The search query to monitor</param>
        /// <param name="callback">Callback invoked when new results are found</param>
        /// <returns>Subscription ID for managing the subscription</returns>
        Task<string> SubscribeAsync(string query, Action<SearchResult> callback);

        /// <summary>
        /// Unsubscribes from a search subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription ID to cancel</param>
        Task UnsubscribeAsync(string subscriptionId);

        /// <summary>
        /// Checks if the provider is properly configured and ready to use
        /// </summary>
        bool IsConfigured { get; }
    }

    /// <summary>
    /// Represents a search result from an advertising API
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// Unique identifier for this result
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The query that produced this result
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Contact information or identifier for the person/entity
        /// </summary>
        public string ContactInfo { get; set; }

        /// <summary>
        /// Description or additional details about the match
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Relevance score (0.0 to 1.0)
        /// </summary>
        public double RelevanceScore { get; set; }

        /// <summary>
        /// Timestamp when this result was found
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Source provider name
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Additional metadata about the result
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}
