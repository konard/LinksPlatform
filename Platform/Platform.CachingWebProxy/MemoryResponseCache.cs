using System;
using System.Collections.Concurrent;

namespace Platform.CachingWebProxy
{
    /// <summary>
    /// In-memory implementation of response cache using ConcurrentDictionary.
    /// </summary>
    public class MemoryResponseCache : IResponseCache
    {
        private readonly ConcurrentDictionary<string, CachedResponse> _cache;
        private readonly TimeSpan _defaultExpiration;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryResponseCache"/> class.
        /// </summary>
        /// <param name="defaultExpiration">Default expiration time for cached responses. Defaults to 1 hour.</param>
        public MemoryResponseCache(TimeSpan? defaultExpiration = null)
        {
            _cache = new ConcurrentDictionary<string, CachedResponse>();
            _defaultExpiration = defaultExpiration ?? TimeSpan.FromHours(1);
        }

        /// <inheritdoc />
        public bool TryGet(string url, out CachedResponse response)
        {
            if (_cache.TryGetValue(url, out response))
            {
                // Check if the cached response has expired
                if (DateTime.UtcNow - response.CachedAt < _defaultExpiration)
                {
                    return true;
                }
                else
                {
                    // Remove expired entry
                    _cache.TryRemove(url, out _);
                    response = null;
                    return false;
                }
            }
            response = null;
            return false;
        }

        /// <inheritdoc />
        public void Set(string url, CachedResponse response)
        {
            response.CachedAt = DateTime.UtcNow;
            _cache[url] = response;
        }

        /// <inheritdoc />
        public void Remove(string url)
        {
            _cache.TryRemove(url, out _);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Gets the current number of cached responses.
        /// </summary>
        public int Count => _cache.Count;
    }
}
