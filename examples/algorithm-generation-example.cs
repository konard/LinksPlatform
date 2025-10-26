/// <summary>
/// Standalone example demonstrating algorithm generation through type transition path searching
/// Based on issue #614: Algorithm generation as searching of path through type transitions
///
/// This example shows how to:
/// 1. Define types and transformations between them
/// 2. Assign costs (CPU and Memory) to each transformation
/// 3. Find the optimal path from input type(s) to output type(s)
/// 4. Compare multiple possible paths
/// </summary>

using System;
using Platform.Sandbox.AlgorithmGeneration;

public class AlgorithmGenerationExample
{
    public static void Main()
    {
        // Create the algorithm generator
        var generator = new AlgorithmGenerator();

        // Define transformations as shown in the diagram from issue #614
        // Format: AddTransformation(operationName, sourceType, targetType, cpuCost, memoryCost)

        // Path 1: Direct transformation with high cost
        generator.AddTransformation("DirectMap", "Tin1", "Tout1", cpuCost: 10, memoryCost: 140);

        // Path 2: Two-step transformation through intermediate type X
        generator.AddTransformation("OR-Operation", "Tin1", "X", cpuCost: 2, memoryCost: 80);
        generator.AddTransformation("X-Transform", "X", "Tout1", cpuCost: 9, memoryCost: 120);

        // Find the optimal path
        Console.WriteLine("Finding optimal path from Tin1 to Tout1...\n");
        var path = generator.FindOptimalPath("Tin1", "Tout1");

        if (path != null)
        {
            Console.WriteLine(path);
            Console.WriteLine($"\nThe algorithm chose the {'path with lowest total cost'} (CPU + MEM)");
        }

        // Find all possible paths
        Console.WriteLine("\n" + new string('-', 50));
        Console.WriteLine("Finding ALL possible paths...\n");
        var allPaths = generator.FindAllPaths("Tin1", "Tout1", maxDepth: 5);

        for (int i = 0; i < allPaths.Count; i++)
        {
            Console.WriteLine($"Path {i + 1}/{allPaths.Count}:");
            Console.WriteLine(allPaths[i]);
        }
    }
}
