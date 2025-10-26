using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Platform.Sandbox.Experiments
{
    /// <summary>
    /// Demonstrates the Copy-On-Write database principle with concurrent readers and writers.
    /// This example shows how multiple readers can access data while a writer applies transactions.
    /// </summary>
    public class CopyOnWriteDatabaseExample
    {
        /// <summary>
        /// Sample data structure for demonstration
        /// </summary>
        public class BankAccount
        {
            public Dictionary<string, decimal> Accounts { get; set; } = new Dictionary<string, decimal>();
        }

        public static void Run()
        {
            Console.WriteLine("=== Copy-On-Write Database Example ===\n");

            // Example 1: Basic operations
            BasicExample();

            Console.WriteLine("\n" + new string('=', 50) + "\n");

            // Example 2: Concurrent readers and writers
            ConcurrentExample();

            Console.WriteLine("\n" + new string('=', 50) + "\n");

            // Example 3: Transaction consistency
            ConsistencyExample();
        }

        /// <summary>
        /// Demonstrates basic read and write operations
        /// </summary>
        private static void BasicExample()
        {
            Console.WriteLine("Example 1: Basic Operations");
            Console.WriteLine("---------------------------");

            var database = new CopyOnWriteDatabase<BankAccount>();

            // Write: Create initial accounts
            database.Write(new Transaction<BankAccount>(data =>
            {
                data.Accounts["Alice"] = 1000m;
                data.Accounts["Bob"] = 500m;
                Console.WriteLine("Transaction 1: Created accounts for Alice ($1000) and Bob ($500)");
            }));

            // Read: Check balances
            var aliceBalance = database.Read(data =>
            {
                return data.Accounts.TryGetValue("Alice", out var balance) ? balance : 0m;
            });
            Console.WriteLine($"Read: Alice's balance: ${aliceBalance}");

            // Write: Transfer money
            database.Write(new Transaction<BankAccount>(data =>
            {
                if (data.Accounts.TryGetValue("Alice", out var aliceAmt) && aliceAmt >= 200m)
                {
                    data.Accounts["Alice"] = aliceAmt - 200m;
                    data.Accounts["Bob"] = data.Accounts["Bob"] + 200m;
                    Console.WriteLine("Transaction 2: Transferred $200 from Alice to Bob");
                }
            }));

            // Read: Check updated balances
            database.Read(data =>
            {
                Console.WriteLine($"Read: Alice's balance: ${data.Accounts["Alice"]}");
                Console.WriteLine($"Read: Bob's balance: ${data.Accounts["Bob"]}");
                return true;
            });

            var stats = database.GetStatistics();
            Console.WriteLine($"\nStatistics: {stats}");
        }

        /// <summary>
        /// Demonstrates concurrent readers and a single writer
        /// </summary>
        private static void ConcurrentExample()
        {
            Console.WriteLine("Example 2: Concurrent Readers and Writers");
            Console.WriteLine("-----------------------------------------");

            var database = new CopyOnWriteDatabase<BankAccount>();

            // Initialize accounts
            database.Write(new Transaction<BankAccount>(data =>
            {
                data.Accounts["Account1"] = 1000m;
                data.Accounts["Account2"] = 2000m;
                data.Accounts["Account3"] = 3000m;
            }));

            Console.WriteLine("Initial accounts created\n");

            var tasks = new List<Task>();

            // Start multiple reader threads
            for (int i = 0; i < 5; i++)
            {
                int readerId = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var total = database.Read(data =>
                        {
                            var sum = data.Accounts.Values.Sum();
                            Console.WriteLine($"Reader {readerId}: Total balance = ${sum}");
                            Thread.Sleep(10); // Simulate some read processing
                            return sum;
                        });
                    }
                }));
            }

            // Start a writer thread that applies transactions
            tasks.Add(Task.Run(() =>
            {
                Thread.Sleep(5); // Let some readers start
                for (int i = 0; i < 3; i++)
                {
                    database.Write(new Transaction<BankAccount>(data =>
                    {
                        // Add interest to all accounts
                        var accounts = data.Accounts.Keys.ToList();
                        foreach (var account in accounts)
                        {
                            data.Accounts[account] *= 1.01m; // 1% interest
                        }
                        Console.WriteLine($"Writer: Applied 1% interest to all accounts (transaction {i + 1})");
                    }));
                    Thread.Sleep(20);
                }
            }));

            Task.WaitAll(tasks.ToArray());

            Console.WriteLine("\nAll operations completed");
            var stats = database.GetStatistics();
            Console.WriteLine($"Statistics: {stats}");
        }

        /// <summary>
        /// Demonstrates transaction consistency and the copy-on-write principle
        /// </summary>
        private static void ConsistencyExample()
        {
            Console.WriteLine("Example 3: Transaction Consistency");
            Console.WriteLine("----------------------------------");

            var database = new CopyOnWriteDatabase<BankAccount>();

            // Initialize
            database.Write(new Transaction<BankAccount>(data =>
            {
                data.Accounts["User1"] = 100m;
                Console.WriteLine("Initial: User1 = $100");
            }));

            var readerStarted = new ManualResetEventSlim(false);
            var writerCanProceed = new ManualResetEventSlim(false);

            // Reader that takes time
            var readerTask = Task.Run(() =>
            {
                database.Read(data =>
                {
                    var balance = data.Accounts["User1"];
                    Console.WriteLine($"Reader: Started reading, User1 = ${balance}");
                    readerStarted.Set();

                    // Simulate long read operation
                    Thread.Sleep(100);

                    // Read again - should still see consistent data
                    balance = data.Accounts["User1"];
                    Console.WriteLine($"Reader: Still reading same transaction, User1 = ${balance}");
                    writerCanProceed.Set();
                    return balance;
                });
            });

            // Wait for reader to start
            readerStarted.Wait();

            // Writer applies transaction
            var writerTask = Task.Run(() =>
            {
                Console.WriteLine("Writer: Waiting for reader to finish...");
                database.Write(new Transaction<BankAccount>(data =>
                {
                    data.Accounts["User1"] = 200m;
                    Console.WriteLine("Writer: Applied transaction, User1 = $200");
                }));
            });

            Task.WaitAll(readerTask, writerTask);

            // Verify final state
            database.Read(data =>
            {
                Console.WriteLine($"\nFinal: User1 = ${data.Accounts["User1"]}");
                return true;
            });

            Console.WriteLine("\nThis demonstrates that:");
            Console.WriteLine("1. Reader sees consistent data throughout its operation");
            Console.WriteLine("2. Writer waits for readers to finish before swapping copies");
            Console.WriteLine("3. After swap, new readers see updated data");
        }

        /// <summary>
        /// Advanced example showing the optimization mentioned in the issue:
        /// Writing pending transactions while waiting for readers to finish
        /// </summary>
        public static void AdvancedOptimizationExample()
        {
            Console.WriteLine("Example 4: Transaction Batching Optimization");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("While waiting for readers to finish, the system can apply");
            Console.WriteLine("additional pending transactions from the transaction log.\n");

            var database = new CopyOnWriteDatabase<BankAccount>();

            // Initialize
            database.Write(new Transaction<BankAccount>(data =>
            {
                data.Accounts["Account"] = 1000m;
            }));

            Console.WriteLine("Initial account created with $1000\n");

            // Simulate multiple rapid write requests
            var writerTasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                int txId = i;
                writerTasks.Add(Task.Run(() =>
                {
                    database.Write(new Transaction<BankAccount>(data =>
                    {
                        data.Accounts["Account"] += 100m;
                        Console.WriteLine($"Transaction {txId}: Added $100 (Total: ${data.Accounts["Account"]})");
                    }));
                }));
            }

            Task.WaitAll(writerTasks.ToArray());

            var final = database.Read(data => data.Accounts["Account"]);
            Console.WriteLine($"\nFinal balance: ${final}");
            Console.WriteLine($"Statistics: {database.GetStatistics()}");
        }
    }
}
