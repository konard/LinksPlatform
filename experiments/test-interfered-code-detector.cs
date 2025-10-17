using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Examples;

namespace Experiments
{
    /// <summary>
    /// Test script for the Interfered Code Detector.
    /// This demonstrates how binary search can isolate problematic code blocks.
    /// </summary>
    class TestInterferedCodeDetector
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing Interfered Code Detector");
            Console.WriteLine("=================================\n");

            // Test 1: Simple case with one interfering block
            Test1_SingleInterferingBlock();

            Console.WriteLine("\n" + new string('=', 50) + "\n");

            // Test 2: Multiple interfering blocks
            Test2_MultipleInterferingBlocks();

            Console.WriteLine("\n" + new string('=', 50) + "\n");

            // Test 3: No interference
            Test3_NoInterference();
        }

        static void Test1_SingleInterferingBlock()
        {
            Console.WriteLine("Test 1: Single Interfering Block");
            Console.WriteLine("---------------------------------");

            var blocks = new List<CodeBlock>
            {
                new CodeBlock { Id = "A", Description = "Safe block A" },
                new CodeBlock { Id = "B", Description = "Safe block B" },
                new CodeBlock { Id = "C", Description = "Interfering block C" },
                new CodeBlock { Id = "D", Description = "Safe block D" }
            };

            Func<List<CodeBlock>, bool> test = (codeBlocks) =>
            {
                // Test fails if block C is enabled
                return !codeBlocks.Any(b => b.Id == "C" && b.IsEnabled);
            };

            var isolator = new BinarySearchIsolator(test, msg => Console.WriteLine($"  {msg}"));
            var interfering = isolator.FindInterferingBlocks(blocks);

            Console.WriteLine($"\nResult: Found {interfering.Count} interfering block(s)");
            foreach (var block in interfering)
            {
                Console.WriteLine($"  - {block.Id}: {block.Description}");
            }

            // Verify
            bool success = interfering.Count == 1 && interfering[0].Id == "C";
            Console.WriteLine($"Test 1: {(success ? "PASS ✓" : "FAIL ✗")}");
        }

        static void Test2_MultipleInterferingBlocks()
        {
            Console.WriteLine("Test 2: Multiple Interfering Blocks");
            Console.WriteLine("------------------------------------");

            var blocks = new List<CodeBlock>
            {
                new CodeBlock { Id = "A", Description = "Safe block A" },
                new CodeBlock { Id = "B", Description = "Interfering block B" },
                new CodeBlock { Id = "C", Description = "Safe block C" },
                new CodeBlock { Id = "D", Description = "Interfering block D" },
                new CodeBlock { Id = "E", Description = "Safe block E" }
            };

            Func<List<CodeBlock>, bool> test = (codeBlocks) =>
            {
                // Test fails if block B or D is enabled
                var enabled = codeBlocks.Where(b => b.IsEnabled).Select(b => b.Id);
                return !enabled.Contains("B") && !enabled.Contains("D");
            };

            var isolator = new BinarySearchIsolator(test, msg => Console.WriteLine($"  {msg}"));
            var interfering = isolator.FindInterferingBlocks(blocks);

            Console.WriteLine($"\nResult: Found {interfering.Count} interfering block(s)");
            foreach (var block in interfering)
            {
                Console.WriteLine($"  - {block.Id}: {block.Description}");
            }

            // Verify
            var interferingIds = interfering.Select(b => b.Id).OrderBy(x => x).ToList();
            bool success = interferingIds.SequenceEqual(new[] { "B", "D" });
            Console.WriteLine($"Test 2: {(success ? "PASS ✓" : "FAIL ✗")}");
        }

        static void Test3_NoInterference()
        {
            Console.WriteLine("Test 3: No Interference");
            Console.WriteLine("------------------------");

            var blocks = new List<CodeBlock>
            {
                new CodeBlock { Id = "A", Description = "Safe block A" },
                new CodeBlock { Id = "B", Description = "Safe block B" },
                new CodeBlock { Id = "C", Description = "Safe block C" }
            };

            Func<List<CodeBlock>, bool> test = (codeBlocks) =>
            {
                // Test always passes
                return true;
            };

            var isolator = new BinarySearchIsolator(test, msg => Console.WriteLine($"  {msg}"));
            var interfering = isolator.FindInterferingBlocks(blocks);

            Console.WriteLine($"\nResult: Found {interfering.Count} interfering block(s)");

            // Verify
            bool success = interfering.Count == 0;
            Console.WriteLine($"Test 3: {(success ? "PASS ✓" : "FAIL ✗")}");
        }
    }
}
