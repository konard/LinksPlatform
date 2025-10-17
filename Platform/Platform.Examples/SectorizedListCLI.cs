using System;
using System.Diagnostics;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for demonstrating the SectorizedList functionality.
    /// </summary>
    public class SectorizedListCLI : ICommandLineInterface
    {
        /// <summary>
        /// Runs the SectorizedList demonstration with the provided command-line arguments.
        /// </summary>
        /// <param name="args">Command-line arguments (optional).</param>
        public void Run(params string[] args)
        {
            Console.WriteLine("SectorizedList Demonstration");
            Console.WriteLine("============================");
            Console.WriteLine();

            if (args.Length > 0 && int.TryParse(args[0], out int elementCount))
            {
                RunWithCustomSize(elementCount);
            }
            else
            {
                RunDefaultDemo();
            }
        }

        private void RunDefaultDemo()
        {
            Console.WriteLine("Running default demonstration...");
            Console.WriteLine();

            // Demonstrate basic usage
            BasicUsageDemo();
            Console.WriteLine();

            // Demonstrate performance
            PerformanceDemo();
        }

        private void RunWithCustomSize(int elementCount)
        {
            Console.WriteLine($"Running demonstration with {elementCount:N0} elements...");
            Console.WriteLine();

            var list = new SectorizedList<int>(pageSize: 1024);
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < elementCount; i++)
            {
                list.Add(i);
            }

            sw.Stop();

            var stats = list.GetMemoryStats();
            Console.WriteLine($"Added {elementCount:N0} elements in {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"Sectors: {stats.Sectors}");
            Console.WriteLine($"Memory utilization: {(double)stats.UsedElements / stats.AllocatedCapacity * 100:F2}%");
            Console.WriteLine($"Page size: {list.PageSize}");
        }

        private void BasicUsageDemo()
        {
            Console.WriteLine("1. Basic Usage");
            Console.WriteLine("--------------");

            var list = new SectorizedList<string>(pageSize: 4);

            // Add elements
            list.Add("First");
            list.Add("Second");
            list.Add("Third");
            list.Add("Fourth");
            list.Add("Fifth"); // This will create a new sector

            Console.WriteLine($"Count: {list.Count}");
            Console.WriteLine($"Sectors: {list.GetSectorCount()}");
            Console.WriteLine($"Page size: {list.PageSize}");
            Console.WriteLine();

            // Access elements
            Console.WriteLine("Elements:");
            for (int i = 0; i < list.Count; i++)
            {
                Console.WriteLine($"  [{i}] = {list[i]}");
            }
            Console.WriteLine();

            // Enumerate
            Console.Write("Enumeration: ");
            foreach (var item in list)
            {
                Console.Write($"{item} ");
            }
            Console.WriteLine();
        }

        private void PerformanceDemo()
        {
            Console.WriteLine("2. Performance Demonstration");
            Console.WriteLine("----------------------------");

            const int testSize = 100000;
            var list = new SectorizedList<int>(pageSize: 512);
            var sw = Stopwatch.StartNew();

            // Add elements
            for (int i = 0; i < testSize; i++)
            {
                list.Add(i);
            }

            sw.Stop();
            Console.WriteLine($"Added {testSize:N0} elements in {sw.ElapsedMilliseconds} ms");

            // Access elements
            sw.Restart();
            long sum = 0;
            for (int i = 0; i < testSize; i++)
            {
                sum += list[i];
            }
            sw.Stop();
            Console.WriteLine($"Sequential access of {testSize:N0} elements in {sw.ElapsedMilliseconds} ms");

            // Memory stats
            var stats = list.GetMemoryStats();
            Console.WriteLine($"Memory stats:");
            Console.WriteLine($"  Sectors: {stats.Sectors}");
            Console.WriteLine($"  Used elements: {stats.UsedElements:N0}");
            Console.WriteLine($"  Allocated capacity: {stats.AllocatedCapacity:N0}");
            Console.WriteLine($"  Utilization: {(double)stats.UsedElements / stats.AllocatedCapacity * 100:F2}%");
        }
    }
}
