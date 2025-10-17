using System;
using System.Collections.Generic;

namespace Platform.Data.Doublets.DistributedTriggers
{
    /// <summary>
    /// Represents a trigger that can be executed in a distributed manner across multiple workers.
    /// Triggers are transformations on the transaction log that can be partitioned and executed in parallel.
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public interface IDistributedTrigger<TLinkAddress>
    {
        /// <summary>
        /// Gets the unique identifier for this trigger.
        /// </summary>
        Guid TriggerId { get; }

        /// <summary>
        /// Gets the partition key that determines which worker handles this trigger.
        /// Multiple triggers with the same partition key will be handled by the same worker.
        /// </summary>
        int PartitionKey { get; }

        /// <summary>
        /// Executes the trigger for a transaction event.
        /// </summary>
        /// <param name="event">The transaction event containing link operations.</param>
        /// <param name="snapshot">The current snapshot of the links store.</param>
        /// <returns>A collection of operations to be merged back to the transaction log.</returns>
        IEnumerable<TransactionOperation<TLinkAddress>> Execute(
            TransactionEvent<TLinkAddress> @event,
            IReadOnlyDictionary<TLinkAddress, Link<TLinkAddress>> snapshot);

        /// <summary>
        /// Determines whether this trigger should be executed for the given event.
        /// </summary>
        /// <param name="event">The transaction event.</param>
        /// <returns>True if the trigger should execute; otherwise, false.</returns>
        bool ShouldExecute(TransactionEvent<TLinkAddress> @event);
    }
}
