using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Examples.AutoTyping;

namespace Platform.Experiments
{
    /// <summary>
    /// Demonstrates AutoTyping with Links Platform associative data structures.
    /// Shows how to infer types from link/doublet patterns.
    /// </summary>
    public class AutoTypingLinksExample
    {
        public static void Main(string[] args)
        {
            var engine = new AutoTypingEngine();

            Console.WriteLine("=== AutoTyping with Links Platform ===\n");

            // Example 1: Infer type from link patterns
            Console.WriteLine("Example 1: Link Pattern Type Inference");
            Console.WriteLine("---------------------------------------");

            // Simulate doublets (pairs) representing data
            var personLinks = new[]
            {
                new Doublet { Source = "Person1", Target = "Name:Alice" },
                new Doublet { Source = "Person1", Target = "Age:30" },
                new Doublet { Source = "Person2", Target = "Name:Bob" },
                new Doublet { Source = "Person2", Target = "Age:25" },
            };

            var linkPatternType = engine.InferType(personLinks, "PersonLinkPattern");
            Console.WriteLine(linkPatternType.GenerateDescription());
            Console.WriteLine();

            // Example 2: Sequence type inference
            Console.WriteLine("Example 2: Sequence Type Inference");
            Console.WriteLine("-----------------------------------");

            var sequences = new[]
            {
                new Sequence { Elements = new object[] { 1, 2, 3, 4, 5 } },
                new Sequence { Elements = new object[] { 10, 20, 30, 40 } },
                new Sequence { Elements = new object[] { 100, 200, 300, 400, 500, 600 } }
            };

            var sequenceType = engine.InferTypeFromExamples(sequences, "NumberSequence");
            Console.WriteLine(sequenceType.GenerateDefinition());
            Console.WriteLine();

            // Example 3: Heterogeneous sequences (like described in links-theory.md)
            Console.WriteLine("Example 3: Heterogeneous Sequence Type Inference");
            Console.WriteLine("-------------------------------------------------");

            var mixedSequences = new[]
            {
                new Sequence { Elements = new object[] { "hello", 42, true } },
                new Sequence { Elements = new object[] { "world", 99, false } },
            };

            var mixedSeqType = engine.InferTypeFromExamples(mixedSequences, "MixedSequence");
            Console.WriteLine(mixedSeqType.GenerateDefinition());
            Console.WriteLine();

            // Example 4: Graph-like structure type inference
            Console.WriteLine("Example 4: Graph Structure Type Inference");
            Console.WriteLine("------------------------------------------");

            var graphNodes = new[]
            {
                new GraphNode
                {
                    Id = 1,
                    Type = "Person",
                    Properties = new Dictionary<string, object>
                    {
                        { "Name", "Alice" },
                        { "Age", 30 }
                    }
                },
                new GraphNode
                {
                    Id = 2,
                    Type = "Person",
                    Properties = new Dictionary<string, object>
                    {
                        { "Name", "Bob" },
                        { "Age", 25 },
                        { "Email", "bob@example.com" }
                    }
                }
            };

            var graphNodeType = engine.InferTypeFromExamples(graphNodes, "PersonNode");
            Console.WriteLine(graphNodeType.GenerateDefinition());
            Console.WriteLine();

            // Example 5: Associative triple inference
            Console.WriteLine("Example 5: Associative Triple Type Inference");
            Console.WriteLine("---------------------------------------------");

            var triples = new[]
            {
                new Triple { Subject = "Alice", Predicate = "knows", Object = "Bob" },
                new Triple { Subject = "Bob", Predicate = "knows", Object = "Charlie" },
                new Triple { Subject = "Alice", Predicate = "age", Object = 30 },
                new Triple { Subject = "Bob", Predicate = "age", Object = 25 }
            };

            var tripleType = engine.InferTypeFromExamples(triples, "KnowledgeTriple");
            Console.WriteLine(tripleType.GenerateDefinition());
            Console.WriteLine();

            // Example 6: Type evolution - learning from new examples
            Console.WriteLine("Example 6: Type Evolution");
            Console.WriteLine("-------------------------");

            // Initial type inference from limited data
            var initialData = new[]
            {
                new { Id = 1, Name = "Alice" }
            };

            var evolvedType = engine.InferTypeFromExamples(initialData, "EvolvedType");
            Console.WriteLine("Initial type:");
            Console.WriteLine(evolvedType.GenerateDefinition());

            // Later, more examples reveal additional properties
            var moreData = new[]
            {
                new { Id = 2, Name = "Bob", Email = "bob@example.com", Age = 25 }
            };

            var updatedType = engine.InferTypeFromExamples(
                initialData.Cast<object>().Concat(moreData).ToArray(),
                "EvolvedType"
            );

            Console.WriteLine("Evolved type after more examples:");
            Console.WriteLine(updatedType.GenerateDefinition());
            Console.WriteLine();

            // Example 7: Type pattern matching
            Console.WriteLine("Example 7: Type Pattern Recognition");
            Console.WriteLine("------------------------------------");

            var dataPoint1 = new { X = 1, Y = 2 };
            var dataPoint2 = new { X = 3, Y = 4 };
            var inferredPoint = engine.InferType(dataPoint1, "Point2D");

            Console.WriteLine("Inferred Point2D type:");
            Console.WriteLine(inferredPoint.GenerateDefinition());

            var dataPoint3 = new { X = 5, Y = 6, Z = 7 };
            var inferredPoint3D = engine.InferType(dataPoint3, "Point3D");

            Console.WriteLine("Inferred Point3D type:");
            Console.WriteLine(inferredPoint3D.GenerateDefinition());

            Console.WriteLine("=== Demo Complete ===");
        }
    }

    // Helper classes for demonstration
    public class Doublet
    {
        public object Source { get; set; }
        public object Target { get; set; }
    }

    public class Sequence
    {
        public object[] Elements { get; set; }
    }

    public class GraphNode
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }

    public class Triple
    {
        public object Subject { get; set; }
        public object Predicate { get; set; }
        public object Object { get; set; }
    }
}
