using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace Platform.BinarySearchBugFinder
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                Description = "Binary Search Bug Finder - Automated tool to locate bugs by iteratively commenting out code sections"
            };

            var sourceFileOption = new Option<FileInfo>(
                aliases: new[] { "--source", "-s" },
                description: "Path to the source file to analyze")
            {
                IsRequired = true
            };

            var testCommandOption = new Option<string>(
                aliases: new[] { "--test-command", "-t" },
                description: "Command to run tests (e.g., 'dotnet test', 'npm test')")
            {
                IsRequired = true
            };

            var workingDirectoryOption = new Option<DirectoryInfo>(
                aliases: new[] { "--working-dir", "-w" },
                description: "Working directory for test execution",
                getDefaultValue: () => new DirectoryInfo(Directory.GetCurrentDirectory()));

            var timeoutOption = new Option<int>(
                aliases: new[] { "--timeout" },
                description: "Timeout for test execution in seconds",
                getDefaultValue: () => 300);

            var modeOption = new Option<ExecutionMode>(
                aliases: new[] { "--mode", "-m" },
                description: "Execution mode: TestCommand or Compilation",
                getDefaultValue: () => ExecutionMode.TestCommand);

            rootCommand.AddOption(sourceFileOption);
            rootCommand.AddOption(testCommandOption);
            rootCommand.AddOption(workingDirectoryOption);
            rootCommand.AddOption(timeoutOption);
            rootCommand.AddOption(modeOption);

            rootCommand.Handler = CommandHandler.Create<FileInfo, string, DirectoryInfo, int, ExecutionMode>(
                (source, testCommand, workingDir, timeout, mode) =>
                {
                    try
                    {
                        Console.WriteLine("╔════════════════════════════════════════════════════╗");
                        Console.WriteLine("║   Binary Search Bug Finder                         ║");
                        Console.WriteLine("║   Automated Bug Localization Tool                  ║");
                        Console.WriteLine("╚════════════════════════════════════════════════════╝");
                        Console.WriteLine();

                        if (!source.Exists)
                        {
                            Console.Error.WriteLine($"Error: Source file not found: {source.FullName}");
                            return 1;
                        }

                        ITestExecutor testExecutor;

                        if (mode == ExecutionMode.Compilation)
                        {
                            Console.WriteLine($"Mode: Compilation test");
                            testExecutor = new CompilationTestExecutor(source.FullName, timeout);
                        }
                        else
                        {
                            Console.WriteLine($"Mode: Test command execution");
                            Console.WriteLine($"Test command: {testCommand}");
                            testExecutor = new CommandLineTestExecutor(testCommand, workingDir.FullName, timeout);
                        }

                        Console.WriteLine($"Source file: {source.FullName}");
                        Console.WriteLine($"Working directory: {workingDir.FullName}");
                        Console.WriteLine($"Timeout: {timeout}s");
                        Console.WriteLine();

                        var bugFinder = new BugFinder(source.FullName, testExecutor);
                        var result = bugFinder.FindBug();

                        Console.WriteLine();
                        Console.WriteLine("════════════════════════════════════════════════════");
                        Console.WriteLine("RESULT");
                        Console.WriteLine("════════════════════════════════════════════════════");

                        if (result.Success)
                        {
                            Console.WriteLine("✓ Bug successfully located!");
                            Console.WriteLine($"Message: {result.Message}");
                            Console.WriteLine($"Problematic statements: {result.ProblematicStatements.Count}");
                            Console.WriteLine();
                            Console.WriteLine("Problematic code sections:");

                            for (int i = 0; i < result.ProblematicStatements.Count; i++)
                            {
                                Console.WriteLine($"\n[Statement #{result.StatementIndices[i]}]");
                                Console.WriteLine("----------------------------------------");
                                Console.WriteLine(result.ProblematicStatements[i].ToFullString().Trim());
                            }

                            return 0;
                        }
                        else
                        {
                            Console.WriteLine("✗ Unable to locate bug");
                            Console.WriteLine($"Message: {result.Message}");
                            return 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error: {ex.Message}");
                        Console.Error.WriteLine(ex.StackTrace);
                        return 1;
                    }
                });

            return await rootCommand.InvokeAsync(args);
        }
    }

    public enum ExecutionMode
    {
        TestCommand,
        Compilation
    }
}
