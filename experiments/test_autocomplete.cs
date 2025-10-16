// Simple test script for sequence autocomplete functionality
// This script demonstrates the three main features from issue #121:
// 1. Autocomplete up to end of the word
// 2. Autocomplete up to end of sentence
// 3. Fuzzy autocomplete (to correct mistakes/errors/typos)

using System;
using Platform.Examples;

namespace AutocompleteExperiment
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing Sequence Autocomplete Implementation");
            Console.WriteLine("============================================");
            Console.WriteLine();
            Console.WriteLine("This demonstrates issue #121 features:");
            Console.WriteLine("1. Word completion");
            Console.WriteLine("2. Sentence completion");
            Console.WriteLine("3. Fuzzy completion (typo correction)");
            Console.WriteLine();

            var cli = new SequenceAutocompleteCLI();

            // Run in demo mode if no arguments provided
            var mode = args.Length > 0 ? args[0] : "demo";
            cli.Run("autocomplete_test.links", mode);
        }
    }
}
