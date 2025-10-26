using System.Collections.Generic;
using Platform.Data.Doublets;

namespace Platform.Sandbox.StoredQueries
{
    /// <summary>
    ///     Type of trigger for query execution.
    ///     Тип триггера для выполнения запроса.
    /// </summary>
    public enum TriggerType
    {
        /// <summary>
        ///     Trigger executes before query execution.
        ///     Триггер выполняется перед выполнением запроса.
        /// </summary>
        Before,

        /// <summary>
        ///     Trigger executes after successful query execution.
        ///     Триггер выполняется после успешного выполнения запроса.
        /// </summary>
        After,

        /// <summary>
        ///     Trigger executes when query execution fails.
        ///     Триггер выполняется при ошибке выполнения запроса.
        /// </summary>
        OnError
    }

    /// <summary>
    ///     Interface for query triggers.
    ///     Интерфейс для триггеров запроса.
    /// </summary>
    /// <typeparam name="TLinkAddress">Type of link address.</typeparam>
    /// <typeparam name="TResult">Type of query result.</typeparam>
    public interface IQueryTrigger<TLinkAddress, TResult>
    {
        /// <summary>
        ///     Type of the trigger (Before, After, or OnError).
        ///     Тип триггера (До, После или При ошибке).
        /// </summary>
        TriggerType Type { get; }

        /// <summary>
        ///     Name of the trigger for identification.
        ///     Имя триггера для идентификации.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Executes the trigger.
        ///     Выполняет триггер.
        /// </summary>
        /// <param name="links">The links database.</param>
        /// <param name="results">Query results (null for Before and OnError triggers).</param>
        void Execute(ILinks<TLinkAddress> links, IEnumerable<TResult> results);
    }
}
