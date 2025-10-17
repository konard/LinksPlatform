using System;
using System.IO;
using Platform.IO;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the bug fixing by analogy tool.
    /// Searches for similar code snippets across GitHub to help identify potential bugs or solutions.
    /// </summary>
    public class BugFixByAnalogyToolCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            Console.WriteLine("=== Bug Fixing by Analogy Tool ===");
            Console.WriteLine("Searches for similar code snippets across GitHub repositories.");
            Console.WriteLine();

            // Get code snippet (from file or direct input)
            var codeInput = ConsoleHelpers.GetOrReadArgument(0, "Code snippet or file path", args);

            string codeSnippet;
            if (File.Exists(codeInput))
            {
                Console.WriteLine($"Reading code from file: {codeInput}");
                codeSnippet = File.ReadAllText(codeInput);
            }
            else
            {
                codeSnippet = codeInput;
            }

            if (string.IsNullOrWhiteSpace(codeSnippet))
            {
                Console.WriteLine("Error: Code snippet cannot be empty.");
                return;
            }

            // Get optional language filter
            var language = args.Length > 1 ? args[1] : null;
            if (string.IsNullOrEmpty(language))
            {
                Console.Write("Programming language (optional, e.g., C#, Python, JavaScript): ");
                language = Console.ReadLine();
            }

            // Get optional limit
            var limitStr = args.Length > 2 ? args[2] : null;
            if (string.IsNullOrEmpty(limitStr))
            {
                Console.Write("Number of results (default 10): ");
                limitStr = Console.ReadLine();
            }

            int limit = 10;
            if (!string.IsNullOrEmpty(limitStr) && !int.TryParse(limitStr, out limit))
            {
                Console.WriteLine("Invalid limit, using default (10).");
                limit = 10;
            }

            Console.WriteLine();
            Console.WriteLine($"Searching for similar code snippets...");
            Console.WriteLine($"Language filter: {(string.IsNullOrEmpty(language) ? "None" : language)}");
            Console.WriteLine($"Result limit: {limit}");
            Console.WriteLine();

            // Perform search
            var searcher = new CodeSimilaritySearcher();
            var results = searcher.SearchSimilarCode(codeSnippet, language, limit);

            // Display results
            if (results.Count == 0)
            {
                Console.WriteLine("No similar code snippets found.");
                Console.WriteLine();
                Console.WriteLine("Tips:");
                Console.WriteLine("- Try removing language-specific syntax");
                Console.WriteLine("- Use more general search terms");
                Console.WriteLine("- Ensure GitHub CLI (gh) is authenticated");
            }
            else
            {
                Console.WriteLine($"Found {results.Count} similar code snippet(s):");
                Console.WriteLine();

                for (int i = 0; i < results.Count; i++)
                {
                    Console.WriteLine($"[{i + 1}] {results[i]}");
                    Console.WriteLine();
                }

                Console.WriteLine("=== Analysis Tips ===");
                Console.WriteLine("1. Compare your code with the similar snippets found");
                Console.WriteLine("2. Look for differences that might indicate bugs in your code");
                Console.WriteLine("3. Check if similar code has different error handling or edge cases");
                Console.WriteLine("4. Review commit history of similar files for bug fixes");
            }

            Console.WriteLine();
            Console.WriteLine("=== Session Complete ===");
        }
    }
}
