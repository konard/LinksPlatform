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

        public enum TransactionItemType
        {
            Creation,
            UpdateOf,
            UpdateTo,
            Deletion
        }

        public struct TransactionItem
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
        /// Represents an event with its time and position for lazy event loop iteration.
        /// </summary>
        public struct LogEvent
        {
            public DateTime Time;
            public long Position;
            public TransactionItem Item;
        }

        /// <summary>
        /// Gets the next log event from the specified position (lazy event loop).
        /// </summary>
        /// <param name="position">The current position in the log.</param>
        /// <returns>The next log event if available, null otherwise.</returns>
        public static LogEvent? GetNextEvent(long position)
        {
            OpenFile();

            if (position < _basicTransactionsOffset || position >= _currentState.FileEndOffset)
            {
                CloseFile();
                return null;
            }

            TransactionItem item;
            _logAccessor.Read(position, out item);

            CloseFile();

            return new LogEvent
            {
                Time = item.DateTime,
                Position = position,
                Item = item
            };
        }

        /// <summary>
        /// Finds a log event by time using binary search.
        /// </summary>
        /// <param name="targetTime">The target time to search for.</param>
        /// <returns>The log event closest to the target time, or null if log is empty.</returns>
        public static LogEvent? FindEventByTime(DateTime targetTime)
        {
            OpenFile();

            long start = _basicTransactionsOffset;
            long end = _currentState.FileEndOffset;

            if (start >= end)
            {
                CloseFile();
                return null;
            }

            long itemCount = (end - start) / _transactionItemSize;
            long left = 0;
            long right = itemCount - 1;
            long resultPosition = start;

            while (left <= right)
            {
                long mid = left + (right - left) / 2;
                long position = start + mid * _transactionItemSize;

                TransactionItem item;
                _logAccessor.Read(position, out item);

                if (item.DateTime == targetTime)
                {
                    resultPosition = position;
                    break;
                }
                else if (item.DateTime < targetTime)
                {
                    resultPosition = position;
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            TransactionItem resultItem;
            _logAccessor.Read(resultPosition, out resultItem);

            CloseFile();

            return new LogEvent
            {
                Time = resultItem.DateTime,
                Position = resultPosition,
                Item = resultItem
            };
        }

        /// <summary>
        /// Gets the next event position in the log.
        /// </summary>
        /// <param name="currentPosition">The current position.</param>
        /// <returns>The next position, or -1 if there is no next event.</returns>
        public static long GetNextEventPosition(long currentPosition)
        {
            long nextPosition = currentPosition + _transactionItemSize;

            OpenFile();
            bool hasNext = nextPosition < _currentState.FileEndOffset;
            CloseFile();

            return hasNext ? nextPosition : -1;
        }

        public static void Run()
        {
        }
    }
}
