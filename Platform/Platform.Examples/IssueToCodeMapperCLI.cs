using System;
using System.IO;
using System.Linq;

namespace Platform.Examples
{
    public class IssueToCodeMapperCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    ShowUsage();
                    return;
                }

                var codebaseRoot = args[0];
                var issueText = args[1];
                int maxResults = args.Length > 2 && int.TryParse(args[2], out int max) ? max : 10;

                if (!Directory.Exists(codebaseRoot))
                {
                    Console.WriteLine($"Error: Directory '{codebaseRoot}' does not exist.");
                    return;
                }

                Console.WriteLine("Issue to Code Mapper");
                Console.WriteLine("===================");
                Console.WriteLine($"Codebase: {codebaseRoot}");
                Console.WriteLine($"Issue Text: {issueText}");
                Console.WriteLine($"Max Results: {maxResults}");
                Console.WriteLine();

                var mapper = new IssueToCodeMapper(codebaseRoot);
                var result = mapper.MapIssueToCode(issueText, maxResults);

                DisplayResults(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void ShowUsage()
        {
            Console.WriteLine("Issue to Code Mapper");
            Console.WriteLine("===================");
            Console.WriteLine();
            Console.WriteLine("Maps issue descriptions to related source code files by analyzing word");
            Console.WriteLine("matches and ranking files based on relevance.");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  IssueToCodeMapperCLI <codebase_path> <issue_text> [max_results]");
            Console.WriteLine();
            Console.WriteLine("Parameters:");
            Console.WriteLine("  codebase_path  - Path to the root directory of the codebase");
            Console.WriteLine("  issue_text     - The issue description or text to search for");
            Console.WriteLine("  max_results    - Maximum number of results to return (default: 10)");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  IssueToCodeMapperCLI /path/to/code \"UI text functions stack traces\" 5");
            Console.WriteLine("  IssueToCodeMapperCLI . \"search indexing implementation\"");
        }

        private void DisplayResults(IssueToCodeMappingResult result)
        {
            Console.WriteLine($"Extracted Words: {string.Join(", ", result.ExtractedWords)}");
            Console.WriteLine($"Files Scanned: {result.TotalFilesScanned}");
            Console.WriteLine($"Results Found: {result.Results.Count}");
            Console.WriteLine();

            if (!result.Results.Any())
            {
                Console.WriteLine("No matching files found.");
                return;
            }

            Console.WriteLine("Top Matching Files:");
            Console.WriteLine("==================");

            for (int i = 0; i < result.Results.Count; i++)
            {
                var match = result.Results[i];
                Console.WriteLine($"{i + 1}. {match.FilePath}");
                Console.WriteLine($"   Score: {match.Score:F2}");
                Console.WriteLine($"   Max Sequence Length: {match.MaxSequenceLength}");
                Console.WriteLine($"   Matched Words: {string.Join(", ", match.MatchedWords)}");

                if (match.LineMatches.Any())
                {
                    Console.WriteLine("   Relevant Lines:");
                    var topLines = match.LineMatches.Take(5); // Show top 5 lines
                    foreach (var line in topLines)
                    {
                        Console.WriteLine($"     {line.LineNumber}: {line.LineContent}");
                        if (line.MatchedWords.Any())
                        {
                            Console.WriteLine($"       Matches: {string.Join(", ", line.MatchedWords)}");
                        }
                    }
                    if (match.LineMatches.Count > 5)
                    {
                        Console.WriteLine($"     ... and {match.LineMatches.Count - 5} more lines");
                    }
                }

                Console.WriteLine();
            }

            Console.WriteLine("Comparison with GitHub Search:");
            Console.WriteLine("=============================");
            Console.WriteLine("To compare with GitHub Search, you can use:");
            Console.WriteLine($"gh search code --owner konard {string.Join(" ", result.ExtractedWords.Take(3))}");
            Console.WriteLine();
            Console.WriteLine("To compare with web search engines, search for:");
            Console.WriteLine($"site:github.com/konard/LinksPlatform {string.Join(" ", result.ExtractedWords.Take(5))}");
        }
    }
}