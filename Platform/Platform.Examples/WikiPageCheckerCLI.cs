using System;
using Platform.IO;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the WikiPageChecker service.
    /// Allows users to check Wikipedia and Wiktionary pages from the command line.
    /// </summary>
    public class WikiPageCheckerCLI : ICommandLineInterface
    {
        public void Run(string[] args)
        {
            var query = ConsoleHelpers.GetOrReadArgument(0, "Word or phrase to search", args);
            var language = args.Length > 1 ? args[1] : "en";

            if (string.IsNullOrWhiteSpace(query))
            {
                Console.WriteLine("Query cannot be empty.");
                return;
            }

            Console.WriteLine($"Checking Wikipedia and Wiktionary for: '{query}' (language: {language})");
            Console.WriteLine();

            var checker = new WikiPageChecker();

            try
            {
                var result = checker.CheckBothAsync(query, language).GetAwaiter().GetResult();

                Console.WriteLine("=== Results ===");
                Console.WriteLine(result.Wikipedia);
                Console.WriteLine(result.Wiktionary);
                Console.WriteLine();

                if (result.Wikipedia.Exists || result.Wiktionary.Exists)
                {
                    Console.WriteLine("Found pages:");
                    if (result.Wikipedia.Exists)
                    {
                        Console.WriteLine($"  Wikipedia: {result.Wikipedia.Url}");
                    }
                    if (result.Wiktionary.Exists)
                    {
                        Console.WriteLine($"  Wiktionary: {result.Wiktionary.Url}");
                    }
                }
                else
                {
                    Console.WriteLine("No pages found for the given query.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
