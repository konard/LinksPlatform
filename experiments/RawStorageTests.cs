using System;
using System.IO;

namespace Platform.Data.Core.RawStorage.Tests
{
    /// <summary>
    /// Simple tests for raw storage implementation.
    /// In a production environment, these would use a proper testing framework like xUnit or NUnit.
    /// </summary>
    public class RawStorageTests
    {
        private static int _testsPassed = 0;
        private static int _testsFailed = 0;

        /// <summary>
        /// Test: Create and dispose a file-backed raw block device.
        /// </summary>
        public static void Test_CreateFileBackedDevice()
        {
            const string testFile = "test_device.raw";
            const long deviceSize = 1024 * 1024; // 1 MB
            const int blockSize = 4096;

            try
            {
                using (var device = new FileBackedRawBlockDevice(testFile, deviceSize, blockSize, createNew: true))
                {
                    Assert(device.SizeInBytes == deviceSize, "Device size should match requested size");
                    Assert(device.BlockSize == blockSize, "Block size should match requested block size");
                    Assert(File.Exists(testFile), "Device file should be created");
                }

                // File should still exist after disposal
                Assert(File.Exists(testFile), "Device file should persist after disposal");
                File.Delete(testFile);

                TestPassed("Test_CreateFileBackedDevice");
            }
            catch (Exception ex)
            {
                TestFailed("Test_CreateFileBackedDevice", ex.Message);
                if (File.Exists(testFile)) File.Delete(testFile);
            }
        }

        /// <summary>
        /// Test: Write and read aligned data.
        /// </summary>
        public static void Test_WriteAndReadAligned()
        {
            const string testFile = "test_rw.raw";
            const long deviceSize = 1024 * 1024;
            const int blockSize = 4096;

            try
            {
                using (var device = new FileBackedRawBlockDevice(testFile, deviceSize, blockSize, createNew: true))
                {
                    var writeBuffer = new byte[blockSize];
                    for (int i = 0; i < blockSize; i++)
                        writeBuffer[i] = (byte)(i % 256);

                    // Write at offset 0
                    device.Write(0, writeBuffer, blockSize);

                    // Read back
                    var readBuffer = new byte[blockSize];
                    var bytesRead = device.Read(0, readBuffer, blockSize);

                    Assert(bytesRead == blockSize, "Should read full block");

                    // Verify data
                    for (int i = 0; i < blockSize; i++)
                    {
                        Assert(readBuffer[i] == writeBuffer[i], $"Data mismatch at byte {i}");
                    }
                }

                File.Delete(testFile);
                TestPassed("Test_WriteAndReadAligned");
            }
            catch (Exception ex)
            {
                TestFailed("Test_WriteAndReadAligned", ex.Message);
                if (File.Exists(testFile)) File.Delete(testFile);
            }
        }

        /// <summary>
        /// Test: Misaligned operations should throw exceptions.
        /// </summary>
        public static void Test_MisalignedOperationsThrow()
        {
            const string testFile = "test_misalign.raw";
            const long deviceSize = 1024 * 1024;
            const int blockSize = 4096;

            try
            {
                using (var device = new FileBackedRawBlockDevice(testFile, deviceSize, blockSize, createNew: true))
                {
                    var buffer = new byte[blockSize];

                    // Try misaligned offset
                    bool threwException = false;
                    try
                    {
                        device.Write(100, buffer, blockSize); // Offset 100 is not aligned to 4096
                    }
                    catch (ArgumentException)
                    {
                        threwException = true;
                    }
                    Assert(threwException, "Should throw on misaligned offset");

                    // Try misaligned count
                    threwException = false;
                    try
                    {
                        device.Write(0, buffer, 100); // Count 100 is not multiple of 4096
                    }
                    catch (ArgumentException)
                    {
                        threwException = true;
                    }
                    Assert(threwException, "Should throw on misaligned count");
                }

                File.Delete(testFile);
                TestPassed("Test_MisalignedOperationsThrow");
            }
            catch (Exception ex)
            {
                TestFailed("Test_MisalignedOperationsThrow", ex.Message);
                if (File.Exists(testFile)) File.Delete(testFile);
            }
        }

        /// <summary>
        /// Test: Initialize raw links storage and verify header.
        /// </summary>
        public static void Test_InitializeRawLinksStorage()
        {
            const string testFile = "test_links.raw";
            const long deviceSize = 2 * 1024 * 1024;
            const uint capacity = 1000;

            try
            {
                using (var device = new FileBackedRawBlockDevice(testFile, deviceSize, createNew: true))
                using (var storage = new RawLinksStorage<uint>(device, capacity, initializeNew: true))
                {
                    Assert(storage.Capacity == capacity, "Capacity should match requested capacity");
                    Assert(storage.Count == 0, "Initial count should be zero");
                }

                File.Delete(testFile);
                TestPassed("Test_InitializeRawLinksStorage");
            }
            catch (Exception ex)
            {
                TestFailed("Test_InitializeRawLinksStorage", ex.Message);
                if (File.Exists(testFile)) File.Delete(testFile);
            }
        }

        /// <summary>
        /// Test: Write and read links.
        /// </summary>
        public static void Test_WriteAndReadLinks()
        {
            const string testFile = "test_links_rw.raw";
            const long deviceSize = 2 * 1024 * 1024;
            const uint capacity = 1000;

            try
            {
                using (var device = new FileBackedRawBlockDevice(testFile, deviceSize, createNew: true))
                using (var storage = new RawLinksStorage<uint>(device, capacity, initializeNew: true))
                {
                    // Write some links
                    storage.WriteLink(0, 10, 20);
                    storage.WriteLink(1, 30, 40);
                    storage.WriteLink(2, 50, 60);

                    // Read them back
                    storage.ReadLink(0, out var src0, out var tgt0);
                    Assert(src0 == 10 && tgt0 == 20, "Link 0 should be 10 -> 20");

                    storage.ReadLink(1, out var src1, out var tgt1);
                    Assert(src1 == 30 && tgt1 == 40, "Link 1 should be 30 -> 40");

                    storage.ReadLink(2, out var src2, out var tgt2);
                    Assert(src2 == 50 && tgt2 == 60, "Link 2 should be 50 -> 60");
                }

                File.Delete(testFile);
                TestPassed("Test_WriteAndReadLinks");
            }
            catch (Exception ex)
            {
                TestFailed("Test_WriteAndReadLinks", ex.Message);
                if (File.Exists(testFile)) File.Delete(testFile);
            }
        }

        /// <summary>
        /// Test: Persistence - write, close, reopen, and verify data is still there.
        /// </summary>
        public static void Test_Persistence()
        {
            const string testFile = "test_persist.raw";
            const long deviceSize = 2 * 1024 * 1024;
            const uint capacity = 1000;

            try
            {
                // Write data and close
                using (var device = new FileBackedRawBlockDevice(testFile, deviceSize, createNew: true))
                using (var storage = new RawLinksStorage<uint>(device, capacity, initializeNew: true))
                {
                    storage.WriteLink(0, 100, 200);
                    storage.WriteLink(1, 300, 400);
                    storage.Sync();
                }

                // Reopen and verify
                using (var device = new FileBackedRawBlockDevice(testFile, deviceSize, createNew: false))
                using (var storage = new RawLinksStorage<uint>(device, capacity, initializeNew: false))
                {
                    storage.ReadLink(0, out var src0, out var tgt0);
                    Assert(src0 == 100 && tgt0 == 200, "Link 0 should persist as 100 -> 200");

                    storage.ReadLink(1, out var src1, out var tgt1);
                    Assert(src1 == 300 && tgt1 == 400, "Link 1 should persist as 300 -> 400");
                }

                File.Delete(testFile);
                TestPassed("Test_Persistence");
            }
            catch (Exception ex)
            {
                TestFailed("Test_Persistence", ex.Message);
                if (File.Exists(testFile)) File.Delete(testFile);
            }
        }

        /// <summary>
        /// Test: Out of bounds access should throw.
        /// </summary>
        public static void Test_OutOfBoundsThrows()
        {
            const string testFile = "test_oob.raw";
            const long deviceSize = 2 * 1024 * 1024;
            const uint capacity = 100;

            try
            {
                using (var device = new FileBackedRawBlockDevice(testFile, deviceSize, createNew: true))
                using (var storage = new RawLinksStorage<uint>(device, capacity, initializeNew: true))
                {
                    bool threwException = false;
                    try
                    {
                        storage.ReadLink(capacity, out _, out _); // Address equals capacity (out of bounds)
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        threwException = true;
                    }
                    Assert(threwException, "Should throw on out-of-bounds read");

                    threwException = false;
                    try
                    {
                        storage.WriteLink(capacity, 1, 2); // Address equals capacity (out of bounds)
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        threwException = true;
                    }
                    Assert(threwException, "Should throw on out-of-bounds write");
                }

                File.Delete(testFile);
                TestPassed("Test_OutOfBoundsThrows");
            }
            catch (Exception ex)
            {
                TestFailed("Test_OutOfBoundsThrows", ex.Message);
                if (File.Exists(testFile)) File.Delete(testFile);
            }
        }

        /// <summary>
        /// Run all tests and report results.
        /// </summary>
        public static void RunAllTests()
        {
            Console.WriteLine("=== Running Raw Storage Tests ===\n");

            Test_CreateFileBackedDevice();
            Test_WriteAndReadAligned();
            Test_MisalignedOperationsThrow();
            Test_InitializeRawLinksStorage();
            Test_WriteAndReadLinks();
            Test_Persistence();
            Test_OutOfBoundsThrows();

            Console.WriteLine("\n=== Test Results ===");
            Console.WriteLine($"Passed: {_testsPassed}");
            Console.WriteLine($"Failed: {_testsFailed}");
            Console.WriteLine($"Total:  {_testsPassed + _testsFailed}");

            if (_testsFailed == 0)
            {
                Console.WriteLine("\n✓ All tests passed!");
            }
            else
            {
                Console.WriteLine($"\n✗ {_testsFailed} test(s) failed.");
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new Exception($"Assertion failed: {message}");
        }

        private static void TestPassed(string testName)
        {
            Console.WriteLine($"✓ {testName}");
            _testsPassed++;
        }

        private static void TestFailed(string testName, string error)
        {
            Console.WriteLine($"✗ {testName}: {error}");
            _testsFailed++;
        }
    }
}
