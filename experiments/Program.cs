using System;
using System.IO;
using Platform.Examples;

namespace Platform.Experiments
{
    /// <summary>
    /// Experiment to test the ProjectExecutor functionality.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Testing ProjectExecutor ===\n");

            // Test 1: Detect the current repository
            var currentPath = Directory.GetCurrentDirectory();
            var parentPath = Directory.GetParent(currentPath)?.FullName ?? currentPath;

            Console.WriteLine("Test 1: Detecting project in parent directory");
            Console.WriteLine($"Path: {parentPath}");
            var executor1 = new ProjectExecutor(parentPath);
            var plan1 = executor1.DetectAndGeneratePlan();
            DisplayPlan(plan1);

            // Test 2: Detect Platform.Examples project
            var examplesPath = Path.Combine(parentPath, "Platform", "Platform.Examples");
            if (Directory.Exists(examplesPath))
            {
                Console.WriteLine("\nTest 2: Detecting Platform.Examples project");
                Console.WriteLine($"Path: {examplesPath}");
                var executor2 = new ProjectExecutor(examplesPath);
                var plan2 = executor2.DetectAndGeneratePlan();
                DisplayPlan(plan2);

                // Test 3: Generate shell script
                Console.WriteLine("\nTest 3: Generating shell script");
                var script = executor2.GenerateShellScript(plan2);
                Console.WriteLine(script);

                // Save the script
                var scriptPath = Path.Combine("/tmp", "run-examples.sh");
                File.WriteAllText(scriptPath, script);
                Console.WriteLine($"Script saved to: {scriptPath}");
            }

            // Test 4: Create a mock README and test parsing
            Console.WriteLine("\nTest 4: Testing README parsing with mock data");
            var testPath = Path.Combine("/tmp", "test-project");
            Directory.CreateDirectory(testPath);

            var mockReadme = @"# Test Project

## Prerequisites
* .NET Core 3.1 or later
* Git

## Setup

First, clone the repository:

```bash
git clone https://github.com/example/test-project.git
cd test-project
```

Then restore dependencies:

```bash
dotnet restore
```

## Building

Build the project:

```bash
dotnet build
```

## Running

Run the application:

```bash
dotnet run
```
";
            File.WriteAllText(Path.Combine(testPath, "README.md"), mockReadme);

            // Create a minimal .csproj file
            var mockCsproj = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netcoreapp3.1</TargetFramework>
  </PropertyGroup>
</Project>";
            File.WriteAllText(Path.Combine(testPath, "TestProject.csproj"), mockCsproj);

            var executor3 = new ProjectExecutor(testPath);
            var plan3 = executor3.DetectAndGeneratePlan();
            DisplayPlan(plan3);

            Console.WriteLine("\n=== All tests completed ===");
        }

        private static void DisplayPlan(ExecutionPlan plan)
        {
            Console.WriteLine($"  Project Type: {plan.ProjectType}");
            Console.WriteLine($"  Steps Count: {plan.Steps.Count}");

            if (plan.ReadmeInstructions != null)
            {
                if (!string.IsNullOrEmpty(plan.ReadmeInstructions.Prerequisites))
                {
                    Console.WriteLine("\n  Prerequisites:");
                    Console.WriteLine($"  {plan.ReadmeInstructions.Prerequisites}");
                }

                if (plan.ReadmeInstructions.Steps.Count > 0)
                {
                    Console.WriteLine("\n  README Steps:");
                    foreach (var step in plan.ReadmeInstructions.Steps)
                    {
                        Console.WriteLine($"    - {step.Description}");
                        Console.WriteLine($"      Command: {step.Command}");
                    }
                }
            }

            if (plan.Steps.Count > 0)
            {
                Console.WriteLine("\n  Execution Steps:");
                for (int i = 0; i < plan.Steps.Count; i++)
                {
                    var step = plan.Steps[i];
                    Console.WriteLine($"    {i + 1}. {step.Description}");
                    Console.WriteLine($"       Command: {step.Command}");
                }
            }
        }
    }
}
