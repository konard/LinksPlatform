using System;
using System.IO;
using Platform.IO;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;
using Platform.Exceptions;

namespace Platform.Examples
{
    /// <summary>
    /// CLI for demonstrating memory page access visualization with Links database operations.
    /// </summary>
    public class MemoryPageAccessVisualizerCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            try
            {
                Console.WriteLine("=== Memory Page Access Visualizer ===");
                Console.WriteLine("This tool helps visualize memory page access patterns during database operations.");
                Console.WriteLine();

                var dbPath = args.Length > 0 ? args[0] : "test.links";
                var operationsCount = args.Length > 1 && int.TryParse(args[1], out int count) ? count : 10000;
                var outputPath = args.Length > 2 ? args[2] : "memory-access-report.csv";

                Console.WriteLine($"Database file: {dbPath}");
                Console.WriteLine($"Operations to perform: {operationsCount}");
                Console.WriteLine($"Output file: {outputPath}");
                Console.WriteLine();

                var visualizer = new MemoryPageAccessVisualizer();

                // Demonstrate with a simple in-memory test if no DB specified
                if (!File.Exists(dbPath) && args.Length == 0)
                {
                    Console.WriteLine("Running demonstration with sample data...");
                    RunDemonstration(visualizer, operationsCount);
                }
                else
                {
                    Console.WriteLine("Running with Links database...");
                    RunWithLinksDatabase(visualizer, dbPath, operationsCount);
                }

                Console.WriteLine();
                visualizer.PrintSummary();

                Console.WriteLine();
                Console.WriteLine(visualizer.GenerateHeatmap());

                Console.WriteLine();
                Console.WriteLine($"Exporting detailed data to {outputPath}...");
                visualizer.ExportToCSV(outputPath);
                Console.WriteLine("Export completed.");

                Console.WriteLine();
                Console.WriteLine("Visualization complete. Use the CSV file for detailed analysis.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred:");
                Console.Write(ex.ToStringWithAllInnerExceptions());
            }
        }

        private void RunDemonstration(MemoryPageAccessVisualizer visualizer, int operationsCount)
        {
            var random = new System.Random(42);

            // Simulate memory accesses in different patterns
            Console.WriteLine("Simulating sequential access pattern...");
            var baseAddress = 0x10000000L;
            for (int i = 0; i < operationsCount / 3; i++)
            {
                var address = new IntPtr(baseAddress + i * 64);
                visualizer.RecordAccess(address, "sequential-read");
            }

            Console.WriteLine("Simulating random access pattern...");
            for (int i = 0; i < operationsCount / 3; i++)
            {
                var offset = random.Next(0, 1000000) * 8;
                var address = new IntPtr(baseAddress + offset);
                visualizer.RecordAccess(address, "random-read");
            }

            Console.WriteLine("Simulating hot-spot access pattern...");
            var hotSpot1 = new IntPtr(baseAddress + 50000);
            var hotSpot2 = new IntPtr(baseAddress + 150000);
            for (int i = 0; i < operationsCount / 3; i++)
            {
                var hotSpot = random.NextDouble() < 0.7 ? hotSpot1 : hotSpot2;
                var offset = random.Next(-1000, 1000) * 8;
                var address = new IntPtr(hotSpot.ToInt64() + offset);
                visualizer.RecordAccess(address, "hotspot-access");
            }
        }

        private void RunWithLinksDatabase(MemoryPageAccessVisualizer visualizer, string dbPath, int operationsCount)
        {
            // Create or open database
            using (var memoryAdapter = new UInt64UnitedMemoryLinks(dbPath, UInt64UnitedMemoryLinks.DefaultLinksSizeStep))
            using (var links = new UInt64Links(memoryAdapter))
            {
                var random = new System.Random(42);

                // Get base address if available
                var baseAddress = IntPtr.Zero;
                try
                {
                    var pointerProperty = memoryAdapter.GetType().GetProperty("Pointer");
                    if (pointerProperty != null)
                    {
                        var pointer = pointerProperty.GetValue(memoryAdapter);
                        if (pointer is IntPtr ptr)
                        {
                            baseAddress = ptr;
                            Console.WriteLine($"Database base address: 0x{baseAddress.ToInt64():X}");
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Could not retrieve base memory address.");
                }

                Console.WriteLine($"Initial links count: {links.Count()}");
                Console.WriteLine();

                // Perform various operations and track memory access
                Console.WriteLine("Creating links...");
                var createdLinks = new ulong[Math.Min(100, operationsCount / 10)];
                for (int i = 0; i < createdLinks.Length; i++)
                {
                    var source = (ulong)(random.Next(1, 100));
                    var target = (ulong)(random.Next(1, 100));
                    createdLinks[i] = links.GetOrCreate(source, target);

                    if (baseAddress != IntPtr.Zero)
                    {
                        // Estimate address based on link index
                        var linkAddress = new IntPtr(baseAddress.ToInt64() + (long)createdLinks[i] * 16);
                        visualizer.RecordAccess(linkAddress, "create-link");
                    }
                }

                Console.WriteLine("Reading links (sequential)...");
                for (ulong i = 1; i <= Math.Min((ulong)operationsCount / 10, links.Count()); i++)
                {
                    if (links.Exists(i))
                    {
                        var link = links.GetLink(i);

                        if (baseAddress != IntPtr.Zero)
                        {
                            var linkAddress = new IntPtr(baseAddress.ToInt64() + (long)i * 16);
                            visualizer.RecordAccess(linkAddress, "sequential-read");
                        }
                    }
                }

                Console.WriteLine("Reading links (random)...");
                for (int i = 0; i < Math.Min(operationsCount / 2, createdLinks.Length * 10); i++)
                {
                    var linkIndex = createdLinks[random.Next(createdLinks.Length)];
                    if (links.Exists(linkIndex))
                    {
                        var link = links.GetLink(linkIndex);

                        if (baseAddress != IntPtr.Zero)
                        {
                            var linkAddress = new IntPtr(baseAddress.ToInt64() + (long)linkIndex * 16);
                            visualizer.RecordAccess(linkAddress, "random-read");
                        }
                    }
                }

                Console.WriteLine("Searching for links...");
                for (int i = 0; i < Math.Min(operationsCount / 5, 100); i++)
                {
                    var source = (ulong)(random.Next(1, 100));
                    var target = (ulong)(random.Next(1, 100));

                    links.Each(link =>
                    {
                        if (baseAddress != IntPtr.Zero)
                        {
                            var linkAddress = new IntPtr(baseAddress.ToInt64() + (long)link[links.Constants.IndexPart] * 16);
                            visualizer.RecordAccess(linkAddress, "search");
                        }
                        return links.Constants.Continue;
                    }, new Link<ulong>(links.Constants.Any, source, target));
                }

                Console.WriteLine($"Final links count: {links.Count()}");
            }
        }
    }
}
