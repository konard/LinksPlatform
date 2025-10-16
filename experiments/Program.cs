using System;
using System.IO;
using Platform.CodeGeneration;

namespace TestDictionaryTransformation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing Dictionary to UnsafeDictionary Transformation");
            Console.WriteLine("=====================================================\n");

            try
            {
                var transformer = new DictionaryToUnsafeDictionaryTransformer();

                Console.WriteLine("Step 1: Downloading Dictionary source from GitHub...");
                var transformedCode = transformer.TransformSync();

                Console.WriteLine("Step 2: Transformation completed!\n");

                // Verify transformations
                var verificationResults = VerifyTransformations(transformedCode);

                Console.WriteLine("Verification Results:");
                Console.WriteLine("---------------------");
                foreach (var result in verificationResults)
                {
                    var icon = result.Value ? "✓" : "✗";
                    Console.WriteLine($"{icon} {result.Key}");
                }

                // Save output
                var outputPath = "UnsafeDictionary.cs";
                File.WriteAllText(outputPath, transformedCode);
                Console.WriteLine($"\nOutput saved to: {outputPath}");

                var allPassed = true;
                foreach (var result in verificationResults)
                {
                    if (!result.Value)
                    {
                        allPassed = false;
                        break;
                    }
                }

                if (allPassed)
                {
                    Console.WriteLine("\n✓ All transformations verified successfully!");
                }
                else
                {
                    Console.WriteLine("\n✗ Some transformations may not have been applied correctly.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine($"Type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");
                }
                Environment.Exit(1);
            }
        }

        static System.Collections.Generic.Dictionary<string, bool> VerifyTransformations(string code)
        {
            var results = new System.Collections.Generic.Dictionary<string, bool>();

            // Check if Dictionary was renamed to UnsafeDictionary
            results["Renamed to UnsafeDictionary"] = code.Contains("class UnsafeDictionary") ||
                                                      code.Contains("public partial class UnsafeDictionary");

            // Check if original Dictionary class name is mostly replaced
            // (Some may remain in comments or strings)
            var dictionaryCount = CountOccurrences(code, "class Dictionary<");
            results["Original Dictionary class removed"] = dictionaryCount == 0;

            // Verify that throw statements are reduced or removed
            // (original has many, transformed should have fewer or none)
            var throwCount = CountOccurrences(code, "throw ");
            results["Throw statements removed/reduced"] = throwCount < 50; // Arbitrary threshold

            // Check code is not empty
            results["Generated code is not empty"] = code.Length > 1000;

            // Check that it still has collection-related code
            results["Contains collection logic"] = code.Contains("Entry") || code.Contains("bucket");

            return results;
        }

        static int CountOccurrences(string text, string pattern)
        {
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) != -1)
            {
                count++;
                index += pattern.Length;
            }
            return count;
        }
    }
}
