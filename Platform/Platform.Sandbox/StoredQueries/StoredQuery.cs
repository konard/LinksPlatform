using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Platform.Data.Doublets;

namespace Platform.Sandbox.StoredQueries
{
    /// <summary>
    ///     Represents a stored query that can be persisted in the database and executed repeatedly.
    ///     Хранимый запрос, который может быть сохранён в базе данных и выполнен многократно.
    /// </summary>
    /// <typeparam name="TLinkAddress">Type of link address.</typeparam>
    /// <typeparam name="TResult">Type of query result.</typeparam>
    public class StoredQuery<TLinkAddress, TResult>
    {
        /// <summary>
        ///     Unique identifier for the stored query.
        ///     Уникальный идентификатор хранимого запроса.
        /// </summary>
        public TLinkAddress Id { get; set; }

        /// <summary>
        ///     Name of the stored query for easy identification.
        ///     Имя хранимого запроса для удобной идентификации.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The query expression to be compiled and executed.
        ///     Выражение запроса для компиляции и выполнения.
        /// </summary>
        public Expression<Func<ILinks<TLinkAddress>, IEnumerable<TResult>>> QueryExpression { get; set; }

        /// <summary>
        ///     Compiled version of the query for faster execution.
        ///     Скомпилированная версия запроса для более быстрого выполнения.
        /// </summary>
        public Func<ILinks<TLinkAddress>, IEnumerable<TResult>> CompiledQuery { get; set; }

        /// <summary>
        ///     Execution statistics for this query.
        ///     Статистика выполнения этого запроса.
        /// </summary>
        public QueryExecutionStatistics Statistics { get; }

        /// <summary>
        ///     List of triggers associated with this query.
        ///     Список триггеров, связанных с этим запросом.
        /// </summary>
        public List<IQueryTrigger<TLinkAddress, TResult>> Triggers { get; }

        /// <summary>
        ///     Date and time when the query was created.
        ///     Дата и время создания запроса.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     Date and time when the query was last modified.
        ///     Дата и время последнего изменения запроса.
        /// </summary>
        public DateTime LastModifiedAt { get; set; }

        /// <summary>
        ///     Indicates whether the query is enabled for execution.
        ///     Указывает, включён ли запрос для выполнения.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        ///     Initializes a new instance of the StoredQuery class.
        ///     Инициализирует новый экземпляр класса StoredQuery.
        /// </summary>
        public StoredQuery()
        {
            Statistics = new QueryExecutionStatistics();
            Triggers = new List<IQueryTrigger<TLinkAddress, TResult>>();
            IsEnabled = true;
            CreatedAt = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        ///     Compiles the query expression if not already compiled.
        ///     Компилирует выражение запроса, если оно ещё не скомпилировано.
        /// </summary>
        public void Compile()
        {
            if (CompiledQuery == null && QueryExpression != null)
            {
                CompiledQuery = QueryExpression.Compile();
                LastModifiedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        ///     Executes the stored query and updates statistics.
        ///     Выполняет хранимый запрос и обновляет статистику.
        /// </summary>
        /// <param name="links">The links database to query.</param>
        /// <returns>Query results.</returns>
        public IEnumerable<TResult> Execute(ILinks<TLinkAddress> links)
        {
            if (!IsEnabled)
            {
                throw new InvalidOperationException($"Stored query '{Name}' is disabled.");
            }

            if (CompiledQuery == null)
            {
                Compile();
            }

            var startTime = DateTime.UtcNow;
            try
            {
                // Execute pre-triggers
                ExecuteTriggers(TriggerType.Before, links, null);

                var results = CompiledQuery(links);
                var resultsList = results as IList<TResult> ?? new List<TResult>(results);

                var endTime = DateTime.UtcNow;
                var executionTime = endTime - startTime;

                // Update statistics
                Statistics.RecordExecution(executionTime, true, resultsList.Count);

                // Execute post-triggers
                ExecuteTriggers(TriggerType.After, links, resultsList);

                return resultsList;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var executionTime = endTime - startTime;
                Statistics.RecordExecution(executionTime, false, 0);

                // Execute error triggers
                ExecuteTriggers(TriggerType.OnError, links, null);

                throw new QueryExecutionException($"Error executing stored query '{Name}': {ex.Message}", ex);
            }
        }

        /// <summary>
        ///     Adds a trigger to the stored query.
        ///     Добавляет триггер к хранимому запросу.
        /// </summary>
        /// <param name="trigger">The trigger to add.</param>
        public void AddTrigger(IQueryTrigger<TLinkAddress, TResult> trigger)
        {
            Triggers.Add(trigger);
        }

        /// <summary>
        ///     Removes a trigger from the stored query.
        ///     Удаляет триггер из хранимого запроса.
        /// </summary>
        /// <param name="trigger">The trigger to remove.</param>
        public bool RemoveTrigger(IQueryTrigger<TLinkAddress, TResult> trigger)
        {
            return Triggers.Remove(trigger);
        }

        /// <summary>
        ///     Executes triggers of a specific type.
        ///     Выполняет триггеры определённого типа.
        /// </summary>
        private void ExecuteTriggers(TriggerType type, ILinks<TLinkAddress> links, IEnumerable<TResult> results)
        {
            foreach (var trigger in Triggers)
            {
                if (trigger.Type == type)
                {
                    trigger.Execute(links, results);
                }
            }
        }
    }

    /// <summary>
    ///     Exception thrown when a stored query execution fails.
    ///     Исключение, выбрасываемое при ошибке выполнения хранимого запроса.
    /// </summary>
    public class QueryExecutionException : Exception
    {
        public QueryExecutionException(string message) : base(message) { }
        public QueryExecutionException(string message, Exception innerException) : base(message, innerException) { }
    }
}
