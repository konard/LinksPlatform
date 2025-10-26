using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Sandbox.StoredQueries
{
    /// <summary>
    ///     Stores execution statistics for a stored query.
    ///     Хранит статистику выполнения хранимого запроса.
    /// </summary>
    public class QueryExecutionStatistics
    {
        private readonly object _lock = new object();
        private readonly List<QueryExecutionRecord> _recentExecutions;
        private const int MaxRecentExecutions = 1000;

        /// <summary>
        ///     Total number of times the query has been executed.
        ///     Общее количество выполнений запроса.
        /// </summary>
        public long TotalExecutions { get; private set; }

        /// <summary>
        ///     Number of successful executions.
        ///     Количество успешных выполнений.
        /// </summary>
        public long SuccessfulExecutions { get; private set; }

        /// <summary>
        ///     Number of failed executions.
        ///     Количество неудачных выполнений.
        /// </summary>
        public long FailedExecutions { get; private set; }

        /// <summary>
        ///     Average execution time across all executions.
        ///     Среднее время выполнения по всем выполнениям.
        /// </summary>
        public TimeSpan AverageExecutionTime { get; private set; }

        /// <summary>
        ///     Minimum execution time recorded.
        ///     Минимальное зафиксированное время выполнения.
        /// </summary>
        public TimeSpan MinExecutionTime { get; private set; }

        /// <summary>
        ///     Maximum execution time recorded.
        ///     Максимальное зафиксированное время выполнения.
        /// </summary>
        public TimeSpan MaxExecutionTime { get; private set; }

        /// <summary>
        ///     Total execution time across all executions.
        ///     Общее время выполнения по всем выполнениям.
        /// </summary>
        public TimeSpan TotalExecutionTime { get; private set; }

        /// <summary>
        ///     Average number of results returned.
        ///     Среднее количество возвращаемых результатов.
        /// </summary>
        public double AverageResultCount { get; private set; }

        /// <summary>
        ///     Date and time of the last execution.
        ///     Дата и время последнего выполнения.
        /// </summary>
        public DateTime? LastExecutionTime { get; private set; }

        /// <summary>
        ///     Date and time of the first execution.
        ///     Дата и время первого выполнения.
        /// </summary>
        public DateTime? FirstExecutionTime { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the QueryExecutionStatistics class.
        ///     Инициализирует новый экземпляр класса QueryExecutionStatistics.
        /// </summary>
        public QueryExecutionStatistics()
        {
            _recentExecutions = new List<QueryExecutionRecord>();
            MinExecutionTime = TimeSpan.MaxValue;
            MaxExecutionTime = TimeSpan.MinValue;
        }

        /// <summary>
        ///     Records a query execution and updates statistics.
        ///     Записывает выполнение запроса и обновляет статистику.
        /// </summary>
        /// <param name="executionTime">Time taken for execution.</param>
        /// <param name="success">Whether the execution was successful.</param>
        /// <param name="resultCount">Number of results returned.</param>
        public void RecordExecution(TimeSpan executionTime, bool success, int resultCount)
        {
            lock (_lock)
            {
                TotalExecutions++;
                if (success)
                {
                    SuccessfulExecutions++;
                }
                else
                {
                    FailedExecutions++;
                }

                TotalExecutionTime += executionTime;
                AverageExecutionTime = TimeSpan.FromTicks(TotalExecutionTime.Ticks / TotalExecutions);

                if (executionTime < MinExecutionTime)
                {
                    MinExecutionTime = executionTime;
                }

                if (executionTime > MaxExecutionTime)
                {
                    MaxExecutionTime = executionTime;
                }

                // Update average result count
                AverageResultCount = ((AverageResultCount * (TotalExecutions - 1)) + resultCount) / TotalExecutions;

                LastExecutionTime = DateTime.UtcNow;
                if (FirstExecutionTime == null)
                {
                    FirstExecutionTime = DateTime.UtcNow;
                }

                // Store recent execution record
                var record = new QueryExecutionRecord
                {
                    ExecutionTime = executionTime,
                    Success = success,
                    ResultCount = resultCount,
                    Timestamp = DateTime.UtcNow
                };

                _recentExecutions.Add(record);

                // Keep only the most recent executions
                if (_recentExecutions.Count > MaxRecentExecutions)
                {
                    _recentExecutions.RemoveAt(0);
                }
            }
        }

        /// <summary>
        ///     Gets recent execution records.
        ///     Получает записи последних выполнений.
        /// </summary>
        /// <param name="count">Number of recent records to retrieve.</param>
        /// <returns>List of recent execution records.</returns>
        public List<QueryExecutionRecord> GetRecentExecutions(int count = 10)
        {
            lock (_lock)
            {
                return _recentExecutions.TakeLast(Math.Min(count, _recentExecutions.Count)).ToList();
            }
        }

        /// <summary>
        ///     Resets all statistics.
        ///     Сбрасывает всю статистику.
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                TotalExecutions = 0;
                SuccessfulExecutions = 0;
                FailedExecutions = 0;
                AverageExecutionTime = TimeSpan.Zero;
                MinExecutionTime = TimeSpan.MaxValue;
                MaxExecutionTime = TimeSpan.MinValue;
                TotalExecutionTime = TimeSpan.Zero;
                AverageResultCount = 0;
                LastExecutionTime = null;
                FirstExecutionTime = null;
                _recentExecutions.Clear();
            }
        }

        /// <summary>
        ///     Gets a formatted string representation of the statistics.
        ///     Получает форматированное строковое представление статистики.
        /// </summary>
        public override string ToString()
        {
            lock (_lock)
            {
                return $"Total: {TotalExecutions}, Success: {SuccessfulExecutions}, Failed: {FailedExecutions}, " +
                       $"Avg Time: {AverageExecutionTime.TotalMilliseconds:F2}ms, " +
                       $"Min: {MinExecutionTime.TotalMilliseconds:F2}ms, Max: {MaxExecutionTime.TotalMilliseconds:F2}ms, " +
                       $"Avg Results: {AverageResultCount:F2}";
            }
        }
    }

    /// <summary>
    ///     Represents a single query execution record.
    ///     Представляет одну запись выполнения запроса.
    /// </summary>
    public class QueryExecutionRecord
    {
        /// <summary>
        ///     Time taken for the execution.
        ///     Время, затраченное на выполнение.
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        ///     Whether the execution was successful.
        ///     Была ли выполнение успешным.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        ///     Number of results returned.
        ///     Количество возвращённых результатов.
        /// </summary>
        public int ResultCount { get; set; }

        /// <summary>
        ///     Timestamp of the execution.
        ///     Временная метка выполнения.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
