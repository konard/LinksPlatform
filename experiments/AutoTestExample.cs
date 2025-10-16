using System;
using System.Linq;
using Platform.Examples.AutoTest;

namespace Platform.Experiments
{
    /// <summary>
    /// Example demonstrating the automatic test generation system.
    /// This shows how to use the AutoTestGenerator to test functions automatically.
    /// </summary>
    public class AutoTestExample
    {
        // Sample functions to demonstrate automatic testing
        public static int Add(int a, int b)
        {
            return a + b;
        }

        public static int Divide(int a, int b)
        {
            if (b == 0)
                throw new DivideByZeroException("Cannot divide by zero");
            return a / b;
        }

        public static bool IsPrime(int n)
        {
            if (n <= 1) return false;
            if (n == 2) return true;
            if (n % 2 == 0) return false;

            for (int i = 3; i * i <= n; i += 2)
            {
                if (n % i == 0) return false;
            }
            return true;
        }

        public static string Concatenate(string a, string b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException("Strings cannot be null");
            return a + b;
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("=== Auto Test Generation Example ===");
            Console.WriteLine();

            // Initialize the test generator
            var generator = new AutoTestGenerator(
                lazyMode: false,
                randomTestsPerFunction: 5,
                timeoutMs: 5000
            );

            var analyzer = new AssemblyAnalyzer();

            // Discover testable functions in this class
            var functions = analyzer.DiscoverTestableFunctions(typeof(AutoTestExample));

            Console.WriteLine($"Discovered {functions.Count} testable functions:");
            foreach (var func in functions)
            {
                Console.WriteLine($"  - {func.FunctionName}");
            }
            Console.WriteLine();

            // Run tests for each function
            var allResults = new System.Collections.Generic.List<TestResult>();

            foreach (var function in functions)
            {
                Console.WriteLine($"Testing: {function.FunctionName}");
                Console.WriteLine(new string('-', 50));

                var results = generator.GenerateAndExecuteTests(function);
                allResults.AddRange(results);

                // Show some results
                var passed = results.FindAll(r => r.Success).Count;
                var failed = results.FindAll(r => !r.Success).Count;

                Console.WriteLine($"Results: {passed} passed, {failed} failed");

                // Show interesting test cases
                var interestingResults = results
                    .Where(r => r.TestType == TestType.BoundaryMin ||
                               r.TestType == TestType.BoundaryMax ||
                               r.TestType == TestType.EdgeCase ||
                               !r.Success)
                    .Take(5);

                foreach (var result in interestingResults)
                {
                    var status = result.Success ? "✓" : "✗";
                    var color = result.Success ? ConsoleColor.Green : ConsoleColor.Red;

                    Console.ForegroundColor = color;
                    Console.Write($"  {status} {result.TestType}: ");

                    if (result.InputParameters != null && result.InputParameters.Length > 0)
                    {
                        Console.Write($"Input=[{string.Join(", ", result.InputParameters.Select(p => p?.ToString() ?? "null"))}] ");
                    }

                    if (result.Success)
                    {
                        Console.Write($"Output={result.Output}");
                    }
                    else
                    {
                        Console.Write($"Exception={result.Exception?.GetType().Name}: {result.Exception?.Message}");
                    }

                    Console.WriteLine($" ({result.ExecutionTimeMs}ms)");
                    Console.ResetColor();
                }

                Console.WriteLine();
            }

            // Generate overall statistics
            var stats = generator.GenerateStatistics(allResults);
            Console.WriteLine("=== Overall Statistics ===");
            Console.WriteLine($"Total Tests: {stats.TotalTests}");
            Console.WriteLine($"Passed: {stats.PassedTests}");
            Console.WriteLine($"Failed: {stats.FailedTests}");
            Console.WriteLine($"Success Rate: {(stats.PassedTests * 100.0 / stats.TotalTests):F2}%");
            Console.WriteLine($"Total Execution Time: {stats.TotalExecutionTimeMs}ms");
            Console.WriteLine();

            Console.WriteLine("Tests by function:");
            foreach (var kvp in stats.TestsByFunction)
            {
                var failures = stats.FailuresByFunction[kvp.Key];
                Console.WriteLine($"  {kvp.Key}: {kvp.Value} tests ({failures} failures)");
            }

            Console.WriteLine();
            Console.WriteLine("Example completed. This demonstrates automatic test generation for:");
            Console.WriteLine("  - Boundary value testing (min/max values)");
            Console.WriteLine("  - Random input testing");
            Console.WriteLine("  - Null/invalid input testing");
            Console.WriteLine("  - Edge case detection");
            Console.WriteLine("  - Automatic exception handling");
            Console.WriteLine("  - Performance metrics collection");
        }
    }
}
