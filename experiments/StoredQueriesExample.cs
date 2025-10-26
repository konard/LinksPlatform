using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Doublets;
using Platform.Sandbox.StoredQueries;

namespace Platform.Experiments
{
    /// <summary>
    ///     Example demonstrating the stored queries functionality.
    ///     Пример демонстрирующий функциональность хранимых запросов.
    /// </summary>
    public class StoredQueriesExample
    {
        public static void Run()
        {
            Console.WriteLine("=== Stored Queries Example ===");
            Console.WriteLine();

            // Note: In a real implementation, you would use an actual ILinks<ulong> instance
            // For this example, we'll use a mock implementation
            var links = CreateMockLinks();
            var manager = new StoredQueriesManager<ulong>(links);

            // Example 1: Create a simple stored query
            Console.WriteLine("1. Creating a simple stored query...");
            var simpleQuery = manager.CreateQuery<ILink<ulong>>(
                "FindAllLinks",
                linksDb => linksDb.All()
            );
            Console.WriteLine($"   Created query: {simpleQuery.Name}");
            Console.WriteLine($"   Query ID: {simpleQuery.Id}");
            Console.WriteLine();

            // Example 2: Create a query with statistics tracking
            Console.WriteLine("2. Creating a query with statistics tracking...");
            var statsQuery = manager.CreateQuery<ILink<ulong>>(
                "FindLinksBySource",
                linksDb => linksDb.All().Where(link => link.Index % 2 == 0)
            );
            Console.WriteLine($"   Created query: {statsQuery.Name}");
            Console.WriteLine();

            // Example 3: Add a logging trigger
            Console.WriteLine("3. Adding a logging trigger...");
            var loggingTrigger = new LoggingTrigger<ulong, ILink<ulong>>(
                "QueryLogger",
                TriggerType.Before
            );
            statsQuery.AddTrigger(loggingTrigger);

            var afterLoggingTrigger = new LoggingTrigger<ulong, ILink<ulong>>(
                "QueryLoggerAfter",
                TriggerType.After
            );
            statsQuery.AddTrigger(afterLoggingTrigger);

            Console.WriteLine($"   Added {statsQuery.Triggers.Count} triggers to query");
            Console.WriteLine();

            // Example 4: Execute the query multiple times
            Console.WriteLine("4. Executing query multiple times to collect statistics...");
            for (int i = 0; i < 5; i++)
            {
                var results = manager.ExecuteQuery<ILink<ulong>>("FindLinksBySource");
                Console.WriteLine($"   Execution {i + 1}: Found {results.Count()} results");
            }
            Console.WriteLine();

            // Example 5: Display statistics
            Console.WriteLine("5. Query execution statistics:");
            Console.WriteLine($"   {statsQuery.Statistics}");
            Console.WriteLine();

            // Example 6: Get recent execution records
            Console.WriteLine("6. Recent execution records:");
            var recentExecutions = statsQuery.Statistics.GetRecentExecutions(3);
            foreach (var record in recentExecutions)
            {
                Console.WriteLine($"   - Timestamp: {record.Timestamp:yyyy-MM-dd HH:mm:ss.fff}, " +
                                  $"Duration: {record.ExecutionTime.TotalMilliseconds:F2}ms, " +
                                  $"Success: {record.Success}, " +
                                  $"Results: {record.ResultCount}");
            }
            Console.WriteLine();

            // Example 7: Disable and re-enable a query
            Console.WriteLine("7. Testing query enable/disable...");
            manager.DisableQuery("FindLinksBySource");
            Console.WriteLine("   Query disabled");
            try
            {
                manager.ExecuteQuery<ILink<ulong>>("FindLinksBySource");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Expected error: {ex.Message}");
            }

            manager.EnableQuery("FindLinksBySource");
            Console.WriteLine("   Query re-enabled");
            var resultsAfterEnable = manager.ExecuteQuery<ILink<ulong>>("FindLinksBySource");
            Console.WriteLine($"   Successfully executed: Found {resultsAfterEnable.Count()} results");
            Console.WriteLine();

            // Example 8: List all queries
            Console.WriteLine("8. All stored queries:");
            var allQueries = manager.GetAllQueries();
            foreach (var query in allQueries)
            {
                var q = query as dynamic;
                Console.WriteLine($"   - {q.Name} (ID: {q.Id}, Enabled: {q.IsEnabled})");
            }
            Console.WriteLine();

            // Example 9: Delete a query
            Console.WriteLine("9. Deleting a query...");
            var deleted = manager.DeleteQuery("FindAllLinks");
            Console.WriteLine($"   Query deleted: {deleted}");
            Console.WriteLine($"   Remaining queries: {manager.Count}");
            Console.WriteLine();

            Console.WriteLine("=== Example completed successfully ===");
        }

        /// <summary>
        ///     Creates a mock ILinks implementation for demonstration purposes.
        ///     Создаёт макет реализации ILinks для демонстрационных целей.
        /// </summary>
        private static ILinks<ulong> CreateMockLinks()
        {
            return new MockLinks();
        }

        /// <summary>
        ///     Mock implementation of ILinks for testing purposes.
        ///     Макет реализации ILinks для целей тестирования.
        /// </summary>
        private class MockLinks : ILinks<ulong>
        {
            private readonly List<MockLink> _links = new List<MockLink>();

            public MockLinks()
            {
                // Create some sample links
                for (ulong i = 1; i <= 10; i++)
                {
                    _links.Add(new MockLink { Index = i, Source = i, Target = i });
                }
            }

            public ILink<ulong> this[ulong index] => _links.FirstOrDefault(l => l.Index == index);

            public ulong Constants_TargetPartIndex => 2;
            public ulong Constants_SourcePartIndex => 1;
            public ulong Constants_IndexPartIndex => 0;
            public ulong Constants_Continue => ulong.MaxValue;
            public ulong Constants_Break => 0;
            public ulong Constants_Skip => 1;
            public ulong Constants_Any => 0;
            public ulong Constants_Null => 0;

            public IEnumerable<ILink<ulong>> All()
            {
                return _links;
            }

            public ulong Count(IList<ulong> restrictions) => (ulong)_links.Count;

            public ulong Create() => throw new NotImplementedException();

            public ulong CreateAndUpdate(ulong source, ulong target) => throw new NotImplementedException();

            public ulong CreatePoint() => throw new NotImplementedException();

            public ulong Delete(IList<ulong> restrictions) => throw new NotImplementedException();

            public ulong Each(Func<IList<ulong>, ulong> handler, IList<ulong> restrictions) => (ulong)_links.Count;

            public ulong SearchOrDefault(ulong source, ulong target) => throw new NotImplementedException();

            public ulong Update(IList<ulong> restrictions, IList<ulong> substitution) => throw new NotImplementedException();
        }

        private class MockLink : ILink<ulong>
        {
            public ulong Index { get; set; }
            public ulong Source { get; set; }
            public ulong Target { get; set; }
        }
    }
}
