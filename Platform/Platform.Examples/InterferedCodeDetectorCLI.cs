using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for automatic bug fixing via interfered code detection.
    /// Uses binary search algorithm to isolate code blocks that cause interference or bugs.
    /// </summary>
    /// <remarks>
    /// Example usage:
    /// InterferedCodeDetectorCLI config.json
    ///
    /// The config.json should specify:
    /// - Code blocks to analyze
    /// - Test command to run
    /// - Success criteria
    /// </remarks>
    public class InterferedCodeDetectorCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            Console.WriteLine("=== Interfered Code Detector ===");
            Console.WriteLine("Automatic bug fixing via binary search isolation");
            Console.WriteLine();

            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            string mode = args[0].ToLowerInvariant();

            switch (mode)
            {
                case "demo":
                    RunDemo();
                    break;
                case "analyze":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Error: 'analyze' mode requires a configuration file path.");
                        PrintUsage();
                        return;
                    }
                    RunAnalysis(args[1]);
                    break;
                default:
                    Console.WriteLine($"Unknown mode: {mode}");
                    PrintUsage();
                    break;
            }
        }

        private void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  InterferedCodeDetectorCLI demo           - Run demonstration");
            Console.WriteLine("  InterferedCodeDetectorCLI analyze <file> - Analyze code using config file");
            Console.WriteLine();
            Console.WriteLine("This tool helps identify code blocks that interfere with UI elements or");
            Console.WriteLine("cause damaging interference using binary search algorithm.");
        }

        private void RunDemo()
        {
            Console.WriteLine("Running demonstration of binary search code isolation...");
            Console.WriteLine();

            // Create sample code blocks
            var codeBlocks = new List<CodeBlock>
            {
                new CodeBlock
                {
                    Id = "block1",
                    Description = "Initialize UI component",
                    FilePath = "example.js",
                    StartLine = 1,
                    EndLine = 10,
                    Content = "function initializeUI() { /* ... */ }"
                },
                new CodeBlock
                {
                    Id = "block2",
                    Description = "Setup event listener - INTERFERING",
                    FilePath = "example.js",
                    StartLine = 11,
                    EndLine = 20,
                    Content = "function setupListener() { /* blocks input */ }"
                },
                new CodeBlock
                {
                    Id = "block3",
                    Description = "Load data",
                    FilePath = "example.js",
                    StartLine = 21,
                    EndLine = 30,
                    Content = "function loadData() { /* ... */ }"
                },
                new CodeBlock
                {
                    Id = "block4",
                    Description = "Render components",
                    FilePath = "example.js",
                    StartLine = 31,
                    EndLine = 40,
                    Content = "function render() { /* ... */ }"
                },
                new CodeBlock
                {
                    Id = "block5",
                    Description = "Another event handler - INTERFERING",
                    FilePath = "example.js",
                    StartLine = 41,
                    EndLine = 50,
                    Content = "function handleEvent() { /* also blocks */ }"
                }
            };

            Console.WriteLine($"Analyzing {codeBlocks.Count} code blocks:");
            foreach (var block in codeBlocks)
            {
                Console.WriteLine($"  - {block}");
            }
            Console.WriteLine();

            // Simulate a test that fails when interfering blocks are enabled
            Func<List<CodeBlock>, bool> testPredicate = (blocks) =>
            {
                var enabledBlocks = blocks.Where(b => b.IsEnabled).ToList();
                Console.WriteLine($"  Testing with {enabledBlocks.Count} enabled blocks: [{string.Join(", ", enabledBlocks.Select(b => b.Id))}]");

                // Simulate: test fails if block2 or block5 is enabled (they interfere with input)
                bool hasInterference = enabledBlocks.Any(b => b.Id == "block2" || b.Id == "block5");
                bool testPassed = !hasInterference;

                Console.WriteLine($"    Result: {(testPassed ? "PASS" : "FAIL")}");
                return testPassed;
            };

            var isolator = new BinarySearchIsolator(testPredicate, msg => Console.WriteLine($"[Isolator] {msg}"));
            var interferingBlocks = isolator.FindInterferingBlocks(codeBlocks);

            Console.WriteLine();
            Console.WriteLine("=== Analysis Complete ===");
            if (interferingBlocks.Count > 0)
            {
                Console.WriteLine($"Found {interferingBlocks.Count} interfering code block(s):");
                foreach (var block in interferingBlocks)
                {
                    Console.WriteLine($"  ⚠️  {block}");
                    Console.WriteLine($"      Content: {block.Content}");
                }
            }
            else
            {
                Console.WriteLine("No interfering blocks found.");
            }
        }

        private void RunAnalysis(string configPath)
        {
            if (!File.Exists(configPath))
            {
                Console.WriteLine($"Error: Configuration file not found: {configPath}");
                return;
            }

            Console.WriteLine($"Loading configuration from: {configPath}");
            Console.WriteLine("Note: Full configuration-based analysis not yet implemented.");
            Console.WriteLine("Please use 'demo' mode to see how the tool works.");
        }
    }
}
