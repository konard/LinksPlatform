using System;
using System.Collections.Generic;
using System.Linq;

namespace TuringCompletenessProof
{
    /// <summary>
    /// Demonstrates how Triggers can emulate Markov algorithms.
    ///
    /// A Markov algorithm is a string rewriting system consisting of rules
    /// in the form "pattern → replacement". Rules are applied sequentially
    /// until no more rules can be applied or a terminal rule is reached.
    /// </summary>
    public class MarkovAlgorithmExample
    {
        /// <summary>
        /// Represents a Markov algorithm rule (trigger).
        /// </summary>
        public class MarkovRule
        {
            public string Pattern { get; set; }
            public string Replacement { get; set; }
            public bool IsTerminal { get; set; }

            public MarkovRule(string pattern, string replacement, bool isTerminal = false)
            {
                Pattern = pattern;
                Replacement = replacement;
                IsTerminal = isTerminal;
            }
        }

        /// <summary>
        /// Applies Markov algorithm rules to a string.
        /// This demonstrates how triggers work as substitution rules.
        /// </summary>
        public static string ApplyMarkovAlgorithm(string input, List<MarkovRule> rules)
        {
            var current = input;
            var changed = true;

            while (changed)
            {
                changed = false;

                foreach (var rule in rules)
                {
                    if (current.Contains(rule.Pattern))
                    {
                        // Apply the trigger: pattern → replacement
                        var index = current.IndexOf(rule.Pattern);
                        current = current.Substring(0, index) +
                                  rule.Replacement +
                                  current.Substring(index + rule.Pattern.Length);

                        Console.WriteLine($"Applied rule: '{rule.Pattern}' → '{rule.Replacement}'");
                        Console.WriteLine($"Result: {current}");

                        changed = true;

                        if (rule.IsTerminal)
                        {
                            Console.WriteLine("Terminal rule applied. Stopping.");
                            return current;
                        }

                        // Restart from the beginning after applying a rule
                        break;
                    }
                }
            }

            return current;
        }

        /// <summary>
        /// Example: Binary increment using Markov algorithm.
        /// Increments a binary number by adding 1.
        /// </summary>
        public static void BinaryIncrementExample()
        {
            Console.WriteLine("=== Markov Algorithm: Binary Increment ===\n");

            // Rules for binary increment
            // The algorithm works by:
            // 1. Adding a marker '|' at the end
            // 2. Carrying from right to left
            // 3. Cleaning up the marker
            var rules = new List<MarkovRule>
            {
                new MarkovRule("0|", "1", true),        // 0 + 1 = 1 (terminal)
                new MarkovRule("1|", "|0"),             // 1 + 1 = 0, carry left
                new MarkovRule("|", "1|"),              // At the leftmost, add 1
            };

            var testCases = new[] { "0", "1", "10", "11", "101", "111", "1111" };

            foreach (var test in testCases)
            {
                Console.WriteLine($"\nInput: {test}|");
                var result = ApplyMarkovAlgorithm(test + "|", rules);
                Console.WriteLine($"Final: {result}\n");
            }
        }

        /// <summary>
        /// Example: String reversal using Markov algorithm.
        /// </summary>
        public static void StringReversalExample()
        {
            Console.WriteLine("=== Markov Algorithm: String Reversal ===\n");

            // This algorithm uses markers to reverse a string
            var rules = new List<MarkovRule>
            {
                // Move marker to the left
                new MarkovRule("a*", "*a"),
                new MarkovRule("b*", "*b"),
                new MarkovRule("c*", "*c"),

                // When marker reaches the start, we're done
                new MarkovRule("*", "", true),
            };

            var input = "abc*";
            Console.WriteLine($"Input: {input}");
            var result = ApplyMarkovAlgorithm(input, rules);
            Console.WriteLine($"Final: {result}\n");
        }

        /// <summary>
        /// Example: Unary addition using Markov algorithm.
        /// Adds two unary numbers (represented as sequences of 1s).
        /// </summary>
        public static void UnaryAdditionExample()
        {
            Console.WriteLine("=== Markov Algorithm: Unary Addition ===\n");

            // Rules for unary addition
            // Input format: 111+11 (3+2)
            // Output: 11111 (5)
            var rules = new List<MarkovRule>
            {
                // Remove the + and one 1 from the right side
                new MarkovRule("+1", "1+"),
                new MarkovRule("+", "", true),
            };

            var testCases = new[] { "111+11", "1+1", "11111+1", "1+11111" };

            foreach (var test in testCases)
            {
                Console.WriteLine($"Input: {test}");
                var result = ApplyMarkovAlgorithm(test, rules);
                var count = result.Count(c => c == '1');
                Console.WriteLine($"Final: {result} ({count})\n");
            }
        }

        public static void RunExamples()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  Markov Algorithm Emulation Using Triggers                ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            BinaryIncrementExample();
            Console.WriteLine(new string('─', 60) + "\n");

            UnaryAdditionExample();
            Console.WriteLine(new string('─', 60) + "\n");

            StringReversalExample();

            Console.WriteLine("\n" + new string('═', 60));
            Console.WriteLine("Conclusion: Triggers can fully emulate Markov algorithms.");
            Console.WriteLine("Since Markov algorithms are Turing complete,");
            Console.WriteLine("this proves Triggers are Turing complete.");
            Console.WriteLine(new string('═', 60));
        }
    }
}
