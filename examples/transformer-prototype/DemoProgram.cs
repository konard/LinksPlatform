using System;
using System.IO;
using System.Threading.Tasks;
using Platform.Transformer.Prototype;

namespace Examples.TransformerPrototype
{
    /// <summary>
    /// Demonstration program showing how to use the new transformer architecture.
    /// </summary>
    public class DemoProgram
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== Transformer Prototype Demo ===");
            Console.WriteLine();
            Console.WriteLine("This prototype demonstrates the new translator architecture:");
            Console.WriteLine("1. Repository-level (not file-level) translation");
            Console.WriteLine("2. AST parsing using Roslyn (not regex)");
            Console.WriteLine("3. Doublets as intermediate storage (not files)");
            Console.WriteLine("4. Function-based transformations (not regex substitutions)");
            Console.WriteLine();

            // Create a temporary test repository
            var sourceRepo = Path.Combine(Path.GetTempPath(), "source-repo");
            var targetRepo = Path.Combine(Path.GetTempPath(), "target-repo");

            try
            {
                // Create source repository with sample code
                Directory.CreateDirectory(sourceRepo);
                var sampleFile = Path.Combine(sourceRepo, "Sample.cs");
                File.WriteAllText(sampleFile, @"
using System;

namespace SampleProject
{
    public class HelloWorld
    {
        public void SayHello()
        {
            Console.WriteLine(""Hello, World!"");
        }
    }
}
");

                Console.WriteLine($"Created sample source repository at: {sourceRepo}");
                Console.WriteLine();

                // Create transformer
                var transformer = new SimplifiedRepositoryTransformer("csharp", "cpp");

                // Add transformation functions
                transformer.AddTransformation(new SimpleTransformation());

                Console.WriteLine("Starting transformation...");
                Console.WriteLine();

                // Transform
                await transformer.TransformAsync(sourceRepo, targetRepo);

                Console.WriteLine();
                Console.WriteLine($"Transformation complete!");
                Console.WriteLine($"Output directory: {targetRepo}");
                Console.WriteLine();

                // Show generated files
                if (Directory.Exists(targetRepo))
                {
                    var files = Directory.GetFiles(targetRepo, "*", SearchOption.AllDirectories);
                    Console.WriteLine($"Generated {files.Length} file(s):");
                    foreach (var file in files)
                    {
                        Console.WriteLine($"  - {Path.GetRelativePath(targetRepo, file)}");
                        Console.WriteLine($"    Content preview:");
                        var content = File.ReadAllText(file);
                        var preview = content.Length > 200 ? content.Substring(0, 200) + "..." : content;
                        foreach (var line in preview.Split('\n'))
                        {
                            Console.WriteLine($"      {line}");
                        }
                        Console.WriteLine();
                    }
                }

                Console.WriteLine("=== Demo Complete ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(sourceRepo))
                {
                    Directory.Delete(sourceRepo, true);
                }
                if (Directory.Exists(targetRepo))
                {
                    Directory.Delete(targetRepo, true);
                }
            }
        }
    }
}
