using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Unicode;
using Platform.Data;

namespace Platform.Examples
{
    /// <summary>
    /// Service that checks if a Wikipedia or Wiktionary page exists for a given word or phrase.
    /// Results can be cached in Links storage for efficient retrieval.
    /// </summary>
    public class WikiPageChecker
    {
        private readonly HttpClient _httpClient;
        private readonly SynchronizedLinks<ulong> _links;
        private readonly Dictionary<string, WikiPageCheckResult> _cache;
        private readonly bool _useLinksCaching;

        /// <summary>
        /// Initializes a new instance of the WikiPageChecker class.
        /// </summary>
        /// <param name="links">Optional synchronized links storage for caching results.</param>
        /// <param name="useLinksCaching">Whether to cache results in Links storage.</param>
        public WikiPageChecker(SynchronizedLinks<ulong> links = null, bool useLinksCaching = false)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            _links = links;
            _cache = new Dictionary<string, WikiPageCheckResult>();
            _useLinksCaching = useLinksCaching && links != null;
        }

        /// <summary>
        /// Checks if a Wikipedia page exists for the given word or phrase.
        /// </summary>
        /// <param name="query">The word or phrase to search for.</param>
        /// <param name="language">Language code (default: "en" for English).</param>
        /// <returns>Result indicating whether the page exists and its URL.</returns>
        public async Task<WikiPageCheckResult> CheckWikipediaPageAsync(string query, string language = "en")
        {
            return await CheckPageAsync(query, WikiType.Wikipedia, language);
        }

        /// <summary>
        /// Checks if a Wiktionary page exists for the given word or phrase.
        /// </summary>
        /// <param name="query">The word or phrase to search for.</param>
        /// <param name="language">Language code (default: "en" for English).</param>
        /// <returns>Result indicating whether the page exists and its URL.</returns>
        public async Task<WikiPageCheckResult> CheckWiktionaryPageAsync(string query, string language = "en")
        {
            return await CheckPageAsync(query, WikiType.Wiktionary, language);
        }

        /// <summary>
        /// Checks both Wikipedia and Wiktionary for the given word or phrase.
        /// </summary>
        /// <param name="query">The word or phrase to search for.</param>
        /// <param name="language">Language code (default: "en" for English).</param>
        /// <returns>Results for both Wikipedia and Wiktionary.</returns>
        public async Task<WikiPageCheckResults> CheckBothAsync(string query, string language = "en")
        {
            var wikipediaTask = CheckWikipediaPageAsync(query, language);
            var wiktionaryTask = CheckWiktionaryPageAsync(query, language);

            await Task.WhenAll(wikipediaTask, wiktionaryTask);

            return new WikiPageCheckResults
            {
                Wikipedia = wikipediaTask.Result,
                Wiktionary = wiktionaryTask.Result
            };
        }

        private async Task<WikiPageCheckResult> CheckPageAsync(string query, WikiType wikiType, string language)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Query cannot be null or empty", nameof(query));
            }

            var cacheKey = $"{wikiType}:{language}:{query}";

            // Check in-memory cache first
            if (_cache.ContainsKey(cacheKey))
            {
                return _cache[cacheKey];
            }

            // Check Links storage cache if enabled
            if (_useLinksCaching)
            {
                var cachedResult = TryGetFromLinksCache(cacheKey);
                if (cachedResult != null)
                {
                    _cache[cacheKey] = cachedResult;
                    return cachedResult;
                }
            }

            // Perform actual HTTP check
            var result = await PerformHttpCheckAsync(query, wikiType, language);

            // Cache the result
            _cache[cacheKey] = result;
            if (_useLinksCaching)
            {
                StoreInLinksCache(cacheKey, result);
            }

            return result;
        }

        private async Task<WikiPageCheckResult> PerformHttpCheckAsync(string query, WikiType wikiType, string language)
        {
            var baseUrl = wikiType == WikiType.Wikipedia
                ? $"https://{language}.wikipedia.org/wiki/"
                : $"https://{language}.wiktionary.org/wiki/";

            var encodedQuery = Uri.EscapeDataString(query.Replace(" ", "_"));
            var url = baseUrl + encodedQuery;

            try
            {
                var response = await _httpClient.GetAsync(url);
                var exists = response.IsSuccessStatusCode;

                return new WikiPageCheckResult
                {
                    Query = query,
                    WikiType = wikiType,
                    Language = language,
                    Exists = exists,
                    Url = exists ? url : null,
                    CheckedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new WikiPageCheckResult
                {
                    Query = query,
                    WikiType = wikiType,
                    Language = language,
                    Exists = false,
                    Url = null,
                    CheckedAt = DateTime.UtcNow,
                    Error = ex.Message
                };
            }
        }

        private WikiPageCheckResult TryGetFromLinksCache(string cacheKey)
        {
            // TODO: Implement Links storage retrieval
            // This would involve searching for the cached key and deserializing the result
            return null;
        }

        private void StoreInLinksCache(string cacheKey, WikiPageCheckResult result)
        {
            // TODO: Implement Links storage caching
            // This would involve serializing the result and storing it as a link sequence
        }

        /// <summary>
        /// Clears the in-memory cache.
        /// </summary>
        public void ClearCache()
        {
            _cache.Clear();
        }
    }

    /// <summary>
    /// Represents the type of wiki service.
    /// </summary>
    public enum WikiType
    {
        Wikipedia,
        Wiktionary
    }

    /// <summary>
    /// Represents the result of checking for a wiki page.
    /// </summary>
    public class WikiPageCheckResult
    {
        public string Query { get; set; }
        public WikiType WikiType { get; set; }
        public string Language { get; set; }
        public bool Exists { get; set; }
        public string Url { get; set; }
        public DateTime CheckedAt { get; set; }
        public string Error { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Error))
            {
                return $"{WikiType} ({Language}): Error checking '{Query}' - {Error}";
            }
            return Exists
                ? $"{WikiType} ({Language}): '{Query}' exists at {Url}"
                : $"{WikiType} ({Language}): '{Query}' does not exist";
        }
    }

    /// <summary>
    /// Represents the combined results for both Wikipedia and Wiktionary.
    /// </summary>
    public class WikiPageCheckResults
    {
        public WikiPageCheckResult Wikipedia { get; set; }
        public WikiPageCheckResult Wiktionary { get; set; }

        public override string ToString()
        {
            return $"{Wikipedia}\n{Wiktionary}";
        }
    }
}
