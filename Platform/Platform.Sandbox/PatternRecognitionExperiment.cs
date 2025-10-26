using System;
using System.Collections.Generic;

namespace Platform.Sandbox
{
    /// <summary>
    /// Experiments demonstrating the pattern recognition strategy based on frequency hypothesis.
    /// Tests examples from issue #628.
    /// </summary>
    public static class PatternRecognitionExperiment
    {
        /// <summary>
        /// Example 1 from issue #628:
        /// a a b
        /// a a c
        /// Expected: Common pattern is "a a *" where b and c have the same frequency.
        /// </summary>
        public static void Example1()
        {
            var contexts = new List<List<string>>
            {
                new List<string> { "a", "a", "b" },
                new List<string> { "a", "a", "c" }
            };

            PatternRecognition.AnalyzeAndPrint("Example 1 (a a b, a a c)", contexts);
        }

        /// <summary>
        /// Example 2 from issue #628:
        /// a b b
        /// c b b
        /// Expected: Common pattern is "* b b" where a and c have the same frequency.
        /// </summary>
        public static void Example2()
        {
            var contexts = new List<List<string>>
            {
                new List<string> { "a", "b", "b" },
                new List<string> { "c", "b", "b" }
            };

            PatternRecognition.AnalyzeAndPrint("Example 2 (a b b, c b b)", contexts);
        }

        /// <summary>
        /// Example 3: Testing with more contexts.
        /// </summary>
        public static void Example3()
        {
            var contexts = new List<List<string>>
            {
                new List<string> { "x", "y", "z" },
                new List<string> { "a", "y", "z" },
                new List<string> { "b", "y", "z" }
            };

            PatternRecognition.AnalyzeAndPrint("Example 3 (x y z, a y z, b y z)", contexts);
        }

        /// <summary>
        /// Example 4: Testing with identical contexts.
        /// </summary>
        public static void Example4()
        {
            var contexts = new List<List<string>>
            {
                new List<string> { "a", "a", "a" },
                new List<string> { "a", "a", "a" }
            };

            PatternRecognition.AnalyzeAndPrint("Example 4 (a a a, a a a)", contexts);
        }

        /// <summary>
        /// Example 5: Testing with longer sequences.
        /// </summary>
        public static void Example5()
        {
            var contexts = new List<List<string>>
            {
                new List<string> { "the", "cat", "sat", "on", "the", "mat" },
                new List<string> { "the", "dog", "sat", "on", "the", "mat" },
                new List<string> { "the", "bird", "sat", "on", "the", "mat" }
            };

            PatternRecognition.AnalyzeAndPrint("Example 5 (sentence patterns)", contexts);
        }

        /// <summary>
        /// Example 6: Testing Markov chain concept (from issue comment).
        /// </summary>
        public static void Example6()
        {
            var contexts = new List<List<string>>
            {
                new List<string> { "state1", "transition", "state2" },
                new List<string> { "state3", "transition", "state4" },
                new List<string> { "state5", "transition", "state6" }
            };

            PatternRecognition.AnalyzeAndPrint("Example 6 (Markov-like transitions)", contexts);
        }

        /// <summary>
        /// Runs all pattern recognition experiments.
        /// </summary>
        public static void RunAll()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  Pattern Recognition Strategy - Frequency Hypothesis Tests   ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("Hypothesis: Everything that has the same frequency of occurrence");
            Console.WriteLine("in multiple contexts is the same thing.");
            Console.WriteLine();
            Console.WriteLine("Formula: total_occurrences_in_all_contexts / total_contexts");

            Example1();
            Example2();
            Example3();
            Example4();
            Example5();
            Example6();

            Console.WriteLine("\n" + new string('=', 64));
            Console.WriteLine("All experiments completed.");
            Console.WriteLine(new string('=', 64));
        }
    }
}
