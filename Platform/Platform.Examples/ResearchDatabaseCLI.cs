using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the Global Research Database and Dynamic Research System.
    /// </summary>
    public class ResearchDatabaseCLI : ICommandLineInterface
    {
        private readonly ResearchDatabase<ulong> _database;
        private readonly DynamicResearchSystem<ulong> _researchSystem;
        private readonly SynchronizedLinks<ulong> _links;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResearchDatabaseCLI"/> class.
        /// </summary>
        /// <param name="links">The links storage.</param>
        public ResearchDatabaseCLI(SynchronizedLinks<ulong> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _database = new ResearchDatabase<ulong>(links);
            _researchSystem = new DynamicResearchSystem<ulong>(links, _database);

            // Register example research queries
            RegisterExampleResearches();

            // Subscribe to execution events
            _researchSystem.ResearchExecuted += OnResearchExecuted;
        }

        /// <summary>
        /// Runs the command-line interface.
        /// </summary>
        public void Run(string[] args)
        {
            Console.WriteLine("=== Global Research Database & Dynamic Research System ===");
            Console.WriteLine("A system for sharing data and running continuous research queries");
            Console.WriteLine();

            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            var command = args[0].ToLowerInvariant();

            switch (command)
            {
                case "list":
                    ListResearches();
                    break;

                case "stats":
                    ShowStatistics();
                    break;

                case "execute":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: execute <research-id>");
                        return;
                    }
                    if (ulong.TryParse(args[1], out var executeId))
                    {
                        ExecuteResearch(executeId);
                    }
                    else
                    {
                        Console.WriteLine($"Invalid research ID: {args[1]}");
                    }
                    break;

                case "start":
                    if (args.Length < 2)
                    {
                        StartAllContinuous();
                    }
                    else if (ulong.TryParse(args[1], out var startId))
                    {
                        StartContinuousResearch(startId);
                    }
                    else
                    {
                        Console.WriteLine($"Invalid research ID: {args[1]}");
                    }
                    break;

                case "stop":
                    if (args.Length < 2)
                    {
                        StopAllContinuous();
                    }
                    else if (ulong.TryParse(args[1], out var stopId))
                    {
                        StopContinuousResearch(stopId);
                    }
                    else
                    {
                        Console.WriteLine($"Invalid research ID: {args[1]}");
                    }
                    break;

                case "status":
                    ShowExecutionStatus();
                    break;

                case "export":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: export <file-path>");
                        return;
                    }
                    ExportData(args[1]);
                    break;

                case "help":
                    ShowHelp();
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}");
                    ShowHelp();
                    break;
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("  list              - List all registered research queries");
            Console.WriteLine("  stats             - Show database statistics");
            Console.WriteLine("  execute <id>      - Execute a research query once");
            Console.WriteLine("  start [id]        - Start continuous execution (all or specific)");
            Console.WriteLine("  stop [id]         - Stop continuous execution (all or specific)");
            Console.WriteLine("  status            - Show execution status of all researches");
            Console.WriteLine("  export <path>     - Export research data to file");
            Console.WriteLine("  help              - Show this help message");
        }

        private void ListResearches()
        {
            Console.WriteLine(_database.ListResearches());
        }

        private void ShowStatistics()
        {
            Console.WriteLine(_database.GetStatistics());
        }

        private void ExecuteResearch(ulong researchId)
        {
            Console.WriteLine($"Executing research {researchId}...");
            var result = _researchSystem.ExecuteResearch(researchId);
            Console.WriteLine("Result:");
            Console.WriteLine(result);
        }

        private void StartAllContinuous()
        {
            Console.WriteLine("Starting all continuous research queries...");
            _researchSystem.Start();
            Console.WriteLine("Continuous research system started. Press Ctrl+C to stop.");

            // Keep running until interrupted
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                StopAllContinuous();
            };

            while (_researchSystem.IsRunning)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        private void StartContinuousResearch(ulong researchId)
        {
            var research = _database.GetResearch(researchId);
            if (research == null)
            {
                Console.WriteLine($"Research {researchId} not found.");
                return;
            }

            if (!research.IsContinuous)
            {
                Console.WriteLine($"Research {researchId} is not configured for continuous execution.");
                return;
            }

            Console.WriteLine($"Starting continuous execution of research {researchId}...");
            _researchSystem.StartContinuousResearch(research);
            Console.WriteLine($"Research {researchId} started. Use 'status' to monitor.");
        }

        private void StopAllContinuous()
        {
            Console.WriteLine("Stopping all continuous research queries...");
            _researchSystem.Stop();
            Console.WriteLine("All continuous research queries stopped.");
        }

        private void StopContinuousResearch(ulong researchId)
        {
            Console.WriteLine($"Stopping continuous execution of research {researchId}...");
            _researchSystem.StopContinuousResearch(researchId);
            Console.WriteLine($"Research {researchId} stopped.");
        }

        private void ShowExecutionStatus()
        {
            Console.WriteLine(_researchSystem.GetExecutionStatus());
        }

        private void ExportData(string path)
        {
            Console.WriteLine($"Exporting research data to {path}...");
            try
            {
                _database.ExportData(path);
                Console.WriteLine("Export completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Export failed: {ex.Message}");
            }
        }

        private void OnResearchExecuted(object sender, ResearchExecutedEventArgs<ulong> e)
        {
            // Optionally log execution events
            // Console.WriteLine($"[{e.ExecutedAt:HH:mm:ss}] Research '{e.ResearchName}' executed.");
        }

        private void RegisterExampleResearches()
        {
            // Example 1: Count all links
            var countLinksResearch = new ResearchQuery<ulong>
            {
                Name = "Count All Links",
                Description = "Counts the total number of links in the database",
                SelectionMethod = links =>
                {
                    var result = new List<IList<ulong>>();
                    links.Each(link =>
                    {
                        result.Add(link);
                        return links.Constants.Continue;
                    });
                    return result;
                },
                TransformationMethod = data => data.Count,
                InterpretationMethod = count => $"Total links in database: {count}",
                IsContinuous = false
            };
            _database.RegisterResearch(countLinksResearch);

            // Example 2: Find links with specific source
            var findBySourceResearch = new ResearchQuery<ulong>
            {
                Name = "Find Links by Source",
                Description = "Finds all links with a specific source (example: source = 1)",
                SelectionMethod = links =>
                {
                    var result = new List<IList<ulong>>();
                    var targetSource = 1UL; // Example source
                    links.Each(link =>
                    {
                        if (links.GetSource(link[links.Constants.IndexPart]) == targetSource)
                        {
                            result.Add(link);
                        }
                        return links.Constants.Continue;
                    });
                    return result;
                },
                TransformationMethod = data => data.Count.ToString(),
                InterpretationMethod = count =>
                {
                    return $"Found {count} links with source=1";
                },
                IsContinuous = false
            };
            _database.RegisterResearch(findBySourceResearch);

            // Example 3: Monitor database growth (continuous)
            var monitorGrowthResearch = new ResearchQuery<ulong>
            {
                Name = "Monitor Database Growth",
                Description = "Continuously monitors the size of the database",
                SelectionMethod = links =>
                {
                    var result = new List<IList<ulong>>();
                    links.Each(link =>
                    {
                        result.Add(link);
                        return links.Constants.Continue;
                    });
                    return result;
                },
                TransformationMethod = data => $"{data.Count}|{DateTime.UtcNow:HH:mm:ss}",
                InterpretationMethod = obj =>
                {
                    var parts = obj.ToString().Split('|');
                    return $"[{parts[1]}] Database size: {parts[0]} links";
                },
                IsContinuous = true
            };
            _database.RegisterResearch(monitorGrowthResearch);
        }
    }
}
