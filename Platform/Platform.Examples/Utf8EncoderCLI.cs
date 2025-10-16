using System;
using System.IO;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the UTF-8 encoder tool.
    /// Converts text files to UTF-8 encoding without BOM (byte order mark).
    /// </summary>
    public class Utf8EncoderCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }

            var path = args[0];
            var recursive = true;

            // Check if recursive flag is provided
            if (args.Length > 1 && (args[1] == "--no-recursive" || args[1] == "-n"))
            {
                recursive = false;
            }

            var encoder = new Utf8Encoder();

            if (File.Exists(path))
            {
                // Single file mode
                Console.WriteLine($"Converting file: {path}");
                if (encoder.ConvertFile(path))
                {
                    Console.WriteLine("File converted successfully.");
                }
                else
                {
                    Console.WriteLine("File was skipped (not a text file).");
                }
            }
            else if (Directory.Exists(path))
            {
                // Directory mode
                Console.WriteLine($"Converting files in directory: {path}");
                Console.WriteLine($"Recursive: {recursive}");
                Console.WriteLine();

                try
                {
                    int convertedCount = encoder.ConvertDirectory(path, recursive);
                    Console.WriteLine();
                    Console.WriteLine($"Total files converted: {convertedCount}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Error: Path not found: {path}");
                Console.WriteLine();
                ShowUsage();
            }
        }

        private void ShowUsage()
        {
            Console.WriteLine("UTF-8 Encoder - Convert text files to UTF-8 without BOM");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  Utf8EncoderCLI <path> [options]");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  <path>                Path to file or directory to convert");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --no-recursive, -n    Do not process subdirectories (directory mode only)");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  Utf8EncoderCLI myfile.txt");
            Console.WriteLine("  Utf8EncoderCLI ./src");
            Console.WriteLine("  Utf8EncoderCLI ./src --no-recursive");
        }
    }
}
