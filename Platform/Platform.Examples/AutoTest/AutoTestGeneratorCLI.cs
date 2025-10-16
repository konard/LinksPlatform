using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Platform.Examples.AutoTest
{
    /// <summary>
    /// Command-line interface for the automatic test generator.
    /// Allows testing assemblies, types, or specific methods from the command line.
    /// </summary>
    public class AutoTestGeneratorCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            Console.WriteLine("=== Auto Test Generator ===");
            Console.WriteLine("Automatically generates and executes tests for functions.");
            Console.WriteLine();

            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }

            var assemblyPath = args[0];
            if (!File.Exists(assemblyPath))
            {
                Console.WriteLine($"Error: Assembly not found: {assemblyPath}");
                return;
            }

            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var analyzer = new AssemblyAnalyzer();
                var generator = new AutoTestGenerator(
                    lazyMode: false,
                    randomTestsPerFunction: 5,
                    timeoutMs: 5000
                );

                List<ITestableFunction> functions;

                // Check if a specific type is requested
                if (args.Length > 1)
                {
                    var typeName = args[1];
                    var type = assembly.GetType(typeName);
                    if (type == null)
                    {
                        Console.WriteLine($"Error: Type not found: {typeName}");
                        return;
                    }
                    Console.WriteLine($"Discovering functions in type: {typeName}");
                    functions = analyzer.DiscoverTestableFunctions(type);
                }
                else
                {
                    Console.WriteLine($"Discovering functions in assembly: {assemblyPath}");
                    functions = analyzer.DiscoverTestableFunctions(assembly);
                }

                Console.WriteLine($"Found {functions.Count} testable functions.");
                Console.WriteLine();

                var allResults = new List<TestResult>();

                foreach (var function in functions)
                {
                    Console.WriteLine($"Testing: {function.FunctionName}");
                    var results = generator.GenerateAndExecuteTests(function);
                    allResults.AddRange(results);

                    var passed = results.Count(r => r.Success);
                    var failed = results.Count(r => !r.Success);

                    if (failed > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"  {passed} passed, {failed} failed");

                        // Show first few failures
                        var failures = results.Where(r => !r.Success).Take(3);
                        foreach (var failure in failures)
                        {
                            Console.WriteLine($"    - {failure.TestType}: {failure.Exception?.Message}");
                        }
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  All {passed} tests passed");
                        Console.ResetColor();
                    }
                }

                Console.WriteLine();
                Console.WriteLine("=== Summary ===");
                var stats = generator.GenerateStatistics(allResults);
                Console.WriteLine($"Total tests: {stats.TotalTests}");
                Console.WriteLine($"Passed: {stats.PassedTests}");
                Console.WriteLine($"Failed: {stats.FailedTests}");
                Console.WriteLine($"Total execution time: {stats.TotalExecutionTimeMs}ms");
                Console.WriteLine();

                if (stats.FailedTests > 0)
                {
                    Console.WriteLine("Functions with failures:");
                    foreach (var kvp in stats.FailuresByFunction.Where(kvp => kvp.Value > 0))
                    {
                        Console.WriteLine($"  {kvp.Key}: {kvp.Value} failures");
                    }
                }

                // Export results to file
                var outputPath = Path.Combine(Path.GetDirectoryName(assemblyPath), "test-results.txt");
                ExportResults(outputPath, allResults, stats);
                Console.WriteLine();
                Console.WriteLine($"Results exported to: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
            }
        }

        private void ShowUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  AutoTestGenerator <assembly-path> [type-name]");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  AutoTestGenerator MyLibrary.dll");
            Console.WriteLine("  AutoTestGenerator MyLibrary.dll MyNamespace.MyClass");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  assembly-path : Path to the assembly to test");
            Console.WriteLine("  type-name     : (Optional) Fully qualified name of a specific type to test");
        }

        private void ExportResults(string path, List<TestResult> results, TestStatistics stats)
        {
            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("Auto Test Generation Results");
                writer.WriteLine("============================");
                writer.WriteLine();
                writer.WriteLine($"Total Tests: {stats.TotalTests}");
                writer.WriteLine($"Passed: {stats.PassedTests}");
                writer.WriteLine($"Failed: {stats.FailedTests}");
                writer.WriteLine($"Total Execution Time: {stats.TotalExecutionTimeMs}ms");
                writer.WriteLine();

                writer.WriteLine("Test Results:");
                writer.WriteLine("-------------");

                foreach (var result in results)
                {
                    writer.WriteLine($"Function: {result.FunctionName}");
                    writer.WriteLine($"  Test Type: {result.TestType}");
                    writer.WriteLine($"  Status: {(result.Success ? "PASSED" : "FAILED")}");
                    writer.WriteLine($"  Execution Time: {result.ExecutionTimeMs}ms");

                    if (result.InputParameters != null && result.InputParameters.Length > 0)
                    {
                        writer.WriteLine($"  Input Parameters: {string.Join(", ", result.InputParameters.Select(p => p?.ToString() ?? "null"))}");
                    }

                    if (result.Success && result.Output != null)
                    {
                        writer.WriteLine($"  Output: {result.Output}");
                    }

                    if (!result.Success && result.Exception != null)
                    {
                        writer.WriteLine($"  Exception: {result.Exception.GetType().Name}");
                        writer.WriteLine($"  Message: {result.Exception.Message}");
                    }

                    writer.WriteLine();
                }
            }
        }
    }
}
