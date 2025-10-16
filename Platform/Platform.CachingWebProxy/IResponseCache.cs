using System;

namespace Platform.CachingWebProxy
{
    /// <summary>
    /// Interface for caching HTTP responses.
    /// </summary>
    public interface IResponseCache
    {
        /// <summary>
        /// Tries to get a cached response for the specified URL.
        /// </summary>
        /// <param name="url">The URL to look up in the cache.</param>
        /// <param name="response">The cached response if found.</param>
        /// <returns>True if a cached response was found; otherwise, false.</returns>
        bool TryGet(string url, out CachedResponse response);

        /// <summary>
        /// Stores a response in the cache.
        /// </summary>
        /// <param name="url">The URL to cache the response for.</param>
        /// <param name="response">The response to cache.</param>
        void Set(string url, CachedResponse response);

        /// <summary>
        /// Removes a cached response for the specified URL.
        /// </summary>
        /// <param name="url">The URL to remove from cache.</param>
        void Remove(string url);

        /// <summary>
        /// Clears all cached responses.
        /// </summary>
        void Clear();
    }
}
