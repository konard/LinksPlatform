using System;
using System.Collections.Generic;

namespace Platform.Data.Doublets.DistributedTriggers
{
    /// <summary>
    /// Represents an event in the transaction log that contains one or more link operations.
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public class TransactionEvent<TLinkAddress>
    {
        /// <summary>
        /// Gets or sets the unique transaction identifier.
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the transaction occurred.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the operations performed in this transaction.
        /// </summary>
        public IReadOnlyList<TransactionOperation<TLinkAddress>> Operations { get; set; }

        public TransactionEvent(long transactionId, DateTime timestamp, IReadOnlyList<TransactionOperation<TLinkAddress>> operations)
        {
            TransactionId = transactionId;
            Timestamp = timestamp;
            Operations = operations ?? Array.Empty<TransactionOperation<TLinkAddress>>();
        }
    }
}
