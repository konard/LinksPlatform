using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BreakingChangesMigrator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Breaking Changes Migration Tool");
            Console.WriteLine("================================\n");

            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }

            string command = args[0].ToLower();

            try
            {
                switch (command)
                {
                    case "init":
                        InitializeMigrationRules(args);
                        break;
                    case "migrate":
                        MigrateCode(args);
                        break;
                    case "analyze":
                        AnalyzeCode(args);
                        break;
                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        ShowUsage();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        static void ShowUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  init <output-file>        - Create a sample migration rules file");
            Console.WriteLine("  analyze <rules> <source>  - Analyze code for breaking changes");
            Console.WriteLine("  migrate <rules> <source>  - Migrate code according to rules");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  BreakingChangesMigrator init migration-rules.json");
            Console.WriteLine("  BreakingChangesMigrator analyze migration-rules.json MyCode.cs");
            Console.WriteLine("  BreakingChangesMigrator migrate migration-rules.json MyCode.cs");
        }

        static void InitializeMigrationRules(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Output file path required.");
                Console.WriteLine("Usage: init <output-file>");
                return;
            }

            string outputPath = args[1];
            MigrationRuleLoader.CreateSampleRulesFile(outputPath);
            Console.WriteLine($"Sample migration rules created: {outputPath}");
            Console.WriteLine("Edit this file to define your migration rules.");
        }

        static void AnalyzeCode(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Error: Rules file and source file required.");
                Console.WriteLine("Usage: analyze <rules-file> <source-file>");
                return;
            }

            string rulesPath = args[1];
            string sourcePath = args[2];

            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"Error: Source file not found: {sourcePath}");
                return;
            }

            var rules = MigrationRuleLoader.LoadFromFile(rulesPath);
            Console.WriteLine($"Loaded {rules.Count} migration rules from {rulesPath}");

            var sourceCode = File.ReadAllText(sourcePath);
            var tree = CSharpSyntaxTree.ParseText(sourceCode);
            var compilation = CSharpCompilation.Create("Analysis")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(tree);
            var semanticModel = compilation.GetSemanticModel(tree);

            var migrator = new CodeMigrator(rules, semanticModel);
            var root = tree.GetRoot();

            Console.WriteLine($"\nAnalyzing {sourcePath}...");
            Console.WriteLine($"Found {rules.Count} applicable migration rules:\n");

            foreach (var rule in rules)
            {
                Console.WriteLine($"  [{rule.FromVersion} → {rule.ToVersion}] {rule.ChangeType}");
                Console.WriteLine($"    {rule.Description}");
                if (rule.OldPattern.MemberName != null)
                {
                    Console.WriteLine($"    Old: {rule.OldPattern.TypeName}.{rule.OldPattern.MemberName}");
                    Console.WriteLine($"    New: {rule.NewPattern.TypeName ?? rule.OldPattern.TypeName}.{rule.NewPattern.MemberName}");
                }
                Console.WriteLine();
            }
        }

        static void MigrateCode(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Error: Rules file and source file required.");
                Console.WriteLine("Usage: migrate <rules-file> <source-file> [output-file]");
                return;
            }

            string rulesPath = args[1];
            string sourcePath = args[2];
            string outputPath = args.Length > 3 ? args[3] : sourcePath + ".migrated";

            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"Error: Source file not found: {sourcePath}");
                return;
            }

            var rules = MigrationRuleLoader.LoadFromFile(rulesPath);
            Console.WriteLine($"Loaded {rules.Count} migration rules from {rulesPath}");

            var sourceCode = File.ReadAllText(sourcePath);
            var tree = CSharpSyntaxTree.ParseText(sourceCode);

            // Create a basic compilation with common references
            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create("Migration")
                .AddReferences(references)
                .AddSyntaxTrees(tree);

            var semanticModel = compilation.GetSemanticModel(tree);

            var migrator = new CodeMigrator(rules, semanticModel);
            var root = tree.GetRoot();

            Console.WriteLine($"Migrating {sourcePath}...");
            var newRoot = migrator.Migrate(root);

            var migratedCode = newRoot.ToFullString();
            File.WriteAllText(outputPath, migratedCode);

            Console.WriteLine($"Migration complete! Output saved to: {outputPath}");
            Console.WriteLine($"\nApplied {rules.Count} migration rules.");
        }
    }
}
