// Example demonstrating the Global Research Database and Dynamic Research System
// This file shows how to use the research system to define, execute, and share research queries

using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Examples;

namespace ResearchSystemDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Global Research Database & Dynamic Research System Demo ===\n");

            // Initialize the links storage (in-memory for demo)
            using (var links = new UnitedMemoryLinks<ulong>())
            {
                // Create some sample data
                CreateSampleData(links);

                // Initialize the research database and system
                var database = new ResearchDatabase<ulong>(links);
                var researchSystem = new DynamicResearchSystem<ulong>(links, database);

                // Subscribe to execution events
                researchSystem.ResearchExecuted += (sender, e) =>
                {
                    Console.WriteLine($"[Event] Research '{e.ResearchName}' executed at {e.ExecutedAt:HH:mm:ss}");
                    Console.WriteLine($"        Result: {e.Result}\n");
                };

                // Example 1: Create a simple counting research
                Console.WriteLine("--- Example 1: Count All Links ---");
                var countResearch = new ResearchQuery<ulong>
                {
                    Name = "Total Link Counter",
                    Description = "Counts all links in the database",
                    SelectionMethod = SelectAllLinks,
                    TransformationMethod = data => data.Count,
                    InterpretationMethod = count => $"Total links: {count}",
                    IsContinuous = false
                };
                var countId = database.RegisterResearch(countResearch);
                Console.WriteLine($"Registered research with ID: {countId}");
                Console.WriteLine($"Executing...");
                var result = researchSystem.ExecuteResearch(countId);
                Console.WriteLine($"Result: {result}\n");

                // Example 2: Analyze link patterns
                Console.WriteLine("--- Example 2: Link Pattern Analysis ---");
                var patternResearch = new ResearchQuery<ulong>
                {
                    Name = "Link Pattern Analyzer",
                    Description = "Analyzes patterns in link connections",
                    SelectionMethod = SelectAllLinks,
                    TransformationMethod = AnalyzeLinkPatterns,
                    InterpretationMethod = FormatPatternAnalysis,
                    IsContinuous = false
                };
                var patternId = database.RegisterResearch(patternResearch);
                Console.WriteLine($"Executing pattern analysis...");
                result = researchSystem.ExecuteResearch(patternId);
                Console.WriteLine($"Result:\n{result}\n");

                // Example 3: Continuous monitoring research
                Console.WriteLine("--- Example 3: Continuous Database Monitor ---");
                var monitorResearch = new ResearchQuery<ulong>
                {
                    Name = "Database Growth Monitor",
                    Description = "Continuously monitors database statistics",
                    SelectionMethod = SelectAllLinks,
                    TransformationMethod = data => new
                    {
                        Count = data.Count,
                        Timestamp = DateTime.UtcNow,
                        SampleLinks = data.Take(3).ToList()
                    },
                    InterpretationMethod = obj =>
                    {
                        dynamic stats = obj;
                        return $"[{stats.Timestamp:HH:mm:ss}] Database has {stats.Count} links";
                    },
                    IsContinuous = true
                };
                var monitorId = database.RegisterResearch(monitorResearch);
                Console.WriteLine($"Registered continuous research with ID: {monitorId}");
                Console.WriteLine($"Starting continuous execution for 5 seconds...\n");

                researchSystem.StartContinuousResearch(monitorResearch);
                System.Threading.Thread.Sleep(5000); // Run for 5 seconds
                researchSystem.StopContinuousResearch(monitorId);

                Console.WriteLine("\nContinuous research stopped.");

                // Display database statistics
                Console.WriteLine("\n--- Database Statistics ---");
                Console.WriteLine(database.GetStatistics());

                // Display all registered researches
                Console.WriteLine("\n--- Registered Researches ---");
                Console.WriteLine(database.ListResearches());

                // Export data
                Console.WriteLine("--- Exporting Research Data ---");
                var exportPath = "research-export.txt";
                database.ExportData(exportPath);
                Console.WriteLine($"Data exported to {exportPath}");

                Console.WriteLine("\n=== Demo Complete ===");
            }
        }

        static IList<IList<ulong>> SelectAllLinks(ILinks<ulong> links)
        {
            var result = new List<IList<ulong>>();
            links.Each(link =>
            {
                result.Add(link);
                return links.Constants.Continue;
            });
            return result;
        }

        static object AnalyzeLinkPatterns(IList<IList<ulong>> data)
        {
            var patterns = new Dictionary<string, int>
            {
                { "SelfReferencing", 0 },
                { "UniqueSource", 0 },
                { "UniqueTarget", 0 }
            };

            var sources = new HashSet<ulong>();
            var targets = new HashSet<ulong>();

            foreach (var link in data)
            {
                if (link.Count >= 3)
                {
                    var source = link[1];
                    var target = link[2];

                    if (source == target)
                    {
                        patterns["SelfReferencing"]++;
                    }

                    sources.Add(source);
                    targets.Add(target);
                }
            }

            patterns["UniqueSource"] = sources.Count;
            patterns["UniqueTarget"] = targets.Count;

            return patterns;
        }

        static string FormatPatternAnalysis(object obj)
        {
            var patterns = (Dictionary<string, int>)obj;
            return string.Join("\n", patterns.Select(kvp => $"  {kvp.Key}: {kvp.Value}"));
        }

        static void CreateSampleData(ILinks<ulong> links)
        {
            // Create some sample links for demonstration
            for (ulong i = 1; i <= 10; i++)
            {
                links.Create(i, i + 1);
            }

            // Create a few self-referencing links
            links.Create(1, 1);
            links.Create(5, 5);

            Console.WriteLine("Sample data created: 12 links\n");
        }
    }
}
