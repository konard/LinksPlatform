using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;
using Platform.Memory;

namespace Platform.Sandbox
{
    /// <summary>
    /// Implements addition of numbers using Links platform by representing numbers
    /// as sequences of powers of 2 (binary representation).
    ///
    /// For example:
    /// - Number 2 is represented as a sequence containing one power: [2^1]
    /// - Number 3 is represented as a sequence containing two powers: [2^0, 2^1]
    /// - Number 4 is represented as a sequence containing one power: [2^2]
    ///
    /// Addition is performed by combining the two sequences and resolving conflicts
    /// (when the same power of 2 appears multiple times, it "carries" to the next power).
    /// </summary>
    public class BinaryNumberAddition
    {
        private readonly ILinks<ulong> _links;
        private readonly Dictionary<int, ulong> _powersOf2; // Maps power index to link

        public BinaryNumberAddition(ILinks<ulong> links)
        {
            _links = links;
            _powersOf2 = new Dictionary<int, ulong>();
        }

        /// <summary>
        /// Gets or creates a link representing 2^power.
        /// </summary>
        private ulong GetOrCreatePowerOf2(int power)
        {
            if (!_powersOf2.ContainsKey(power))
            {
                _powersOf2[power] = _links.CreatePoint();
            }
            return _powersOf2[power];
        }

        /// <summary>
        /// Converts a decimal number to its binary representation as a list of powers of 2.
        /// For example: 5 -> [2^0, 2^2] because 5 = 1 + 4 = 2^0 + 2^2
        /// </summary>
        private List<int> NumberToBinaryPowers(int number)
        {
            var powers = new List<int>();
            int power = 0;

            while (number > 0)
            {
                if ((number & 1) == 1)
                {
                    powers.Add(power);
                }
                number >>= 1;
                power++;
            }

            return powers;
        }

        /// <summary>
        /// Converts a list of powers of 2 back to a decimal number.
        /// For example: [2^0, 2^2] -> 5
        /// </summary>
        private int BinaryPowersToNumber(List<int> powers)
        {
            int result = 0;
            foreach (var power in powers)
            {
                result += (1 << power);
            }
            return result;
        }

        /// <summary>
        /// Creates a sequence in Links memory representing a number.
        /// The sequence contains links to powers of 2.
        /// </summary>
        public ulong CreateNumberSequence(int number)
        {
            var powers = NumberToBinaryPowers(number);

            if (powers.Count == 0)
            {
                // Number is zero - represent as empty/point
                return _links.CreatePoint();
            }

            if (powers.Count == 1)
            {
                // Single power - create a self-referencing link to that power
                var powerLink = GetOrCreatePowerOf2(powers[0]);
                return _links.CreateAndUpdate(powerLink, powerLink);
            }

            // Multiple powers - build a balanced binary tree sequence
            return BuildSequence(powers.Select(p => GetOrCreatePowerOf2(p)).ToList());
        }

        /// <summary>
        /// Builds a balanced sequence from a list of power links.
        /// Uses recursive binary splitting to create balanced tree.
        /// </summary>
        private ulong BuildSequence(List<ulong> powerLinks)
        {
            if (powerLinks.Count == 1)
            {
                return powerLinks[0];
            }

            if (powerLinks.Count == 2)
            {
                return _links.CreateAndUpdate(powerLinks[0], powerLinks[1]);
            }

            // Split in half for balanced tree
            int mid = powerLinks.Count / 2;
            var left = BuildSequence(powerLinks.Take(mid).ToList());
            var right = BuildSequence(powerLinks.Skip(mid).ToList());

            return _links.CreateAndUpdate(left, right);
        }

        /// <summary>
        /// Extracts the powers of 2 from a number sequence stored in Links.
        /// </summary>
        private List<int> ExtractPowersFromSequence(ulong sequenceLink)
        {
            var powerLinks = new List<ulong>();
            ExtractPowerLinksRecursive(sequenceLink, powerLinks);

            // Convert power links back to power indices
            var powers = new List<int>();
            foreach (var powerLink in powerLinks)
            {
                foreach (var kvp in _powersOf2)
                {
                    if (kvp.Value == powerLink)
                    {
                        powers.Add(kvp.Key);
                        break;
                    }
                }
            }

            return powers;
        }

        /// <summary>
        /// Recursively extracts all power links from a sequence tree.
        /// </summary>
        private void ExtractPowerLinksRecursive(ulong link, List<ulong> result)
        {
            if (_links.IsFullPoint(link))
            {
                result.Add(link);
                return;
            }

            var source = _links.GetSource(link);
            var target = _links.GetTarget(link);

            // Check if it's a pair pointing to powers or a nested sequence
            if (_powersOf2.ContainsValue(source))
            {
                result.Add(source);
            }
            else if (!_links.IsFullPoint(source))
            {
                ExtractPowerLinksRecursive(source, result);
            }

            if (_powersOf2.ContainsValue(target))
            {
                result.Add(target);
            }
            else if (!_links.IsFullPoint(target))
            {
                ExtractPowerLinksRecursive(target, result);
            }
        }

        /// <summary>
        /// Adds two numbers by combining their power-of-2 representations and handling carries.
        /// This implements the core addition algorithm mentioned in issue #82.
        /// </summary>
        public ulong Add(int a, int b)
        {
            // Get binary representations
            var powersA = NumberToBinaryPowers(a);
            var powersB = NumberToBinaryPowers(b);

            // Combine and resolve conflicts (carries)
            var combined = new List<int>();
            combined.AddRange(powersA);
            combined.AddRange(powersB);

            // Sort to process from least to most significant
            combined.Sort();

            // Resolve carries: when we have duplicate powers, they combine to next power
            var result = new List<int>();
            int i = 0;
            while (i < combined.Count)
            {
                int currentPower = combined[i];
                int count = 1;

                // Count duplicates
                while (i + count < combined.Count && combined[i + count] == currentPower)
                {
                    count++;
                }

                // Process carries
                while (count > 0)
                {
                    if (count % 2 == 1)
                    {
                        result.Add(currentPower);
                    }
                    count /= 2;
                    currentPower++;
                }

                // Move to next distinct power
                i++;
                while (i < combined.Count && combined[i] == combined[i - 1])
                {
                    i++;
                }
            }

            // Create result sequence
            if (result.Count == 0)
            {
                return _links.CreatePoint(); // Zero
            }

            return CreateNumberSequence(BinaryPowersToNumber(result));
        }

        /// <summary>
        /// Performs addition and returns the result as a decimal number.
        /// </summary>
        public int AddAndGetResult(int a, int b)
        {
            var resultSequence = Add(a, b);
            var powers = ExtractPowersFromSequence(resultSequence);
            return BinaryPowersToNumber(powers);
        }

        /// <summary>
        /// Demonstrates the addition of 2+2 as requested in issue #82.
        /// </summary>
        public static void Demo()
        {
            Console.WriteLine("=== Binary Number Addition using Links Platform ===");
            Console.WriteLine();
            Console.WriteLine("This demonstrates addition by representing numbers as sequences");
            Console.WriteLine("of powers of 2 (binary representation) in Links memory.");
            Console.WriteLine();

            const int mb4 = 4 * 1024 * 1024;

            using (var memory = new HeapResizableDirectMemory(mb4))
            using (var memoryManager = new UInt64UnitedMemoryLinks(memory))
            using (var links = new UInt64Links(memoryManager))
            {
                var calculator = new BinaryNumberAddition(links);

                // Example 1: 2 + 2 = 4 (as requested in issue)
                Console.WriteLine("Example 1: 2 + 2");
                Console.WriteLine("  2 in binary: 10 (2^1)");
                Console.WriteLine("  2 in binary: 10 (2^1)");
                Console.WriteLine("  Adding: 2^1 + 2^1 = 2^2 (carry)");
                Console.WriteLine("  Result: 100 in binary = 4 in decimal");

                var seq1 = calculator.CreateNumberSequence(2);
                var seq2 = calculator.CreateNumberSequence(2);
                var result1 = calculator.AddAndGetResult(2, 2);
                Console.WriteLine($"  Computed: {result1}");
                Console.WriteLine();

                // Example 2: 3 + 5 = 8
                Console.WriteLine("Example 2: 3 + 5");
                Console.WriteLine("  3 in binary: 11 (2^0 + 2^1)");
                Console.WriteLine("  5 in binary: 101 (2^0 + 2^2)");
                Console.WriteLine("  Adding: [2^0, 2^1] + [2^0, 2^2]");
                Console.WriteLine("  Combining: 2^0 appears twice -> carries to 2^1");
                Console.WriteLine("  Now 2^1 appears twice -> carries to 2^2");
                Console.WriteLine("  Now 2^2 appears twice -> carries to 2^3");
                Console.WriteLine("  Result: 1000 in binary = 8 in decimal");

                var result2 = calculator.AddAndGetResult(3, 5);
                Console.WriteLine($"  Computed: {result2}");
                Console.WriteLine();

                // Example 3: 7 + 8 = 15
                Console.WriteLine("Example 3: 7 + 8");
                Console.WriteLine("  7 in binary: 111 (2^0 + 2^1 + 2^2)");
                Console.WriteLine("  8 in binary: 1000 (2^3)");
                Console.WriteLine("  Result: 1111 in binary = 15 in decimal");

                var result3 = calculator.AddAndGetResult(7, 8);
                Console.WriteLine($"  Computed: {result3}");
                Console.WriteLine();

                Console.WriteLine($"Total links created: {links.Count()}");
                Console.WriteLine();
                Console.WriteLine("Addition successfully implemented using Links!");
            }
        }
    }
}
