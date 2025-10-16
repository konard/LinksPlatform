using System;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for demonstrating sentence parsing capabilities.
    /// Shows how sentences can be mapped to pair links and triplet links.
    /// </summary>
    public class SentenceParserCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            Console.WriteLine("=== Sentence Parser: Subject-Predicate-Object Detector ===");
            Console.WriteLine("This tool demonstrates how sentences can be mapped to pair and triplet links.\n");

            // Example sentences for demonstration
            var examples = new[]
            {
                "Dogs bark",
                "The cat sleeps",
                "John runs fast",
                "She is happy",
                "They are students",
                "Birds fly",
                "The sun shines brightly",
                "Alice reads books",
                "Bob eats pizza",
                "The teacher explains concepts",
                "Water flows downstream",
                "Children play games",
                "The computer works well",
                "Scientists study nature"
            };

            Console.WriteLine("--- Example Sentences Analysis ---\n");

            foreach (var sentence in examples)
            {
                var parsed = SentenceParser.Parse(sentence);
                if (parsed != null)
                {
                    Console.WriteLine($"Input: \"{sentence}\"");
                    Console.WriteLine($"  {parsed}");

                    if (SentenceParser.IsPairLink(parsed))
                    {
                        Console.WriteLine($"  → Maps to PAIR LINK: [{parsed.Subject}] → [{parsed.Predicate}]");
                    }
                    else if (SentenceParser.IsTripletLink(parsed))
                    {
                        Console.WriteLine($"  → Maps to TRIPLET LINK: [{parsed.Subject}] → [{parsed.Predicate}] → [{parsed.Object}]");
                    }

                    Console.WriteLine();
                }
            }

            // Interactive mode
            Console.WriteLine("\n--- Interactive Mode ---");
            Console.WriteLine("Enter sentences to parse (or 'exit' to quit):\n");

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input) || input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                var result = SentenceParser.Parse(input);
                if (result != null)
                {
                    Console.WriteLine($"  {result}");

                    if (SentenceParser.IsPairLink(result))
                    {
                        Console.WriteLine($"  → Maps to PAIR LINK: [{result.Subject}] → [{result.Predicate}]");
                    }
                    else if (SentenceParser.IsTripletLink(result))
                    {
                        Console.WriteLine($"  → Maps to TRIPLET LINK: [{result.Subject}] → [{result.Predicate}] → [{result.Object}]");
                    }
                }
                else
                {
                    Console.WriteLine("  (Unable to parse sentence)");
                }

                Console.WriteLine();
            }

            Console.WriteLine("\nGoodbye!");
        }
    }
}
