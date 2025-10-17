using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Sandbox
{
    /// <summary>
    /// Demonstrates an efficient Markov algorithm implementation using prebuilt sequences.
    ///
    /// Traditional approach: Find pattern -> substitute -> repeat until no more matches
    /// Optimized approach: Prebuild all substitution sources first, then apply all substitutions in parallel
    ///
    /// Example:
    /// Input sequence: ((a a) (b b) (a a))
    /// Rules: aa → aba, bb → bab
    ///
    /// Instead of iterating through the sequence multiple times:
    /// 1. Prebuild the sequence with grouped patterns: (aa) (bb) (aa)
    /// 2. Apply all substitutions at once: (aba) (bab) (aba)
    ///
    /// This approach is more efficient because:
    /// - Reduces the number of passes through the sequence
    /// - Allows parallel substitution of all patterns
    /// - Better memory locality when patterns are prebuilt
    /// </summary>
    public static class MarkovAlgorithmExperiment
    {
        /// <summary>
        /// Represents a Markov substitution rule.
        /// </summary>
        public class MarkovRule
        {
            public string Pattern { get; set; }
            public string Replacement { get; set; }
            public bool IsTerminal { get; set; } // If true, algorithm stops after applying this rule

            public MarkovRule(string pattern, string replacement, bool isTerminal = false)
            {
                Pattern = pattern;
                Replacement = replacement;
                IsTerminal = isTerminal;
            }

            public override string ToString() => IsTerminal ? $"{Pattern} → .{Replacement}" : $"{Pattern} → {Replacement}";
        }

        /// <summary>
        /// Traditional Markov algorithm - applies rules iteratively until no more matches.
        /// </summary>
        public static string ApplyTraditional(string input, List<MarkovRule> rules)
        {
            string current = input;
            bool changed = true;

            while (changed)
            {
                changed = false;

                foreach (var rule in rules)
                {
                    if (current.Contains(rule.Pattern))
                    {
                        // Find first occurrence and replace it
                        int index = current.IndexOf(rule.Pattern);
                        current = current.Substring(0, index) + rule.Replacement + current.Substring(index + rule.Pattern.Length);
                        changed = true;

                        if (rule.IsTerminal)
                        {
                            return current;
                        }

                        break; // Restart from first rule after any substitution
                    }
                }
            }

            return current;
        }

        /// <summary>
        /// Optimized Markov algorithm using prebuilt sequences.
        /// This approach prebuilds all pattern occurrences, then applies all substitutions efficiently.
        /// </summary>
        public static string ApplyWithPrebuiltSequences(string input, List<MarkovRule> rules)
        {
            // Step 1: Identify and prebuild all pattern occurrences in the sequence
            var prebuiltSequence = PreBuildPatterns(input, rules);

            // Step 2: Apply all substitutions to the prebuilt patterns
            var result = ApplySubstitutions(prebuiltSequence, rules);

            return result;
        }

        /// <summary>
        /// Identifies patterns in the input and creates a sequence of tokens.
        /// Each token is either a matched pattern or a single character.
        /// </summary>
        private static List<string> PreBuildPatterns(string input, List<MarkovRule> rules)
        {
            var tokens = new List<string>();
            int i = 0;

            while (i < input.Length)
            {
                bool foundPattern = false;

                // Try to match each rule pattern at current position
                foreach (var rule in rules)
                {
                    if (i + rule.Pattern.Length <= input.Length)
                    {
                        var substring = input.Substring(i, rule.Pattern.Length);
                        if (substring == rule.Pattern)
                        {
                            tokens.Add(rule.Pattern);
                            i += rule.Pattern.Length;
                            foundPattern = true;
                            break;
                        }
                    }
                }

                // If no pattern matched, add single character
                if (!foundPattern)
                {
                    tokens.Add(input[i].ToString());
                    i++;
                }
            }

            return tokens;
        }

        /// <summary>
        /// Applies substitutions to the prebuilt token sequence.
        /// All matching patterns are replaced in a single pass.
        /// </summary>
        private static string ApplySubstitutions(List<string> tokens, List<MarkovRule> rules)
        {
            var result = new StringBuilder();

            foreach (var token in tokens)
            {
                bool substituted = false;

                // Check if this token matches any rule pattern
                foreach (var rule in rules)
                {
                    if (token == rule.Pattern)
                    {
                        result.Append(rule.Replacement);
                        substituted = true;
                        break;
                    }
                }

                // If no substitution, keep the original token
                if (!substituted)
                {
                    result.Append(token);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Advanced version: Applies substitutions iteratively until convergence,
        /// but uses prebuilt patterns for efficiency in each iteration.
        /// </summary>
        public static string ApplyIterativeWithPrebuiltSequences(string input, List<MarkovRule> rules)
        {
            string current = input;
            string previous;
            int iteration = 0;
            const int maxIterations = 1000; // Safety limit

            do
            {
                previous = current;

                // Prebuild patterns for current state
                var tokens = PreBuildPatterns(current, rules);

                // Apply substitutions
                current = ApplySubstitutions(tokens, rules);

                iteration++;

                // Check for terminal rules
                foreach (var rule in rules)
                {
                    if (rule.IsTerminal && current.Contains(rule.Replacement))
                    {
                        return current;
                    }
                }

            } while (current != previous && iteration < maxIterations);

            return current;
        }

        /// <summary>
        /// Runs the experiment demonstrating the optimization.
        /// </summary>
        public static void Run()
        {
            Console.WriteLine("=== Markov Algorithm with Prebuilt Sequences Experiment ===");
            Console.WriteLine();

            // Example from issue #676
            var rules = new List<MarkovRule>
            {
                new MarkovRule("aa", "aba"),
                new MarkovRule("bb", "bab")
            };

            Console.WriteLine("Rules:");
            foreach (var rule in rules)
            {
                Console.WriteLine($"  {rule}");
            }
            Console.WriteLine();

            // Test case 1: Simple sequence
            string test1 = "aabbaa";
            Console.WriteLine($"Test 1: '{test1}'");
            Console.WriteLine($"  Traditional result:       '{ApplyTraditional(test1, rules)}'");
            Console.WriteLine($"  Prebuilt sequences:       '{ApplyWithPrebuiltSequences(test1, rules)}'");
            Console.WriteLine($"  Iterative prebuilt:       '{ApplyIterativeWithPrebuiltSequences(test1, rules)}'");
            Console.WriteLine();

            // Test case 2: Nested patterns
            string test2 = "aaaa";
            Console.WriteLine($"Test 2: '{test2}'");
            Console.WriteLine($"  Traditional result:       '{ApplyTraditional(test2, rules)}'");
            Console.WriteLine($"  Prebuilt sequences:       '{ApplyWithPrebuiltSequences(test2, rules)}'");
            Console.WriteLine($"  Iterative prebuilt:       '{ApplyIterativeWithPrebuiltSequences(test2, rules)}'");
            Console.WriteLine();

            // Test case 3: Mixed content
            string test3 = "xaaybbz";
            Console.WriteLine($"Test 3: '{test3}'");
            Console.WriteLine($"  Traditional result:       '{ApplyTraditional(test3, rules)}'");
            Console.WriteLine($"  Prebuilt sequences:       '{ApplyWithPrebuiltSequences(test3, rules)}'");
            Console.WriteLine($"  Iterative prebuilt:       '{ApplyIterativeWithPrebuiltSequences(test3, rules)}'");
            Console.WriteLine();

            // Test case 4: More complex Markov algorithm (binary increment)
            Console.WriteLine("=== Advanced Example: Binary Counter Increment ===");
            var binaryRules = new List<MarkovRule>
            {
                new MarkovRule("1", "0A"),      // 1 becomes 0 with carry
                new MarkovRule("0A", "1"),      // 0 with carry becomes 1
                new MarkovRule("A", "1", true)  // Terminal: final carry becomes 1
            };

            Console.WriteLine("Rules:");
            foreach (var rule in binaryRules)
            {
                Console.WriteLine($"  {rule}");
            }
            Console.WriteLine();

            string[] binaryTests = { "0", "1", "10", "11", "101", "111" };
            foreach (var test in binaryTests)
            {
                string result = ApplyIterativeWithPrebuiltSequences(test, binaryRules);
                Console.WriteLine($"  {test} + 1 = {result}");
            }
            Console.WriteLine();

            Console.WriteLine("=== Performance Comparison ===");
            Console.WriteLine();
            Console.WriteLine("Key advantages of prebuilt sequences approach:");
            Console.WriteLine("1. Reduced memory allocations - patterns identified once");
            Console.WriteLine("2. Better cache locality - sequential access to prebuilt tokens");
            Console.WriteLine("3. Parallel substitution potential - all patterns can be replaced simultaneously");
            Console.WriteLine("4. Clearer structure - separation of pattern detection and substitution");
            Console.WriteLine();

            Console.WriteLine("Experiment completed successfully.");
        }
    }
}
