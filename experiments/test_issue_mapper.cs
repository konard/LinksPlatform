using System;
using System.IO;
using Platform.Examples;

// Simple test script to verify the IssueToCodeMapper implementation
class TestProgram
{
    static void Main(string[] args)
    {
        Console.WriteLine("Testing Issue to Code Mapper Implementation");
        Console.WriteLine("==========================================");
        Console.WriteLine();

        try
        {
            // Test with the current directory (LinksPlatform codebase)
            string codebaseRoot = Directory.GetCurrentDirectory();
            Console.WriteLine($"Testing with codebase: {codebaseRoot}");

            var mapper = new IssueToCodeMapper(codebaseRoot);

            // Test case 1: Search for indexing-related terms
            Console.WriteLine("Test 1: Searching for 'index search file code'");
            var result1 = mapper.MapIssueToCode("index search file code", 5);
            DisplayResults(result1);

            Console.WriteLine("\n" + new string('=', 50) + "\n");

            // Test case 2: Search for the actual issue #681 text
            Console.WriteLine("Test 2: Searching with issue #681 text");
            string issueText = "We can use any mentions of UI text or functions or stack traces from all of files descriptions for the issue to find exact code that might be related to the problem First we start by searching each word case insensitive and get the instant index of all source code files that might be related";
            var result2 = mapper.MapIssueToCode(issueText, 10);
            DisplayResults(result2);

            Console.WriteLine("\n" + new string('=', 50) + "\n");

            // Test case 3: Search for platform-specific terms
            Console.WriteLine("Test 3: Searching for 'platform data doublets links'");
            var result3 = mapper.MapIssueToCode("platform data doublets links", 5);
            DisplayResults(result3);

            Console.WriteLine("\nAll tests completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during testing: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    static void DisplayResults(IssueToCodeMappingResult result)
    {
        Console.WriteLine($"Query: {result.Query}");
        Console.WriteLine($"Extracted Words: {string.Join(", ", result.ExtractedWords)}");
        Console.WriteLine($"Files Scanned: {result.TotalFilesScanned}");
        Console.WriteLine($"Results Found: {result.Results.Count}");
        Console.WriteLine();

        if (result.Results.Count == 0)
        {
            Console.WriteLine("No matching files found.");
            return;
        }

        Console.WriteLine("Top Results:");
        for (int i = 0; i < Math.Min(result.Results.Count, 5); i++)
        {
            var match = result.Results[i];
            string relativePath = match.FilePath.Replace(Directory.GetCurrentDirectory(), ".");
            Console.WriteLine($"{i + 1}. {relativePath}");
            Console.WriteLine($"   Score: {match.Score:F2}, Sequence: {match.MaxSequenceLength}");
            Console.WriteLine($"   Matched Words: {string.Join(", ", match.MatchedWords)}");

            if (match.LineMatches.Count > 0)
            {
                Console.WriteLine($"   Sample Lines ({Math.Min(match.LineMatches.Count, 2)} of {match.LineMatches.Count}):");
                foreach (var line in match.LineMatches.Take(2))
                {
                    Console.WriteLine($"     {line.LineNumber}: {line.LineContent.Substring(0, Math.Min(line.LineContent.Length, 80))}...");
                }
            }
            Console.WriteLine();
        }
    }
}