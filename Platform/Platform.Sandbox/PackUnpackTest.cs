using System;
using System.IO;
using System.Threading;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;
using Platform.Examples;

namespace Platform.Sandbox
{
    /// <summary>
    /// Test and demonstration of Pack/Unpack functionality.
    /// </summary>
    public static class PackUnpackTest
    {
        public static void Run()
        {
            Console.WriteLine("=== Pack/Unpack Test ===\n");

            // Create temp files
            var originalDb = Path.GetTempFileName();
            var packedFile = Path.GetTempFileName() + ".pack";
            var restoredDb = Path.GetTempFileName();

            try
            {
                // Step 1: Create a sample database with some links
                Console.WriteLine("Step 1: Creating sample database...");
                CreateSampleDatabase(originalDb);
                Console.WriteLine($"Created database: {originalDb}\n");

                // Step 2: Pack the database
                Console.WriteLine("Step 2: Packing database...");
                PackDatabase(originalDb, packedFile);
                Console.WriteLine($"Packed to: {packedFile}");
                Console.WriteLine($"Packed file size: {new FileInfo(packedFile).Length} bytes\n");

                // Step 3: Unpack to a new database
                Console.WriteLine("Step 3: Unpacking to new database...");
                UnpackDatabase(packedFile, restoredDb);
                Console.WriteLine($"Restored to: {restoredDb}\n");

                // Step 4: Verify the restored database
                Console.WriteLine("Step 4: Verifying restored database...");
                VerifyDatabase(restoredDb);
                Console.WriteLine("Verification complete!\n");

                Console.WriteLine("=== Test Passed Successfully! ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n=== Test Failed ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                // Cleanup
                TryDeleteFile(originalDb);
                TryDeleteFile(packedFile);
                TryDeleteFile(restoredDb);
            }
        }

        private static void CreateSampleDatabase(string dbPath)
        {
            using (var memoryAdapter = new UInt64UnitedMemoryLinks(dbPath))
            using (var links = new UInt64Links(memoryAdapter))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);

                // Create some sample links
                // Point links (self-referencing)
                var point1 = syncLinks.Create();
                var point2 = syncLinks.Create();
                var point3 = syncLinks.Create();

                // Links between points
                syncLinks.GetOrCreate(point1, point2);
                syncLinks.GetOrCreate(point2, point3);
                syncLinks.GetOrCreate(point3, point1);
                syncLinks.GetOrCreate(point1, point3);

                // Some more complex structures
                var link1 = syncLinks.GetOrCreate(point1, point2);
                var link2 = syncLinks.GetOrCreate(point2, point3);
                syncLinks.GetOrCreate(link1, link2);

                ulong linkCount = 0;
                syncLinks.Each(link => { linkCount++; return syncLinks.Constants.Continue; });
                Console.WriteLine($"Created {linkCount} links");
            }
        }

        private static void PackDatabase(string dbPath, string packPath)
        {
            using (var cancellation = new CancellationTokenSource())
            using (var memoryAdapter = new UInt64UnitedMemoryLinks(dbPath))
            using (var links = new UInt64Links(memoryAdapter))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);
                var packer = new LinksPacker();

                // Use auto-detect for bytes per index
                packer.Pack(syncLinks, packPath, 0, cancellation.Token);

                ulong linkCount = 0;
                syncLinks.Each(link => { linkCount++; return syncLinks.Constants.Continue; });
                Console.WriteLine($"Packed {linkCount} links");
            }
        }

        private static void UnpackDatabase(string packPath, string dbPath)
        {
            using (var cancellation = new CancellationTokenSource())
            using (var memoryAdapter = new UInt64UnitedMemoryLinks(dbPath))
            using (var links = new UInt64Links(memoryAdapter))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);
                var unpacker = new LinksUnpacker();

                var restoredCount = unpacker.Unpack(syncLinks, packPath, cancellation.Token);

                Console.WriteLine($"Restored {restoredCount} links");
            }
        }

        private static void VerifyDatabase(string dbPath)
        {
            using (var memoryAdapter = new UInt64UnitedMemoryLinks(dbPath))
            using (var links = new UInt64Links(memoryAdapter))
            {
                var syncLinks = new SynchronizedLinks<ulong>(links);

                ulong count = 0;
                syncLinks.Each(link => { count++; return syncLinks.Constants.Continue; });

                Console.WriteLine($"Database contains {count} links");

                // Verify we can read all links
                ulong checkedLinks = 0;
                syncLinks.Each(link =>
                {
                    var index = link[syncLinks.Constants.IndexPart];
                    var source = link[syncLinks.Constants.SourcePart];
                    var target = link[syncLinks.Constants.TargetPart];

                    // Verify link has valid structure
                    if (index == 0 || source == 0 || target == 0)
                    {
                        throw new InvalidDataException($"Invalid link: [{index}:{source}->{target}]");
                    }

                    checkedLinks++;
                    return syncLinks.Constants.Continue;
                });

                if (checkedLinks != count)
                {
                    throw new InvalidDataException($"Count mismatch: expected {count} but verified {checkedLinks}");
                }

                Console.WriteLine($"Verified {checkedLinks} links successfully");
            }
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
