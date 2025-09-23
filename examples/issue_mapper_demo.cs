using System;
using System.IO;
using Platform.Examples;

// Demo script showing how to use IssueToCodeMapper
// This demonstrates the core functionality requested in issue #681

class IssueMapperDemo
{
    static void Main(string[] args)
    {
        Console.WriteLine("Issue to Code Mapper Demo");
        Console.WriteLine("=========================");
        Console.WriteLine("This demo shows how to map issue descriptions to related source code files");
        Console.WriteLine("by analyzing word matches and ranking files based on sequence relevance.");
        Console.WriteLine();

        // Use the current directory as the codebase root
        string codebaseRoot = Directory.GetCurrentDirectory();
        var mapper = new IssueToCodeMapper(codebaseRoot);

        // Example 1: Issue about search functionality
        Console.WriteLine("Example 1: Issue about search functionality");
        Console.WriteLine("-------------------------------------------");
        string searchIssue = "The search function is not working properly when indexing files. Users report that text search returns no results even when the keywords exist in the codebase.";

        var result1 = mapper.MapIssueToCode(searchIssue, 5);
        PrintSummary(result1);

        Console.WriteLine("\n" + new string('=', 60) + "\n");

        // Example 2: Issue about UI components
        Console.WriteLine("Example 2: Issue about UI components");
        Console.WriteLine("------------------------------------");
        string uiIssue = "The web terminal interface has rendering issues. The links controller is not properly displaying the model data in the view.";

        var result2 = mapper.MapIssueToCode(uiIssue, 5);
        PrintSummary(result2);

        Console.WriteLine("\n" + new string('=', 60) + "\n");

        // Example 3: The actual issue #681
        Console.WriteLine("Example 3: The actual issue #681");
        Console.WriteLine("---------------------------------");
        string actualIssue = "We can use any mentions of UI text or functions or stack traces from all of files/descriptions for the issue, to find exact code, that might be related to the problem. First we start by searching each word case insensitive and get the instant index of all source code files, that might be related, after that we just range files by how long sequences of words we get there.";

        var result3 = mapper.MapIssueToCode(actualIssue, 8);
        PrintSummary(result3);

        Console.WriteLine("\n" + new string('=', 60) + "\n");
        Console.WriteLine("Demo completed! This shows how the issue-to-code mapping");
        Console.WriteLine("can help developers quickly locate relevant source files");
        Console.WriteLine("based on issue descriptions, stack traces, or UI text mentions.");
    }

    static void PrintSummary(IssueToCodeMappingResult result)
    {
        Console.WriteLine($"Query: {result.Query.Substring(0, Math.Min(result.Query.Length, 100))}...");
        Console.WriteLine($"Key Words: {string.Join(", ", result.ExtractedWords.Take(8))}");
        Console.WriteLine($"Files Analyzed: {result.TotalFilesScanned}, Matches Found: {result.Results.Count}");
        Console.WriteLine();

        if (result.Results.Count == 0)
        {
            Console.WriteLine("No relevant files found.");
            return;
        }

        Console.WriteLine("Most Relevant Files:");
        for (int i = 0; i < Math.Min(result.Results.Count, 5); i++)
        {
            var match = result.Results[i];
            string fileName = Path.GetFileName(match.FilePath);
            string directory = Path.GetDirectoryName(match.FilePath)?.Replace(Directory.GetCurrentDirectory(), "");

            Console.WriteLine($"{i + 1}. {fileName} (in {directory})");
            Console.WriteLine($"   Relevance Score: {match.Score:F1}, Word Sequence Length: {match.MaxSequenceLength}");
            Console.WriteLine($"   Matched Keywords: {string.Join(", ", match.MatchedWords.Take(5))}");

            if (match.LineMatches.Count > 0)
            {
                var topLine = match.LineMatches.First();
                Console.WriteLine($"   Example (line {topLine.LineNumber}): {topLine.LineContent.Trim().Substring(0, Math.Min(topLine.LineContent.Trim().Length, 70))}...");
            }
            Console.WriteLine();
        }

        // Show comparison suggestions
        Console.WriteLine("For comparison:");
        Console.WriteLine($"GitHub Search: gh search code --owner konard {string.Join(" ", result.ExtractedWords.Take(3))}");
        Console.WriteLine($"Web Search: site:github.com/konard/LinksPlatform {string.Join(" ", result.ExtractedWords.Take(4))}");
    }
}