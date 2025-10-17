using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Doublets.DistributedTriggers;

namespace Platform.Data.Doublets.Examples.DistributedTriggers
{
    /// <summary>
    /// Example trigger that logs all create operations.
    /// Demonstrates how to implement a simple distributed trigger.
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public class SimpleLoggingTrigger<TLinkAddress> : IDistributedTrigger<TLinkAddress>
    {
        private readonly Guid _triggerId;
        private readonly int _partitionKey;
        private readonly Action<string> _logger;

        public SimpleLoggingTrigger(int partitionKey, Action<string> logger = null)
        {
            _triggerId = Guid.NewGuid();
            _partitionKey = partitionKey;
            _logger = logger ?? Console.WriteLine;
        }

        public Guid TriggerId => _triggerId;

        public int PartitionKey => _partitionKey;

        public IEnumerable<TransactionOperation<TLinkAddress>> Execute(
            TransactionEvent<TLinkAddress> @event,
            IReadOnlyDictionary<TLinkAddress, Link<TLinkAddress>> snapshot)
        {
            foreach (var operation in @event.Operations.Where(op => op.OperationType == OperationType.Create))
            {
                _logger($"[Trigger {_triggerId:N}] Link created: {operation.LinkAddress} " +
                       $"(Source: {operation.Source}, Target: {operation.Target})");
            }

            // This trigger doesn't produce new operations
            return Enumerable.Empty<TransactionOperation<TLinkAddress>>();
        }

        public bool ShouldExecute(TransactionEvent<TLinkAddress> @event)
        {
            // Execute if there are any create operations
            return @event.Operations.Any(op => op.OperationType == OperationType.Create);
        }
    }
}
