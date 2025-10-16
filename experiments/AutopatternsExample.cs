using System;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Examples;

namespace Experiments
{
    /// <summary>
    /// Example demonstrating the Autopatterns engine capabilities.
    /// Shows how to discover and analyze patterns in various types of data.
    /// </summary>
    public class AutopatternsExample
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Autopatterns Engine - Example Demonstration");
            Console.WriteLine("============================================\n");

            // Example 1: Simple repeating pattern
            Example1_RepeatingPattern();

            Console.WriteLine("\n" + new string('=', 60) + "\n");

            // Example 2: Language structure analysis
            Example2_LanguageStructure();

            Console.WriteLine("\n" + new string('=', 60) + "\n");

            // Example 3: Scientific data pattern
            Example3_ScientificData();
        }

        private static void Example1_RepeatingPattern()
        {
            Console.WriteLine("Example 1: Discovering Repeating Patterns");
            Console.WriteLine("------------------------------------------\n");

            var data = "hello world hello world the quick brown fox hello world";

            using (var memory = new HeapResizableDirectMemory())
            using (var links = new UnitedMemoryLinks<uint>(memory))
            {
                var engine = new AutopatternsEngine<uint>(links);

                Console.WriteLine($"Input data: \"{data}\"\n");

                var patterns = engine.DiscoverPatterns(data);

                Console.WriteLine("Discovered patterns:\n");
                foreach (var pattern in patterns)
                {
                    if (pattern.Type == PatternType.Repeating && pattern.Frequency > 2)
                    {
                        Console.WriteLine(engine.GeneratePatternDefinition(pattern));
                    }
                }
            }
        }

        private static void Example2_LanguageStructure()
        {
            Console.WriteLine("Example 2: Understanding Language Structure");
            Console.WriteLine("--------------------------------------------\n");

            // Example of code structure
            var codeData = @"
function add(a, b) {
    return a + b;
}

function multiply(a, b) {
    return a * b;
}

function subtract(a, b) {
    return a - b;
}";

            using (var memory = new HeapResizableDirectMemory())
            using (var links = new UnitedMemoryLinks<uint>(memory))
            {
                var engine = new AutopatternsEngine<uint>(links);

                Console.WriteLine("Analyzing JavaScript code structure...\n");

                var patterns = engine.DiscoverPatterns(codeData);

                Console.WriteLine("Structural patterns found:\n");

                // Display patterns that reveal language structure
                foreach (var pattern in patterns)
                {
                    if (pattern.Type == PatternType.Repeating &&
                        pattern.Frequency >= 3 &&
                        pattern.Example.Contains("function"))
                    {
                        Console.WriteLine($"Pattern Type: {pattern.Type}");
                        Console.WriteLine($"Description: {pattern.Description}");
                        Console.WriteLine($"Frequency: {pattern.Frequency}");
                        Console.WriteLine($"Example: {pattern.Example}");
                        Console.WriteLine();
                    }
                }

                Console.WriteLine("\nThis demonstrates how Autopatterns can identify:");
                Console.WriteLine("- Function declaration patterns");
                Console.WriteLine("- Parameter patterns");
                Console.WriteLine("- Return statement patterns");
                Console.WriteLine("- Structural similarities in code");
            }
        }

        private static void Example3_ScientificData()
        {
            Console.WriteLine("Example 3: Scientific Data Pattern Analysis");
            Console.WriteLine("--------------------------------------------\n");

            // Example of DNA sequence
            var dnaSequence = "ATGCATGCATGCTAGCTAGCTAGCGGCCGGCCGGCCAAATAAATAAATCGTCGTCGT";

            using (var memory = new HeapResizableDirectMemory())
            using (var links = new UnitedMemoryLinks<uint>(memory))
            {
                var engine = new AutopatternsEngine<uint>(links);

                Console.WriteLine($"Analyzing DNA sequence: {dnaSequence}\n");

                var patterns = engine.DiscoverPatterns(dnaSequence);

                Console.WriteLine("Repeating motifs discovered:\n");

                foreach (var pattern in patterns)
                {
                    if (pattern.Type == PatternType.Repeating && pattern.Frequency > 2)
                    {
                        Console.WriteLine($"Motif: {pattern.Example}");
                        Console.WriteLine($"Frequency: {pattern.Frequency}");
                        Console.WriteLine($"Length: {pattern.Properties.GetValueOrDefault("length", "N/A")}");
                        Console.WriteLine();
                    }
                }

                Console.WriteLine("This demonstrates how Autopatterns can:");
                Console.WriteLine("- Identify repeating DNA motifs");
                Console.WriteLine("- Discover genetic patterns");
                Console.WriteLine("- Help understand sequence structure");
                Console.WriteLine("- Support bioinformatics research");
            }
        }
    }
}
