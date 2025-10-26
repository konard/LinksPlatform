using System;
using System.Threading;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Examples.ReadAccessMetrics
{
    /// <summary>
    /// Simple example demonstrating the usage of read access frequency metrics tracker.
    /// Shows both variant 1 (incremental average) and variant 2 (formula-based average).
    /// </summary>
    public class ReadAccessMetricsSimpleExample
    {
        /// <summary>
        /// Runs the example demonstrating read access metrics.
        /// </summary>
        public static void Run()
        {
            Console.WriteLine("=== Read Access Frequency Metrics Example ===\n");

            // Create a temporary file for links storage
            var tempFile = System.IO.Path.GetTempFileName();

            try
            {
                // Create links storage
                using (var links = new UnitedMemoryLinks<uint>(tempFile, 8 * 1024 * 1024))
            {
                // Create the metrics tracker
                var metricsTracker = new ReadAccessMetricsTracker<uint>();

                Console.WriteLine("Creating links...");
                var link1 = links.Create();
                var link2 = links.Create();
                var link3 = links.Create();

                // Register created links with the tracker
                metricsTracker.RegisterCreatedLink(link1);
                metricsTracker.RegisterCreatedLink(link2);
                metricsTracker.RegisterCreatedLink(link3);

                Console.WriteLine($"Created links: {link1}, {link2}, {link3}\n");

                // Simulate read access patterns
                Console.WriteLine("Simulating read access patterns...\n");

                // Access link1 frequently
                Console.WriteLine("Reading link1 three times with 100ms intervals:");
                metricsTracker.RecordReadAccess(link1);
                Thread.Sleep(100);
                metricsTracker.RecordReadAccess(link1);
                Thread.Sleep(100);
                metricsTracker.RecordReadAccess(link1);

                // Access link2 less frequently
                Console.WriteLine("Reading link2 twice with 200ms intervals:");
                metricsTracker.RecordReadAccess(link2);
                Thread.Sleep(200);
                metricsTracker.RecordReadAccess(link2);

                // Access link3 once
                Console.WriteLine("Reading link3 once:\n");
                metricsTracker.RecordReadAccess(link3);

                // Display metrics
                Console.WriteLine("\n=== Metrics Report ===\n");

                PrintLinkMetrics(metricsTracker, link1, "Link1 (frequent access)");
                PrintLinkMetrics(metricsTracker, link2, "Link2 (moderate access)");
                PrintLinkMetrics(metricsTracker, link3, "Link3 (single access)");

                // Demonstrate the difference between variant 1 and variant 2
                Console.WriteLine("\n=== Variant Comparison ===\n");
                Console.WriteLine("Variant 1: AverageReadAccessTime (incremental calculation)");
                Console.WriteLine("  - Updates average based on time between consecutive reads");
                Console.WriteLine("  - Reflects actual read frequency patterns\n");

                Console.WriteLine("Variant 2: (Now - CreationDateTime) / TotalReadAccessCount");
                Console.WriteLine("  - Calculates average time per read since creation");
                Console.WriteLine("  - Simpler formula, but doesn't capture read frequency dynamics\n");

                // Show formula-based calculation
                var variant2Average = metricsTracker.GetAverageReadAccessTimeVariant2(link1);
                Console.WriteLine($"Link1 Variant 2 Average: {variant2Average?.TotalMilliseconds:F2} ms");
                }
            }
            finally
            {
                // Clean up temporary file
                if (System.IO.File.Exists(tempFile))
                {
                    System.IO.File.Delete(tempFile);
                }
            }

            Console.WriteLine("\n=== Example Complete ===");
        }

        private static void PrintLinkMetrics(ReadAccessMetricsTracker<uint> metricsTracker, uint linkAddress, string description)
        {
            var avgTime = metricsTracker.GetAverageReadAccessTime(linkAddress);
            var lastAccess = metricsTracker.GetLastReadAccessTime(linkAddress);
            var creationTime = metricsTracker.GetCreationDateTime(linkAddress);
            var totalReads = metricsTracker.GetTotalReadAccessCount(linkAddress);

            Console.WriteLine($"{description}:");
            Console.WriteLine($"  Creation Time: {creationTime:HH:mm:ss.fff}");
            Console.WriteLine($"  Last Access: {lastAccess:HH:mm:ss.fff}");
            Console.WriteLine($"  Total Reads: {totalReads}");
            Console.WriteLine($"  Average Read Access Time (Variant 1): {avgTime?.TotalMilliseconds:F2} ms");

            if (creationTime.HasValue && totalReads > 0)
            {
                var timeSinceCreation = (lastAccess ?? DateTime.UtcNow) - creationTime.Value;
                var variant2Avg = timeSinceCreation.TotalMilliseconds / totalReads;
                Console.WriteLine($"  Average Time Per Read (Variant 2): {variant2Avg:F2} ms");
            }

            Console.WriteLine();
        }
    }
}
