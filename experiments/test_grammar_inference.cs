using System;
using System.Linq;
using Platform.Examples.GrammarInference;

namespace GrammarInferenceExperiment
{
    /// <summary>
    /// Experiment script to test grammar inference implementation
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Grammar Inference Experiment");
            Console.WriteLine("===========================");
            Console.WriteLine();

            TestBasicSequitur();
            TestRepeatedPatterns();
            TestComplexPatterns();

            Console.WriteLine();
            Console.WriteLine("All experiments completed successfully!");
        }

        static void TestBasicSequitur()
        {
            Console.WriteLine("Test 1: Basic Sequitur - Simple repetition");
            Console.WriteLine("-------------------------------------------");

            var input = "abcabc";
            var sequence = input.Select(c => (ulong)c);

            var grammar = new SequiturGrammar();
            grammar.LearnFromSequence(sequence);

            Console.WriteLine($"Input: {input}");
            Console.WriteLine(grammar.ToString());
            Console.WriteLine();
        }

        static void TestRepeatedPatterns()
        {
            Console.WriteLine("Test 2: Repeated Patterns");
            Console.WriteLine("--------------------------");

            var input = "ababababab";
            var sequence = input.Select(c => (ulong)c);

            var grammar = new SequiturGrammar();
            grammar.LearnFromSequence(sequence);

            Console.WriteLine($"Input: {input}");
            Console.WriteLine(grammar.ToString());
            Console.WriteLine();
        }

        static void TestComplexPatterns()
        {
            Console.WriteLine("Test 3: Complex Hierarchical Patterns");
            Console.WriteLine("--------------------------------------");

            var input = "abcabcdefdef";
            var sequence = input.Select(c => (ulong)c);

            var grammar = new SequiturGrammar();
            grammar.LearnFromSequence(sequence);

            Console.WriteLine($"Input: {input}");
            Console.WriteLine(grammar.ToString());
            Console.WriteLine();
        }
    }
}
