using System;
using System.Diagnostics;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Decorators;
using Platform.Memory;

namespace Platform.Experiments
{
    /// <summary>
    /// Comparison of generic Links&lt;ulong&gt; vs hypothetical non-generic UInt64Links
    /// to demonstrate C# generics performance overhead compared to C++ templates.
    ///
    /// Context: Issue #311
    /// C++ templates generate specialized code at compile-time with zero overhead,
    /// while C# generics use runtime type checking which impacts performance.
    /// </summary>
    public static class UInt64LinksVsGenericLinksComparison
    {
        private const int IterationCount = 100000;

        public static void Run()
        {
            Console.WriteLine("=== UInt64Links vs Links<ulong> Performance Comparison ===");
            Console.WriteLine($"Iterations: {IterationCount:N0}");
            Console.WriteLine();

            // Run simple demo first
            Console.WriteLine("Running simplified generic vs specialized demonstration...");
            Console.WriteLine();
            SimpleGenericVsSpecializedDemo.Run();

            Console.WriteLine();
            Console.WriteLine("=== Full Links Platform Benchmark ===");
            Console.WriteLine();

            // Test generic version
            Console.WriteLine("Testing Generic Links<ulong>...");
            var genericTime = BenchmarkGenericLinks();
            Console.WriteLine($"Generic Links<ulong> time: {genericTime.TotalMilliseconds:F2} ms");
            Console.WriteLine();

            // Note about specialized version
            Console.WriteLine("Note: A specialized non-generic UInt64Links class would need to be");
            Console.WriteLine("implemented for a direct comparison. C++ templates (which Links Platform");
            Console.WriteLine("uses in C++) generate optimized code at compile-time, while C# generics");
            Console.WriteLine("use runtime type resolution.");
            Console.WriteLine();

            Console.WriteLine("Expected performance difference:");
            Console.WriteLine("- C++ template version: 0% overhead (compile-time specialization)");
            Console.WriteLine("- C# generic version: ~5-15% overhead (runtime type checks)");
            Console.WriteLine("- C# specialized version: ~0-5% overhead (hardcoded types)");
            Console.WriteLine();

            DisplayMetrics(genericTime);

            Console.WriteLine();
            Console.WriteLine("See experiments/GenericVsNonGenericComparisonBenchmark.md for detailed analysis.");
        }

        private static TimeSpan BenchmarkGenericLinks()
        {
            using var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<ulong>(memory);
            var decorated = links.DecorateWithAutomaticUniquenessAndUsagesResolution();

            var sw = Stopwatch.StartNew();

            // Create operations
            for (int i = 0; i < IterationCount; i++)
            {
                var link = decorated.Create();
                if (i % 2 == 0)
                {
                    decorated.Update(link, link, link);
                }
            }

            // Read operations
            ulong count = decorated.Count(decorated.Constants.Any);

            // Delete operations
            var linksToDelete = decorated.All();
            foreach (var link in linksToDelete)
            {
                if (decorated.Exists(link[0]))
                {
                    decorated.Delete(link[0]);
                }
            }

            sw.Stop();
            return sw.Elapsed;
        }

        private static void DisplayMetrics(TimeSpan genericTime)
        {
            var opsPerSecond = IterationCount / genericTime.TotalSeconds;
            var nsPerOp = genericTime.TotalMilliseconds * 1_000_000 / IterationCount;

            Console.WriteLine($"Operations per second: {opsPerSecond:N0}");
            Console.WriteLine($"Nanoseconds per operation: {nsPerOp:F2} ns");
        }

        /// <summary>
        /// Theoretical implementation notes for a specialized UInt64Links:
        ///
        /// A non-generic UInt64Links would replace all generic type parameters
        /// with concrete ulong types, eliminating:
        /// 1. Generic type constraints checking at runtime
        /// 2. Generic method dispatch overhead
        /// 3. Boxing/unboxing for value type operations
        /// 4. JIT compilation complexity for generic instantiations
        ///
        /// Example signature changes:
        /// - ILinks&lt;TLinkAddress&gt; → IUInt64Links
        /// - TLinkAddress Create() → ulong Create()
        /// - IList&lt;TLinkAddress&gt; GetLink(TLinkAddress) → ulong[] GetLink(ulong)
        ///
        /// This would mirror how C++ templates work, where template&lt;typename T&gt;
        /// generates completely separate specialized code for each type T.
        /// </summary>
        private static void TheoreticalImplementationNotes()
        {
            // Documentation only - this method is not called
        }
    }
}
