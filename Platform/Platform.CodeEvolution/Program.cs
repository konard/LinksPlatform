using Platform.CodeEvolution.Services;
using System.CommandLine;

namespace Platform.CodeEvolution;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Code Evolution Analyzer - Track and recommend code changes based on Git history");

        var analyzeCommand = new Command("analyze", "Analyze Git repository for code evolution patterns");
        var repositoryOption = new Option<string>("--repository", "Path to Git repository") { IsRequired = true };
        var branchOption = new Option<string?>("--branch", "Branch to analyze (default: current branch)");
        var maxCommitsOption = new Option<int>("--max-commits", () => 1000, "Maximum commits to analyze");
        var outputOption = new Option<string>("--output", () => "output", "Output directory for results");

        analyzeCommand.AddOption(repositoryOption);
        analyzeCommand.AddOption(branchOption);
        analyzeCommand.AddOption(maxCommitsOption);
        analyzeCommand.AddOption(outputOption);

        analyzeCommand.SetHandler(async (repository, branch, maxCommits, output) =>
        {
            await AnalyzeRepository(repository, branch, maxCommits, output);
        }, repositoryOption, branchOption, maxCommitsOption, outputOption);

        var recommendCommand = new Command("recommend", "Get code recommendations for a specific file or code snippet");
        var codeOption = new Option<string>("--code", "Code snippet to analyze") { IsRequired = true };
        var languageOption = new Option<string>("--language", "Programming language") { IsRequired = true };
        var dependenciesOption = new Option<string[]>("--dependencies", "Dependencies used in the code");

        recommendCommand.AddOption(codeOption);
        recommendCommand.AddOption(languageOption);
        recommendCommand.AddOption(dependenciesOption);

        recommendCommand.SetHandler(async (code, language, dependencies) =>
        {
            await GetRecommendations(code, language, dependencies?.ToList());
        }, codeOption, languageOption, dependenciesOption);

        rootCommand.AddCommand(analyzeCommand);
        rootCommand.AddCommand(recommendCommand);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task AnalyzeRepository(string repositoryPath, string? branch, int maxCommits, string outputDir)
    {
        try
        {
            Console.WriteLine($"Analyzing repository: {repositoryPath}");
            Console.WriteLine($"Branch: {branch ?? "current"}");
            Console.WriteLine($"Max commits: {maxCommits}");
            Console.WriteLine();

            Directory.CreateDirectory(outputDir);

            using var analyzer = new GitHistoryAnalyzer(repositoryPath);
            var changes = await analyzer.AnalyzeHistoryAsync(branch, maxCommits);

            Console.WriteLine($"Found {changes.Count} code changes");

            var frequencyTracker = new ChangeFrequencyTracker();
            frequencyTracker.ProcessChanges(changes);

            var topChanges = frequencyTracker.GetTopChanges(100);
            var topChains = frequencyTracker.GetTopChains(50);
            var statistics = frequencyTracker.GetStatistics();

            Console.WriteLine($"Identified {topChanges.Count} change patterns");
            Console.WriteLine($"Identified {topChains.Count} change chains");

            var exporter = new DataExporter();

            await exporter.ExportToJsonAsync(Path.Combine(outputDir, "changes.json"), changes);
            await exporter.ExportToJsonAsync(Path.Combine(outputDir, "frequencies.json"), topChanges);
            await exporter.ExportToJsonAsync(Path.Combine(outputDir, "chains.json"), topChains);
            await exporter.ExportToJsonAsync(Path.Combine(outputDir, "statistics.json"), statistics);

            await exporter.ExportChangeFrequenciesToCsvAsync(Path.Combine(outputDir, "frequencies.csv"), topChanges);

            var recommendationEngine = new RecommendationEngine(frequencyTracker);
            var bestPractices = recommendationEngine.GetBestPractices(null, null);

            await exporter.ExportRecommendationsToHtmlAsync(Path.Combine(outputDir, "recommendations.html"), bestPractices);

            var report = await exporter.GenerateReportAsync(statistics, topChanges, bestPractices);
            await File.WriteAllTextAsync(Path.Combine(outputDir, "report.md"), report);

            Console.WriteLine($"\nAnalysis complete! Results saved to: {outputDir}");
            Console.WriteLine("Files generated:");
            Console.WriteLine("- changes.json: All detected changes");
            Console.WriteLine("- frequencies.json: Change frequency analysis");
            Console.WriteLine("- chains.json: Change evolution chains");
            Console.WriteLine("- statistics.json: Overall statistics");
            Console.WriteLine("- frequencies.csv: Frequencies in CSV format");
            Console.WriteLine("- recommendations.html: Visual recommendations report");
            Console.WriteLine("- report.md: Summary report");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static async Task GetRecommendations(string code, string language, List<string>? dependencies)
    {
        try
        {
            Console.WriteLine($"Getting recommendations for {language} code...");
            Console.WriteLine();

            // For demonstration, we'll create a simple frequency tracker with sample data
            var frequencyTracker = new ChangeFrequencyTracker();
            var recommendationEngine = new RecommendationEngine(frequencyTracker);

            var recommendations = recommendationEngine.GetRecommendations(code, language, dependencies);

            if (recommendations.Count == 0)
            {
                Console.WriteLine("No specific recommendations found for the provided code.");
                Console.WriteLine("Try analyzing a repository first to build a recommendation database.");
                return;
            }

            Console.WriteLine($"Found {recommendations.Count} recommendations:");
            Console.WriteLine();

            foreach (var rec in recommendations)
            {
                Console.WriteLine($"**{rec.Title}**");
                Console.WriteLine($"Type: {rec.Type}");
                Console.WriteLine($"Confidence: {rec.Confidence:P}");
                Console.WriteLine($"Description: {rec.Description}");

                if (!string.IsNullOrEmpty(rec.BeforeCode))
                {
                    Console.WriteLine("Before:");
                    Console.WriteLine(rec.BeforeCode);
                }

                if (!string.IsNullOrEmpty(rec.AfterCode))
                {
                    Console.WriteLine("After:");
                    Console.WriteLine(rec.AfterCode);
                }

                Console.WriteLine(new string('-', 50));
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}