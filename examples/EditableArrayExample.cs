using System;
using Platform.Sandbox;

namespace Platform.Examples
{
    /// <summary>
    /// Examples demonstrating the usage of EditableArray.
    /// </summary>
    public static class EditableArrayExample
    {
        /// <summary>
        /// Demonstrates basic usage of EditableArray.
        /// </summary>
        public static void BasicExample()
        {
            Console.WriteLine("=== EditableArray Basic Example ===");

            // Create an editable array with default value 0
            var array = new EditableArray<int>(defaultValue: 0);

            // Write some data
            array.Write(0, new[] { 1, 2, 3, 4, 5 });
            Console.WriteLine("Wrote [1, 2, 3, 4, 5] at offset 0");

            // Read individual values
            Console.WriteLine($"Read at index 2: {array.Read(2)}"); // Should be 3

            // Write overlapping data (immutably edits the array)
            array.Write(2, new[] { 10, 20, 30 });
            Console.WriteLine("Wrote [10, 20, 30] at offset 2");

            // Read the updated value
            Console.WriteLine($"Read at index 2: {array.Read(2)}"); // Should be 10 (latest value)
            Console.WriteLine($"Read at index 0: {array.Read(0)}"); // Should be 1 (original value)
            Console.WriteLine($"Read at index 4: {array.Read(4)}"); // Should be 30 (new value)

            // Read a range
            var range = array.Read(0, 6);
            Console.WriteLine($"Read range [0-6): [{string.Join(", ", range)}]");

            // Read undefined index (returns default value)
            Console.WriteLine($"Read at index 1000: {array.Read(1000)}"); // Should be 0 (default)

            Console.WriteLine($"Total ranges: {array.RangeCount}");
            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates array optimization/rebuilding.
        /// </summary>
        public static void OptimizationExample()
        {
            Console.WriteLine("=== EditableArray Optimization Example ===");

            var array = new EditableArray<int>(defaultValue: -1);

            // Write multiple overlapping ranges
            array.Write(0, new[] { 1, 2, 3 });
            array.Write(2, new[] { 20, 30 });
            array.Write(1, new[] { 100, 200 });
            array.Write(10, new[] { 5, 6, 7 });

            Console.WriteLine($"Before optimization - Ranges: {array.RangeCount}");
            foreach (var (offset, length) in array.GetRangesInfo())
            {
                Console.WriteLine($"  Range: offset={offset}, length={length}");
            }

            // Optimize the array
            var optimized = array.Optimize();
            Console.WriteLine($"\nAfter optimization - Ranges: {optimized.RangeCount}");
            foreach (var (offset, length) in optimized.GetRangesInfo())
            {
                Console.WriteLine($"  Range: offset={offset}, length={length}");
            }

            // Verify data is preserved
            Console.WriteLine("\nVerifying data integrity:");
            Console.WriteLine($"Original array[0]: {array.Read(0)}");
            Console.WriteLine($"Optimized array[0]: {optimized.Read(0)}");
            Console.WriteLine($"Original array[2]: {array.Read(2)}");
            Console.WriteLine($"Optimized array[2]: {optimized.Read(2)}");
            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates file persistence.
        /// </summary>
        public static void FilePersistenceExample()
        {
            Console.WriteLine("=== EditableArray File Persistence Example ===");

            var array = new EditableArray<long>(defaultValue: 0);
            array.Write(0, new[] { 100L, 200L, 300L });
            array.Write(10, new[] { 1000L, 2000L });
            array.Write(2, new[] { 999L }); // Overlapping write

            string filePath = "/tmp/editable_array_test.dat";
            Console.WriteLine($"Saving to file: {filePath}");
            array.SaveToFile(filePath);

            Console.WriteLine("Loading from file...");
            var loaded = EditableArray<long>.LoadFromFile(filePath);

            Console.WriteLine("Verifying loaded data:");
            Console.WriteLine($"Value at index 0: {loaded.Read(0)}");
            Console.WriteLine($"Value at index 2: {loaded.Read(2)}");
            Console.WriteLine($"Value at index 10: {loaded.Read(10)}");
            Console.WriteLine($"Ranges: {loaded.RangeCount}");

            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates EditableBitString usage.
        /// </summary>
        public static void BitStringExample()
        {
            Console.WriteLine("=== EditableBitString Example ===");

            var bitString = new EditableBitString(defaultBit: false);

            // Set individual bits
            bitString.SetBit(0, true);
            bitString.SetBit(1, true);
            bitString.SetBit(5, true);

            Console.WriteLine("Set bits at positions 0, 1, and 5");
            Console.WriteLine($"Bit at 0: {bitString.GetBit(0)}");
            Console.WriteLine($"Bit at 1: {bitString.GetBit(1)}");
            Console.WriteLine($"Bit at 2: {bitString.GetBit(2)}");
            Console.WriteLine($"Bit at 5: {bitString.GetBit(5)}");

            // Set a range of bits
            bitString.SetBits(10, new[] { true, false, true, true, false });
            Console.WriteLine("\nSet bits [T, F, T, T, F] at offset 10");

            var bits = bitString.GetBits(10, 5);
            Console.WriteLine($"Read bits: [{string.Join(", ", bits)}]");

            // Optimize
            var optimized = bitString.Optimize();
            Console.WriteLine($"\nRanges before optimization: {bitString.RangeCount}");
            Console.WriteLine($"Ranges after optimization: {optimized.RangeCount}");

            Console.WriteLine();
        }

        /// <summary>
        /// Runs all examples.
        /// </summary>
        public static void RunAll()
        {
            BasicExample();
            OptimizationExample();
            FilePersistenceExample();
            BitStringExample();
        }
    }
}
