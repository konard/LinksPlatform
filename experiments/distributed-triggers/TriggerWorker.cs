using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Data.Doublets.DistributedTriggers
{
    /// <summary>
    /// Represents a worker that processes a subset of triggers for transaction events.
    /// Each worker is responsible for triggers with specific partition keys.
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public class TriggerWorker<TLinkAddress> : ITransactionLogSubscriber<TLinkAddress>
    {
        private readonly int _workerId;
        private readonly int _totalWorkers;
        private readonly List<IDistributedTrigger<TLinkAddress>> _triggers;
        private readonly object _triggersLock = new object();
        private readonly IResultCollector<TLinkAddress> _resultCollector;

        public TriggerWorker(int workerId, int totalWorkers, IResultCollector<TLinkAddress> resultCollector)
        {
            if (workerId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(workerId), "Worker ID must be non-negative.");
            }
            if (totalWorkers <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalWorkers), "Total workers must be positive.");
            }
            if (workerId >= totalWorkers)
            {
                throw new ArgumentOutOfRangeException(nameof(workerId), "Worker ID must be less than total workers.");
            }

            _workerId = workerId;
            _totalWorkers = totalWorkers;
            _resultCollector = resultCollector ?? throw new ArgumentNullException(nameof(resultCollector));
            _triggers = new List<IDistributedTrigger<TLinkAddress>>();
        }

        /// <summary>
        /// Gets the worker ID.
        /// </summary>
        public int WorkerId => _workerId;

        /// <summary>
        /// Registers a trigger with this worker.
        /// The worker will only execute triggers whose partition key maps to this worker.
        /// </summary>
        /// <param name="trigger">The trigger to register.</param>
        public void RegisterTrigger(IDistributedTrigger<TLinkAddress> trigger)
        {
            if (trigger == null)
            {
                throw new ArgumentNullException(nameof(trigger));
            }

            // Check if this trigger should be handled by this worker
            if (!ShouldHandleTrigger(trigger))
            {
                throw new ArgumentException(
                    $"Trigger with partition key {trigger.PartitionKey} cannot be handled by worker {_workerId}. " +
                    $"Expected worker: {GetWorkerForPartition(trigger.PartitionKey)}",
                    nameof(trigger));
            }

            lock (_triggersLock)
            {
                if (!_triggers.Any(t => t.TriggerId == trigger.TriggerId))
                {
                    _triggers.Add(trigger);
                }
            }
        }

        /// <summary>
        /// Unregisters a trigger from this worker.
        /// </summary>
        /// <param name="triggerId">The ID of the trigger to unregister.</param>
        public bool UnregisterTrigger(Guid triggerId)
        {
            lock (_triggersLock)
            {
                var trigger = _triggers.FirstOrDefault(t => t.TriggerId == triggerId);
                if (trigger != null)
                {
                    _triggers.Remove(trigger);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Called when a transaction event is received from the broadcaster.
        /// Executes all registered triggers that should handle this event.
        /// </summary>
        /// <param name="event">The transaction event.</param>
        public void OnTransactionEvent(TransactionEvent<TLinkAddress> @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            IDistributedTrigger<TLinkAddress>[] triggersCopy;
            lock (_triggersLock)
            {
                triggersCopy = _triggers.ToArray();
            }

            var results = new List<TransactionOperation<TLinkAddress>>();

            // Get the snapshot for this event
            var snapshot = _resultCollector.GetSnapshot(@event.TransactionId);

            foreach (var trigger in triggersCopy)
            {
                if (trigger.ShouldExecute(@event))
                {
                    try
                    {
                        var triggerResults = trigger.Execute(@event, snapshot);
                        if (triggerResults != null)
                        {
                            results.AddRange(triggerResults);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue processing other triggers
                        Console.Error.WriteLine($"Worker {_workerId}: Error executing trigger {trigger.TriggerId}: {ex.Message}");
                    }
                }
            }

            // Send results to the collector for merging
            if (results.Count > 0)
            {
                _resultCollector.CollectResults(_workerId, @event.TransactionId, results);
            }
        }

        /// <summary>
        /// Determines if this worker should handle a trigger based on its partition key.
        /// </summary>
        private bool ShouldHandleTrigger(IDistributedTrigger<TLinkAddress> trigger)
        {
            return GetWorkerForPartition(trigger.PartitionKey) == _workerId;
        }

        /// <summary>
        /// Calculates which worker should handle a given partition key.
        /// </summary>
        private int GetWorkerForPartition(int partitionKey)
        {
            // Use modulo to distribute partitions evenly across workers
            return Math.Abs(partitionKey) % _totalWorkers;
        }

        /// <summary>
        /// Gets the number of registered triggers.
        /// </summary>
        public int TriggerCount
        {
            get
            {
                lock (_triggersLock)
                {
                    return _triggers.Count;
                }
            }
        }
    }

    /// <summary>
    /// Interface for collecting results from workers.
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public interface IResultCollector<TLinkAddress>
    {
        /// <summary>
        /// Collects results from a worker after processing a transaction event.
        /// </summary>
        /// <param name="workerId">The ID of the worker.</param>
        /// <param name="transactionId">The transaction ID.</param>
        /// <param name="results">The operations produced by the worker's triggers.</param>
        void CollectResults(int workerId, long transactionId, IEnumerable<TransactionOperation<TLinkAddress>> results);

        /// <summary>
        /// Gets the snapshot for a specific transaction.
        /// </summary>
        /// <param name="transactionId">The transaction ID.</param>
        /// <returns>The snapshot.</returns>
        IReadOnlyDictionary<TLinkAddress, Link<TLinkAddress>> GetSnapshot(long transactionId);
    }
}
