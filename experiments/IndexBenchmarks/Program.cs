using System;
using BenchmarkDotNet.Running;

namespace IndexBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Index Benchmarks for LinksPlatform Issue #5");
            Console.WriteLine("Testing alternative forms of index (lists of connections between links)");
            Console.WriteLine();
            Console.WriteLine("Index types being tested:");
            Console.WriteLine("1. Hashtable (Dictionary-based)");
            Console.WriteLine("2. Linked-list (Linear search)");
            Console.WriteLine("3. AVL Tree (Self-balancing binary search tree)");
            Console.WriteLine("4. Skip List (Probabilistic data structure)");
            Console.WriteLine("5. B-Tree (Balanced tree, using SortedDictionary)");
            Console.WriteLine("6. Matrix (Sparse matrix with mapping optimization)");
            Console.WriteLine("7. Bit Index (Bit string with sparse storage)");
            Console.WriteLine();

            var summary = BenchmarkRunner.Run<LinkIndexBenchmarks>();
        }
    }
}
