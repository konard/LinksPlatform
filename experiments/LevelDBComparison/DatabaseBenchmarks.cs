using System;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using LevelDB;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace LevelDBComparison
{
    /// <summary>
    /// Performance comparison benchmarks between Google's LevelDB and LinksPlatform
    /// This benchmark measures key-value storage operations for both systems
    ///
    /// LinksPlatform uses a doublet model (source-target pairs) which is similar to key-value
    /// but with additional capabilities for creating associative data structures.
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class DatabaseBenchmarks
    {
        private const int OperationsCount = 10000;
        private string _levelDbPath;
        private string _linksDbPath;
        private DB _levelDb;
        private UnitedMemoryLinks<ulong> _links;
        private byte[][] _testKeys;
        private byte[][] _testValues;

        [GlobalSetup]
        public void Setup()
        {
            // Setup paths
            _levelDbPath = Path.Combine(Path.GetTempPath(), "leveldb_benchmark");
            _linksDbPath = Path.Combine(Path.GetTempPath(), "links_benchmark.links");

            // Clean up previous test data
            CleanupPreviousData();

            // Prepare test data
            PrepareTestData();

            // Initialize LevelDB
            var options = new Options { CreateIfMissing = true };
            _levelDb = new DB(options, _levelDbPath);

            // Initialize LinksPlatform
            _links = new UnitedMemoryLinks<ulong>(_linksDbPath);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _levelDb?.Dispose();
            _links?.Dispose();
            CleanupPreviousData();
        }

        private void CleanupPreviousData()
        {
            try
            {
                if (Directory.Exists(_levelDbPath))
                    Directory.Delete(_levelDbPath, true);
                if (File.Exists(_linksDbPath))
                    File.Delete(_linksDbPath);
            }
            catch { }
        }

        private void PrepareTestData()
        {
            _testKeys = new byte[OperationsCount][];
            _testValues = new byte[OperationsCount][];

            for (int i = 0; i < OperationsCount; i++)
            {
                _testKeys[i] = Encoding.UTF8.GetBytes($"key_{i:D8}");
                _testValues[i] = Encoding.UTF8.GetBytes($"value_{i:D8}_data_payload");
            }
        }

        /// <summary>
        /// Benchmark sequential write operations for LevelDB
        /// </summary>
        [Benchmark(Description = "LevelDB Sequential Writes")]
        public void LevelDB_SequentialWrites()
        {
            for (int i = 0; i < OperationsCount; i++)
            {
                _levelDb.Put(_testKeys[i], _testValues[i]);
            }
        }

        /// <summary>
        /// Benchmark sequential write operations for LinksPlatform
        /// Note: Links uses a doublet model (source-target pairs) rather than key-value
        /// We create links where each pair represents a key-value relationship
        /// </summary>
        [Benchmark(Description = "LinksPlatform Sequential Writes")]
        public void Links_SequentialWrites()
        {
            for (int i = 0; i < OperationsCount; i++)
            {
                // In LinksPlatform, we create links using numeric indices
                // Each link is a doublet (source, target) similar to (key, value)
                ulong source = (ulong)(i + 1);
                ulong target = (ulong)(i + 1000000);
                var link = _links.Create();
                _links.Update(link, source, target);
            }
        }

        /// <summary>
        /// Benchmark batch write operations for LevelDB
        /// </summary>
        [Benchmark(Description = "LevelDB Batch Writes")]
        public void LevelDB_BatchWrites()
        {
            using (var batch = new WriteBatch())
            {
                for (int i = 0; i < OperationsCount; i++)
                {
                    batch.Put(_testKeys[i], _testValues[i]);
                }
                _levelDb.Write(batch);
            }
        }

        /// <summary>
        /// Benchmark sequential read operations for LevelDB
        /// </summary>
        [Benchmark(Description = "LevelDB Sequential Reads")]
        public void LevelDB_SequentialReads()
        {
            // First write the data
            for (int i = 0; i < OperationsCount; i++)
            {
                _levelDb.Put(_testKeys[i], _testValues[i]);
            }

            // Then read it
            for (int i = 0; i < OperationsCount; i++)
            {
                var value = _levelDb.Get(_testKeys[i]);
            }
        }

        /// <summary>
        /// Benchmark sequential read operations for LinksPlatform
        /// </summary>
        [Benchmark(Description = "LinksPlatform Sequential Reads")]
        public void Links_SequentialReads()
        {
            // First create the links
            var linkIds = new ulong[OperationsCount];
            for (int i = 0; i < OperationsCount; i++)
            {
                ulong source = (ulong)(i + 1);
                ulong target = (ulong)(i + 1000000);
                var link = _links.Create();
                linkIds[i] = _links.Update(link, source, target);
            }

            // Then read them
            for (int i = 0; i < OperationsCount; i++)
            {
                var link = _links.GetLink(linkIds[i]);
            }
        }

        /// <summary>
        /// Benchmark random read operations for LevelDB
        /// </summary>
        [Benchmark(Description = "LevelDB Random Reads")]
        public void LevelDB_RandomReads()
        {
            // First write the data
            for (int i = 0; i < OperationsCount; i++)
            {
                _levelDb.Put(_testKeys[i], _testValues[i]);
            }

            // Then read randomly
            var random = new Random(42); // Fixed seed for reproducibility
            for (int i = 0; i < OperationsCount; i++)
            {
                int index = random.Next(OperationsCount);
                var value = _levelDb.Get(_testKeys[index]);
            }
        }

        /// <summary>
        /// Benchmark random read operations for LinksPlatform
        /// </summary>
        [Benchmark(Description = "LinksPlatform Random Reads")]
        public void Links_RandomReads()
        {
            // First create the links
            var linkIds = new ulong[OperationsCount];
            for (int i = 0; i < OperationsCount; i++)
            {
                ulong source = (ulong)(i + 1);
                ulong target = (ulong)(i + 1000000);
                var link = _links.Create();
                linkIds[i] = _links.Update(link, source, target);
            }

            // Then read randomly
            var random = new Random(42); // Fixed seed for reproducibility
            for (int i = 0; i < OperationsCount; i++)
            {
                int index = random.Next(OperationsCount);
                var link = _links.GetLink(linkIds[index]);
            }
        }

        /// <summary>
        /// Benchmark delete operations for LevelDB
        /// </summary>
        [Benchmark(Description = "LevelDB Sequential Deletes")]
        public void LevelDB_SequentialDeletes()
        {
            // First write the data
            for (int i = 0; i < OperationsCount; i++)
            {
                _levelDb.Put(_testKeys[i], _testValues[i]);
            }

            // Then delete it
            for (int i = 0; i < OperationsCount; i++)
            {
                _levelDb.Delete(_testKeys[i]);
            }
        }

        /// <summary>
        /// Benchmark delete operations for LinksPlatform
        /// </summary>
        [Benchmark(Description = "LinksPlatform Sequential Deletes")]
        public void Links_SequentialDeletes()
        {
            // First create the links
            var linkIds = new ulong[OperationsCount];
            for (int i = 0; i < OperationsCount; i++)
            {
                ulong source = (ulong)(i + 1);
                ulong target = (ulong)(i + 1000000);
                var link = _links.Create();
                linkIds[i] = _links.Update(link, source, target);
            }

            // Then delete them
            for (int i = 0; i < OperationsCount; i++)
            {
                _links.Delete(linkIds[i]);
            }
        }

        /// <summary>
        /// Benchmark iteration over all records for LevelDB
        /// </summary>
        [Benchmark(Description = "LevelDB Full Iteration")]
        public void LevelDB_FullIteration()
        {
            // First ensure data exists
            for (int i = 0; i < OperationsCount; i++)
            {
                _levelDb.Put(_testKeys[i], _testValues[i]);
            }

            // Iterate through all records
            int count = 0;
            using (var iterator = _levelDb.CreateIterator())
            {
                iterator.SeekToFirst();
                while (iterator.IsValid())
                {
                    var key = iterator.Key();
                    var value = iterator.Value();
                    count++;
                    iterator.Next();
                }
            }
        }

        /// <summary>
        /// Benchmark iteration over all records for LinksPlatform
        /// </summary>
        [Benchmark(Description = "LinksPlatform Full Iteration")]
        public void Links_FullIteration()
        {
            // First ensure data exists
            for (int i = 0; i < OperationsCount; i++)
            {
                ulong source = (ulong)(i + 1);
                ulong target = (ulong)(i + 1000000);
                var link = _links.Create();
                _links.Update(link, source, target);
            }

            // Iterate through all links
            int count = 0;
            var any = _links.Constants.Any;
            _links.Each((link) =>
            {
                count++;
                return _links.Constants.Continue;
            }, new Link<ulong>(any, any, any));
        }

        /// <summary>
        /// Benchmark mixed read/write operations for LevelDB
        /// </summary>
        [Benchmark(Description = "LevelDB Mixed Operations")]
        public void LevelDB_MixedOperations()
        {
            var random = new Random(42);
            for (int i = 0; i < OperationsCount; i++)
            {
                if (random.Next(2) == 0)
                {
                    // Write
                    _levelDb.Put(_testKeys[i], _testValues[i]);
                }
                else
                {
                    // Read
                    try { var value = _levelDb.Get(_testKeys[i]); } catch { }
                }
            }
        }

        /// <summary>
        /// Benchmark mixed read/write operations for LinksPlatform
        /// </summary>
        [Benchmark(Description = "LinksPlatform Mixed Operations")]
        public void Links_MixedOperations()
        {
            var random = new Random(42);
            var linkIds = new ulong[OperationsCount];

            for (int i = 0; i < OperationsCount; i++)
            {
                if (random.Next(2) == 0)
                {
                    // Write
                    ulong source = (ulong)(i + 1);
                    ulong target = (ulong)(i + 1000000);
                    var link = _links.Create();
                    linkIds[i] = _links.Update(link, source, target);
                }
                else
                {
                    // Read
                    if (linkIds[i] != 0)
                    {
                        var link = _links.GetLink(linkIds[i]);
                    }
                }
            }
        }
    }
}
