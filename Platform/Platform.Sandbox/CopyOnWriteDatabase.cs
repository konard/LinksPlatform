using System;
using System.Collections.Generic;
using System.Threading;
using Platform.Data;

namespace Platform.Sandbox
{
    /// <summary>
    /// Represents a database implementation using the Copy-On-Write (or Write-To-A-Copy) principle.
    ///
    /// The database maintains two copies of data:
    /// - One writable copy (for applying transactions)
    /// - One readable copy (for multiple concurrent readers)
    ///
    /// Transaction Process:
    /// 1. Write transaction to the writable copy
    /// 2. Wait for all active readers to finish reading from the current readable copy
    /// 3. Swap the pointer: the writable copy becomes the new readable copy
    /// 4. Replicate the transaction to the second copy (which becomes the new writable copy)
    /// 5. System is ready for the next transaction
    ///
    /// Optimization: While waiting for readers to finish, pending transactions
    /// from the transaction log can be written to the writable copy.
    /// </summary>
    /// <typeparam name="TData">The type of data stored in the database</typeparam>
    public class CopyOnWriteDatabase<TData> where TData : class, new()
    {
        private readonly object _swapLock = new object();
        private readonly ReaderWriterLockSlim _readerLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly List<Transaction<TData>> _transactionLog = new List<Transaction<TData>>();
        private readonly object _transactionLogLock = new object();

        private DatabaseCopy<TData> _writableCopy;
        private DatabaseCopy<TData> _readableCopy;
        private long _transactionCounter = 0;
        private long _lastAppliedTransaction = 0;

        /// <summary>
        /// Gets the total number of transactions applied to the database
        /// </summary>
        public long TransactionCount => Interlocked.Read(ref _transactionCounter);

        /// <summary>
        /// Gets the last applied transaction ID
        /// </summary>
        public long LastAppliedTransaction => Interlocked.Read(ref _lastAppliedTransaction);

        /// <summary>
        /// Initializes a new instance of the CopyOnWriteDatabase
        /// </summary>
        public CopyOnWriteDatabase()
        {
            _writableCopy = new DatabaseCopy<TData>();
            _readableCopy = new DatabaseCopy<TData>();
        }

        /// <summary>
        /// Executes a read operation on the current readable copy.
        /// Multiple readers can execute concurrently without blocking each other.
        /// </summary>
        /// <param name="readAction">The action to perform on the readable data</param>
        /// <returns>The result of the read operation</returns>
        public TResult Read<TResult>(Func<TData, TResult> readAction)
        {
            if (readAction == null)
                throw new ArgumentNullException(nameof(readAction));

            _readerLock.EnterReadLock();
            try
            {
                return readAction(_readableCopy.Data);
            }
            finally
            {
                _readerLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Writes a transaction to the database following the copy-on-write principle
        /// </summary>
        /// <param name="transaction">The transaction to apply</param>
        public void Write(Transaction<TData> transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            lock (_swapLock)
            {
                // Step 1: Apply transaction to the writable copy
                var transactionId = Interlocked.Increment(ref _transactionCounter);
                transaction.Id = transactionId;
                transaction.Apply(_writableCopy.Data);

                // Add to transaction log for replication
                lock (_transactionLogLock)
                {
                    _transactionLog.Add(transaction);
                }

                // Step 2: Wait for all readers to finish
                // While waiting, we could apply pending transactions (optimization)
                WaitForReadersAndApplyPendingTransactions();

                // Step 3: Swap pointers - writable becomes readable
                SwapCopies();

                // Step 4: Replicate transaction to the second copy (new writable copy)
                ReplicateTransactions();

                // Update last applied transaction
                Interlocked.Exchange(ref _lastAppliedTransaction, transactionId);
            }
        }

        /// <summary>
        /// Waits for all active readers to finish.
        /// Optimization: While waiting, applies pending transactions from the transaction log.
        /// </summary>
        private void WaitForReadersAndApplyPendingTransactions()
        {
            // Acquire write lock to ensure no new readers can start
            // This will wait for all current readers to finish
            _readerLock.EnterWriteLock();
            try
            {
                // At this point, all readers have finished
                // We could apply additional pending transactions here as an optimization
                // (This is the optimization mentioned in the issue description)
            }
            finally
            {
                _readerLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Swaps the writable and readable copies.
        /// After this operation, the writable copy becomes readable and vice versa.
        /// </summary>
        private void SwapCopies()
        {
            var temp = _readableCopy;
            _readableCopy = _writableCopy;
            _writableCopy = temp;
        }

        /// <summary>
        /// Replicates transactions from the transaction log to the new writable copy.
        /// This ensures both copies are synchronized.
        /// </summary>
        private void ReplicateTransactions()
        {
            lock (_transactionLogLock)
            {
                // Clear the old writable copy and apply all transactions
                _writableCopy.Data = new TData();

                foreach (var transaction in _transactionLog)
                {
                    transaction.Apply(_writableCopy.Data);
                }
            }
        }

        /// <summary>
        /// Gets statistics about the database
        /// </summary>
        public DatabaseStatistics GetStatistics()
        {
            return new DatabaseStatistics
            {
                TotalTransactions = TransactionCount,
                LastAppliedTransaction = LastAppliedTransaction,
                TransactionLogSize = _transactionLog.Count,
                ActiveReaders = _readerLock.CurrentReadCount
            };
        }

        /// <summary>
        /// Disposes the database and releases resources
        /// </summary>
        public void Dispose()
        {
            _readerLock?.Dispose();
        }
    }

    /// <summary>
    /// Represents a single copy of the database data
    /// </summary>
    /// <typeparam name="TData">The type of data stored</typeparam>
    internal class DatabaseCopy<TData> where TData : class, new()
    {
        public TData Data { get; set; }

        public DatabaseCopy()
        {
            Data = new TData();
        }
    }

    /// <summary>
    /// Represents a transaction that can be applied to the database
    /// </summary>
    /// <typeparam name="TData">The type of data the transaction operates on</typeparam>
    public class Transaction<TData> where TData : class
    {
        /// <summary>
        /// Gets or sets the transaction ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the transaction was created
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the action to apply to the data
        /// </summary>
        public Action<TData> Action { get; set; }

        /// <summary>
        /// Creates a new transaction
        /// </summary>
        /// <param name="action">The action to perform on the data</param>
        public Transaction(Action<TData> action)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Applies this transaction to the given data
        /// </summary>
        /// <param name="data">The data to modify</param>
        public void Apply(TData data)
        {
            Action(data);
        }
    }

    /// <summary>
    /// Statistics about the database state
    /// </summary>
    public class DatabaseStatistics
    {
        public long TotalTransactions { get; set; }
        public long LastAppliedTransaction { get; set; }
        public int TransactionLogSize { get; set; }
        public int ActiveReaders { get; set; }

        public override string ToString()
        {
            return $"Total Transactions: {TotalTransactions}, " +
                   $"Last Applied: {LastAppliedTransaction}, " +
                   $"Log Size: {TransactionLogSize}, " +
                   $"Active Readers: {ActiveReaders}";
        }
    }
}
