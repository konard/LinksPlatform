using System;
using System.Numerics;
using System.Collections.Generic;
using Platform.Converters;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Platform.Examples;

namespace Experiments
{
    /// <summary>
    /// Demonstrates the use of decimal digits representation for integer numbers.
    /// This experiment shows how integers can be represented as sequences of decimal digits.
    /// For example: 12345 = [1, 2, 3, 4, 5]
    /// </summary>
    public class DecimalDigitsNumbersExperiment
    {
        public static void Run()
        {
            Console.WriteLine("=== Decimal Digits Integer Numbers Experiment ===");
            Console.WriteLine();

            // Create an in-memory links storage
            using var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);

            // Initialize markers
            var meaningRoot = links.GetOrCreate(1, 1);
            var decimalDigitsSequenceMarker = links.GetOrCreate(meaningRoot, 2);
            var negativeNumberMarker = links.GetOrCreate(meaningRoot, 3);

            // Create converters for digits and sequences
            var digitToLinkConverter = new TrivialConverter<uint>();
            var linkToDigitConverter = new TrivialConverter<uint>();
            var listToSequenceConverter = new ListToSequenceConverter<uint>(links);
            var sequenceWalker = new SequenceWalker<uint>(links);

            // Create the bidirectional converters
            var integerToDecimalDigits = new IntegerToDecimalDigitsSequenceConverter<uint>(
                links,
                digitToLinkConverter,
                listToSequenceConverter,
                decimalDigitsSequenceMarker,
                negativeNumberMarker
            );

            var decimalDigitsToInteger = new DecimalDigitsSequenceToIntegerConverter<uint>(
                links,
                linkToDigitConverter,
                sequenceWalker,
                decimalDigitsSequenceMarker,
                negativeNumberMarker
            );

            // Test various numbers
            TestNumber(0, integerToDecimalDigits, decimalDigitsToInteger); // Zero
            TestNumber(7, integerToDecimalDigits, decimalDigitsToInteger); // Single digit
            TestNumber(42, integerToDecimalDigits, decimalDigitsToInteger); // Two digits
            TestNumber(12345, integerToDecimalDigits, decimalDigitsToInteger); // Multiple digits
            TestNumber(-999, integerToDecimalDigits, decimalDigitsToInteger); // Negative number
            TestNumber(1000000, integerToDecimalDigits, decimalDigitsToInteger); // Large number with zeros

            Console.WriteLine();
            Console.WriteLine($"Total links created: {links.Count()}");
            Console.WriteLine();
            Console.WriteLine("Experiment completed successfully!");
        }

        private static void TestNumber(
            BigInteger number,
            IntegerToDecimalDigitsSequenceConverter<uint> toConverter,
            DecimalDigitsSequenceToIntegerConverter<uint> fromConverter)
        {
            Console.WriteLine($"Testing number: {number}");

            // Convert to decimal digits sequence
            var sequenceLink = toConverter.Convert(number);
            Console.WriteLine($"Sequence link created: {sequenceLink}");

            // Convert back to integer
            var reconstructed = fromConverter.Convert(sequenceLink);
            Console.WriteLine($"Reconstructed number: {reconstructed}");

            // Verify
            var success = number == reconstructed;
            Console.WriteLine($"Verification: {(success ? "PASSED ✓" : "FAILED ✗")}");
            Console.WriteLine();
        }
    }

    // Note: TrivialConverter, ListToSequenceConverter, and SequenceWalker
    // are already defined in PowersOf2NumbersExperiment.cs
    // In a real implementation, these would be in a shared utilities file
}
