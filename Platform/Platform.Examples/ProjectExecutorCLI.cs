using System;
using System.IO;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the ProjectExecutor.
    /// </summary>
    public class ProjectExecutorCLI : ICommandLineInterface
    {
        public void Run(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            var command = args[0].ToLowerInvariant();

            switch (command)
            {
                case "detect":
                    HandleDetect(args);
                    break;
                case "generate":
                    HandleGenerate(args);
                    break;
                case "execute":
                    HandleExecute(args);
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

        private void HandleDetect(string[] args)
        {
            var path = args.Length > 1 ? args[1] : Directory.GetCurrentDirectory();

            if (!Directory.Exists(path))
            {
                Console.WriteLine($"Error: Directory not found: {path}");
                return;
            }

            Console.WriteLine($"Detecting project type in: {path}");
            var executor = new ProjectExecutor(path);
            var plan = executor.DetectAndGeneratePlan();

            Console.WriteLine($"\nProject Type: {plan.ProjectType}");
            Console.WriteLine($"Steps: {plan.Steps.Count}");

            if (plan.ReadmeInstructions != null)
            {
                if (!string.IsNullOrEmpty(plan.ReadmeInstructions.Prerequisites))
                {
                    Console.WriteLine("\nPrerequisites:");
                    Console.WriteLine(plan.ReadmeInstructions.Prerequisites);
                }

                if (!string.IsNullOrEmpty(plan.ReadmeInstructions.SetupInstructions))
                {
                    Console.WriteLine("\nSetup Instructions:");
                    Console.WriteLine(plan.ReadmeInstructions.SetupInstructions);
                }
            }

            Console.WriteLine("\nExecution Steps:");
            for (int i = 0; i < plan.Steps.Count; i++)
            {
                var step = plan.Steps[i];
                Console.WriteLine($"{i + 1}. {step.Description}");
                Console.WriteLine($"   Command: {step.Command}");
                Console.WriteLine($"   Directory: {step.WorkingDirectory}");
            }
        }

        private void HandleGenerate(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Missing required argument [output-path]");
                Console.WriteLine("Usage: generate [project-path] [output-path] [format]");
                return;
            }

            var projectPath = args.Length > 2 ? args[1] : Directory.GetCurrentDirectory();
            var outputPath = args.Length > 2 ? args[2] : args[1];
            var format = args.Length > 3 ? args[3] : "sh";

            if (!Directory.Exists(projectPath))
            {
                Console.WriteLine($"Error: Directory not found: {projectPath}");
                return;
            }

            Console.WriteLine($"Generating execution script for: {projectPath}");
            var executor = new ProjectExecutor(projectPath);
            var plan = executor.DetectAndGeneratePlan();

            string script;
            switch (format.ToLowerInvariant())
            {
                case "sh":
                case "bash":
                    script = executor.GenerateShellScript(plan);
                    break;
                case "bat":
                case "cmd":
                    script = executor.GenerateBatchScript(plan);
                    break;
                default:
                    Console.WriteLine($"Error: Unknown format: {format}");
                    return;
            }

            File.WriteAllText(outputPath, script);
            Console.WriteLine($"Script generated: {outputPath}");

            // Make the script executable on Unix-like systems
            if (format.ToLowerInvariant() == "sh" || format.ToLowerInvariant() == "bash")
            {
                try
                {
                    if (Environment.OSVersion.Platform == PlatformID.Unix ||
                        Environment.OSVersion.Platform == PlatformID.MacOSX)
                    {
                        var process = System.Diagnostics.Process.Start("chmod", $"+x {outputPath}");
                        process?.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not make script executable: {ex.Message}");
                }
            }
        }

        private void HandleExecute(string[] args)
        {
            var path = args.Length > 1 ? args[1] : Directory.GetCurrentDirectory();

            if (!Directory.Exists(path))
            {
                Console.WriteLine($"Error: Directory not found: {path}");
                return;
            }

            Console.WriteLine($"Executing project in: {path}");
            var executor = new ProjectExecutor(path);
            var plan = executor.DetectAndGeneratePlan();

            Console.WriteLine($"Project Type: {plan.ProjectType}");
            Console.WriteLine($"Executing {plan.Steps.Count} steps...\n");

            foreach (var step in plan.Steps)
            {
                Console.WriteLine($">>> {step.Description}");
                Console.WriteLine($"    Command: {step.Command}");
                Console.WriteLine($"    Directory: {step.WorkingDirectory}");

                try
                {
                    var processInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{step.Command}\"",
                        WorkingDirectory = step.WorkingDirectory,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        processInfo.FileName = "cmd.exe";
                        processInfo.Arguments = $"/c {step.Command}";
                    }

                    using (var process = System.Diagnostics.Process.Start(processInfo))
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        var error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (!string.IsNullOrEmpty(output))
                        {
                            Console.WriteLine(output);
                        }

                        if (!string.IsNullOrEmpty(error))
                        {
                            Console.Error.WriteLine(error);
                        }

                        if (process.ExitCode != 0)
                        {
                            Console.WriteLine($"Error: Command failed with exit code {process.ExitCode}");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing command: {ex.Message}");
                    return;
                }

                Console.WriteLine();
            }

            Console.WriteLine("Execution completed successfully!");
        }

        private void ShowHelp()
        {
            Console.WriteLine("ProjectExecutor - Automatically detect and execute project setups");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  projectexecutor detect [path]              - Detect project type and show execution plan");
            Console.WriteLine("  projectexecutor generate [path] output [format] - Generate execution script");
            Console.WriteLine("  projectexecutor execute [path]             - Execute the project setup and run");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  path     - Path to the project directory (default: current directory)");
            Console.WriteLine("  output   - Path to the output script file");
            Console.WriteLine("  format   - Script format: sh (default), bash, bat, cmd");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  projectexecutor detect /path/to/project");
            Console.WriteLine("  projectexecutor generate /path/to/project run.sh");
            Console.WriteLine("  projectexecutor generate /path/to/project run.bat bat");
            Console.WriteLine("  projectexecutor execute /path/to/project");
        }
    }
}
