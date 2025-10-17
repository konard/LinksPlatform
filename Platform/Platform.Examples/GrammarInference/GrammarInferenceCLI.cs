using System;
using System.IO;
using System.Linq;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Examples.GrammarInference
{
    /// <summary>
    /// Command-line interface for grammar inference from examples
    /// Demonstrates both standalone Sequitur algorithm and Doublets-based implementation
    /// </summary>
    public class GrammarInferenceCLI : ICommandLineInterface
    {
        public void Run(string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage();
                return;
            }

            var mode = args[0].ToLower();
            var input = args[1];

            switch (mode)
            {
                case "sequitur":
                    RunSequiturMode(input);
                    break;
                case "doublets":
                    RunDoubletsMode(input, args.Length > 2 ? args[2] : null);
                    break;
                case "demo":
                    RunDemo();
                    break;
                default:
                    Console.WriteLine($"Unknown mode: {mode}");
                    PrintUsage();
                    break;
            }
        }

        private void PrintUsage()
        {
            Console.WriteLine("Grammar Inference CLI");
            Console.WriteLine("====================");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  grammarinference sequitur <input_string>");
            Console.WriteLine("    Runs Sequitur algorithm on the input string");
            Console.WriteLine();
            Console.WriteLine("  grammarinference doublets <input_string> [db_path]");
            Console.WriteLine("    Runs grammar inference using Doublets storage");
            Console.WriteLine();
            Console.WriteLine("  grammarinference demo");
            Console.WriteLine("    Runs demonstration examples");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  grammarinference sequitur \"abcabcabc\"");
            Console.WriteLine("  grammarinference doublets \"hello world hello world\"");
            Console.WriteLine("  grammarinference demo");
        }

        private void RunSequiturMode(string input)
        {
            Console.WriteLine("Running Sequitur Algorithm");
            Console.WriteLine("=========================");
            Console.WriteLine($"Input: {input}");
            Console.WriteLine();

            // Convert string to sequence of character codes
            var sequence = input.Select(c => (ulong)c);

            var grammar = new SequiturGrammar();
            grammar.LearnFromSequence(sequence);

            Console.WriteLine(grammar.ToString());

            var rules = grammar.GetRules();
            Console.WriteLine($"\nTotal rules: {rules.Count}");
            Console.WriteLine($"Compression ratio: {input.Length}/{CalculateGrammarSize(grammar):F2}");
        }

        private void RunDoubletsMode(string input, string dbPath)
        {
            Console.WriteLine("Running Grammar Inference with Doublets");
            Console.WriteLine("======================================");
            Console.WriteLine($"Input: {input}");
            Console.WriteLine();

            // Use temporary database if no path specified
            var dbFile = dbPath ?? Path.Combine(Path.GetTempPath(), $"grammar_inference_{Guid.NewGuid()}.links");
            var useTemp = dbPath == null;

            try
            {
                using (var links = new UnitedMemoryLinks<ulong>(dbFile))
                {
                    var inference = new DoubletsGrammarInference<ulong>(links);

                    // Convert string to sequence of character code links
                    var sequence = input.Select(c => links.GetOrCreate(links.Constants.Null, links.CreatePoint()));

                    var result = inference.LearnFromSequence(sequence);

                    Console.WriteLine($"Sequence stored as link: {result}");
                    Console.WriteLine();

                    inference.PrintGrammar();

                    Console.WriteLine();
                    Console.WriteLine($"Total links in database: {links.Count()}");
                    Console.WriteLine($"Database location: {dbFile}");

                    if (!useTemp)
                    {
                        Console.WriteLine("(Database persisted for future use)");
                    }
                }
            }
            finally
            {
                // Clean up temporary database
                if (useTemp && File.Exists(dbFile))
                {
                    try
                    {
                        File.Delete(dbFile);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Could not delete temporary file: {ex.Message}");
                    }
                }
            }
        }

        private void RunDemo()
        {
            Console.WriteLine("Grammar Inference Demonstration");
            Console.WriteLine("==============================");
            Console.WriteLine();

            var examples = new[]
            {
                "abcabc",
                "ababab",
                "abcabcabc",
                "hello world hello world",
                "the cat in the hat",
                "aaaaaa",
                "abcdefg"
            };

            foreach (var example in examples)
            {
                Console.WriteLine($"Example: \"{example}\"");
                Console.WriteLine(new string('-', 50));

                var sequence = example.Select(c => (ulong)c);
                var grammar = new SequiturGrammar();
                grammar.LearnFromSequence(sequence);

                var rules = grammar.GetRules();
                Console.WriteLine($"Rules found: {rules.Count}");

                foreach (var rule in rules)
                {
                    Console.WriteLine($"  {rule}");
                }

                var grammarSize = CalculateGrammarSize(grammar);
                var compressionRatio = example.Length / grammarSize;
                Console.WriteLine($"Original size: {example.Length} symbols");
                Console.WriteLine($"Grammar size: {grammarSize:F2} symbols");
                Console.WriteLine($"Compression ratio: {compressionRatio:F2}x");
                Console.WriteLine();
            }

            Console.WriteLine("Demonstration complete!");
            Console.WriteLine();
            Console.WriteLine("Key observations:");
            Console.WriteLine("- Repeated patterns are identified and replaced with rules");
            Console.WriteLine("- Each rule must be used at least twice (rule utility)");
            Console.WriteLine("- No digram appears more than once (digram uniqueness)");
            Console.WriteLine("- Grammar inference is useful for data mining and pattern discovery");
        }

        private double CalculateGrammarSize(SequiturGrammar grammar)
        {
            double size = 0;
            foreach (var rule in grammar.GetRules())
            {
                // Count symbols in each rule
                size += rule.Length();
            }
            return size;
        }
    }
}
