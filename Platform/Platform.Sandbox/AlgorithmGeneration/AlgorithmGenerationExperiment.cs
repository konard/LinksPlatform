using System;

namespace Platform.Sandbox.AlgorithmGeneration
{
    /// <summary>
    /// Experiment demonstrating algorithm generation through type transition path searching
    /// Based on issue #614 diagram showing type transformations with CPU/MEM costs
    /// </summary>
    public static class AlgorithmGenerationExperiment
    {
        public static void Run()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("Algorithm Generation Experiment");
            Console.WriteLine("Searching paths through type transitions");
            Console.WriteLine("========================================\n");

            RunBasicExample();
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            RunComplexExample();
        }

        /// <summary>
        /// Basic example matching the diagram from issue #614
        /// </summary>
        private static void RunBasicExample()
        {
            Console.WriteLine("BASIC EXAMPLE - From Diagram #614");
            Console.WriteLine("----------------------------------\n");

            var generator = new AlgorithmGenerator();

            // Define type transformations based on the diagram
            // The diagram shows transformations between Tin1, Tin2 and Tout1, Tout2
            // with various operations (or, X, Y) and their costs

            generator.AddTransformation("DirectMap", "Tin1", "Tout1", cpuCost: 10, memoryCost: 140);
            generator.AddTransformation("OR-Operation", "Tin1", "X", cpuCost: 2, memoryCost: 80);
            generator.AddTransformation("X-Transform", "X", "Tout1", cpuCost: 9, memoryCost: 120);

            generator.AddTransformation("DirectMap2", "Tin2", "Tout2", cpuCost: 0.1, memoryCost: 40);
            generator.AddTransformation("OR-Operation2", "Tin2", "Y", cpuCost: 7, memoryCost: 115);
            generator.AddTransformation("Y-Transform", "Y", "Tout2", cpuCost: 2, memoryCost: 70);

            // Cross transformations
            generator.AddTransformation("Cross1", "Tin1", "Y", cpuCost: 1, memoryCost: 20);
            generator.AddTransformation("Cross2", "X", "Tout2", cpuCost: 2, memoryCost: 80);

            generator.PrintGraph();

            Console.WriteLine("\n\nFinding optimal path: Tin1 -> Tout1");
            Console.WriteLine("------------------------------------");
            var path1 = generator.FindOptimalPath("Tin1", "Tout1");
            if (path1 != null)
            {
                Console.WriteLine(path1);
            }
            else
            {
                Console.WriteLine("No path found!");
            }

            Console.WriteLine("\nFinding optimal path: Tin2 -> Tout2");
            Console.WriteLine("------------------------------------");
            var path2 = generator.FindOptimalPath("Tin2", "Tout2");
            if (path2 != null)
            {
                Console.WriteLine(path2);
            }
            else
            {
                Console.WriteLine("No path found!");
            }

            Console.WriteLine("\nFinding optimal path: Tin1 -> Tout2");
            Console.WriteLine("------------------------------------");
            var path3 = generator.FindOptimalPath("Tin1", "Tout2");
            if (path3 != null)
            {
                Console.WriteLine(path3);
            }
            else
            {
                Console.WriteLine("No path found!");
            }
        }

        /// <summary>
        /// Complex example with multiple paths to demonstrate optimization
        /// </summary>
        private static void RunComplexExample()
        {
            Console.WriteLine("COMPLEX EXAMPLE - Multiple Paths");
            Console.WriteLine("----------------------------------\n");

            var generator = new AlgorithmGenerator();

            // Create a more complex graph with multiple paths
            generator.AddTransformation("A->B (Fast)", "A", "B", cpuCost: 5, memoryCost: 10);
            generator.AddTransformation("B->C (Fast)", "B", "C", cpuCost: 5, memoryCost: 10);
            generator.AddTransformation("A->D (Slow)", "A", "D", cpuCost: 100, memoryCost: 50);
            generator.AddTransformation("D->C (Fast)", "D", "C", cpuCost: 1, memoryCost: 1);
            generator.AddTransformation("A->E (Medium)", "A", "E", cpuCost: 20, memoryCost: 20);
            generator.AddTransformation("E->F (Medium)", "E", "F", cpuCost: 15, memoryCost: 15);
            generator.AddTransformation("F->C (Medium)", "F", "C", cpuCost: 10, memoryCost: 10);

            generator.PrintGraph();

            Console.WriteLine("\n\nFinding optimal path: A -> C");
            Console.WriteLine("-----------------------------");
            var optimalPath = generator.FindOptimalPath("A", "C");
            if (optimalPath != null)
            {
                Console.WriteLine(optimalPath);
            }

            Console.WriteLine("\nFinding ALL paths: A -> C (max depth 5)");
            Console.WriteLine("----------------------------------------");
            var allPaths = generator.FindAllPaths("A", "C", maxDepth: 5);
            Console.WriteLine($"Found {allPaths.Count} path(s):\n");

            for (int i = 0; i < allPaths.Count; i++)
            {
                Console.WriteLine($"Path {i + 1}:");
                Console.WriteLine(allPaths[i]);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Demonstrates finding paths for data structure transformations
        /// </summary>
        public static void RunDataStructureExample()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("Data Structure Transformation Example");
            Console.WriteLine("========================================\n");

            var generator = new AlgorithmGenerator();

            // Data structure transformations
            generator.AddTransformation("ToArray", "List", "Array", cpuCost: 5, memoryCost: 50);
            generator.AddTransformation("ToList", "Array", "List", cpuCost: 10, memoryCost: 60);
            generator.AddTransformation("ToDict", "List", "Dictionary", cpuCost: 20, memoryCost: 100);
            generator.AddTransformation("ToSet", "List", "HashSet", cpuCost: 15, memoryCost: 80);
            generator.AddTransformation("SetToList", "HashSet", "List", cpuCost: 10, memoryCost: 40);
            generator.AddTransformation("DictToList", "Dictionary", "List", cpuCost: 15, memoryCost: 50);

            generator.PrintGraph();

            Console.WriteLine("\n\nFinding optimal path: Array -> Dictionary");
            Console.WriteLine("------------------------------------------");
            var path = generator.FindOptimalPath("Array", "Dictionary");
            if (path != null)
            {
                Console.WriteLine(path);
            }
            else
            {
                Console.WriteLine("No path found!");
            }
        }
    }
}
