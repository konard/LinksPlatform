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
            public long LinkIndex;
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
                LinkIndex = 0, // Initial link index
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

        public static void RecordCreation(long linkIndex, Link source, Link linker, Link target)
        {
            if (!_transactionOpened)
            {
                StartTransaction();
            }

            var item = new TransactionItem
            {
                TransactionId = _currentState.LastTransactionId,
                DateTime = DateTime.UtcNow,
                Type = TransactionItemType.Creation,
                LinkIndex = linkIndex,
                Source = source,
                Linker = linker,
                Target = target,
            };

            EnsureFileSize();
            _logAccessor.Write(_currentState.FileEndOffset, ref item);
            _currentState.FileEndOffset += _transactionItemSize;
            _currentState.LastTransactionItemsCount++;
        }

        public static void RecordUpdate(long linkIndex, Link oldSource, Link oldLinker, Link oldTarget, Link newSource, Link newLinker, Link newTarget)
        {
            if (!_transactionOpened)
            {
                StartTransaction();
            }

            // Record the "UpdateOf" (before state)
            var itemOf = new TransactionItem
            {
                TransactionId = _currentState.LastTransactionId,
                DateTime = DateTime.UtcNow,
                Type = TransactionItemType.UpdateOf,
                LinkIndex = linkIndex,
                Source = oldSource,
                Linker = oldLinker,
                Target = oldTarget,
            };

            EnsureFileSize();
            _logAccessor.Write(_currentState.FileEndOffset, ref itemOf);
            _currentState.FileEndOffset += _transactionItemSize;
            _currentState.LastTransactionItemsCount++;

            // Record the "UpdateTo" (after state)
            var itemTo = new TransactionItem
            {
                TransactionId = _currentState.LastTransactionId,
                DateTime = DateTime.UtcNow,
                Type = TransactionItemType.UpdateTo,
                LinkIndex = linkIndex,
                Source = newSource,
                Linker = newLinker,
                Target = newTarget,
            };

            EnsureFileSize();
            _logAccessor.Write(_currentState.FileEndOffset, ref itemTo);
            _currentState.FileEndOffset += _transactionItemSize;
            _currentState.LastTransactionItemsCount++;
        }

        public static void RecordDeletion(long linkIndex, Link source, Link linker, Link target)
        {
            if (!_transactionOpened)
            {
                StartTransaction();
            }

            var item = new TransactionItem
            {
                TransactionId = _currentState.LastTransactionId,
                DateTime = DateTime.UtcNow,
                Type = TransactionItemType.Deletion,
                LinkIndex = linkIndex,
                Source = source,
                Linker = linker,
                Target = target,
            };

            EnsureFileSize();
            _logAccessor.Write(_currentState.FileEndOffset, ref item);
            _currentState.FileEndOffset += _transactionItemSize;
            _currentState.LastTransactionItemsCount++;
        }

        public static void RevertTransaction(long transactionId)
        {
            // Find the transaction offset
            long offset = _basicTransactionsOffset;
            long targetTransactionOffset = -1;
            long itemsToRevert = 0;

            while (offset < _currentState.FileEndOffset)
            {
                TransactionItem item;
                _logAccessor.Read(offset, out item);

                if (item.TransactionId == transactionId)
                {
                    if (targetTransactionOffset == -1)
                    {
                        targetTransactionOffset = offset;
                    }
                    itemsToRevert++;
                }
                else if (targetTransactionOffset != -1)
                {
                    // We've passed the target transaction
                    break;
                }

                offset += _transactionItemSize;
            }

            if (targetTransactionOffset == -1)
            {
                throw new InvalidOperationException($"Transaction {transactionId} not found.");
            }

            // Revert items in reverse order
            var itemsToRevertList = new System.Collections.Generic.List<TransactionItem>();
            offset = targetTransactionOffset;
            for (long i = 0; i < itemsToRevert; i++)
            {
                TransactionItem item;
                _logAccessor.Read(offset, out item);
                itemsToRevertList.Add(item);
                offset += _transactionItemSize;
            }

            // Reverse the list to process in reverse order
            itemsToRevertList.Reverse();

            foreach (var item in itemsToRevertList)
            {
                RevertTransactionItem(item);
            }
        }

        private static void RevertTransactionItem(TransactionItem item)
        {
            // This is a placeholder implementation that demonstrates the concept.
            // In a real implementation, this would interact with the actual link storage
            // to restore the link at the specified index.

            switch (item.Type)
            {
                case TransactionItemType.Creation:
                    // To revert a creation, delete the link at LinkIndex
                    // DeleteLinkAtIndex(item.LinkIndex);
                    break;

                case TransactionItemType.UpdateOf:
                    // UpdateOf is followed by UpdateTo, skip it here
                    break;

                case TransactionItemType.UpdateTo:
                    // To revert an update, restore the previous state
                    // We need to look back to find the corresponding UpdateOf
                    // RestoreLinkAtIndex(item.LinkIndex, previousState);
                    break;

                case TransactionItemType.Deletion:
                    // To revert a deletion, recreate the link at the specified index
                    // CreateLinkAtIndex(item.LinkIndex, item.Source, item.Linker, item.Target);
                    break;
            }
        }

        public static void Run()
        {
        }
    }
}
