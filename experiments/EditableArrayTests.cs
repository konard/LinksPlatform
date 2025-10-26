using System;
using System.IO;
using Platform.Sandbox;

namespace Platform.Experiments
{
    /// <summary>
    /// Test/experiment suite for EditableArray functionality.
    /// </summary>
    public static class EditableArrayTests
    {
        private static int _testsPassed = 0;
        private static int _testsFailed = 0;

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                Console.WriteLine($"  [FAIL] {message}");
                _testsFailed++;
                throw new Exception($"Assertion failed: {message}");
            }
            else
            {
                Console.WriteLine($"  [PASS] {message}");
                _testsPassed++;
            }
        }

        public static void TestBasicReadWrite()
        {
            Console.WriteLine("\nTest: Basic Read/Write");
            var array = new EditableArray<int>(0);

            // Write and read
            array.Write(0, new[] { 1, 2, 3 });
            Assert(array.Read(0) == 1, "Read value at index 0");
            Assert(array.Read(1) == 2, "Read value at index 1");
            Assert(array.Read(2) == 3, "Read value at index 2");

            // Read default value
            Assert(array.Read(100) == 0, "Read default value at undefined index");
        }

        public static void TestOverlappingWrites()
        {
            Console.WriteLine("\nTest: Overlapping Writes");
            var array = new EditableArray<int>(0);

            array.Write(0, new[] { 1, 2, 3, 4, 5 });
            array.Write(2, new[] { 10, 20 });

            Assert(array.Read(0) == 1, "First element unchanged");
            Assert(array.Read(1) == 2, "Second element unchanged");
            Assert(array.Read(2) == 10, "Overlapping write at index 2");
            Assert(array.Read(3) == 20, "Overlapping write at index 3");
            Assert(array.Read(4) == 5, "Fifth element unchanged");
        }

        public static void TestRangeRead()
        {
            Console.WriteLine("\nTest: Range Read");
            var array = new EditableArray<int>(-1);

            array.Write(5, new[] { 100, 200, 300 });

            var range = array.Read(4, 5);
            Assert(range[0] == -1, "Default value before range");
            Assert(range[1] == 100, "First written value");
            Assert(range[2] == 200, "Second written value");
            Assert(range[3] == 300, "Third written value");
            Assert(range[4] == -1, "Default value after range");
        }

        public static void TestOptimization()
        {
            Console.WriteLine("\nTest: Array Optimization");
            var array = new EditableArray<int>(0);

            // Create multiple ranges
            array.Write(0, new[] { 1, 2, 3 });
            array.Write(5, new[] { 10, 20 });
            array.Write(2, new[] { 99 });

            int rangeCountBefore = array.RangeCount;
            Assert(rangeCountBefore == 3, $"Should have 3 ranges before optimization, got {rangeCountBefore}");

            var optimized = array.Optimize();
            Assert(optimized.RangeCount == 1, $"Should have 1 range after optimization, got {optimized.RangeCount}");

            // Verify data integrity
            Assert(optimized.Read(0) == 1, "Data preserved at index 0");
            Assert(optimized.Read(2) == 99, "Data preserved at index 2 (overlapped)");
            Assert(optimized.Read(5) == 10, "Data preserved at index 5");
        }

        public static void TestFilePersistence()
        {
            Console.WriteLine("\nTest: File Persistence");
            var array = new EditableArray<long>(0);

            array.Write(0, new[] { 100L, 200L, 300L });
            array.Write(10, new[] { 1000L, 2000L });

            string tempFile = Path.GetTempFileName();
            try
            {
                array.SaveToFile(tempFile);
                var loaded = EditableArray<long>.LoadFromFile(tempFile);

                Assert(loaded.Read(0) == 100L, "Loaded value at index 0");
                Assert(loaded.Read(1) == 200L, "Loaded value at index 1");
                Assert(loaded.Read(10) == 1000L, "Loaded value at index 10");
                Assert(loaded.Read(5) == 0L, "Loaded default value at undefined index");
                Assert(loaded.RangeCount == array.RangeCount, "Range count preserved");
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        public static void TestBitString()
        {
            Console.WriteLine("\nTest: BitString");
            var bitString = new EditableBitString(false);

            bitString.SetBit(0, true);
            bitString.SetBit(5, true);

            Assert(bitString.GetBit(0) == true, "Bit 0 is true");
            Assert(bitString.GetBit(1) == false, "Bit 1 is false (default)");
            Assert(bitString.GetBit(5) == true, "Bit 5 is true");

            // Test bit range
            bitString.SetBits(10, new[] { true, false, true });
            var bits = bitString.GetBits(10, 3);
            Assert(bits[0] == true, "Bit range [0] is true");
            Assert(bits[1] == false, "Bit range [1] is false");
            Assert(bits[2] == true, "Bit range [2] is true");
        }

        public static void TestBitStringPersistence()
        {
            Console.WriteLine("\nTest: BitString Persistence");
            var bitString = new EditableBitString(false);

            bitString.SetBits(0, new[] { true, true, false, true });

            string tempFile = Path.GetTempFileName();
            try
            {
                bitString.SaveToFile(tempFile);
                var loaded = EditableBitString.LoadFromFile(tempFile);

                Assert(loaded.GetBit(0) == true, "Loaded bit 0");
                Assert(loaded.GetBit(1) == true, "Loaded bit 1");
                Assert(loaded.GetBit(2) == false, "Loaded bit 2");
                Assert(loaded.GetBit(3) == true, "Loaded bit 3");
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        public static void TestRebuild()
        {
            Console.WriteLine("\nTest: Array Rebuild");
            var array = new EditableArray<int>(0);

            array.Write(0, new[] { 1, 2, 3 });
            array.Write(10, new[] { 10, 20 });
            array.Write(1, new[] { 99 });

            // Rebuild a specific region
            var rebuilt = array.Rebuild(0, 5);
            Assert(rebuilt.RangeCount == 1, "Rebuilt array has single range");
            Assert(rebuilt.Read(0) == 1, "Rebuilt value at 0");
            Assert(rebuilt.Read(1) == 99, "Rebuilt value at 1 (overlapped)");
            Assert(rebuilt.Read(2) == 3, "Rebuilt value at 2");
        }

        public static void TestLargeOffset()
        {
            Console.WriteLine("\nTest: Large Offset");
            var array = new EditableArray<int>(0);

            long largeOffset = 1000000L;
            array.Write(largeOffset, new[] { 42, 43, 44 });

            Assert(array.Read(largeOffset) == 42, "Read at large offset");
            Assert(array.Read(largeOffset + 1) == 43, "Read at large offset + 1");
            Assert(array.Read(0) == 0, "Default value at 0");
        }

        public static void RunAllTests()
        {
            Console.WriteLine("=== EditableArray Test Suite ===");
            _testsPassed = 0;
            _testsFailed = 0;

            try
            {
                TestBasicReadWrite();
                TestOverlappingWrites();
                TestRangeRead();
                TestOptimization();
                TestFilePersistence();
                TestBitString();
                TestBitStringPersistence();
                TestRebuild();
                TestLargeOffset();

                Console.WriteLine($"\n=== Test Results ===");
                Console.WriteLine($"Passed: {_testsPassed}");
                Console.WriteLine($"Failed: {_testsFailed}");
                Console.WriteLine($"Total:  {_testsPassed + _testsFailed}");

                if (_testsFailed == 0)
                {
                    Console.WriteLine("\n✓ All tests passed!");
                    return;
                }
                else
                {
                    Console.WriteLine($"\n✗ {_testsFailed} test(s) failed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Test suite failed with exception: {ex.Message}");
            }
        }
    }
}
