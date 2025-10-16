using System;
using System.IO;
using Platform.Sandbox;

namespace Platform.Experiments
{
    /// <summary>
    /// Experiment to test transaction log compaction functionality.
    /// This demonstrates the compaction of redundant transitions in the transaction log.
    /// </summary>
    public class TransactionLogCompactionTest
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Transaction Log Compaction Experiment");
            Console.WriteLine("======================================");
            Console.WriteLine();

            // Clean up any existing transaction log
            const string logFile = "transactions.log";
            if (File.Exists(logFile))
            {
                Console.WriteLine($"Removing existing log file: {logFile}");
                File.Delete(logFile);
            }

            Console.WriteLine("Step 1: Initialize transaction log");
            Console.WriteLine("This will create initial transaction entries.");
            Transactions.Run();

            // Get initial log size
            var initialSize = File.Exists(logFile) ? new FileInfo(logFile).Length : 0;
            Console.WriteLine($"Initial log file size: {initialSize} bytes");
            Console.WriteLine();

            Console.WriteLine("Step 2: Simulate redundant transactions");
            Console.WriteLine("In a real scenario, multiple create/delete operations would occur.");
            Console.WriteLine("The current implementation has basic transaction tracking.");
            Console.WriteLine();

            Console.WriteLine("Step 3: Compact the transaction log");
            Console.WriteLine("Removing redundant transitions...");

            try
            {
                Transactions.CompactLog();
                Console.WriteLine("✓ Compaction completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Compaction failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return;
            }

            // Get final log size
            var finalSize = File.Exists(logFile) ? new FileInfo(logFile).Length : 0;
            Console.WriteLine($"Final log file size: {finalSize} bytes");
            Console.WriteLine();

            if (initialSize > finalSize)
            {
                var reduction = ((initialSize - finalSize) / (double)initialSize) * 100;
                Console.WriteLine($"✓ Log size reduced by {reduction:F2}%");
            }
            else if (initialSize == finalSize)
            {
                Console.WriteLine("Log size unchanged (no redundant transactions to remove)");
            }
            else
            {
                Console.WriteLine("Note: Log size increased (compaction may have reorganized data)");
            }

            Console.WriteLine();
            Console.WriteLine("Experiment completed.");
            Console.WriteLine();
            Console.WriteLine("Expected behavior:");
            Console.WriteLine("- For each link, only the final state is kept");
            Console.WriteLine("- Timestamps of the last action are preserved");
            Console.WriteLine("- Redundant intermediate states are removed");
        }
    }
}
