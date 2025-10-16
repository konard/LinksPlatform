using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Platform.Examples
{
    /// <summary>
    /// Automatically detects project type and generates execution scripts.
    /// </summary>
    public class ProjectExecutor
    {
        private readonly string _projectPath;
        private readonly Dictionary<string, Func<string, ExecutionPlan>> _detectors;

        public ProjectExecutor(string projectPath)
        {
            _projectPath = projectPath ?? throw new ArgumentNullException(nameof(projectPath));
            _detectors = new Dictionary<string, Func<string, ExecutionPlan>>
            {
                { "*.csproj", DetectDotNetProject },
                { "Cargo.toml", DetectRustProject },
                { "package.json", DetectNodeProject },
                { "setup.py", DetectPythonProject },
                { "Makefile", DetectMakefileProject }
            };
        }

        /// <summary>
        /// Detects project type and generates execution plan.
        /// </summary>
        public ExecutionPlan DetectAndGeneratePlan()
        {
            foreach (var detector in _detectors)
            {
                var pattern = detector.Key;
                var files = pattern.Contains('*')
                    ? Directory.GetFiles(_projectPath, pattern, SearchOption.AllDirectories)
                    : new[] { Path.Combine(_projectPath, pattern) }.Where(File.Exists).ToArray();

                if (files.Length > 0)
                {
                    var plan = detector.Value(_projectPath);
                    if (plan != null)
                    {
                        return plan;
                    }
                }
            }

            return DetectGenericProject(_projectPath);
        }

        private ExecutionPlan DetectDotNetProject(string path)
        {
            var csprojFiles = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);
            if (csprojFiles.Length == 0)
                return null;

            var readmeInstructions = ExtractReadmeInstructions(path);
            var steps = new List<ExecutionStep>
            {
                new ExecutionStep("Restore dependencies", "dotnet restore", path),
                new ExecutionStep("Build project", "dotnet build", path),
                new ExecutionStep("Run project", "dotnet run", path)
            };

            return new ExecutionPlan("DotNet", steps, readmeInstructions);
        }

        private ExecutionPlan DetectRustProject(string path)
        {
            var cargoToml = Path.Combine(path, "Cargo.toml");
            if (!File.Exists(cargoToml))
                return null;

            var readmeInstructions = ExtractReadmeInstructions(path);
            var steps = new List<ExecutionStep>
            {
                new ExecutionStep("Build project", "cargo build", path),
                new ExecutionStep("Run project", "cargo run", path),
                new ExecutionStep("Run tests", "cargo test", path)
            };

            return new ExecutionPlan("Rust", steps, readmeInstructions);
        }

        private ExecutionPlan DetectNodeProject(string path)
        {
            var packageJson = Path.Combine(path, "package.json");
            if (!File.Exists(packageJson))
                return null;

            var readmeInstructions = ExtractReadmeInstructions(path);
            var steps = new List<ExecutionStep>
            {
                new ExecutionStep("Install dependencies", "npm install", path),
                new ExecutionStep("Run project", "npm start", path),
                new ExecutionStep("Run tests", "npm test", path)
            };

            return new ExecutionPlan("Node.js", steps, readmeInstructions);
        }

        private ExecutionPlan DetectPythonProject(string path)
        {
            var setupPy = Path.Combine(path, "setup.py");
            if (!File.Exists(setupPy))
                return null;

            var readmeInstructions = ExtractReadmeInstructions(path);
            var steps = new List<ExecutionStep>
            {
                new ExecutionStep("Install dependencies", "pip install -r requirements.txt", path),
                new ExecutionStep("Run setup", "python setup.py install", path)
            };

            return new ExecutionPlan("Python", steps, readmeInstructions);
        }

        private ExecutionPlan DetectMakefileProject(string path)
        {
            var makefile = Path.Combine(path, "Makefile");
            if (!File.Exists(makefile))
                return null;

            var readmeInstructions = ExtractReadmeInstructions(path);
            var steps = new List<ExecutionStep>
            {
                new ExecutionStep("Build project", "make", path),
                new ExecutionStep("Run project", "make run", path)
            };

            return new ExecutionPlan("Makefile", steps, readmeInstructions);
        }

        private ExecutionPlan DetectGenericProject(string path)
        {
            var readmeInstructions = ExtractReadmeInstructions(path);
            var steps = new List<ExecutionStep>();

            // Try to extract steps from README
            if (readmeInstructions != null && readmeInstructions.Steps.Any())
            {
                steps.AddRange(readmeInstructions.Steps.Select(s =>
                    new ExecutionStep(s.Description, s.Command, path)));
            }

            return new ExecutionPlan("Generic", steps, readmeInstructions);
        }

        private ReadmeInstructions ExtractReadmeInstructions(string path)
        {
            var readmeFiles = new[] { "README.md", "README.txt", "README", "readme.md" };
            string readmeContent = null;

            foreach (var readmeFile in readmeFiles)
            {
                var readmePath = Path.Combine(path, readmeFile);
                if (File.Exists(readmePath))
                {
                    readmeContent = File.ReadAllText(readmePath);
                    break;
                }
            }

            if (readmeContent == null)
                return null;

            return ParseReadmeInstructions(readmeContent);
        }

        private ReadmeInstructions ParseReadmeInstructions(string content)
        {
            var instructions = new ReadmeInstructions();

            // Extract prerequisites
            var prerequisitesMatch = Regex.Match(content, @"##?\s*Prerequisites?\s*\n(.*?)(?=\n##|\z)",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (prerequisitesMatch.Success)
            {
                instructions.Prerequisites = prerequisitesMatch.Groups[1].Value.Trim();
            }

            // Extract code blocks that look like commands
            var codeBlockMatches = Regex.Matches(content, @"```(?:bash|shell|sh)?\s*\n(.*?)\n```",
                RegexOptions.Singleline);
            foreach (Match match in codeBlockMatches)
            {
                var commands = match.Groups[1].Value.Trim().Split('\n');
                foreach (var cmd in commands)
                {
                    var trimmedCmd = cmd.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedCmd) && !trimmedCmd.StartsWith("#"))
                    {
                        instructions.Steps.Add(new InstructionStep
                        {
                            Description = $"Execute: {trimmedCmd}",
                            Command = trimmedCmd
                        });
                    }
                }
            }

            // Extract setup/installation sections
            var setupMatch = Regex.Match(content, @"##?\s*(?:Setup|Installation|Getting Started)\s*\n(.*?)(?=\n##|\z)",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (setupMatch.Success)
            {
                instructions.SetupInstructions = setupMatch.Groups[1].Value.Trim();
            }

            return instructions;
        }

        /// <summary>
        /// Generates a shell script that can execute the plan.
        /// </summary>
        public string GenerateShellScript(ExecutionPlan plan)
        {
            var script = new System.Text.StringBuilder();
            script.AppendLine("#!/bin/bash");
            script.AppendLine("# Auto-generated execution script");
            script.AppendLine($"# Project type: {plan.ProjectType}");
            script.AppendLine();
            script.AppendLine("set -e  # Exit on error");
            script.AppendLine();

            if (plan.ReadmeInstructions?.Prerequisites != null)
            {
                script.AppendLine("# Prerequisites:");
                foreach (var line in plan.ReadmeInstructions.Prerequisites.Split('\n'))
                {
                    script.AppendLine($"# {line}");
                }
                script.AppendLine();
            }

            foreach (var step in plan.Steps)
            {
                script.AppendLine($"echo \">>> {step.Description}\"");
                script.AppendLine($"cd \"{step.WorkingDirectory}\"");
                script.AppendLine(step.Command);
                script.AppendLine();
            }

            return script.ToString();
        }

        /// <summary>
        /// Generates a batch script for Windows.
        /// </summary>
        public string GenerateBatchScript(ExecutionPlan plan)
        {
            var script = new System.Text.StringBuilder();
            script.AppendLine("@echo off");
            script.AppendLine("REM Auto-generated execution script");
            script.AppendLine($"REM Project type: {plan.ProjectType}");
            script.AppendLine();

            foreach (var step in plan.Steps)
            {
                script.AppendLine($"echo >>> {step.Description}");
                script.AppendLine($"cd /d \"{step.WorkingDirectory}\"");
                script.AppendLine(step.Command);
                script.AppendLine("if %errorlevel% neq 0 exit /b %errorlevel%");
                script.AppendLine();
            }

            return script.ToString();
        }
    }

    public class ExecutionPlan
    {
        public string ProjectType { get; }
        public List<ExecutionStep> Steps { get; }
        public ReadmeInstructions ReadmeInstructions { get; }

        public ExecutionPlan(string projectType, List<ExecutionStep> steps, ReadmeInstructions readmeInstructions)
        {
            ProjectType = projectType;
            Steps = steps ?? new List<ExecutionStep>();
            ReadmeInstructions = readmeInstructions;
        }
    }

    public class ExecutionStep
    {
        public string Description { get; }
        public string Command { get; }
        public string WorkingDirectory { get; }

        public ExecutionStep(string description, string command, string workingDirectory)
        {
            Description = description;
            Command = command;
            WorkingDirectory = workingDirectory;
        }
    }

    public class ReadmeInstructions
    {
        public string Prerequisites { get; set; }
        public string SetupInstructions { get; set; }
        public List<InstructionStep> Steps { get; } = new List<InstructionStep>();
    }

    public class InstructionStep
    {
        public string Description { get; set; }
        public string Command { get; set; }
    }
}
