using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Platform.Data.Doublets.DistributedTriggers
{
    /// <summary>
    /// Implements the reduce phase of the map-reduce pattern.
    /// Collects results from all workers and merges them back to the transaction log.
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public class ResultsMerger<TLinkAddress> : IResultCollector<TLinkAddress>
    {
        private readonly SnapshotManager<TLinkAddress> _snapshotManager;
        private readonly Dictionary<long, TransactionResults> _pendingResults;
        private readonly object _resultsLock = new object();
        private readonly int _expectedWorkerCount;
        private readonly ITransactionLogWriter<TLinkAddress> _transactionLogWriter;

        public ResultsMerger(
            int expectedWorkerCount,
            SnapshotManager<TLinkAddress> snapshotManager,
            ITransactionLogWriter<TLinkAddress> transactionLogWriter)
        {
            if (expectedWorkerCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expectedWorkerCount));
            }

            _expectedWorkerCount = expectedWorkerCount;
            _snapshotManager = snapshotManager ?? throw new ArgumentNullException(nameof(snapshotManager));
            _transactionLogWriter = transactionLogWriter ?? throw new ArgumentNullException(nameof(transactionLogWriter));
            _pendingResults = new Dictionary<long, TransactionResults>();
        }

        /// <summary>
        /// Collects results from a worker.
        /// When all workers have reported results for a transaction, merges them.
        /// </summary>
        public void CollectResults(int workerId, long transactionId, IEnumerable<TransactionOperation<TLinkAddress>> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            lock (_resultsLock)
            {
                if (!_pendingResults.ContainsKey(transactionId))
                {
                    _pendingResults[transactionId] = new TransactionResults(_expectedWorkerCount);
                }

                var transactionResults = _pendingResults[transactionId];
                transactionResults.AddWorkerResults(workerId, results);

                // Check if all workers have completed
                if (transactionResults.IsComplete)
                {
                    // Merge results
                    var mergedResults = MergeResults(transactionResults.GetAllResults());

                    // Write to transaction log
                    if (mergedResults.Any())
                    {
                        _transactionLogWriter.WriteOperations(transactionId, mergedResults);
                    }

                    // Clean up
                    _pendingResults.Remove(transactionId);
                }
            }
        }

        /// <summary>
        /// Gets the snapshot for a specific transaction.
        /// </summary>
        public IReadOnlyDictionary<TLinkAddress, Link<TLinkAddress>> GetSnapshot(long transactionId)
        {
            // In this implementation, we use the current snapshot
            // In a more sophisticated implementation, we might keep historical snapshots
            return _snapshotManager.GetCurrentSnapshot();
        }

        /// <summary>
        /// Merges results from multiple workers, resolving conflicts and deduplicating operations.
        /// </summary>
        private List<TransactionOperation<TLinkAddress>> MergeResults(
            IEnumerable<IEnumerable<TransactionOperation<TLinkAddress>>> workerResults)
        {
            var merged = new List<TransactionOperation<TLinkAddress>>();
            var operationsByLink = new Dictionary<TLinkAddress, List<TransactionOperation<TLinkAddress>>>();

            // Group operations by link address
            foreach (var workerResult in workerResults)
            {
                foreach (var operation in workerResult)
                {
                    if (!operationsByLink.ContainsKey(operation.LinkAddress))
                    {
                        operationsByLink[operation.LinkAddress] = new List<TransactionOperation<TLinkAddress>>();
                    }
                    operationsByLink[operation.LinkAddress].Add(operation);
                }
            }

            // Merge operations for each link
            foreach (var kvp in operationsByLink)
            {
                var operations = kvp.Value;
                if (operations.Count == 1)
                {
                    // No conflict, add as-is
                    merged.Add(operations[0]);
                }
                else
                {
                    // Multiple operations on the same link - apply conflict resolution
                    var resolvedOperation = ResolveConflicts(operations);
                    if (resolvedOperation != null)
                    {
                        merged.Add(resolvedOperation);
                    }
                }
            }

            return merged;
        }

        /// <summary>
        /// Resolves conflicts when multiple workers produce operations for the same link.
        /// Current strategy: last write wins (can be customized).
        /// </summary>
        private TransactionOperation<TLinkAddress> ResolveConflicts(
            List<TransactionOperation<TLinkAddress>> conflictingOperations)
        {
            if (conflictingOperations == null || conflictingOperations.Count == 0)
            {
                return null;
            }

            // Simple strategy: return the last operation
            // More sophisticated strategies could consider operation types, timestamps, etc.
            return conflictingOperations.Last();
        }

        /// <summary>
        /// Helper class to track results from all workers for a transaction.
        /// </summary>
        private class TransactionResults
        {
            private readonly Dictionary<int, List<TransactionOperation<TLinkAddress>>> _workerResults;
            private readonly int _expectedWorkerCount;

            public TransactionResults(int expectedWorkerCount)
            {
                _expectedWorkerCount = expectedWorkerCount;
                _workerResults = new Dictionary<int, List<TransactionOperation<TLinkAddress>>>();
            }

            public void AddWorkerResults(int workerId, IEnumerable<TransactionOperation<TLinkAddress>> results)
            {
                if (!_workerResults.ContainsKey(workerId))
                {
                    _workerResults[workerId] = new List<TransactionOperation<TLinkAddress>>();
                }
                _workerResults[workerId].AddRange(results);
            }

            public bool IsComplete => _workerResults.Count == _expectedWorkerCount;

            public IEnumerable<IEnumerable<TransactionOperation<TLinkAddress>>> GetAllResults()
            {
                return _workerResults.Values;
            }
        }
    }

    /// <summary>
    /// Interface for writing operations back to the transaction log.
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    public interface ITransactionLogWriter<TLinkAddress>
    {
        /// <summary>
        /// Writes operations to the transaction log.
        /// </summary>
        /// <param name="transactionId">The transaction ID.</param>
        /// <param name="operations">The operations to write.</param>
        void WriteOperations(long transactionId, IEnumerable<TransactionOperation<TLinkAddress>> operations);
    }
}
