using System;
using System.IO;
using Platform.CodeGeneration;

namespace Examples
{
    /// <summary>
    /// Example demonstrating how to use the Dictionary to UnsafeDictionary transformer
    /// </summary>
    class UnsafeDictionaryGeneratorExample
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Dictionary to UnsafeDictionary Transformer Example");
            Console.WriteLine("===================================================\n");

            try
            {
                // Create the transformer
                var transformer = new DictionaryToUnsafeDictionaryTransformer();

                Console.WriteLine("Downloading latest Dictionary source code from GitHub...");

                // Transform the Dictionary source code
                var transformedCode = transformer.TransformSync();

                Console.WriteLine("Transformation completed successfully!\n");

                // Save to file
                var outputPath = "UnsafeDictionary.cs";
                File.WriteAllText(outputPath, transformedCode);

                Console.WriteLine($"UnsafeDictionary source code saved to: {outputPath}");
                Console.WriteLine($"File size: {new FileInfo(outputPath).Length} bytes");

                // Display some statistics
                var lines = transformedCode.Split('\n').Length;
                Console.WriteLine($"Total lines: {lines}");

                Console.WriteLine("\nKey transformations applied:");
                Console.WriteLine("  ✓ Renamed Dictionary to UnsafeDictionary");
                Console.WriteLine("  ✓ Made entries field/property public");
                Console.WriteLine("  ✓ Removed exception throwing statements");
                Console.WriteLine("  ✓ Removed validation checks");

                Console.WriteLine("\nUnsafeDictionary is now ready for use!");
                Console.WriteLine("WARNING: This is an unsafe version without bounds checking.");
                Console.WriteLine("Use only when performance is critical and you control all inputs.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Environment.Exit(1);
            }
        }
    }
}
