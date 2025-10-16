using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

namespace MemoryStorageBenchmarks
{
    /// <summary>
    /// Benchmarks comparing different storage strategies for link data:
    /// 1. UnitedMemoryLinks - all data in single memory/file (traditional approach)
    /// 2. SplitMemoryLinks - data separated from indexes (2 files)
    ///
    /// This addresses issue #74: Compare performance of storing all link fields in one file vs one field per file
    /// Related to data-oriented design and parallel array patterns for improved cache locality.
    /// </summary>
    [SimpleJob(RunStrategy.Throughput, launchCount: 1, warmupCount: 3, iterationCount: 5)]
    [MemoryDiagnoser]
    [MarkdownExporter]
    public class StorageStrategyBenchmarks
    {
        private const int N = 10000; // Number of operations per benchmark
        private const string TestDir = "benchmark_temp";

        private UnitedMemoryLinks<uint> _unitedMemory;
        private ILinks<uint> _unitedLinks;

        private SplitMemoryLinks<uint> _splitMemory;
        private ILinks<uint> _splitLinks;

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Clean up any previous test files
            if (Directory.Exists(TestDir))
            {
                Directory.Delete(TestDir, true);
            }
            Directory.CreateDirectory(TestDir);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            // Clean up test files
            if (Directory.Exists(TestDir))
            {
                try
                {
                    Directory.Delete(TestDir, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        [IterationSetup(Target = nameof(UnitedMemory_CreateAndDelete))]
        public void SetupUnited()
        {
            var memory = new HeapResizableDirectMemory();
            _unitedMemory = new UnitedMemoryLinks<uint>(memory);
            _unitedLinks = _unitedMemory.DecorateWithAutomaticUniquenessAndUsagesResolution();
        }

        [IterationCleanup(Target = nameof(UnitedMemory_CreateAndDelete))]
        public void CleanupUnited()
        {
            _unitedMemory?.Dispose();
            _unitedMemory = null;
            _unitedLinks = null;
        }

        [IterationSetup(Target = nameof(SplitMemory_CreateAndDelete))]
        public void SetupSplit()
        {
            var dataMemory = new HeapResizableDirectMemory();
            var indexMemory = new HeapResizableDirectMemory();
            _splitMemory = new SplitMemoryLinks<uint>(dataMemory, indexMemory);
            _splitLinks = _splitMemory.DecorateWithAutomaticUniquenessAndUsagesResolution();
        }

        [IterationCleanup(Target = nameof(SplitMemory_CreateAndDelete))]
        public void CleanupSplit()
        {
            _splitMemory?.Dispose();
            _splitMemory = null;
            _splitLinks = null;
        }

        /// <summary>
        /// Benchmark for UnitedMemoryLinks - all link fields stored in single memory block.
        /// This is the traditional approach where all data (source, target, indexes) are in one file.
        /// </summary>
        [Benchmark(Description = "United: All fields in one file")]
        public void UnitedMemory_CreateAndDelete()
        {
            var random = new Random(12345);
            var createdLinks = new uint[N];

            // Create N random links
            for (int i = 0; i < N; i++)
            {
                uint source = (uint)random.Next(1, 1000);
                uint target = (uint)random.Next(1, 1000);
                createdLinks[i] = _unitedLinks.GetOrCreate(source, target);
            }

            // Delete half of them
            for (int i = 0; i < N / 2; i++)
            {
                try
                {
                    _unitedLinks.Delete(createdLinks[i]);
                }
                catch
                {
                    // Link might already be deleted due to uniqueness
                }
            }

            // Create more links (to test reuse of deleted slots)
            for (int i = 0; i < N / 4; i++)
            {
                uint source = (uint)random.Next(1000, 2000);
                uint target = (uint)random.Next(1000, 2000);
                _unitedLinks.GetOrCreate(source, target);
            }
        }

        /// <summary>
        /// Benchmark for SplitMemoryLinks - data separated from indexes (2 files).
        /// This follows data-oriented design by separating data that's accessed together.
        /// Data file contains: Source and Target values
        /// Index file contains: Tree structures for fast lookups
        /// </summary>
        [Benchmark(Description = "Split: Data + Indexes (2 files)")]
        public void SplitMemory_CreateAndDelete()
        {
            var random = new Random(12345);
            var createdLinks = new uint[N];

            // Create N random links
            for (int i = 0; i < N; i++)
            {
                uint source = (uint)random.Next(1, 1000);
                uint target = (uint)random.Next(1, 1000);
                createdLinks[i] = _splitLinks.GetOrCreate(source, target);
            }

            // Delete half of them
            for (int i = 0; i < N / 2; i++)
            {
                try
                {
                    _splitLinks.Delete(createdLinks[i]);
                }
                catch
                {
                    // Link might already be deleted due to uniqueness
                }
            }

            // Create more links (to test reuse of deleted slots)
            for (int i = 0; i < N / 4; i++)
            {
                uint source = (uint)random.Next(1000, 2000);
                uint target = (uint)random.Next(1000, 2000);
                _splitLinks.GetOrCreate(source, target);
            }
        }

        // Additional benchmarks for read-heavy workloads

        [IterationSetup(Target = nameof(UnitedMemory_ReadHeavy))]
        public void SetupUnitedWithData()
        {
            SetupUnited();
            var random = new Random(12345);
            // Pre-populate with data
            for (int i = 0; i < N; i++)
            {
                uint source = (uint)random.Next(1, 1000);
                uint target = (uint)random.Next(1, 1000);
                _unitedLinks.GetOrCreate(source, target);
            }
        }

        [IterationCleanup(Target = nameof(UnitedMemory_ReadHeavy))]
        public void CleanupUnitedWithData()
        {
            CleanupUnited();
        }

        [IterationSetup(Target = nameof(SplitMemory_ReadHeavy))]
        public void SetupSplitWithData()
        {
            SetupSplit();
            var random = new Random(12345);
            // Pre-populate with data
            for (int i = 0; i < N; i++)
            {
                uint source = (uint)random.Next(1, 1000);
                uint target = (uint)random.Next(1, 1000);
                _splitLinks.GetOrCreate(source, target);
            }
        }

        [IterationCleanup(Target = nameof(SplitMemory_ReadHeavy))]
        public void CleanupSplitWithData()
        {
            CleanupSplit();
        }

        /// <summary>
        /// Read-heavy benchmark for UnitedMemoryLinks.
        /// Tests performance when frequently searching for links.
        /// </summary>
        [Benchmark(Description = "United: Read-heavy workload")]
        public void UnitedMemory_ReadHeavy()
        {
            var random = new Random(54321);
            long sum = 0;

            // Perform many searches
            for (int i = 0; i < N * 2; i++)
            {
                uint source = (uint)random.Next(1, 1000);
                uint target = (uint)random.Next(1, 1000);
                var link = _unitedLinks.SearchOrDefault(source, target);
                sum += link;
            }

            // Prevent optimization from removing the loop
            if (sum < 0) throw new Exception("Impossible");
        }

        /// <summary>
        /// Read-heavy benchmark for SplitMemoryLinks.
        /// Tests whether separating data from indexes improves read performance due to better cache locality.
        /// </summary>
        [Benchmark(Description = "Split: Read-heavy workload")]
        public void SplitMemory_ReadHeavy()
        {
            var random = new Random(54321);
            long sum = 0;

            // Perform many searches
            for (int i = 0; i < N * 2; i++)
            {
                uint source = (uint)random.Next(1, 1000);
                uint target = (uint)random.Next(1, 1000);
                var link = _splitLinks.SearchOrDefault(source, target);
                sum += link;
            }

            // Prevent optimization from removing the loop
            if (sum < 0) throw new Exception("Impossible");
        }

        // Benchmarks for sequential access patterns (better for cache)

        [IterationSetup(Target = nameof(UnitedMemory_SequentialAccess))]
        public void SetupUnitedSequential()
        {
            SetupUnitedWithData();
        }

        [IterationCleanup(Target = nameof(UnitedMemory_SequentialAccess))]
        public void CleanupUnitedSequential()
        {
            CleanupUnited();
        }

        [IterationSetup(Target = nameof(SplitMemory_SequentialAccess))]
        public void SetupSplitSequential()
        {
            SetupSplitWithData();
        }

        [IterationCleanup(Target = nameof(SplitMemory_SequentialAccess))]
        public void CleanupSplitSequential()
        {
            CleanupSplit();
        }

        /// <summary>
        /// Sequential access for UnitedMemoryLinks.
        /// Tests cache-friendly sequential iteration through all links.
        /// </summary>
        [Benchmark(Description = "United: Sequential access")]
        public void UnitedMemory_SequentialAccess()
        {
            long sum = 0;
            var any = _unitedLinks.Constants.Any;
            var @continue = _unitedLinks.Constants.Continue;

            _unitedLinks.Each(link =>
            {
                sum += link[0] + link[1] + link[2]; // Index, Source, Target
                return @continue;
            }, any);

            if (sum < 0) throw new Exception("Impossible");
        }

        /// <summary>
        /// Sequential access for SplitMemoryLinks.
        /// With split storage, this should show if separating data improves cache locality
        /// when accessing only link values (not indexes) sequentially.
        /// </summary>
        [Benchmark(Description = "Split: Sequential access")]
        public void SplitMemory_SequentialAccess()
        {
            long sum = 0;
            var any = _splitLinks.Constants.Any;
            var @continue = _splitLinks.Constants.Continue;

            _splitLinks.Each(link =>
            {
                sum += link[0] + link[1] + link[2]; // Index, Source, Target
                return @continue;
            }, any);

            if (sum < 0) throw new Exception("Impossible");
        }
    }
}
