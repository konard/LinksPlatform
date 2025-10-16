using System;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;
using Platform.Data.Core.Pairs;

namespace Platform.Data.Core.Tests
{
    /// <summary>
    /// Performance comparison tests for different memory management strategies.
    /// Compares the GC-based Links2 implementation with potential Memory-Mapped Files implementations.
    /// </summary>
    public class PerformanceComparisonTests
    {
        private readonly ITestOutputHelper _output;

        public PerformanceComparisonTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Compare_AllocationPerformance_GarbageCollector()
        {
            // Arrange
            const int iterations = 10000;
            using var links = new Links2<ulong>();
            var stopwatch = Stopwatch.StartNew();

            // Act
            for (int i = 0; i < iterations; i++)
            {
                links.Allocate();
            }
            stopwatch.Stop();

            // Assert & Report
            var avgTimePerAllocation = stopwatch.Elapsed.TotalMilliseconds / iterations;
            _output.WriteLine($"GC-based Allocation Performance:");
            _output.WriteLine($"  Total time for {iterations} allocations: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
            _output.WriteLine($"  Average time per allocation: {avgTimePerAllocation:F6} ms");
            _output.WriteLine($"  Allocations per second: {(iterations / stopwatch.Elapsed.TotalSeconds):F0}");

            // Performance should be reasonable (less than 1ms per allocation on average)
            Assert.True(avgTimePerAllocation < 1.0,
                $"Allocation performance is too slow: {avgTimePerAllocation:F6} ms per allocation");
        }

        [Fact]
        public void Compare_ReadWritePerformance_GarbageCollector()
        {
            // Arrange
            const int iterations = 10000;
            using var links = new Links2<ulong>();

            // Pre-allocate links
            var testLinks = new ulong[iterations];
            for (int i = 0; i < iterations; i++)
            {
                testLinks[i] = links.Allocate();
            }

            var stopwatch = Stopwatch.StartNew();

            // Act - Write operations
            for (int i = 0; i < iterations; i++)
            {
                links.SetSource(testLinks[i], (ulong)(i % 100));
                links.SetTarget(testLinks[i], (ulong)((i + 1) % 100));
            }

            var writeTime = stopwatch.Elapsed;

            // Read operations
            stopwatch.Restart();
            for (int i = 0; i < iterations; i++)
            {
                _ = links.GetSource(testLinks[i]);
                _ = links.GetTarget(testLinks[i]);
            }
            stopwatch.Stop();

            var readTime = stopwatch.Elapsed;

            // Assert & Report
            _output.WriteLine($"GC-based Read/Write Performance:");
            _output.WriteLine($"  Write time for {iterations} operations: {writeTime.TotalMilliseconds:F2} ms");
            _output.WriteLine($"  Read time for {iterations} operations: {readTime.TotalMilliseconds:F2} ms");
            _output.WriteLine($"  Average write time: {(writeTime.TotalMilliseconds / iterations):F6} ms");
            _output.WriteLine($"  Average read time: {(readTime.TotalMilliseconds / iterations):F6} ms");
            _output.WriteLine($"  Writes per second: {(iterations / writeTime.TotalSeconds):F0}");
            _output.WriteLine($"  Reads per second: {(iterations / readTime.TotalSeconds):F0}");

            Assert.True(writeTime.TotalMilliseconds / iterations < 1.0);
            Assert.True(readTime.TotalMilliseconds / iterations < 1.0);
        }

        [Fact]
        public void Compare_AllocationAndFreePerformance_GarbageCollector()
        {
            // Arrange
            const int iterations = 5000;
            using var links = new Links2<ulong>();
            var stopwatch = Stopwatch.StartNew();

            // Act - Allocate and free repeatedly
            for (int i = 0; i < iterations; i++)
            {
                var link = links.Allocate();
                links.Free(link);
            }
            stopwatch.Stop();

            // Assert & Report
            var avgTimePerCycle = stopwatch.Elapsed.TotalMilliseconds / iterations;
            _output.WriteLine($"GC-based Allocation/Free Cycle Performance:");
            _output.WriteLine($"  Total time for {iterations} cycles: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
            _output.WriteLine($"  Average time per cycle: {avgTimePerCycle:F6} ms");
            _output.WriteLine($"  Cycles per second: {(iterations / stopwatch.Elapsed.TotalSeconds):F0}");

            Assert.True(avgTimePerCycle < 1.0,
                $"Allocation/Free cycle is too slow: {avgTimePerCycle:F6} ms per cycle");
        }

        [Fact]
        public void Measure_MemoryFootprint_GarbageCollector()
        {
            // Arrange
            const int linkCount = 10000;
            var initialMemory = GC.GetTotalMemory(forceFullCollection: true);

            // Act
            using (var links = new Links2<ulong>(linkCount))
            {
                for (int i = 0; i < linkCount; i++)
                {
                    var link = links.Allocate();
                    links.SetSource(link, (ulong)(i % 1000));
                    links.SetTarget(link, (ulong)((i + 1) % 1000));
                }

                var memoryAfterAllocation = GC.GetTotalMemory(forceFullCollection: false);
                var memoryUsed = memoryAfterAllocation - initialMemory;

                // Report
                _output.WriteLine($"GC-based Memory Footprint:");
                _output.WriteLine($"  Links allocated: {linkCount}");
                _output.WriteLine($"  Memory used: {memoryUsed:N0} bytes");
                _output.WriteLine($"  Bytes per link: {(double)memoryUsed / linkCount:F2}");
                _output.WriteLine($"  MB used: {memoryUsed / (1024.0 * 1024.0):F2} MB");

                // Assert - Memory usage should be reasonable
                var bytesPerLink = (double)memoryUsed / linkCount;
                Assert.True(bytesPerLink < 1000,
                    $"Memory usage per link is too high: {bytesPerLink:F2} bytes per link");
            }

            // Force cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var finalMemory = GC.GetTotalMemory(forceFullCollection: true);
            var memoryFreed = initialMemory - finalMemory;

            _output.WriteLine($"  Memory freed after disposal: {Math.Abs(memoryFreed):N0} bytes");
        }

        [Fact]
        public void Compare_LargeScale_AllocationPerformance()
        {
            // Arrange
            const int linkCount = 100000;
            using var links = new Links2<ulong>(linkCount);
            var stopwatch = Stopwatch.StartNew();

            // Act
            for (int i = 0; i < linkCount; i++)
            {
                links.Allocate();
            }
            stopwatch.Stop();

            // Assert & Report
            _output.WriteLine($"Large-Scale GC-based Allocation Performance:");
            _output.WriteLine($"  Total links allocated: {linkCount:N0}");
            _output.WriteLine($"  Total time: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
            _output.WriteLine($"  Average time per allocation: {(stopwatch.Elapsed.TotalMilliseconds / linkCount):F6} ms");
            _output.WriteLine($"  Allocations per second: {(linkCount / stopwatch.Elapsed.TotalSeconds):F0}");

            // Should complete in reasonable time (< 10 seconds for 100k allocations)
            Assert.True(stopwatch.Elapsed.TotalSeconds < 10,
                $"Large-scale allocation took too long: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
        }

        [Fact]
        public void Compare_ComplexGraphConstruction_Performance()
        {
            // Arrange
            const int nodeCount = 1000;
            const int edgesPerNode = 10;
            using var links = new Links2<ulong>();

            // Create nodes
            var nodes = new ulong[nodeCount];
            for (int i = 0; i < nodeCount; i++)
            {
                nodes[i] = links.Allocate();
            }

            var stopwatch = Stopwatch.StartNew();

            // Act - Create edges between nodes
            var random = new Random(42); // Fixed seed for reproducibility
            for (int i = 0; i < nodeCount; i++)
            {
                for (int j = 0; j < edgesPerNode; j++)
                {
                    var edge = links.Allocate();
                    var targetNode = nodes[random.Next(nodeCount)];
                    links.SetSource(edge, nodes[i]);
                    links.SetTarget(edge, targetNode);
                }
            }
            stopwatch.Stop();

            var totalEdges = nodeCount * edgesPerNode;

            // Assert & Report
            _output.WriteLine($"Complex Graph Construction Performance:");
            _output.WriteLine($"  Nodes: {nodeCount}");
            _output.WriteLine($"  Edges per node: {edgesPerNode}");
            _output.WriteLine($"  Total edges: {totalEdges}");
            _output.WriteLine($"  Construction time: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
            _output.WriteLine($"  Time per edge: {(stopwatch.Elapsed.TotalMilliseconds / totalEdges):F6} ms");
            _output.WriteLine($"  Edges per second: {(totalEdges / stopwatch.Elapsed.TotalSeconds):F0}");

            Assert.True(stopwatch.Elapsed.TotalSeconds < 5,
                $"Graph construction took too long: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
        }
    }
}
