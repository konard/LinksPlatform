using System;
using System.IO;
using System.Linq;
using Platform.Memory;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the Autopatterns engine.
    /// Allows users to analyze data and discover patterns from various sources.
    /// </summary>
    public class AutopatternsEngineCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            var command = args[0].ToLower();

            switch (command)
            {
                case "analyze":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Error: 'analyze' command requires input data or file path.");
                        ShowHelp();
                        return;
                    }
                    AnalyzeData(args[1], args);
                    break;

                case "file":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Error: 'file' command requires a file path.");
                        ShowHelp();
                        return;
                    }
                    AnalyzeFile(args[1], args);
                    break;

                case "help":
                case "--help":
                case "-h":
                    ShowHelp();
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}");
                    ShowHelp();
                    break;
            }
        }

        private void AnalyzeData(string data, string[] args)
        {
            var verbose = args.Contains("--verbose") || args.Contains("-v");
            var limit = GetIntParameter(args, "--limit", 20);

            Console.WriteLine("Autopatterns Engine - Data Analysis");
            Console.WriteLine("====================================\n");

            try
            {
                using (var memory = new HeapResizableDirectMemory())
                using (var links = new UnitedMemoryLinks<uint>(memory))
                {
                    var engine = new AutopatternsEngine<uint>(links);

                    Console.WriteLine($"Analyzing data of length: {data.Length} characters\n");

                    // Discover patterns
                    var patterns = engine.DiscoverPatterns(data).ToList();

                    Console.WriteLine($"Discovered {patterns.Count} patterns:\n");

                    // Display patterns
                    var displayedCount = 0;
                    foreach (var pattern in patterns.Take(limit))
                    {
                        if (verbose)
                        {
                            Console.WriteLine(engine.GeneratePatternDefinition(pattern));
                            Console.WriteLine();
                        }
                        else
                        {
                            Console.WriteLine($"[{pattern.Type}] {pattern.Description} (frequency: {pattern.Frequency})");
                            if (!string.IsNullOrEmpty(pattern.Example) && pattern.Example.Length <= 50)
                            {
                                Console.WriteLine($"  Example: {pattern.Example}");
                            }
                        }
                        displayedCount++;
                    }

                    if (patterns.Count > limit)
                    {
                        Console.WriteLine($"\n... and {patterns.Count - limit} more patterns (use --limit to see more)");
                    }

                    // Analyze structural relationships if links were created
                    var structuralPatterns = engine.AnalyzeStructuralRelationships().ToList();
                    if (structuralPatterns.Any())
                    {
                        Console.WriteLine($"\n\nStructural Relationship Patterns: {structuralPatterns.Count}");
                        foreach (var pattern in structuralPatterns.Take(10))
                        {
                            Console.WriteLine($"  {pattern.Description} (frequency: {pattern.Frequency})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during analysis: {ex.Message}");
                if (verbose)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        private void AnalyzeFile(string filePath, string[] args)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found: {filePath}");
                return;
            }

            try
            {
                Console.WriteLine($"Reading file: {filePath}");
                var data = File.ReadAllText(filePath);
                Console.WriteLine($"File size: {data.Length} characters\n");

                // Update args to pass data to AnalyzeData
                var newArgs = new string[args.Length];
                Array.Copy(args, newArgs, args.Length);
                newArgs[1] = data;

                AnalyzeData(data, args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }

        private int GetIntParameter(string[] args, string paramName, int defaultValue)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == paramName)
                {
                    if (int.TryParse(args[i + 1], out int value))
                    {
                        return value;
                    }
                }
            }
            return defaultValue;
        }

        private void ShowHelp()
        {
            Console.WriteLine("Autopatterns Engine - Command Line Interface");
            Console.WriteLine("=============================================\n");
            Console.WriteLine("A minimal engine that generates definitions/descriptions of patterns based on any data.");
            Console.WriteLine("Can be used for science. Can be used to understand and recreate structural definition of any language.\n");
            Console.WriteLine("Usage:");
            Console.WriteLine("  autopatterns analyze <data> [options]");
            Console.WriteLine("  autopatterns file <file_path> [options]");
            Console.WriteLine("  autopatterns help\n");
            Console.WriteLine("Commands:");
            Console.WriteLine("  analyze <data>     Analyze the provided text data for patterns");
            Console.WriteLine("  file <path>        Analyze patterns in the specified file");
            Console.WriteLine("  help               Show this help message\n");
            Console.WriteLine("Options:");
            Console.WriteLine("  --verbose, -v      Show detailed pattern information");
            Console.WriteLine("  --limit <n>        Limit the number of patterns displayed (default: 20)\n");
            Console.WriteLine("Examples:");
            Console.WriteLine("  autopatterns analyze \"hello world hello world\"");
            Console.WriteLine("  autopatterns file input.txt --verbose");
            Console.WriteLine("  autopatterns analyze \"abcabcabc\" --limit 10");
        }
    }
}
