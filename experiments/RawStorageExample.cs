using System;

namespace Platform.Data.Core.RawStorage.Examples
{
    /// <summary>
    /// Example demonstrating the use of raw drive storage for links without file system dependencies.
    /// This example shows how to create, read, write, and manage links on a raw block device.
    /// </summary>
    public class RawStorageExample
    {
        /// <summary>
        /// Example 1: Basic usage with file-backed device (for testing/development).
        /// </summary>
        public static void Example1_BasicUsage()
        {
            Console.WriteLine("=== Example 1: Basic Raw Storage Usage ===\n");

            const string deviceFile = "links_storage.raw";
            const long deviceSize = 1024 * 1024; // 1 MB
            const int blockSize = 4096; // 4 KB blocks
            const uint capacity = 1000; // Store up to 1000 links

            // Create a new raw block device (file-backed for this example)
            using (var device = new FileBackedRawBlockDevice(deviceFile, deviceSize, blockSize, createNew: true))
            {
                Console.WriteLine($"Created raw device: {deviceFile}");
                Console.WriteLine($"  Size: {device.SizeInBytes} bytes");
                Console.WriteLine($"  Block Size: {device.BlockSize} bytes");

                // Create raw links storage on top of the device
                using (var storage = new RawLinksStorage<uint>(device, capacity, initializeNew: true))
                {
                    Console.WriteLine($"\nInitialized links storage:");
                    Console.WriteLine($"  Capacity: {storage.Capacity} links");
                    Console.WriteLine($"  Current Count: {storage.Count} links");

                    // Write some links
                    Console.WriteLine("\nWriting links...");
                    storage.WriteLink(0, 1, 2);  // Link 0: 1 -> 2
                    storage.WriteLink(1, 2, 3);  // Link 1: 2 -> 3
                    storage.WriteLink(2, 3, 1);  // Link 2: 3 -> 1

                    // Read back the links
                    Console.WriteLine("\nReading links...");
                    for (uint i = 0; i < 3; i++)
                    {
                        storage.ReadLink(i, out var source, out var target);
                        Console.WriteLine($"  Link {i}: {source} -> {target}");
                    }

                    // Sync to ensure persistence
                    storage.Sync();
                    Console.WriteLine("\nData synced to device.");
                }
            }

            // Reopen the device to verify persistence
            using (var device = new FileBackedRawBlockDevice(deviceFile, deviceSize, blockSize, createNew: false))
            using (var storage = new RawLinksStorage<uint>(device, capacity, initializeNew: false))
            {
                Console.WriteLine("\n=== Reopening storage to verify persistence ===");
                Console.WriteLine($"Capacity: {storage.Capacity} links");
                Console.WriteLine($"Current Count: {storage.Count} links");

                Console.WriteLine("\nReading persisted links...");
                for (uint i = 0; i < 3; i++)
                {
                    storage.ReadLink(i, out var source, out var target);
                    Console.WriteLine($"  Link {i}: {source} -> {target}");
                }
            }

            // Cleanup
            if (System.IO.File.Exists(deviceFile))
            {
                System.IO.File.Delete(deviceFile);
            }

            Console.WriteLine("\n=== Example 1 Complete ===\n");
        }

        /// <summary>
        /// Example 2: Simulating a simple graph structure using raw storage.
        /// </summary>
        public static void Example2_SimpleGraph()
        {
            Console.WriteLine("=== Example 2: Simple Graph with Raw Storage ===\n");

            const string deviceFile = "graph_storage.raw";
            const long deviceSize = 2 * 1024 * 1024; // 2 MB
            const ulong capacity = 10000; // Store up to 10,000 links

            using (var device = new FileBackedRawBlockDevice(deviceFile, deviceSize, createNew: true))
            using (var storage = new RawLinksStorage<ulong>(device, capacity, initializeNew: true))
            {
                Console.WriteLine("Creating a simple directed graph:");
                Console.WriteLine("  Nodes: A(1), B(2), C(3), D(4)");
                Console.WriteLine("  Edges: A->B, B->C, C->D, D->A, A->C");

                // Represent edges as links (edge index -> source node, target node)
                storage.WriteLink(0, 1, 2); // A -> B
                storage.WriteLink(1, 2, 3); // B -> C
                storage.WriteLink(2, 3, 4); // C -> D
                storage.WriteLink(3, 4, 1); // D -> A
                storage.WriteLink(4, 1, 3); // A -> C

                Console.WriteLine("\nStored graph edges:");
                for (ulong i = 0; i < 5; i++)
                {
                    storage.ReadLink(i, out var source, out var target);
                    Console.WriteLine($"  Edge {i}: Node {source} -> Node {target}");
                }

                storage.Sync();
            }

            // Cleanup
            if (System.IO.File.Exists(deviceFile))
            {
                System.IO.File.Delete(deviceFile);
            }

            Console.WriteLine("\n=== Example 2 Complete ===\n");
        }

        /// <summary>
        /// Example 3: Performance comparison concept (not actual benchmarking).
        /// This demonstrates the API that would be used for performance testing.
        /// </summary>
        public static void Example3_PerformanceConcept()
        {
            Console.WriteLine("=== Example 3: Performance Testing Concept ===\n");

            const string deviceFile = "perf_test.raw";
            const long deviceSize = 10 * 1024 * 1024; // 10 MB
            const uint capacity = 100000;
            const uint testCount = 1000;

            using (var device = new FileBackedRawBlockDevice(deviceFile, deviceSize, createNew: true))
            using (var storage = new RawLinksStorage<uint>(device, capacity, initializeNew: true))
            {
                Console.WriteLine($"Device initialized: {deviceSize / (1024 * 1024)} MB");
                Console.WriteLine($"Link capacity: {capacity}");

                var sw = System.Diagnostics.Stopwatch.StartNew();

                // Sequential writes
                Console.WriteLine($"\nPerforming {testCount} sequential writes...");
                for (uint i = 0; i < testCount; i++)
                {
                    storage.WriteLink(i, i * 2, i * 2 + 1);
                }
                sw.Stop();
                Console.WriteLine($"  Time: {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"  Rate: {testCount * 1000 / sw.ElapsedMilliseconds} writes/sec");

                sw.Restart();

                // Sequential reads
                Console.WriteLine($"\nPerforming {testCount} sequential reads...");
                for (uint i = 0; i < testCount; i++)
                {
                    storage.ReadLink(i, out _, out _);
                }
                sw.Stop();
                Console.WriteLine($"  Time: {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"  Rate: {testCount * 1000 / sw.ElapsedMilliseconds} reads/sec");

                // Random access would be tested similarly with randomized indices
                Console.WriteLine("\nNote: For production use, implement proper benchmarking with:");
                Console.WriteLine("  - Warmed-up cache");
                Console.WriteLine("  - Multiple iterations");
                Console.WriteLine("  - Statistical analysis");
                Console.WriteLine("  - Comparison with file-based storage");

                storage.Sync();
            }

            // Cleanup
            if (System.IO.File.Exists(deviceFile))
            {
                System.IO.File.Delete(deviceFile);
            }

            Console.WriteLine("\n=== Example 3 Complete ===\n");
        }

        /// <summary>
        /// Main entry point to run all examples.
        /// </summary>
        public static void RunAllExamples()
        {
            try
            {
                Example1_BasicUsage();
                Example2_SimpleGraph();
                Example3_PerformanceConcept();

                Console.WriteLine("\n=== All Examples Completed Successfully ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
