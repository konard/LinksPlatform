using System;
using System.Diagnostics;
using System.Linq;
using Platform.Examples;

namespace Experiments
{
    /// <summary>
    /// Experiment to test and demonstrate the SectorizedList implementation.
    /// </summary>
    public static class SectorizedListExperiment
    {
        public static void Run()
        {
            Console.WriteLine("=== Sectorized List Experiment ===");
            Console.WriteLine();

            BasicOperationsTest();
            PerformanceTest();
            MemoryEfficiencyTest();

            Console.WriteLine();
            Console.WriteLine("=== All Experiments Completed ===");
        }

        private static void BasicOperationsTest()
        {
            Console.WriteLine("1. Basic Operations Test");
            Console.WriteLine("------------------------");

            var list = new SectorizedList<int>(pageSize: 8);

            // Add elements
            for (int i = 0; i < 25; i++)
            {
                list.Add(i);
            }

            Console.WriteLine($"Added 25 elements");
            Console.WriteLine($"Count: {list.Count}");
            Console.WriteLine($"Page Size: {list.PageSize}");

            var stats = list.GetMemoryStats();
            Console.WriteLine($"Memory Stats: Sectors={stats.Sectors}, Used={stats.UsedElements}, Allocated={stats.AllocatedCapacity}");

            // Test indexer
            Console.WriteLine($"Element at index 0: {list[0]}");
            Console.WriteLine($"Element at index 10: {list[10]}");
            Console.WriteLine($"Element at index 24: {list[24]}");

            // Test Contains
            Console.WriteLine($"Contains 10: {list.Contains(10)}");
            Console.WriteLine($"Contains 100: {list.Contains(100)}");

            // Test enumeration
            Console.Write("First 10 elements: ");
            foreach (var item in list.Take(10))
            {
                Console.Write($"{item} ");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        private static void PerformanceTest()
        {
            Console.WriteLine("2. Performance Test");
            Console.WriteLine("-------------------");

            var sw = Stopwatch.StartNew();
            var list = new SectorizedList<long>(pageSize: 512);

            // Add 1 million elements
            const int elementCount = 1_000_000;
            for (int i = 0; i < elementCount; i++)
            {
                list.Add(i);
            }

            sw.Stop();
            Console.WriteLine($"Added {elementCount:N0} elements in {sw.ElapsedMilliseconds} ms");

            // Test random access
            sw.Restart();
            long sum = 0;
            var random = new Random(42);
            for (int i = 0; i < 10000; i++)
            {
                int index = random.Next(elementCount);
                sum += list[index];
            }
            sw.Stop();
            Console.WriteLine($"Random access of 10,000 elements in {sw.ElapsedMilliseconds} ms (sum: {sum})");

            // Test sequential access
            sw.Restart();
            sum = 0;
            foreach (var item in list.Take(100000))
            {
                sum += item;
            }
            sw.Stop();
            Console.WriteLine($"Sequential access of 100,000 elements in {sw.ElapsedMilliseconds} ms (sum: {sum})");
            Console.WriteLine();
        }

        private static void MemoryEfficiencyTest()
        {
            Console.WriteLine("3. Memory Efficiency Test");
            Console.WriteLine("-------------------------");

            // Test with different page sizes
            int[] pageSizes = { 64, 256, 1024, 4096 };
            const int elements = 10000;

            foreach (var pageSize in pageSizes)
            {
                var list = new SectorizedList<int>(pageSize);

                for (int i = 0; i < elements; i++)
                {
                    list.Add(i);
                }

                var stats = list.GetMemoryStats();
                double utilizationPercent = (double)stats.UsedElements / stats.AllocatedCapacity * 100;

                Console.WriteLine($"Page Size: {pageSize,4} | Sectors: {stats.Sectors,3} | " +
                                  $"Utilization: {utilizationPercent:F2}% " +
                                  $"({stats.UsedElements}/{stats.AllocatedCapacity})");
            }

            Console.WriteLine();
        }

        public static void RunQuickTest()
        {
            Console.WriteLine("Quick Sectorized List Test:");

            var list = new SectorizedList<string>(pageSize: 4);
            list.Add("Hello");
            list.Add("World");
            list.Add("Sectorized");
            list.Add("List");
            list.Add("Test");

            Console.WriteLine($"Count: {list.Count}");
            Console.WriteLine($"Sectors: {list.GetSectorCount()}");
            Console.Write("Elements: ");
            foreach (var item in list)
            {
                Console.Write($"{item} ");
            }
            Console.WriteLine();
        }
    }
}
