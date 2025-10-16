using System;
using System.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the Resource Transaction Tracker example.
    /// Demonstrates a real-life application of the Links Platform.
    /// </summary>
    public class ResourceTransactionTrackerCLI : ICommandLineInterface
    {
        private const string DefaultDatabaseFilename = "transactions.links";

        public void Run(params string[] args)
        {
            Console.WriteLine("=== Resource Transaction Tracker ===");
            Console.WriteLine("A real-life application example using Links Platform");
            Console.WriteLine();

            try
            {
#if DEBUG
                if (File.Exists(DefaultDatabaseFilename))
                {
                    File.Delete(DefaultDatabaseFilename);
                }
#endif
                // Initialize Links storage
                using (var memoryAdapter = new UInt64UnitedMemoryLinks(DefaultDatabaseFilename, 1024 * 1024))
                using (var links = new UInt64Links(memoryAdapter))
                {
                    var tracker = new ResourceTransactionTracker(links);

                    // Create users in different cities
                    Console.WriteLine("Creating users...");
                    tracker.CreateUser("Alice", "New York");
                    tracker.CreateUser("Bob", "San Francisco");
                    tracker.CreateUser("Charlie", "New York");
                    tracker.CreateUser("David", "London");

                    // Create resources
                    Console.WriteLine("Creating resources...");
                    tracker.CreateResource("Gold", 1000);
                    tracker.CreateResource("Silver", 5000);
                    tracker.CreateResource("Credits", 10000);

                    // Create transactions
                    Console.WriteLine("Creating transactions...");
                    tracker.CreateTransaction("Alice", "Bob", "Gold", 50, DateTime.UtcNow);
                    tracker.CreateTransaction("Bob", "Charlie", "Silver", 200, DateTime.UtcNow);
                    tracker.CreateTransaction("Charlie", "David", "Credits", 500, DateTime.UtcNow);
                    tracker.CreateTransaction("David", "Alice", "Gold", 25, DateTime.UtcNow);

                    Console.WriteLine();
                    Console.WriteLine("=== Statistics ===");
                    tracker.PrintStatistics();

                    Console.WriteLine();
                    Console.WriteLine("=== User Transactions ===");
                    var aliceCount = tracker.GetUserTransactionCount("Alice");
                    Console.WriteLine($"Alice has {aliceCount} link(s) involving her");

                    var bobCount = tracker.GetUserTransactionCount("Bob");
                    Console.WriteLine($"Bob has {bobCount} link(s) involving him");

                    Console.WriteLine();
                    Console.WriteLine("Example completed successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
