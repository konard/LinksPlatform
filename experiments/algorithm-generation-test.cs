/// <summary>
/// Test/experiment script for algorithm generation functionality
/// This file can be used to test and validate the implementation
/// </summary>

using System;
using System.Diagnostics;
using Platform.Sandbox.AlgorithmGeneration;

public class AlgorithmGenerationTest
{
    public static void RunAllTests()
    {
        Console.WriteLine("Running Algorithm Generation Tests...\n");

        TestBasicPathFinding();
        TestOptimalPathSelection();
        TestMultiplePathsDiscovery();
        TestNoPathScenario();
        TestPerformance();

        Console.WriteLine("\n✓ All tests completed!");
    }

    private static void TestBasicPathFinding()
    {
        Console.WriteLine("TEST 1: Basic Path Finding");
        Console.WriteLine("---------------------------");

        var generator = new AlgorithmGenerator();
        generator.AddTransformation("A->B", "A", "B", 1, 1);
        generator.AddTransformation("B->C", "B", "C", 1, 1);

        var path = generator.FindOptimalPath("A", "C");

        Debug.Assert(path != null, "Path should be found");
        Debug.Assert(path.Transitions.Count == 2, "Path should have 2 transitions");
        Debug.Assert(path.TotalCost == 4, $"Total cost should be 4, got {path.TotalCost}");

        Console.WriteLine("✓ Basic path finding works correctly\n");
    }

    private static void TestOptimalPathSelection()
    {
        Console.WriteLine("TEST 2: Optimal Path Selection");
        Console.WriteLine("-------------------------------");

        var generator = new AlgorithmGenerator();

        // Expensive direct path
        generator.AddTransformation("Expensive", "Start", "End", 100, 100);

        // Cheap multi-step path
        generator.AddTransformation("Step1", "Start", "Mid", 1, 1);
        generator.AddTransformation("Step2", "Mid", "End", 1, 1);

        var path = generator.FindOptimalPath("Start", "End");

        Debug.Assert(path != null, "Path should be found");
        Debug.Assert(path.TotalCost == 4, $"Should select cheaper path with cost 4, got {path.TotalCost}");
        Debug.Assert(path.Transitions.Count == 2, "Should select multi-step path");

        Console.WriteLine("✓ Optimal path selection works correctly\n");
    }

    private static void TestMultiplePathsDiscovery()
    {
        Console.WriteLine("TEST 3: Multiple Paths Discovery");
        Console.WriteLine("----------------------------------");

        var generator = new AlgorithmGenerator();

        generator.AddTransformation("Path1-Step1", "A", "B", 1, 1);
        generator.AddTransformation("Path1-Step2", "B", "C", 1, 1);
        generator.AddTransformation("Path2-Step1", "A", "D", 2, 2);
        generator.AddTransformation("Path2-Step2", "D", "C", 2, 2);

        var allPaths = generator.FindAllPaths("A", "C", maxDepth: 10);

        Debug.Assert(allPaths.Count == 2, $"Should find 2 paths, found {allPaths.Count}");
        Debug.Assert(allPaths[0].TotalCost <= allPaths[1].TotalCost, "Paths should be sorted by cost");

        Console.WriteLine($"✓ Found {allPaths.Count} paths, correctly sorted by cost\n");
    }

    private static void TestNoPathScenario()
    {
        Console.WriteLine("TEST 4: No Path Scenario");
        Console.WriteLine("-------------------------");

        var generator = new AlgorithmGenerator();

        generator.AddTransformation("A->B", "A", "B", 1, 1);
        generator.AddTransformation("C->D", "C", "D", 1, 1);

        var path = generator.FindOptimalPath("A", "D");

        Debug.Assert(path == null, "Should return null when no path exists");

        Console.WriteLine("✓ Correctly handles disconnected graph\n");
    }

    private static void TestPerformance()
    {
        Console.WriteLine("TEST 5: Performance Test");
        Console.WriteLine("-------------------------");

        var generator = new AlgorithmGenerator();

        // Create a larger graph
        for (int i = 0; i < 100; i++)
        {
            generator.AddTransformation($"Op{i}", $"Type{i}", $"Type{i + 1}", i % 10, i % 5);
        }

        var sw = Stopwatch.StartNew();
        var path = generator.FindOptimalPath("Type0", "Type100");
        sw.Stop();

        Debug.Assert(path != null, "Path should be found");
        Debug.Assert(sw.ElapsedMilliseconds < 1000, $"Should complete in less than 1 second, took {sw.ElapsedMilliseconds}ms");

        Console.WriteLine($"✓ Found path through 100 nodes in {sw.ElapsedMilliseconds}ms\n");
    }
}
