using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Doublets;

namespace Platform.Sandbox.StoredQueries
{
    /// <summary>
    ///     A trigger that logs query execution events.
    ///     Триггер, который записывает события выполнения запроса в лог.
    /// </summary>
    /// <typeparam name="TLinkAddress">Type of link address.</typeparam>
    /// <typeparam name="TResult">Type of query result.</typeparam>
    public class LoggingTrigger<TLinkAddress, TResult> : IQueryTrigger<TLinkAddress, TResult>
    {
        private readonly Action<string> _logger;

        /// <summary>
        ///     Gets the type of the trigger.
        ///     Получает тип триггера.
        /// </summary>
        public TriggerType Type { get; }

        /// <summary>
        ///     Gets the name of the trigger.
        ///     Получает имя триггера.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Initializes a new instance of the LoggingTrigger class.
        ///     Инициализирует новый экземпляр класса LoggingTrigger.
        /// </summary>
        /// <param name="name">Name of the trigger.</param>
        /// <param name="type">Type of the trigger.</param>
        /// <param name="logger">Logger action (defaults to Console.WriteLine).</param>
        public LoggingTrigger(string name, TriggerType type, Action<string> logger = null)
        {
            Name = name;
            Type = type;
            _logger = logger ?? Console.WriteLine;
        }

        /// <summary>
        ///     Executes the logging trigger.
        ///     Выполняет триггер записи в лог.
        /// </summary>
        /// <param name="links">The links database.</param>
        /// <param name="results">Query results (null for Before and OnError triggers).</param>
        public void Execute(ILinks<TLinkAddress> links, IEnumerable<TResult> results)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");

            switch (Type)
            {
                case TriggerType.Before:
                    _logger($"[{timestamp}] {Name}: Query execution started");
                    break;

                case TriggerType.After:
                    var resultCount = results?.Count() ?? 0;
                    _logger($"[{timestamp}] {Name}: Query execution completed. Results: {resultCount}");
                    break;

                case TriggerType.OnError:
                    _logger($"[{timestamp}] {Name}: Query execution failed");
                    break;
            }
        }
    }
}
