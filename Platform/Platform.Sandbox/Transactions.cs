using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Platform.Data.Triplets;

namespace Platform.Sandbox
{
    public static class Transactions
    {
        private const string TransactionsFileName = "transactions.log";
        private const string TransactionsMapName = "Links.Net.Transactions";

        private enum TransactionItemType
        {
            Creation,
            UpdateOf,
            UpdateTo,
            Deletion
        }

        private struct TransactionItem
        {
            public long TransactionId;
            public DateTime DateTime;
            public TransactionItemType Type;
            public Link Source;
            public Link Linker;
            public Link Target;
        }

        private struct TransactionsState
        {
            public long LastTransactionId;
            public long LastTransactionOffset;
            public long LastTransactionItemsCount;
            public long LastHandledTransactionId;
            public long LastHandledTransactionOffset;
            public long LastHandledTransactionItemOffset;
            public long FileEndOffset;
            public long FileSizeInTransactionItems;
        }

        private static readonly long _basicTransactionsOffset = Marshal.SizeOf<TransactionsState>();
        private static readonly long _transactionItemSize = Marshal.SizeOf<TransactionItem>();

        private static long _currentFileSizeInBytes;
        private static MemoryMappedFile _log;
        private static MemoryMappedViewAccessor _logAccessor;
        private static TransactionsState _currentState;

        private static bool _transactionOpened;

        static Transactions()
        {
            OpenFile();

            var item = new TransactionItem
            {
                TransactionId = _currentState.LastTransactionId,
                DateTime = DateTime.UtcNow,
                Type = TransactionItemType.Creation,
                Source = Net.Link.Source,
                Linker = Net.Link.Linker,
                Target = Net.Link.Target,
            };

            EnsureFileSize();

            _logAccessor.Write(_basicTransactionsOffset, ref item);

            _logAccessor.Read(_basicTransactionsOffset, out item);

            CloseFile();
        }

        static void StartTransaction()
        {
            // Защита от накопления кучи транзакций, которые не совершили ни одной операции
            if (_currentState.LastTransactionItemsCount > 0)
            {
                _currentState.LastTransactionId++;
                _currentState.LastTransactionItemsCount = 0;
                _currentState.LastTransactionOffset = _currentState.FileEndOffset;
            }
            _transactionOpened = true;
        }

        static void CloseTransaction() => _transactionOpened = false;

        static void LoadState()
        {
            _logAccessor.Read(0, out _currentState);
            if (_currentState.FileEndOffset == 0)
            {
                _currentState.FileEndOffset = _basicTransactionsOffset;
            }
        }

        static void StoreState() => _logAccessor.Write(0, ref _currentState);

        static void EnsureFileSize()
        {
            var sizeInItems = _currentState.FileSizeInTransactionItems;
            if ((_currentState.FileEndOffset - _basicTransactionsOffset) / _transactionItemSize == sizeInItems)
            {
                if (sizeInItems < 16)
                {
                    sizeInItems = 16;
                }
                else
                {
                    sizeInItems *= 2;
                }
                var newSizeInBates = _basicTransactionsOffset + sizeInItems * _transactionItemSize;
                ReloadFile(newSizeInBates);
                _currentState.LastTransactionItemsCount = sizeInItems;
            }
        }

        static void ReloadFile(long sizeInBytes = 0)
        {
            CloseFile();
            OpenFile(sizeInBytes);
        }

        static void CloseFile()
        {
            StoreState();
            _logAccessor.Flush();
            _logAccessor.Dispose();
            _log.Dispose();
        }

        static void OpenFile(long sizeInBytes = 0)
        {
            if (sizeInBytes < _basicTransactionsOffset)
            {
                sizeInBytes = _basicTransactionsOffset;
            }
            long savedSizeInBytes = 0;
            if (File.Exists(TransactionsFileName))
            {
                var fileInfo = new FileInfo(TransactionsFileName);
                savedSizeInBytes = fileInfo.Length;
            }
            if (sizeInBytes < savedSizeInBytes)
            {
                sizeInBytes = savedSizeInBytes;
            }
            _log = MemoryMappedFile.CreateFromFile(TransactionsFileName, FileMode.OpenOrCreate, TransactionsMapName, sizeInBytes);
            _logAccessor = _log.CreateViewAccessor();
            LoadState();
            _currentFileSizeInBytes = sizeInBytes;
        }

        /// <summary>
        /// Compacts the transaction log by removing redundant transitions.
        /// For each link, only the final state (latest transaction) is kept.
        /// Timestamps of the last action are preserved.
        /// </summary>
        /// <remarks>
        /// This method reduces the log size by removing intermediate states
        /// (e.g., if a link is created and deleted 20 times, only the final state remains).
        /// </remarks>
        public static void CompactLog()
        {
            OpenFile();

            // Read all transaction items from the log
            var items = new System.Collections.Generic.List<TransactionItem>();
            var currentOffset = _basicTransactionsOffset;

            while (currentOffset < _currentState.FileEndOffset)
            {
                _logAccessor.Read(currentOffset, out TransactionItem item);
                items.Add(item);
                currentOffset += _transactionItemSize;
            }

            // Group by link identity (Source, Linker, Target) and keep only the latest transaction for each
            var compactedItems = new System.Collections.Generic.Dictionary<string, TransactionItem>();

            foreach (var item in items)
            {
                // Create a unique key for each link based on its Source, Linker, and Target
                // Using hash codes since Link objects may not be directly comparable after reload
                string key = $"{item.Source.GetHashCode()}_{item.Linker.GetHashCode()}_{item.Target.GetHashCode()}";

                // If we haven't seen this link before, or this transaction is newer, keep it
                if (!compactedItems.ContainsKey(key) ||
                    compactedItems[key].DateTime < item.DateTime ||
                    (compactedItems[key].DateTime == item.DateTime && compactedItems[key].TransactionId < item.TransactionId))
                {
                    compactedItems[key] = item;
                }
            }

            // Write compacted items back to the log
            var compactedList = new System.Collections.Generic.List<TransactionItem>(compactedItems.Values);

            // Sort by transaction ID to maintain chronological order
            compactedList.Sort((a, b) => a.TransactionId.CompareTo(b.TransactionId));

            // Reset state for writing
            _currentState.FileEndOffset = _basicTransactionsOffset;
            _currentState.LastTransactionItemsCount = 0;

            // Write compacted items
            currentOffset = _basicTransactionsOffset;
            for (int i = 0; i < compactedList.Count; i++)
            {
                EnsureFileSize();
                var item = compactedList[i];
                _logAccessor.Write(currentOffset, ref item);
                currentOffset += _transactionItemSize;
                _currentState.FileEndOffset = currentOffset;
            }

            // Update last transaction information
            if (compactedList.Count > 0)
            {
                var lastItem = compactedList[compactedList.Count - 1];
                _currentState.LastTransactionId = lastItem.TransactionId;
                _currentState.LastTransactionOffset = currentOffset - _transactionItemSize;
            }

            StoreState();
            CloseFile();
        }

        public static void Run()
        {
        }
    }
}
