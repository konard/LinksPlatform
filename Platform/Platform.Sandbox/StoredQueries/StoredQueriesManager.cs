using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Platform.Data.Doublets;

namespace Platform.Sandbox.StoredQueries
{
    /// <summary>
    ///     Manager for stored queries that can be persisted in the database.
    ///     Менеджер для хранимых запросов, которые могут быть сохранены в базе данных.
    /// </summary>
    /// <typeparam name="TLinkAddress">Type of link address.</typeparam>
    public class StoredQueriesManager<TLinkAddress>
    {
        private readonly ConcurrentDictionary<TLinkAddress, object> _queriesById;
        private readonly ConcurrentDictionary<string, object> _queriesByName;
        private readonly ILinks<TLinkAddress> _links;
        private readonly object _lock = new object();

        /// <summary>
        ///     Initializes a new instance of the StoredQueriesManager class.
        ///     Инициализирует новый экземпляр класса StoredQueriesManager.
        /// </summary>
        /// <param name="links">The links database where queries will be stored.</param>
        public StoredQueriesManager(ILinks<TLinkAddress> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _queriesById = new ConcurrentDictionary<TLinkAddress, object>();
            _queriesByName = new ConcurrentDictionary<string, object>();
        }

        /// <summary>
        ///     Creates and stores a new query.
        ///     Создаёт и сохраняет новый запрос.
        /// </summary>
        /// <typeparam name="TResult">Type of query result.</typeparam>
        /// <param name="name">Name of the query.</param>
        /// <param name="queryExpression">Query expression.</param>
        /// <returns>The created stored query.</returns>
        public StoredQuery<TLinkAddress, TResult> CreateQuery<TResult>(
            string name,
            Expression<Func<ILinks<TLinkAddress>, IEnumerable<TResult>>> queryExpression)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Query name cannot be null or empty.", nameof(name));
            }

            if (queryExpression == null)
            {
                throw new ArgumentNullException(nameof(queryExpression));
            }

            lock (_lock)
            {
                if (_queriesByName.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Query with name '{name}' already exists.");
                }

                // Create a unique ID for the query (in a real implementation, this would be a link)
                var id = GetNextQueryId();

                var query = new StoredQuery<TLinkAddress, TResult>
                {
                    Id = id,
                    Name = name,
                    QueryExpression = queryExpression
                };

                // Compile the query immediately for optimization
                query.Compile();

                _queriesById[id] = query;
                _queriesByName[name] = query;

                return query;
            }
        }

        /// <summary>
        ///     Gets a stored query by its ID.
        ///     Получает хранимый запрос по его идентификатору.
        /// </summary>
        /// <typeparam name="TResult">Type of query result.</typeparam>
        /// <param name="id">Query ID.</param>
        /// <returns>The stored query, or null if not found.</returns>
        public StoredQuery<TLinkAddress, TResult> GetQueryById<TResult>(TLinkAddress id)
        {
            if (_queriesById.TryGetValue(id, out var query))
            {
                return query as StoredQuery<TLinkAddress, TResult>;
            }
            return null;
        }

        /// <summary>
        ///     Gets a stored query by its name.
        ///     Получает хранимый запрос по его имени.
        /// </summary>
        /// <typeparam name="TResult">Type of query result.</typeparam>
        /// <param name="name">Query name.</param>
        /// <returns>The stored query, or null if not found.</returns>
        public StoredQuery<TLinkAddress, TResult> GetQueryByName<TResult>(string name)
        {
            if (_queriesByName.TryGetValue(name, out var query))
            {
                return query as StoredQuery<TLinkAddress, TResult>;
            }
            return null;
        }

        /// <summary>
        ///     Executes a stored query by its name.
        ///     Выполняет хранимый запрос по его имени.
        /// </summary>
        /// <typeparam name="TResult">Type of query result.</typeparam>
        /// <param name="name">Query name.</param>
        /// <returns>Query results.</returns>
        public IEnumerable<TResult> ExecuteQuery<TResult>(string name)
        {
            var query = GetQueryByName<TResult>(name);
            if (query == null)
            {
                throw new InvalidOperationException($"Query with name '{name}' not found.");
            }

            return query.Execute(_links);
        }

        /// <summary>
        ///     Executes a stored query by its ID.
        ///     Выполняет хранимый запрос по его идентификатору.
        /// </summary>
        /// <typeparam name="TResult">Type of query result.</typeparam>
        /// <param name="id">Query ID.</param>
        /// <returns>Query results.</returns>
        public IEnumerable<TResult> ExecuteQueryById<TResult>(TLinkAddress id)
        {
            var query = GetQueryById<TResult>(id);
            if (query == null)
            {
                throw new InvalidOperationException($"Query with ID '{id}' not found.");
            }

            return query.Execute(_links);
        }

        /// <summary>
        ///     Deletes a stored query by its name.
        ///     Удаляет хранимый запрос по его имени.
        /// </summary>
        /// <param name="name">Query name.</param>
        /// <returns>True if the query was deleted, false if it was not found.</returns>
        public bool DeleteQuery(string name)
        {
            lock (_lock)
            {
                if (_queriesByName.TryRemove(name, out var queryObj))
                {
                    // Remove from ID dictionary as well
                    var query = queryObj as dynamic;
                    if (query != null)
                    {
                        _queriesById.TryRemove(query.Id, out object removedQuery);
                    }
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        ///     Gets all stored queries.
        ///     Получает все хранимые запросы.
        /// </summary>
        /// <returns>List of all stored queries.</returns>
        public List<object> GetAllQueries()
        {
            return _queriesByName.Values.ToList();
        }

        /// <summary>
        ///     Gets the count of stored queries.
        ///     Получает количество хранимых запросов.
        /// </summary>
        public int Count => _queriesByName.Count;

        /// <summary>
        ///     Enables a stored query.
        ///     Включает хранимый запрос.
        /// </summary>
        /// <param name="name">Query name.</param>
        public void EnableQuery(string name)
        {
            if (_queriesByName.TryGetValue(name, out var queryObj))
            {
                var query = queryObj as dynamic;
                if (query != null)
                {
                    query.IsEnabled = true;
                }
            }
        }

        /// <summary>
        ///     Disables a stored query.
        ///     Отключает хранимый запрос.
        /// </summary>
        /// <param name="name">Query name.</param>
        public void DisableQuery(string name)
        {
            if (_queriesByName.TryGetValue(name, out var queryObj))
            {
                var query = queryObj as dynamic;
                if (query != null)
                {
                    query.IsEnabled = false;
                }
            }
        }

        private TLinkAddress GetNextQueryId()
        {
            // In a real implementation, this would create a new link in the database
            // For now, we'll use a simple counter-based approach
            var count = _queriesById.Count;
            return (TLinkAddress)(object)(count + 1);
        }
    }
}
