using System;
using System.Linq;
using Platform.Experiments.CodeGraphVCS;

namespace Platform.Examples
{
    /// <summary>
    /// Example demonstrating Code Graph Version Control System
    /// This shows how to version control at function/class level instead of file level
    /// </summary>
    public class CodeGraphVCSExample
    {
        public static void Run()
        {
            Console.WriteLine("=== Code Graph VCS Example ===");
            Console.WriteLine();

            var vcs = new CodeGraphVCS();

            // Example 1: Create a simple class with methods
            Console.WriteLine("Step 1: Creating initial code structure");
            Console.WriteLine("----------------------------------------");

            var calculatorClass = vcs.CreateEntity("Calculator", CodeEntityType.Class,
                "public class Calculator { }", "Alice");

            var addMethod = vcs.CreateEntity("Add", CodeEntityType.Method,
                "public int Add(int a, int b) { return a + b; }", "Alice");

            var subtractMethod = vcs.CreateEntity("Subtract", CodeEntityType.Method,
                "public int Subtract(int a, int b) { return a - b; }", "Alice");

            // Create containment relationships
            vcs.CreateRelation(calculatorClass.Id, addMethod.Id, RelationType.Contains);
            vcs.CreateRelation(calculatorClass.Id, subtractMethod.Id, RelationType.Contains);

            Console.WriteLine(vcs.VisualizeGraph());

            // Example 2: Modify just the Add method (not the whole file!)
            Console.WriteLine("\nStep 2: Modifying only the Add method");
            Console.WriteLine("---------------------------------------");

            var addMethodV2 = vcs.CreateNewVersion(addMethod.Id,
                "public int Add(int a, int b) { \n    // Added validation\n    return a + b; \n}", "Bob");

            // Update the containment relationship to point to new version
            vcs.CreateRelation(calculatorClass.Id, addMethodV2.Id, RelationType.Contains);

            Console.WriteLine($"Created new version: {addMethodV2}");
            Console.WriteLine($"Original version: {addMethod}");

            // Example 3: Show version history of a method
            Console.WriteLine("\nStep 3: Version history of Add method");
            Console.WriteLine("--------------------------------------");

            var history = vcs.GetVersionHistory(addMethodV2.Id);
            Console.WriteLine($"Version history ({history.Count} versions):");
            foreach (var version in history)
            {
                Console.WriteLine($"  - {version.Id}: by {version.Author} at {version.Timestamp:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"    Content: {version.Content}");
            }

            // Example 4: Create dependencies between methods
            Console.WriteLine("\nStep 4: Adding method dependencies");
            Console.WriteLine("-----------------------------------");

            var multiplyMethod = vcs.CreateEntity("Multiply", CodeEntityType.Method,
                "public int Multiply(int a, int b) { \n    int result = 0;\n    for(int i = 0; i < b; i++) {\n        result = Add(result, a);\n    }\n    return result;\n}", "Charlie");

            vcs.CreateRelation(calculatorClass.Id, multiplyMethod.Id, RelationType.Contains);
            vcs.CreateRelation(multiplyMethod.Id, addMethodV2.Id, RelationType.Calls);

            Console.WriteLine($"Created {multiplyMethod}");
            Console.WriteLine($"  Calls: {addMethodV2}");

            // Example 5: Get dependency graph
            Console.WriteLine("\nStep 5: Dependency graph of Multiply method");
            Console.WriteLine("--------------------------------------------");

            var deps = vcs.GetDependencyGraph(multiplyMethod.Id);
            Console.WriteLine($"Dependencies of Multiply ({deps.Count} entities):");
            foreach (var depId in deps)
            {
                if (vcs.Entities.TryGetValue(depId, out var entity))
                {
                    Console.WriteLine($"  - {entity}");
                }
            }

            // Example 6: Create a snapshot (commit)
            Console.WriteLine("\nStep 6: Creating a snapshot (commit)");
            Console.WriteLine("-------------------------------------");

            var allEntityIds = vcs.Entities.Keys.ToHashSet();
            var snapshot = vcs.CreateSnapshot(
                "Initial Calculator implementation with Add, Subtract, and Multiply",
                "Charlie",
                allEntityIds
            );

            Console.WriteLine(snapshot);
            Console.WriteLine($"  Entities: {snapshot.EntityIds.Count}");
            Console.WriteLine($"  Relations: {snapshot.RelationIds.Count}");

            // Example 7: Demonstrate benefits over file-based VCS
            Console.WriteLine("\nStep 7: Benefits demonstration");
            Console.WriteLine("-------------------------------");
            Console.WriteLine("Traditional file-based VCS would show:");
            Console.WriteLine("  - Changed: Calculator.cs (entire file)");
            Console.WriteLine("  - Must diff entire file to see what changed");
            Console.WriteLine();
            Console.WriteLine("Code Graph VCS shows:");
            Console.WriteLine("  - Changed: Method 'Add' (ID: {0})", addMethodV2.Id);
            Console.WriteLine("  - Previous version: Method 'Add' (ID: {0})", addMethod.Id);
            Console.WriteLine("  - Exact change visible at method level");
            Console.WriteLine("  - Dependencies automatically tracked");
            Console.WriteLine("  - Can see impact of change (Multiply depends on Add)");

            // Example 8: Find all methods
            Console.WriteLine("\nStep 8: Query all methods in the system");
            Console.WriteLine("----------------------------------------");

            var methods = vcs.GetEntitiesByType(CodeEntityType.Method);
            Console.WriteLine($"Found {methods.Count()} methods:");
            foreach (var method in methods)
            {
                Console.WriteLine($"  - {method}");
            }

            // Final visualization
            Console.WriteLine("\nFinal Code Graph:");
            Console.WriteLine("=================");
            Console.WriteLine(vcs.VisualizeGraph());
        }
    }
}
