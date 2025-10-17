using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for analyzing media comments and grouping them by semantic similarity.
    /// Demonstrates the MediaCommentsAnalyzer functionality.
    /// </summary>
    public class MediaCommentsAnalyzerCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }

            var command = args[0].ToLowerInvariant();

            switch (command)
            {
                case "analyze":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Error: Please specify a file path containing comments.");
                        ShowUsage();
                        return;
                    }
                    AnalyzeComments(args[1]);
                    break;

                case "demo":
                    RunDemo();
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}");
                    ShowUsage();
                    break;
            }
        }

        private void ShowUsage()
        {
            Console.WriteLine("MediaCommentsAnalyzer CLI - Groups media event comments by semantic similarity");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  analyze <file>  - Analyze comments from a text file (one comment per line)");
            Console.WriteLine("  demo            - Run a demonstration with sample YouTube-style comments");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  MediaCommentsAnalyzerCLI analyze comments.txt");
        }

        private void AnalyzeComments(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found: {filePath}");
                return;
            }

            Console.WriteLine($"Analyzing comments from: {filePath}");
            Console.WriteLine();

            var comments = File.ReadAllLines(filePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            Console.WriteLine($"Loaded {comments.Count} comments.");
            Console.WriteLine();

            AnalyzeAndDisplay(comments);
        }

        private void RunDemo()
        {
            Console.WriteLine("=== Media Comments Analyzer Demo ===");
            Console.WriteLine();
            Console.WriteLine("Analyzing sample YouTube-style comments...");
            Console.WriteLine();

            var sampleComments = new List<string>
            {
                "This is amazing! Great work!",
                "Absolutely amazing content, keep it up!",
                "This video is incredible. Thanks for sharing!",
                "Great job on this video!",
                "I learned so much from this tutorial. Very helpful!",
                "Super helpful tutorial, thank you!",
                "This tutorial helped me understand the concept better.",
                "The explanation was very clear and easy to follow.",
                "Clear explanation, well done!",
                "Easy to understand, great teaching style.",
                "The music in the background is too loud.",
                "Background music is distracting.",
                "Could you lower the background music next time?",
                "I love this channel!",
                "Best channel ever!",
                "This is my favorite channel on YouTube.",
                "Looking forward to more videos like this!",
                "Can't wait for the next video!",
                "Please make more videos on this topic.",
                "When is the next part coming out?"
            };

            Console.WriteLine("Sample comments:");
            for (int i = 0; i < sampleComments.Count && i < 5; i++)
            {
                Console.WriteLine($"  {i + 1}. \"{sampleComments[i]}\"");
            }
            Console.WriteLine($"  ... and {sampleComments.Count - 5} more");
            Console.WriteLine();

            AnalyzeAndDisplay(sampleComments);
        }

        private void AnalyzeAndDisplay(List<string> comments)
        {
            // Create a temporary Links storage for analysis
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var links = new UnitedMemoryLinks<ulong>(tempFile))
                {
                    var synchronizedLinks = new SynchronizedLinks<ulong>(links);
                    var analyzer = new MediaCommentsAnalyzer<ulong>(synchronizedLinks);

                Console.WriteLine("Analyzing and grouping comment fragments by semantic similarity...");
                Console.WriteLine();

                var groups = analyzer.AnalyzeComments(comments).ToList();

                Console.WriteLine($"Found {groups.Count} semantic groups:");
                Console.WriteLine();
                Console.WriteLine("=== Results (sorted by frequency) ===");
                Console.WriteLine();

                int rank = 1;
                foreach (var group in groups.Take(20)) // Show top 20 groups
                {
                    Console.WriteLine($"{rank}. Count: {group.Count}");
                    Console.WriteLine($"   Representative: \"{group.RepresentativeText}\"");

                    if (group.Variations.Count > 1)
                    {
                        Console.WriteLine($"   Variations ({group.Variations.Count}):");
                        foreach (var variation in group.Variations.Take(3))
                        {
                            Console.WriteLine($"     - \"{variation}\"");
                        }
                        if (group.Variations.Count > 3)
                        {
                            Console.WriteLine($"     ... and {group.Variations.Count - 3} more");
                        }
                    }

                    Console.WriteLine();
                    rank++;
                }

                if (groups.Count > 20)
                {
                    Console.WriteLine($"... and {groups.Count - 20} more groups with lower frequencies");
                    Console.WriteLine();
                }

                // Display summary statistics
                Console.WriteLine("=== Summary ===");
                Console.WriteLine($"Total comments analyzed: {comments.Count}");
                Console.WriteLine($"Total semantic groups: {groups.Count}");
                Console.WriteLine($"Most common fragment: \"{groups.First().RepresentativeText}\" ({groups.First().Count} occurrences)");
                Console.WriteLine($"Average occurrences per group: {groups.Average(g => g.Count):F2}");
                }
            }
            finally
            {
                // Clean up temporary file
                if (File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { }
                }
            }
        }
    }
}
