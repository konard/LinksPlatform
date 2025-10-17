using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Platform.Tests.Generator
{
    /// <summary>
    /// Automatic test generator that analyzes C# source files and generates basic test scaffolding
    /// to maximize test coverage for LinksPlatform projects.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Platform.Tests.Generator - Automatic Test Generation Tool");
            Console.WriteLine("=========================================================");

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: Platform.Tests.Generator <source-project-path> <test-project-path>");
                Console.WriteLine("Example: Platform.Tests.Generator ../Platform.Examples ../Platform.Examples.Tests");
                return;
            }

            var sourceProjectPath = args[0];
            var testProjectPath = args[1];

            if (!Directory.Exists(sourceProjectPath))
            {
                Console.WriteLine($"Error: Source project path not found: {sourceProjectPath}");
                return;
            }

            Console.WriteLine($"Source Project: {sourceProjectPath}");
            Console.WriteLine($"Test Project: {testProjectPath}");
            Console.WriteLine();

            var generator = new TestGenerator();
            generator.GenerateTests(sourceProjectPath, testProjectPath);

            Console.WriteLine();
            Console.WriteLine("Test generation completed!");
        }
    }

    /// <summary>
    /// Generates test scaffolding for C# classes and methods.
    /// </summary>
    public class TestGenerator
    {
        public void GenerateTests(string sourceProjectPath, string testProjectPath)
        {
            // Create test project directory if it doesn't exist
            if (!Directory.Exists(testProjectPath))
            {
                Directory.CreateDirectory(testProjectPath);
                Console.WriteLine($"Created test project directory: {testProjectPath}");
            }

            // Find all .cs files in source project
            var sourceFiles = Directory.GetFiles(sourceProjectPath, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains("\\obj\\") && !f.Contains("\\bin\\"))
                .ToList();

            Console.WriteLine($"Found {sourceFiles.Count} source files to analyze");
            Console.WriteLine();

            foreach (var sourceFile in sourceFiles)
            {
                try
                {
                    GenerateTestForFile(sourceFile, sourceProjectPath, testProjectPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {Path.GetFileName(sourceFile)}: {ex.Message}");
                }
            }
        }

        private void GenerateTestForFile(string sourceFile, string sourceProjectPath, string testProjectPath)
        {
            var sourceCode = File.ReadAllText(sourceFile);
            var tree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = tree.GetRoot() as CompilationUnitSyntax;

            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();

            if (!classes.Any())
            {
                return;
            }

            var fileName = Path.GetFileName(sourceFile);
            var testFileName = fileName.Replace(".cs", "Tests.cs");
            var testFilePath = Path.Combine(testProjectPath, testFileName);

            Console.WriteLine($"Generating tests for: {fileName}");

            var testCode = GenerateTestClass(root, classes);
            File.WriteAllText(testFilePath, testCode);
        }

        private string GenerateTestClass(CompilationUnitSyntax root, List<ClassDeclarationSyntax> classes)
        {
            var usings = new List<string>
            {
                "using System;",
                "using Xunit;"
            };

            // Get namespace from original file
            var namespaceDecl = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            var originalNamespace = namespaceDecl?.Name.ToString() ?? "Platform.Tests";
            var testNamespace = $"{originalNamespace}.Tests";

            // Add usings from original file
            foreach (var usingDirective in root.Usings)
            {
                var usingStatement = usingDirective.ToString().Trim();
                if (!usings.Contains(usingStatement))
                {
                    usings.Add(usingStatement);
                }
            }

            var testClassesCode = new List<string>();

            foreach (var classDecl in classes)
            {
                var className = classDecl.Identifier.Text;
                var testClassName = $"{className}Tests";

                var methods = classDecl.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)))
                    .ToList();

                if (!methods.Any())
                {
                    continue;
                }

                var testMethods = new List<string>();
                foreach (var method in methods)
                {
                    var methodName = method.Identifier.Text;
                    var testMethodName = $"{methodName}Test";

                    var testMethod = $@"        [Fact]
        public void {testMethodName}()
        {{
            // Arrange
            // TODO: Set up test data and dependencies

            // Act
            // TODO: Call {className}.{methodName}

            // Assert
            // TODO: Verify expected behavior
            Assert.True(true, ""Test not yet implemented"");
        }}";
                    testMethods.Add(testMethod);
                }

                var testClass = $@"    public class {testClassName}
    {{
{string.Join(Environment.NewLine + Environment.NewLine, testMethods)}
    }}";
                testClassesCode.Add(testClass);
            }

            var fullTestCode = $@"{string.Join(Environment.NewLine, usings)}

namespace {testNamespace}
{{
{string.Join(Environment.NewLine + Environment.NewLine, testClassesCode)}
}}
";

            return fullTestCode;
        }
    }
}
