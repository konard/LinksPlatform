using System;
using System.Diagnostics;

namespace Platform.Sandbox
{
    /// <summary>
    /// Experiments demonstrating the bit string variations:
    /// 1. BitStringBitmap - bitmap optimization for faster searching
    /// 2. SectorBasedBitString - sparse storage with on-demand allocation
    /// </summary>
    public static class BitStringExperiments
    {
        /// <summary>
        /// Demonstrates the BitStringBitmap with different granularities.
        /// Shows how bitmap allows skipping empty segments during search.
        /// </summary>
        public static void DemonstrateBitmapOptimization()
        {
            Console.WriteLine("=== BitStringBitmap Demonstration ===\n");

            // Test with different granularities
            int[] granularities = { 8, 64, 512, 4096 };
            int bitStringSize = 1024 * 1024; // 1 Mbit

            foreach (int granularity in granularities)
            {
                Console.WriteLine($"Granularity: {granularity} bits ({granularity / 8} bytes)");

                var bitmap = new BitStringBitmap(bitStringSize, granularity);

                // Set some sparse bits
                var random = new System.Random(42);
                int setBitsCount = 100;
                int[] setBits = new int[setBitsCount];

                for (int i = 0; i < setBitsCount; i++)
                {
                    setBits[i] = random.Next(bitStringSize);
                    bitmap.SetBit(setBits[i]);
                }

                Console.WriteLine($"Bit String Size: {bitmap.BitStringSize} bits ({bitmap.BitStringSize / 8} bytes)");
                Console.WriteLine($"Bitmap Size: {bitmap.BitmapSize} bits ({bitmap.BitmapSize / 8} bytes)");
                Console.WriteLine($"Compression Ratio: {(double)bitmap.BitmapSize / bitmap.BitStringSize:P2}");
                Console.WriteLine($"Set Bits: {bitmap.CountSetBits()}");

                // Performance test: find all set bits
                var sw = Stopwatch.StartNew();
                int foundBits = 0;
                int currentBit = bitmap.FindNextSetBit(0);
                while (currentBit >= 0)
                {
                    foundBits++;
                    currentBit = bitmap.FindNextSetBit(currentBit + 1);
                }
                sw.Stop();

                Console.WriteLine($"Found {foundBits} bits in {sw.ElapsedMilliseconds}ms");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Demonstrates the SectorBasedBitString for sparse bit strings.
        /// Shows memory savings and ability to fill in random order.
        /// </summary>
        public static void DemonstrateSectorBasedStorage()
        {
            Console.WriteLine("=== SectorBasedBitString Demonstration ===\n");

            // Test with different sector sizes
            int[] sectorSizes = {
                SectorBasedBitString.CacheLineSectorSize,  // 64 bytes (cache line)
                512,                                        // 512 bytes
                SectorBasedBitString.DefaultSectorSize      // 4096 bytes (page size)
            };

            long totalBits = 1024L * 1024 * 1024 * 8; // 1 GB worth of bits

            foreach (int sectorSize in sectorSizes)
            {
                Console.WriteLine($"Sector Size: {sectorSize} bytes ({sectorSize * 8} bits)");

                var bitString = new SectorBasedBitString(totalBits, sectorSize);

                // Set bits in random order (sparse pattern)
                var random = new System.Random(42);
                int setBitsCount = 10000;

                for (int i = 0; i < setBitsCount; i++)
                {
                    long bitIndex = (long)random.Next((int)(totalBits / 1000)) * 1000;
                    bitString.SetBit(bitIndex);
                }

                Console.WriteLine(bitString.GetStatistics());
                Console.WriteLine($"Memory Usage: ~{bitString.AllocatedSectorsCount * sectorSize / 1024.0:F2} KB " +
                                $"(vs {totalBits / 8 / 1024 / 1024:F2} MB if fully allocated)");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Compares performance of bitmap vs naive search in sparse bit strings.
        /// </summary>
        public static void CompareBitmapPerformance()
        {
            Console.WriteLine("=== Bitmap Performance Comparison ===\n");

            int bitStringSize = 10 * 1024 * 1024; // 10 Mbit
            int granularity = 512; // Cache line aligned

            var bitmap = new BitStringBitmap(bitStringSize, granularity);

            // Create sparse pattern: set 1000 bits randomly
            var random = new System.Random(42);
            int setBitsCount = 1000;

            Console.WriteLine($"Setting {setBitsCount} random bits in {bitStringSize} bit string...");
            for (int i = 0; i < setBitsCount; i++)
            {
                bitmap.SetBit(random.Next(bitStringSize));
            }

            Console.WriteLine($"Actual set bits after deduplication: {bitmap.CountSetBits()}\n");

            // Test: Find all set bits using bitmap-optimized search
            var sw = Stopwatch.StartNew();
            int found = 0;
            int pos = bitmap.FindNextSetBit(0);
            while (pos >= 0)
            {
                found++;
                pos = bitmap.FindNextSetBit(pos + 1);
            }
            sw.Stop();

            Console.WriteLine($"Bitmap-optimized search:");
            Console.WriteLine($"  Found: {found} bits");
            Console.WriteLine($"  Time: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"  Throughput: {(double)bitStringSize / sw.ElapsedTicks * Stopwatch.Frequency / 1_000_000:F2} Mbit/s");
        }

        /// <summary>
        /// Demonstrates sparse bit string filling in random order.
        /// Shows that sectors can be allocated on demand.
        /// </summary>
        public static void DemonstrateRandomOrderFilling()
        {
            Console.WriteLine("=== Random Order Filling Demonstration ===\n");

            long totalBits = 1024L * 1024 * 1024; // 128 MB worth of bits
            var bitString = new SectorBasedBitString(totalBits, SectorBasedBitString.DefaultSectorSize);

            Console.WriteLine("Filling bit string in random order...\n");

            var random = new System.Random(42);

            // Fill in batches to show progressive allocation
            int[] batchSizes = { 100, 500, 1000, 5000, 10000 };
            int totalSet = 0;

            foreach (int batchSize in batchSizes)
            {
                for (int i = 0; i < batchSize; i++)
                {
                    long bitIndex = (long)random.Next(0, (int)(totalBits / 10000)) * 10000;
                    bitString.SetBit(bitIndex);
                    totalSet++;
                }

                Console.WriteLine($"After setting {totalSet} bits:");
                Console.WriteLine($"  Allocated Sectors: {bitString.AllocatedSectorsCount}/{bitString.TotalSectors}");
                Console.WriteLine($"  Memory Usage: ~{bitString.AllocatedSectorsCount * bitString.SectorSize / 1024.0:F2} KB");
                Console.WriteLine($"  Memory Saved: {bitString.MemoryEfficiency:F2}%");
                Console.WriteLine();
            }

            // Demonstrate compaction
            Console.WriteLine("Clearing some bits and compacting...");
            for (int i = 0; i < 1000; i++)
            {
                long bitIndex = (long)random.Next(0, (int)(totalBits / 10000)) * 10000;
                bitString.ClearBit(bitIndex, false); // Don't auto-deallocate
            }

            int compacted = bitString.Compact();
            Console.WriteLine($"Compacted {compacted} empty sectors");
            Console.WriteLine($"Allocated Sectors after compaction: {bitString.AllocatedSectorsCount}");
        }

        /// <summary>
        /// Demonstrates combined use of bitmap and sector-based storage.
        /// Shows how granularity can be aligned with cache/page sizes.
        /// </summary>
        public static void DemonstrateCombinedOptimization()
        {
            Console.WriteLine("=== Combined Optimization Demonstration ===\n");
            Console.WriteLine("Using bitmap with cache-line aligned granularity\n");

            // Align bitmap granularity with cache line (64 bytes = 512 bits)
            int granularity = 512;
            int bitStringSize = 64 * 1024 * 8; // 64 KB

            var bitmap = new BitStringBitmap(bitStringSize, granularity);

            Console.WriteLine($"Configuration:");
            Console.WriteLine($"  Granularity: {granularity} bits = {granularity / 8} bytes (cache line aligned)");
            Console.WriteLine($"  Bit String: {bitStringSize} bits = {bitStringSize / 8} bytes");
            Console.WriteLine($"  Bitmap: {bitmap.BitmapSize} bits = {bitmap.BitmapSize / 8} bytes");
            Console.WriteLine();

            // Set bits in pattern that benefits from cache alignment
            Console.WriteLine("Setting bits in cache-aligned pattern...");
            for (int i = 0; i < 1000; i++)
            {
                // Set one bit per cache line
                bitmap.SetBit(i * granularity + (i % 64));
            }

            Console.WriteLine($"Set bits: {bitmap.CountSetBits()}");
            Console.WriteLine($"Bitmap compression: {(double)bitmap.BitmapSize / bitmap.BitStringSize:P2}");
        }

        /// <summary>
        /// Runs all bit string experiments.
        /// </summary>
        public static void RunAllExperiments()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║        Bit String Variations Experiments                   ║");
            Console.WriteLine("║        Issue #663: Bitmap and Sector-Based Storage         ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            try
            {
                DemonstrateBitmapOptimization();
                Console.WriteLine("\n" + new string('=', 60) + "\n");

                DemonstrateSectorBasedStorage();
                Console.WriteLine("\n" + new string('=', 60) + "\n");

                CompareBitmapPerformance();
                Console.WriteLine("\n" + new string('=', 60) + "\n");

                DemonstrateRandomOrderFilling();
                Console.WriteLine("\n" + new string('=', 60) + "\n");

                DemonstrateCombinedOptimization();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during experiments: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("All experiments completed!");
        }
    }
}
