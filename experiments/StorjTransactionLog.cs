using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Platform.Data.Transactions.Storj
{
    /// <summary>
    /// Interface for distributed transaction log storage
    /// </summary>
    public interface IDistributedTransactionLog
    {
        Task AppendTransactionAsync(byte[] transactionData);
        Task<byte[]> ReadTransactionAsync(long transactionId);
        Task<IEnumerable<byte[]>> ReadTransactionsAsync(long startId, long count);
        Task FlushAsync();
    }

    /// <summary>
    /// StorJ-based distributed transaction log storage implementation
    /// This implementation uses StorJ (S3-compatible) decentralized cloud storage
    /// to store transaction logs in a distributed manner.
    /// </summary>
    public class StorjTransactionLog : IDistributedTransactionLog, IDisposable
    {
        private readonly AmazonS3Client _s3Client;
        private readonly string _bucketName;
        private readonly string _logPrefix;
        private long _currentTransactionId;
        private readonly object _lock = new object();

        /// <summary>
        /// Configuration for StorJ connection
        /// </summary>
        public class StorjConfig
        {
            public string AccessKey { get; set; }
            public string SecretKey { get; set; }
            public string BucketName { get; set; }
            public string LogPrefix { get; set; } = "transaction-log";
            public string ServiceUrl { get; set; } = "https://gateway.storjshare.io";
        }

        /// <summary>
        /// Initialize StorJ transaction log with configuration
        /// </summary>
        public StorjTransactionLog(StorjConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrEmpty(config.AccessKey))
                throw new ArgumentException("AccessKey is required", nameof(config));
            if (string.IsNullOrEmpty(config.SecretKey))
                throw new ArgumentException("SecretKey is required", nameof(config));
            if (string.IsNullOrEmpty(config.BucketName))
                throw new ArgumentException("BucketName is required", nameof(config));

            _bucketName = config.BucketName;
            _logPrefix = config.LogPrefix;

            var s3Config = new AmazonS3Config
            {
                ServiceURL = config.ServiceUrl,
                ForcePathStyle = true
            };

            _s3Client = new AmazonS3Client(
                config.AccessKey,
                config.SecretKey,
                s3Config
            );

            InitializeAsync().Wait();
        }

        private async Task InitializeAsync()
        {
            try
            {
                // Check if bucket exists, create if not
                var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName);
                if (!bucketExists)
                {
                    await _s3Client.PutBucketAsync(_bucketName);
                }

                // Read the last transaction ID from metadata
                _currentTransactionId = await GetLastTransactionIdAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize StorJ transaction log", ex);
            }
        }

        private async Task<long> GetLastTransactionIdAsync()
        {
            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = _logPrefix,
                    MaxKeys = 1
                };

                var response = await _s3Client.ListObjectsV2Async(request);

                if (response.S3Objects.Count == 0)
                    return 0;

                // Parse the last transaction ID from the object key
                var lastKey = response.S3Objects[response.S3Objects.Count - 1].Key;
                var parts = lastKey.Split('/');
                if (parts.Length > 0 && long.TryParse(parts[parts.Length - 1].Replace(".bin", ""), out long id))
                {
                    return id;
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Append a transaction to the distributed log
        /// </summary>
        public async Task AppendTransactionAsync(byte[] transactionData)
        {
            if (transactionData == null || transactionData.Length == 0)
                throw new ArgumentException("Transaction data cannot be null or empty", nameof(transactionData));

            long transactionId;
            lock (_lock)
            {
                transactionId = ++_currentTransactionId;
            }

            var key = GetTransactionKey(transactionId);

            using (var stream = new MemoryStream(transactionData))
            {
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = "application/octet-stream",
                    Metadata =
                    {
                        ["transaction-id"] = transactionId.ToString(),
                        ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
                    }
                };

                await _s3Client.PutObjectAsync(request);
            }
        }

        /// <summary>
        /// Read a specific transaction from the distributed log
        /// </summary>
        public async Task<byte[]> ReadTransactionAsync(long transactionId)
        {
            if (transactionId <= 0)
                throw new ArgumentException("Transaction ID must be positive", nameof(transactionId));

            var key = GetTransactionKey(transactionId);

            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                using (var response = await _s3Client.GetObjectAsync(request))
                using (var memoryStream = new MemoryStream())
                {
                    await response.ResponseStream.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Transaction {transactionId} not found");
            }
        }

        /// <summary>
        /// Read multiple transactions from the distributed log
        /// </summary>
        public async Task<IEnumerable<byte[]>> ReadTransactionsAsync(long startId, long count)
        {
            if (startId <= 0)
                throw new ArgumentException("Start ID must be positive", nameof(startId));
            if (count <= 0)
                throw new ArgumentException("Count must be positive", nameof(count));

            var transactions = new List<byte[]>();

            for (long i = startId; i < startId + count; i++)
            {
                try
                {
                    var data = await ReadTransactionAsync(i);
                    transactions.Add(data);
                }
                catch (KeyNotFoundException)
                {
                    // Transaction doesn't exist, stop reading
                    break;
                }
            }

            return transactions;
        }

        /// <summary>
        /// Flush any pending operations (no-op for StorJ as writes are immediate)
        /// </summary>
        public Task FlushAsync()
        {
            // StorJ writes are immediate, no buffering needed
            return Task.CompletedTask;
        }

        private string GetTransactionKey(long transactionId)
        {
            // Organize transactions into folders by ranges for better organization
            // e.g., transaction-log/00000000/00001234.bin
            var folder = (transactionId / 10000).ToString("D8");
            var file = transactionId.ToString("D8") + ".bin";
            return $"{_logPrefix}/{folder}/{file}";
        }

        public void Dispose()
        {
            _s3Client?.Dispose();
        }
    }

    /// <summary>
    /// Transaction log entry structure matching the original Transactions.cs
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TransactionLogEntry
    {
        public long TransactionId;
        public long Timestamp; // Unix timestamp
        public int Type; // TransactionItemType
        public long SourceLinkId;
        public long LinkerLinkId;
        public long TargetLinkId;

        public byte[] ToBytes()
        {
            int size = Marshal.SizeOf<TransactionLogEntry>();
            byte[] buffer = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(this, ptr, false);
                Marshal.Copy(ptr, buffer, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return buffer;
        }

        public static TransactionLogEntry FromBytes(byte[] data)
        {
            if (data == null || data.Length != Marshal.SizeOf<TransactionLogEntry>())
                throw new ArgumentException("Invalid data length", nameof(data));

            IntPtr ptr = Marshal.AllocHGlobal(data.Length);
            try
            {
                Marshal.Copy(data, 0, ptr, data.Length);
                return Marshal.PtrToStructure<TransactionLogEntry>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
