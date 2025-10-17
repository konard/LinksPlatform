using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Platform.Data.Doublets.DistributedTriggers
{
    /// <summary>
    /// Coordinates the distributed execution of triggers across multiple workers.
    /// Implements the complete map-reduce workflow:
    /// 1. Broadcasts transaction events to all workers
    /// 2. Creates snapshots after each event
    /// 3. Collects and merges results from workers
    /// 4. Writes merged results back to transaction log
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public class DistributedTriggersCoordinator<TLinkAddress>
    {
        private readonly TransactionLogBroadcaster<TLinkAddress> _broadcaster;
        private readonly SnapshotManager<TLinkAddress> _snapshotManager;
        private readonly ResultsMerger<TLinkAddress> _resultsMerger;
        private readonly List<TriggerWorker<TLinkAddress>> _workers;
        private readonly object _workersLock = new object();
        private bool _isRunning;

        public DistributedTriggersCoordinator(
            int workerCount,
            ITransactionLogWriter<TLinkAddress> transactionLogWriter)
        {
            if (workerCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(workerCount), "Worker count must be positive.");
            }

            _broadcaster = new TransactionLogBroadcaster<TLinkAddress>();
            _snapshotManager = new SnapshotManager<TLinkAddress>();
            _resultsMerger = new ResultsMerger<TLinkAddress>(
                workerCount,
                _snapshotManager,
                transactionLogWriter ?? throw new ArgumentNullException(nameof(transactionLogWriter)));

            _workers = new List<TriggerWorker<TLinkAddress>>(workerCount);

            // Initialize workers
            for (int i = 0; i < workerCount; i++)
            {
                var worker = new TriggerWorker<TLinkAddress>(i, workerCount, _resultsMerger);
                _workers.Add(worker);
                _broadcaster.Subscribe(worker);
            }

            _isRunning = true;
        }

        /// <summary>
        /// Gets the number of workers.
        /// </summary>
        public int WorkerCount => _workers.Count;

        /// <summary>
        /// Registers a trigger with the appropriate worker based on its partition key.
        /// </summary>
        /// <param name="trigger">The trigger to register.</param>
        public void RegisterTrigger(IDistributedTrigger<TLinkAddress> trigger)
        {
            if (trigger == null)
            {
                throw new ArgumentNullException(nameof(trigger));
            }

            lock (_workersLock)
            {
                var workerId = Math.Abs(trigger.PartitionKey) % _workers.Count;
                _workers[workerId].RegisterTrigger(trigger);
            }
        }

        /// <summary>
        /// Unregisters a trigger from all workers.
        /// </summary>
        /// <param name="triggerId">The ID of the trigger to unregister.</param>
        /// <returns>True if the trigger was found and unregistered; otherwise, false.</returns>
        public bool UnregisterTrigger(Guid triggerId)
        {
            lock (_workersLock)
            {
                bool removed = false;
                foreach (var worker in _workers)
                {
                    if (worker.UnregisterTrigger(triggerId))
                    {
                        removed = true;
                    }
                }
                return removed;
            }
        }

        /// <summary>
        /// Processes a transaction event through the distributed triggers system.
        /// This implements the complete workflow:
        /// 1. Creates a snapshot by applying the event operations
        /// 2. Broadcasts the event to all workers (map phase)
        /// 3. Workers execute their triggers in parallel
        /// 4. Results are merged back to transaction log (reduce phase)
        /// </summary>
        /// <param name="event">The transaction event to process.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the processing operation.</returns>
        public async Task ProcessTransactionEventAsync(
            TransactionEvent<TLinkAddress> @event,
            CancellationToken cancellationToken = default)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            if (!_isRunning)
            {
                throw new InvalidOperationException("Coordinator is not running.");
            }

            // Step 1: Create snapshot after the event
            _snapshotManager.CreateSnapshot(@event.Operations);

            // Step 2: Broadcast event to all workers (map phase)
            // Workers will execute their triggers in parallel and send results to the merger
            await _broadcaster.BroadcastAsync(@event, cancellationToken).ConfigureAwait(false);

            // Step 3 & 4: Results collection and merging happens automatically in ResultsMerger
            // as workers complete their processing
        }

        /// <summary>
        /// Initializes the snapshot from an existing links store.
        /// Should be called before processing any events.
        /// </summary>
        /// <param name="links">The initial links.</param>
        public void InitializeSnapshot(IEnumerable<Link<TLinkAddress>> links)
        {
            _snapshotManager.InitializeFromLinks(links);
        }

        /// <summary>
        /// Gets statistics about the coordinator.
        /// </summary>
        public CoordinatorStatistics GetStatistics()
        {
            lock (_workersLock)
            {
                return new CoordinatorStatistics
                {
                    WorkerCount = _workers.Count,
                    TotalRegisteredTriggers = _workers.Sum(w => w.TriggerCount),
                    SnapshotVersion = _snapshotManager.SnapshotVersion,
                    SnapshotSize = _snapshotManager.Count,
                    IsRunning = _isRunning
                };
            }
        }

        /// <summary>
        /// Stops the coordinator and cleans up resources.
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            lock (_workersLock)
            {
                foreach (var worker in _workers)
                {
                    _broadcaster.Unsubscribe(worker);
                }
            }
        }

        /// <summary>
        /// Gets a specific worker by ID.
        /// </summary>
        public TriggerWorker<TLinkAddress> GetWorker(int workerId)
        {
            lock (_workersLock)
            {
                if (workerId < 0 || workerId >= _workers.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(workerId));
                }
                return _workers[workerId];
            }
        }
    }

    /// <summary>
    /// Statistics about the distributed triggers coordinator.
    /// </summary>
    public class CoordinatorStatistics
    {
        public int WorkerCount { get; set; }
        public int TotalRegisteredTriggers { get; set; }
        public long SnapshotVersion { get; set; }
        public int SnapshotSize { get; set; }
        public bool IsRunning { get; set; }

        public override string ToString()
        {
            return $"Workers: {WorkerCount}, Triggers: {TotalRegisteredTriggers}, " +
                   $"Snapshot: v{SnapshotVersion} ({SnapshotSize} links), Running: {IsRunning}";
        }
    }
}
