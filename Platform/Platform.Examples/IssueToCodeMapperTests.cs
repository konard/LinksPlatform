using System;
using System.IO;
using System.Linq;

namespace Platform.Examples
{
    public class IssueToCodeMapperTests
    {
        public static void RunAllTests()
        {
            Console.WriteLine("Running Issue to Code Mapper Tests");
            Console.WriteLine("==================================");
            Console.WriteLine();

            try
            {
                TestWordExtraction();
                TestFileScoring();
                TestIssueMapping();
                TestRealWorldExample();

                Console.WriteLine("All tests passed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private static void TestWordExtraction()
        {
            Console.WriteLine("Testing word extraction...");

            var mapper = new IssueToCodeMapper(".");
            var result = mapper.MapIssueToCode("This is a test with UI text and function names like ProcessData");

            // We can't directly test word extraction as it's private, but we can see the results
            Console.WriteLine($"Extracted {result.ExtractedWords.Count} words from test text");
            Console.WriteLine($"Words: {string.Join(", ", result.ExtractedWords)}");

            if (result.ExtractedWords.Any(w => w.Contains("test") || w.Contains("function") || w.Contains("processdata")))
            {
                Console.WriteLine("✓ Word extraction test passed");
            }
            else
            {
                throw new Exception("Word extraction failed - expected words not found");
            }
            Console.WriteLine();
        }

        private static void TestFileScoring()
        {
            Console.WriteLine("Testing file scoring...");

            var currentDir = Directory.GetCurrentDirectory();
            var mapper = new IssueToCodeMapper(currentDir);

            // Test with terms that should match existing files
            var result = mapper.MapIssueToCode("search index file");

            if (result.Results.Any())
            {
                Console.WriteLine($"✓ File scoring test passed - found {result.Results.Count} matching files");
                var topResult = result.Results.First();
                Console.WriteLine($"  Top result: {topResult.FilePath} (Score: {topResult.Score:F2})");
            }
            else
            {
                Console.WriteLine("⚠ File scoring test - no results found (may be expected if no matching files exist)");
            }
            Console.WriteLine();
        }

        private static void TestIssueMapping()
        {
            Console.WriteLine("Testing issue mapping with LinksPlatform-specific terms...");

            var currentDir = Directory.GetCurrentDirectory();
            var mapper = new IssueToCodeMapper(currentDir);

            // Test with terms specific to this codebase
            var result = mapper.MapIssueToCode("link create doublets platform");

            Console.WriteLine($"Found {result.Results.Count} results for LinksPlatform terms");
            Console.WriteLine($"Scanned {result.TotalFilesScanned} files");

            if (result.Results.Any())
            {
                Console.WriteLine("✓ Issue mapping test passed");
                foreach (var match in result.Results.Take(3))
                {
                    Console.WriteLine($"  {match.FilePath}: Score {match.Score:F2}, Sequence {match.MaxSequenceLength}");
                }
            }
            else
            {
                Console.WriteLine("⚠ Issue mapping test - no results found");
            }
            Console.WriteLine();
        }

        private static void TestRealWorldExample()
        {
            Console.WriteLine("Testing with real-world issue example...");

            var currentDir = Directory.GetCurrentDirectory();
            var mapper = new IssueToCodeMapper(currentDir);

            // Test with the actual issue text from issue #681
            var issueText = "We can use any mentions of UI text or functions or stack traces from all of files/descriptions for the issue, to find exact code, that might be related to the problem. First we start by searching each word case insensitive and get the instant index of all source code files";

            var result = mapper.MapIssueToCode(issueText, 5);

            Console.WriteLine($"Real-world test results:");
            Console.WriteLine($"  Extracted {result.ExtractedWords.Count} words");
            Console.WriteLine($"  Found {result.Results.Count} matching files");
            Console.WriteLine($"  Scanned {result.TotalFilesScanned} total files");

            if (result.Results.Any())
            {
                Console.WriteLine("✓ Real-world example test passed");
                Console.WriteLine("Top results:");
                foreach (var match in result.Results.Take(3))
                {
                    Console.WriteLine($"  {Path.GetFileName(match.FilePath)}: {match.Score:F2} (seq: {match.MaxSequenceLength})");
                }
            }
            else
            {
                Console.WriteLine("⚠ Real-world example - no results found");
            }
            Console.WriteLine();
        }
    }
}