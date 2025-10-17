using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Platform.Data.Doublets.DistributedTriggers;

namespace Platform.Data.Doublets.Examples.DistributedTriggers
{
    /// <summary>
    /// Demonstrates the distributed triggers execution process.
    /// Shows how to:
    /// 1. Set up a coordinator with multiple workers
    /// 2. Register triggers with different partition keys
    /// 3. Process transaction events
    /// 4. View the parallel execution results
    /// </summary>
    public class DistributedTriggersExample
    {
        public static async Task RunExample()
        {
            Console.WriteLine("=== Distributed Triggers Execution Example ===\n");

            // Create a simple transaction log writer
            var logWriter = new ConsoleTransactionLogWriter<ulong>();

            // Initialize coordinator with 4 workers
            const int workerCount = 4;
            var coordinator = new DistributedTriggersCoordinator<ulong>(workerCount, logWriter);

            Console.WriteLine($"Initialized coordinator with {workerCount} workers\n");

            // Initialize with some existing links
            var initialLinks = new[]
            {
                new Link<ulong>(1, 0, 0),
                new Link<ulong>(2, 1, 1),
                new Link<ulong>(3, 2, 2)
            };
            coordinator.InitializeSnapshot(initialLinks);
            Console.WriteLine($"Initialized snapshot with {initialLinks.Length} links\n");

            // Register triggers with different partition keys
            // This ensures they are distributed across different workers
            var loggingTrigger1 = new SimpleLoggingTrigger<ulong>(
                partitionKey: 0,
                logger: msg => Console.WriteLine($"[Worker 0] {msg}"));

            var loggingTrigger2 = new SimpleLoggingTrigger<ulong>(
                partitionKey: 1,
                logger: msg => Console.WriteLine($"[Worker 1] {msg}"));

            var loggingTrigger3 = new SimpleLoggingTrigger<ulong>(
                partitionKey: 2,
                logger: msg => Console.WriteLine($"[Worker 2] {msg}"));

            var counterTrigger = new LinkCounterTrigger<ulong>(
                partitionKey: 3,
                counterLinkAddress: 9999);

            coordinator.RegisterTrigger(loggingTrigger1);
            coordinator.RegisterTrigger(loggingTrigger2);
            coordinator.RegisterTrigger(loggingTrigger3);
            coordinator.RegisterTrigger(counterTrigger);

            Console.WriteLine("Registered 4 triggers across workers\n");

            // Display statistics
            var stats = coordinator.GetStatistics();
            Console.WriteLine($"Coordinator Statistics: {stats}\n");

            // Create some transaction events
            Console.WriteLine("Processing transaction events...\n");

            // Transaction 1: Create new links
            var transaction1 = new TransactionEvent<ulong>(
                transactionId: 1,
                timestamp: DateTime.UtcNow,
                operations: new[]
                {
                    new TransactionOperation<ulong>(OperationType.Create, 4, 1, 2),
                    new TransactionOperation<ulong>(OperationType.Create, 5, 2, 3)
                });

            Console.WriteLine("--- Transaction 1: Creating 2 new links ---");
            await coordinator.ProcessTransactionEventAsync(transaction1);
            await Task.Delay(100); // Allow triggers to complete
            Console.WriteLine();

            // Transaction 2: Update and delete
            var transaction2 = new TransactionEvent<ulong>(
                transactionId: 2,
                timestamp: DateTime.UtcNow,
                operations: new[]
                {
                    new TransactionOperation<ulong>(OperationType.Update, 4, 3, 4)
                    {
                        PreviousSource = 1,
                        PreviousTarget = 2
                    },
                    new TransactionOperation<ulong>(OperationType.Delete, 2, 0, 0)
                });

            Console.WriteLine("--- Transaction 2: Updating 1 link and deleting 1 link ---");
            await coordinator.ProcessTransactionEventAsync(transaction2);
            await Task.Delay(100);
            Console.WriteLine();

            // Transaction 3: More creates
            var transaction3 = new TransactionEvent<ulong>(
                transactionId: 3,
                timestamp: DateTime.UtcNow,
                operations: new[]
                {
                    new TransactionOperation<ulong>(OperationType.Create, 6, 4, 5),
                    new TransactionOperation<ulong>(OperationType.Create, 7, 5, 6),
                    new TransactionOperation<ulong>(OperationType.Create, 8, 6, 7)
                });

            Console.WriteLine("--- Transaction 3: Creating 3 new links ---");
            await coordinator.ProcessTransactionEventAsync(transaction3);
            await Task.Delay(100);
            Console.WriteLine();

            // Final statistics
            stats = coordinator.GetStatistics();
            Console.WriteLine($"Final Statistics: {stats}\n");

            // Stop coordinator
            coordinator.Stop();
            Console.WriteLine("Coordinator stopped.");
        }
    }

    /// <summary>
    /// Simple transaction log writer that outputs to console.
    /// In a real implementation, this would write to a persistent transaction log.
    /// </summary>
    internal class ConsoleTransactionLogWriter<TLinkAddress> : ITransactionLogWriter<TLinkAddress>
    {
        public void WriteOperations(long transactionId, IEnumerable<TransactionOperation<TLinkAddress>> operations)
        {
            Console.WriteLine($"\n[Transaction Log] Writing merged results for transaction {transactionId}:");
            foreach (var operation in operations)
            {
                Console.WriteLine($"  - {operation.OperationType}: Link {operation.LinkAddress} " +
                                $"(Source: {operation.Source}, Target: {operation.Target})");
            }
        }
    }
}
