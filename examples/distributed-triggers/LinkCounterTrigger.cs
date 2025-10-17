using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Doublets.DistributedTriggers;

namespace Platform.Data.Doublets.Examples.DistributedTriggers
{
    /// <summary>
    /// Example trigger that maintains a count of total links.
    /// Demonstrates how triggers can produce new operations.
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public class LinkCounterTrigger<TLinkAddress> : IDistributedTrigger<TLinkAddress>
        where TLinkAddress : struct, IEquatable<TLinkAddress>, IComparable<TLinkAddress>
    {
        private readonly Guid _triggerId;
        private readonly int _partitionKey;
        private readonly TLinkAddress _counterLinkAddress;
        private int _currentCount;

        public LinkCounterTrigger(int partitionKey, TLinkAddress counterLinkAddress)
        {
            _triggerId = Guid.NewGuid();
            _partitionKey = partitionKey;
            _counterLinkAddress = counterLinkAddress;
            _currentCount = 0;
        }

        public Guid TriggerId => _triggerId;

        public int PartitionKey => _partitionKey;

        public IEnumerable<TransactionOperation<TLinkAddress>> Execute(
            TransactionEvent<TLinkAddress> @event,
            IReadOnlyDictionary<TLinkAddress, Link<TLinkAddress>> snapshot)
        {
            // Count net changes (creates - deletes)
            int netChange = 0;
            foreach (var operation in @event.Operations)
            {
                switch (operation.OperationType)
                {
                    case OperationType.Create:
                        netChange++;
                        break;
                    case OperationType.Delete:
                        netChange--;
                        break;
                }
            }

            if (netChange != 0)
            {
                _currentCount += netChange;

                // Create an update operation for the counter link
                // In a real implementation, this would update a special counter link
                // Here we demonstrate the pattern
                var counterSource = (TLinkAddress)(object)(ulong)_currentCount;
                var counterTarget = (TLinkAddress)(object)(ulong)snapshot.Count;

                yield return new TransactionOperation<TLinkAddress>(
                    OperationType.Update,
                    _counterLinkAddress,
                    counterSource,
                    counterTarget);
            }
        }

        public bool ShouldExecute(TransactionEvent<TLinkAddress> @event)
        {
            // Execute if there are any create or delete operations
            return @event.Operations.Any(op =>
                op.OperationType == OperationType.Create ||
                op.OperationType == OperationType.Delete);
        }
    }
}
