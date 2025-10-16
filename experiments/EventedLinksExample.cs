using System;
using System.Collections.Generic;
using Platform.Data.EventedIO;

namespace Platform.Examples
{
    /// <summary>
    /// Example demonstrating the Evented IO style API for Links CRUD operations
    /// This shows how to use operation queues, transactions, and native triggers
    /// </summary>
    public class EventedLinksExample
    {
        public static void Main()
        {
            Console.WriteLine("=== Evented Links API Example ===\n");

            // Create an instance of the evented links store
            var eventedLinks = new LinksEvented<long>();

            // Example 1: Basic CRUD with event handlers
            BasicCRUDExample(eventedLinks);

            // Example 2: Transaction with conflict detection
            TransactionExample(eventedLinks);

            // Example 3: Custom triggers and validation
            CustomTriggersExample(eventedLinks);

            Console.WriteLine("\n=== Example Complete ===");
        }

        /// <summary>
        /// Example 1: Demonstrates basic CRUD operations with Before/After event handlers
        /// </summary>
        static void BasicCRUDExample(LinksEvented<long> links)
        {
            Console.WriteLine("--- Example 1: Basic CRUD Operations ---");

            // Setup event handlers (native triggers)
            links.BeforeCreate += (sender, args) =>
            {
                Console.WriteLine($"[Trigger] Before Create: Source={args.Operation.Source}, Target={args.Operation.Target}");
            };

            links.AfterCreate += (sender, args) =>
            {
                Console.WriteLine($"[Trigger] After Create: LinkId={args.Operation.LinkId}");
            };

            links.BeforeUpdate += (sender, args) =>
            {
                Console.WriteLine($"[Trigger] Before Update: LinkId={args.Operation.LinkId}");
            };

            links.AfterUpdate += (sender, args) =>
            {
                Console.WriteLine($"[Trigger] After Update: LinkId={args.Operation.LinkId}");
            };

            links.BeforeDelete += (sender, args) =>
            {
                Console.WriteLine($"[Trigger] Before Delete: LinkId={args.Operation.LinkId}");
            };

            links.AfterDelete += (sender, args) =>
            {
                Console.WriteLine($"[Trigger] After Delete: LinkId={args.Operation.LinkId}");
            };

            // Queue operations
            Console.WriteLine("Queueing operations...");
            var linkId1 = links.CreateAsync(1, 2);
            var linkId2 = links.CreateAsync(2, 3);

            Console.WriteLine($"Queued {links.GetPendingOperations().Count()} operations");

            // Process the queue
            Console.WriteLine("\nProcessing queue...");
            links.ProcessQueue();

            Console.WriteLine();
        }

        /// <summary>
        /// Example 2: Demonstrates transactions with conflict detection and consistency checking
        /// </summary>
        static void TransactionExample(LinksEvented<long> links)
        {
            Console.WriteLine("--- Example 2: Transactions with Conflict Detection ---");

            // Setup conflict handler
            links.OnConflict += (sender, args) =>
            {
                Console.WriteLine($"[Conflict Detected] Operation: {args.Operation.Type}, LinkId: {args.Operation.LinkId}");
                Console.WriteLine("Allowing operation to proceed anyway (set Cancel=false)");
                // You could set args.Cancel = true to prevent the operation
            };

            // Begin a transaction
            var transaction = links.BeginTransaction();
            Console.WriteLine($"Started transaction: {transaction.TransactionId}");

            // Add operations to the transaction
            var createOp = new LinkOperation<long>
            {
                Type = OperationType.Create,
                Source = 10,
                Target = 20,
                LinkId = 100,
                TransactionId = transaction.TransactionId
            };
            transaction.Operations.Add(createOp);

            var updateOp = new LinkOperation<long>
            {
                Type = OperationType.Update,
                LinkId = 100,
                NewSource = 11,
                NewTarget = 21,
                TransactionId = transaction.TransactionId
            };
            transaction.Operations.Add(updateOp);

            Console.WriteLine($"Added {transaction.Operations.Count} operations to transaction");

            // Commit the transaction (with consistency checking)
            try
            {
                Console.WriteLine("Committing transaction...");
                bool success = links.CommitTransaction(transaction.TransactionId);
                Console.WriteLine($"Transaction committed: {success}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Transaction failed: {ex.Message}");
                links.RollbackTransaction(transaction.TransactionId);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example 3: Demonstrates custom validation triggers
        /// </summary>
        static void CustomTriggersExample(LinksEvented<long> links)
        {
            Console.WriteLine("--- Example 3: Custom Triggers and Validation ---");

            // Create a fresh instance for this example
            var customLinks = new LinksEvented<long>();

            // Setup custom validation trigger
            customLinks.OnValidationError += (sender, args) =>
            {
                Console.WriteLine($"[Validation Error] {args.ValidationError}");
                Console.WriteLine("Operation rejected");
            };

            // Setup a custom business rule trigger
            customLinks.BeforeCreate += (sender, args) =>
            {
                // Example business rule: Source must be less than Target
                if (args.Operation.Source != null && args.Operation.Target != null)
                {
                    if (Convert.ToInt64(args.Operation.Source) >= Convert.ToInt64(args.Operation.Target))
                    {
                        Console.WriteLine($"[Business Rule] Source ({args.Operation.Source}) must be less than Target ({args.Operation.Target})");
                        args.Cancel = true;
                        Console.WriteLine("Operation cancelled by business rule");
                    }
                }
            };

            // Try operations that will trigger validation
            Console.WriteLine("Attempting valid operation (5 -> 10)...");
            try
            {
                var validLink = customLinks.CreateAsync(5, 10);
                Console.WriteLine("Operation queued successfully");
                customLinks.ProcessQueue();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Operation failed: {ex.Message}");
            }

            Console.WriteLine("\nAttempting invalid operation (10 -> 5)...");
            try
            {
                var invalidLink = customLinks.CreateAsync(10, 5);
                Console.WriteLine("Operation queued");
                customLinks.ProcessQueue();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Operation failed: {ex.Message}");
            }

            Console.WriteLine("\nAttempting operation with missing data...");
            try
            {
                var invalidOp = new LinkOperation<long>
                {
                    Type = OperationType.Create,
                    Source = 1
                    // Missing Target - should trigger validation error
                };
                customLinks.EnqueueOperation(invalidOp);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Operation rejected: {ex.Message}");
            }

            Console.WriteLine();
        }
    }

    /// <summary>
    /// Example showing advanced usage patterns
    /// </summary>
    public class AdvancedEventedLinksExample
    {
        /// <summary>
        /// Demonstrates how to implement a logging trigger
        /// </summary>
        public static void LoggingTriggerExample()
        {
            Console.WriteLine("--- Advanced: Logging Trigger ---");

            var links = new LinksEvented<long>();
            var operationLog = new List<string>();

            // Setup comprehensive logging
            EventHandler<LinkEventArgs<long>> logger = (sender, args) =>
            {
                var logEntry = $"[{DateTime.UtcNow:O}] {args.Operation.Type} - LinkId: {args.Operation.LinkId}, " +
                              $"Source: {args.Operation.Source}, Target: {args.Operation.Target}";
                operationLog.Add(logEntry);
                Console.WriteLine(logEntry);
            };

            links.AfterCreate += logger;
            links.AfterUpdate += logger;
            links.AfterDelete += logger;

            // Perform operations
            var link1 = links.CreateAsync(100, 200);
            links.UpdateAsync(link1, 101, 201);
            links.DeleteAsync(link1);

            links.ProcessQueue();

            Console.WriteLine($"\nLogged {operationLog.Count} operations");
            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates how to implement a caching trigger
        /// </summary>
        public static void CachingTriggerExample()
        {
            Console.WriteLine("--- Advanced: Caching Trigger ---");

            var links = new LinksEvented<long>();
            var cache = new Dictionary<long, (long Source, long Target)>();

            // Update cache on create
            links.AfterCreate += (sender, args) =>
            {
                if (args.Operation.LinkId != null && args.Operation.Source != null && args.Operation.Target != null)
                {
                    cache[args.Operation.LinkId.Value] = (args.Operation.Source.Value, args.Operation.Target.Value);
                    Console.WriteLine($"[Cache] Added link {args.Operation.LinkId} to cache");
                }
            };

            // Invalidate cache on update
            links.AfterUpdate += (sender, args) =>
            {
                if (args.Operation.LinkId != null)
                {
                    cache.Remove(args.Operation.LinkId.Value);
                    Console.WriteLine($"[Cache] Invalidated link {args.Operation.LinkId} in cache");
                }
            };

            // Remove from cache on delete
            links.AfterDelete += (sender, args) =>
            {
                if (args.Operation.LinkId != null)
                {
                    cache.Remove(args.Operation.LinkId.Value);
                    Console.WriteLine($"[Cache] Removed link {args.Operation.LinkId} from cache");
                }
            };

            // Perform operations
            links.CreateAsync(1, 2);
            links.CreateAsync(3, 4);
            links.ProcessQueue();

            Console.WriteLine($"\nCache contains {cache.Count} entries");
            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates how to implement a consistency enforcement trigger
        /// </summary>
        public static void ConsistencyEnforcementExample()
        {
            Console.WriteLine("--- Advanced: Consistency Enforcement ---");

            var links = new LinksEvented<long>();
            var existingLinks = new HashSet<long>();

            // Ensure link uniqueness
            links.BeforeCreate += (sender, args) =>
            {
                if (args.Operation.LinkId != null && existingLinks.Contains(args.Operation.LinkId.Value))
                {
                    Console.WriteLine($"[Consistency] Link {args.Operation.LinkId} already exists - cancelling create");
                    args.Cancel = true;
                }
            };

            links.AfterCreate += (sender, args) =>
            {
                if (args.Operation.LinkId != null)
                {
                    existingLinks.Add(args.Operation.LinkId.Value);
                }
            };

            // Track deletions
            links.AfterDelete += (sender, args) =>
            {
                if (args.Operation.LinkId != null)
                {
                    existingLinks.Remove(args.Operation.LinkId.Value);
                    Console.WriteLine($"[Consistency] Link {args.Operation.LinkId} removed from tracking");
                }
            };

            // Test consistency enforcement
            links.CreateAsync(1, 2);
            links.ProcessQueue();

            Console.WriteLine();
        }
    }
}
