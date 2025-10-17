using System;
using System.Threading.Tasks;
using Platform.Examples;

namespace Examples
{
    /// <summary>
    /// Example demonstrating how to use the WikiPageChecker service
    /// to check if Wikipedia and Wiktionary pages exist for words or phrases.
    /// </summary>
    public class WikiPageCheckerExample
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== Wikipedia/Wiktionary Page Checker Example ===\n");

            var checker = new WikiPageChecker();

            // Example 1: Check a common English word
            Console.WriteLine("Example 1: Checking 'computer'");
            await CheckWord(checker, "computer");

            // Example 2: Check a phrase
            Console.WriteLine("\nExample 2: Checking 'artificial intelligence'");
            await CheckWord(checker, "artificial intelligence");

            // Example 3: Check a word that might not exist
            Console.WriteLine("\nExample 3: Checking 'xyzabc123' (non-existent word)");
            await CheckWord(checker, "xyzabc123");

            // Example 4: Check in different language (Russian)
            Console.WriteLine("\nExample 4: Checking 'компьютер' in Russian");
            await CheckWordInLanguage(checker, "компьютер", "ru");

            // Example 5: Individual checks
            Console.WriteLine("\nExample 5: Individual Wikipedia and Wiktionary checks");
            var wikipediaResult = await checker.CheckWikipediaPageAsync("philosophy");
            var wiktionaryResult = await checker.CheckWiktionaryPageAsync("philosophy");
            Console.WriteLine(wikipediaResult);
            Console.WriteLine(wiktionaryResult);

            Console.WriteLine("\n=== Example completed ===");
        }

        private static async Task CheckWord(WikiPageChecker checker, string query)
        {
            var results = await checker.CheckBothAsync(query);
            Console.WriteLine(results);
        }

        private static async Task CheckWordInLanguage(WikiPageChecker checker, string query, string language)
        {
            var results = await checker.CheckBothAsync(query, language);
            Console.WriteLine(results);
        }
    }
}
