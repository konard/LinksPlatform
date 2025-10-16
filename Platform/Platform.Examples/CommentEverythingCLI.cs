using System;
using System.Threading;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the CommentEverything tool.
    /// Provides commands to generate comments from code and code from comments.
    /// </summary>
    public class CommentEverythingCLI : ICommandLineInterface
    {
        /// <summary>
        /// Runs the CommentEverything CLI with the specified arguments.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        public void Run(params string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            var command = args[0].ToLower();
            var tool = new CommentEverything(verbose: true);
            var cancellationToken = new CancellationToken();

            try
            {
                switch (command)
                {
                    case "generate":
                    case "gen":
                        HandleGenerateComments(tool, args, cancellationToken);
                        break;

                    case "scaffold":
                    case "code":
                        HandleGenerateCode(tool, args, cancellationToken);
                        break;

                    case "help":
                    case "-h":
                    case "--help":
                        PrintUsage();
                        break;

                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        PrintUsage();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (args.Length > 0 && (args[args.Length - 1] == "-v" || args[args.Length - 1] == "--verbose"))
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        private void HandleGenerateComments(CommentEverything tool, string[] args, CancellationToken cancellationToken)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Missing path argument");
                Console.WriteLine("Usage: generate <file-or-directory> [output-path]");
                return;
            }

            var path = args[1];
            var outputPath = args.Length > 2 ? args[2] : null;

            if (System.IO.Directory.Exists(path))
            {
                Console.WriteLine($"Generating comments for all C# files in: {path}");
                var count = tool.GenerateCommentsForDirectory(path, "*.cs", cancellationToken);
                Console.WriteLine($"Complete! Generated {count} comments.");
            }
            else if (System.IO.File.Exists(path))
            {
                Console.WriteLine($"Generating comments for: {path}");
                var count = tool.GenerateComments(path, outputPath, cancellationToken);
                Console.WriteLine($"Complete! Generated {count} comments.");
            }
            else
            {
                Console.WriteLine($"Error: Path not found: {path}");
            }
        }

        private void HandleGenerateCode(CommentEverything tool, string[] args, CancellationToken cancellationToken)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Error: Missing required arguments");
                Console.WriteLine("Usage: scaffold <input-file> <output-file>");
                return;
            }

            var inputPath = args[1];
            var outputPath = args[2];

            Console.WriteLine($"Generating code from comments in: {inputPath}");
            var count = tool.GenerateCodeFromComments(inputPath, outputPath, cancellationToken);
            Console.WriteLine($"Complete! Generated {count} code elements to {outputPath}");
        }

        private void PrintUsage()
        {
            Console.WriteLine("CommentEverything - Bidirectional code and comment synchronization tool");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  CommentEverythingCLI <command> [options]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  generate, gen    Generate XML documentation comments from code");
            Console.WriteLine("                   Usage: generate <file-or-directory> [output-path]");
            Console.WriteLine();
            Console.WriteLine("  scaffold, code   Generate code scaffolding from special comments");
            Console.WriteLine("                   Usage: scaffold <input-file> <output-file>");
            Console.WriteLine("                   Comment format: /// <scaffold type=\"class\" name=\"MyClass\" />");
            Console.WriteLine();
            Console.WriteLine("  help            Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  # Generate comments for a single file");
            Console.WriteLine("  CommentEverythingCLI generate MyClass.cs");
            Console.WriteLine();
            Console.WriteLine("  # Generate comments for all files in a directory");
            Console.WriteLine("  CommentEverythingCLI generate ./src/");
            Console.WriteLine();
            Console.WriteLine("  # Generate code from scaffold comments");
            Console.WriteLine("  CommentEverythingCLI scaffold scaffold.txt output.cs");
            Console.WriteLine();
        }
    }
}
