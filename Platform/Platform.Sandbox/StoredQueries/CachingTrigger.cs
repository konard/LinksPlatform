using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Doublets;

namespace Platform.Sandbox.StoredQueries
{
    /// <summary>
    ///     A trigger that caches query results.
    ///     Триггер, который кэширует результаты запроса.
    /// </summary>
    /// <typeparam name="TLinkAddress">Type of link address.</typeparam>
    /// <typeparam name="TResult">Type of query result.</typeparam>
    public class CachingTrigger<TLinkAddress, TResult> : IQueryTrigger<TLinkAddress, TResult>
    {
        private readonly Dictionary<string, CachedResult> _cache;
        private readonly TimeSpan _cacheDuration;
        private readonly int _maxCacheSize;

        /// <summary>
        ///     Gets the type of the trigger (always After for caching).
        ///     Получает тип триггера (всегда After для кэширования).
        /// </summary>
        public TriggerType Type => TriggerType.After;

        /// <summary>
        ///     Gets the name of the trigger.
        ///     Получает имя триггера.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Initializes a new instance of the CachingTrigger class.
        ///     Инициализирует новый экземпляр класса CachingTrigger.
        /// </summary>
        /// <param name="name">Name of the trigger.</param>
        /// <param name="cacheDuration">How long to cache results.</param>
        /// <param name="maxCacheSize">Maximum number of cached results.</param>
        public CachingTrigger(string name, TimeSpan cacheDuration, int maxCacheSize = 100)
        {
            Name = name;
            _cacheDuration = cacheDuration;
            _maxCacheSize = maxCacheSize;
            _cache = new Dictionary<string, CachedResult>();
        }

        /// <summary>
        ///     Executes the caching trigger.
        ///     Выполняет триггер кэширования.
        /// </summary>
        /// <param name="links">The links database.</param>
        /// <param name="results">Query results to cache.</param>
        public void Execute(ILinks<TLinkAddress> links, IEnumerable<TResult> results)
        {
            if (results == null) return;

            var key = GenerateCacheKey();
            var cachedResult = new CachedResult
            {
                Results = results.ToList(),
                Timestamp = DateTime.UtcNow,
                ExpirationTime = DateTime.UtcNow.Add(_cacheDuration)
            };

            lock (_cache)
            {
                // Remove expired entries
                var expiredKeys = _cache.Where(kvp => kvp.Value.ExpirationTime < DateTime.UtcNow)
                                        .Select(kvp => kvp.Key)
                                        .ToList();
                foreach (var expiredKey in expiredKeys)
                {
                    _cache.Remove(expiredKey);
                }

                // If cache is full, remove oldest entry
                if (_cache.Count >= _maxCacheSize)
                {
                    var oldestKey = _cache.OrderBy(kvp => kvp.Value.Timestamp).First().Key;
                    _cache.Remove(oldestKey);
                }

                _cache[key] = cachedResult;
            }
        }

        /// <summary>
        ///     Tries to get cached results.
        ///     Пытается получить кэшированные результаты.
        /// </summary>
        /// <param name="results">Cached results if found.</param>
        /// <returns>True if cached results were found and are still valid.</returns>
        public bool TryGetCachedResults(out IEnumerable<TResult> results)
        {
            var key = GenerateCacheKey();

            lock (_cache)
            {
                if (_cache.TryGetValue(key, out var cachedResult))
                {
                    if (cachedResult.ExpirationTime > DateTime.UtcNow)
                    {
                        results = cachedResult.Results;
                        return true;
                    }
                    else
                    {
                        // Remove expired entry
                        _cache.Remove(key);
                    }
                }
            }

            results = null;
            return false;
        }

        /// <summary>
        ///     Clears the cache.
        ///     Очищает кэш.
        /// </summary>
        public void ClearCache()
        {
            lock (_cache)
            {
                _cache.Clear();
            }
        }

        private string GenerateCacheKey()
        {
            // In a real implementation, this would generate a unique key based on query parameters
            return $"{Name}_{DateTime.UtcNow.Ticks}";
        }

        private class CachedResult
        {
            public List<TResult> Results { get; set; }
            public DateTime Timestamp { get; set; }
            public DateTime ExpirationTime { get; set; }
        }
    }
}
