using System;
using Platform.CodeTranslation;

namespace CodeTranslationExperiments
{
    /// <summary>
    /// Demonstrates the usage of Platform.CodeTranslation library for parsing and analyzing C# code.
    /// </summary>
    class CodeTranslationExample
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Platform.CodeTranslation Example ===\n");

            // Example C# code to analyze
            string sampleCode = @"
using System;

namespace Example
{
    public class Calculator
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

        public int Multiply(int x, int y)
        {
            return x * y;
        }
    }
}";

            // 1. Parse C# code using Roslyn
            Console.WriteLine("1. Parsing C# code...");
            var parser = new CSharpParser();
            var syntaxTree = parser.Parse(sampleCode);
            Console.WriteLine("   ✓ Parsing complete\n");

            // 2. Analyze the syntax tree
            Console.WriteLine("2. Analyzing syntax tree structure...");
            var analyzer = new SyntaxTreeAnalyzer();
            var summary = analyzer.GetStructureSummary(syntaxTree);
            Console.WriteLine($"   {summary}\n");

            // 3. Extract specific elements
            Console.WriteLine("3. Extracting classes:");
            var classes = analyzer.GetClassDeclarations(syntaxTree);
            foreach (var cls in classes)
            {
                Console.WriteLine($"   - Class: {cls.Identifier.Text}");
            }
            Console.WriteLine();

            Console.WriteLine("4. Extracting methods:");
            var methods = analyzer.GetMethodDeclarations(syntaxTree);
            foreach (var method in methods)
            {
                Console.WriteLine($"   - Method: {method.Identifier.Text}");
            }
            Console.WriteLine();

            // 4. Demonstrate code transformation
            Console.WriteLine("5. Transforming code to verbose version...");
            var translator = new CSharpToVerboseTranslator();
            Console.WriteLine($"   Translator: {translator.SourceLanguage} → {translator.TargetLanguage}");

            string verboseCode = translator.Translate(sampleCode);
            Console.WriteLine("\n   Transformed code:");
            Console.WriteLine("   " + new string('-', 60));
            Console.WriteLine(verboseCode);
            Console.WriteLine("   " + new string('-', 60));

            Console.WriteLine("\n=== Example Complete ===");
        }
    }
}
