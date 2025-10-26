using System;
using System.Threading.Tasks;
using Platform.Data.Transactions.Storj;

namespace Platform.Examples.Storj
{
    /// <summary>
    /// Example demonstrating how to use StorJ for distributed transaction log storage
    /// This example shows basic operations: writing and reading transaction logs
    /// </summary>
    public class StorjTransactionLogExample
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("StorJ Distributed Transaction Log Example");
            Console.WriteLine("==========================================\n");

            // Configuration - these would normally come from environment variables or config file
            var config = new StorjTransactionLog.StorjConfig
            {
                AccessKey = Environment.GetEnvironmentVariable("STORJ_ACCESS_KEY") ?? "your-access-key",
                SecretKey = Environment.GetEnvironmentVariable("STORJ_SECRET_KEY") ?? "your-secret-key",
                BucketName = Environment.GetEnvironmentVariable("STORJ_BUCKET") ?? "links-platform-transactions",
                LogPrefix = "transaction-log",
                ServiceUrl = "https://gateway.storjshare.io"
            };

            try
            {
                // Initialize the StorJ transaction log
                Console.WriteLine("Initializing StorJ transaction log...");
                using (var transactionLog = new StorjTransactionLog(config))
                {
                    Console.WriteLine("Connected to StorJ successfully!\n");

                    // Example 1: Write a transaction
                    Console.WriteLine("Example 1: Writing transactions to StorJ");
                    Console.WriteLine("-----------------------------------------");

                    var transaction1 = new TransactionLogEntry
                    {
                        TransactionId = 1,
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        Type = 0, // Creation
                        SourceLinkId = 100,
                        LinkerLinkId = 200,
                        TargetLinkId = 300
                    };

                    Console.WriteLine($"Writing transaction: {transaction1.TransactionId}");
                    await transactionLog.AppendTransactionAsync(transaction1.ToBytes());
                    Console.WriteLine("Transaction written successfully!\n");

                    // Example 2: Write multiple transactions
                    Console.WriteLine("Writing multiple transactions...");
                    for (int i = 2; i <= 5; i++)
                    {
                        var transaction = new TransactionLogEntry
                        {
                            TransactionId = i,
                            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                            Type = i % 2, // Alternate between Creation and UpdateOf
                            SourceLinkId = 100 + i,
                            LinkerLinkId = 200 + i,
                            TargetLinkId = 300 + i
                        };

                        await transactionLog.AppendTransactionAsync(transaction.ToBytes());
                        Console.WriteLine($"  Transaction {i} written");
                    }
                    Console.WriteLine("All transactions written!\n");

                    // Example 3: Read a specific transaction
                    Console.WriteLine("Example 2: Reading a specific transaction");
                    Console.WriteLine("-----------------------------------------");

                    var readData = await transactionLog.ReadTransactionAsync(1);
                    var readTransaction = TransactionLogEntry.FromBytes(readData);

                    Console.WriteLine($"Transaction ID: {readTransaction.TransactionId}");
                    Console.WriteLine($"Type: {readTransaction.Type}");
                    Console.WriteLine($"Source Link: {readTransaction.SourceLinkId}");
                    Console.WriteLine($"Linker Link: {readTransaction.LinkerLinkId}");
                    Console.WriteLine($"Target Link: {readTransaction.TargetLinkId}");
                    Console.WriteLine($"Timestamp: {DateTimeOffset.FromUnixTimeSeconds(readTransaction.Timestamp)}\n");

                    // Example 4: Read multiple transactions
                    Console.WriteLine("Example 3: Reading multiple transactions");
                    Console.WriteLine("----------------------------------------");

                    var transactions = await transactionLog.ReadTransactionsAsync(1, 5);
                    int count = 0;
                    foreach (var txData in transactions)
                    {
                        var tx = TransactionLogEntry.FromBytes(txData);
                        Console.WriteLine($"  Transaction {++count}: ID={tx.TransactionId}, Type={tx.Type}");
                    }

                    Console.WriteLine("\nExample completed successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"\nInner exception: {ex.InnerException.Message}");
                }
            }
        }

        /// <summary>
        /// Example showing how to integrate StorJ transaction log with the existing local transaction system
        /// </summary>
        public static async Task HybridExample()
        {
            Console.WriteLine("\nHybrid Local + StorJ Transaction Log Example");
            Console.WriteLine("=============================================\n");

            var config = new StorjTransactionLog.StorjConfig
            {
                AccessKey = Environment.GetEnvironmentVariable("STORJ_ACCESS_KEY") ?? "your-access-key",
                SecretKey = Environment.GetEnvironmentVariable("STORJ_SECRET_KEY") ?? "your-secret-key",
                BucketName = "links-platform-transactions",
                LogPrefix = "transaction-log"
            };

            using (var storjLog = new StorjTransactionLog(config))
            {
                Console.WriteLine("Strategy: Write to local log first for speed,");
                Console.WriteLine("          then replicate to StorJ for durability and distribution.");
                Console.WriteLine();

                // In a real implementation, you would:
                // 1. Write to local memory-mapped file (fast)
                // 2. Asynchronously replicate to StorJ (durable, distributed)
                // 3. Use StorJ for disaster recovery and cross-node replication

                var transaction = new TransactionLogEntry
                {
                    TransactionId = 1000,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Type = 0,
                    SourceLinkId = 1001,
                    LinkerLinkId = 1002,
                    TargetLinkId = 1003
                };

                Console.WriteLine("1. Writing to local transaction log (fast)...");
                // Platform.Sandbox.Transactions would handle this
                Console.WriteLine("   Local write completed");

                Console.WriteLine("2. Replicating to StorJ (distributed)...");
                await storjLog.AppendTransactionAsync(transaction.ToBytes());
                Console.WriteLine("   StorJ replication completed");

                Console.WriteLine("\nTransaction is now safely stored both locally and in distributed cloud!");
            }
        }
    }
}
