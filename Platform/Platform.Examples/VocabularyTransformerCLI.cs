using System;
using System.Collections.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for demonstrating vocabulary transformation functionality.
    /// </summary>
    public class VocabularyTransformerCLI : ICommandLineInterface
    {
        /// <summary>
        /// Runs the vocabulary transformer demonstration.
        /// </summary>
        /// <param name="args">Command-line arguments (not used).</param>
        public void Run(params string[] args)
        {
            Console.WriteLine("=== Vocabulary Transformer Demo ===");
            Console.WriteLine();

            // Create transformer and add sample mappings
            var transformer = new VocabularyTransformer();

            // Add complex-to-simple mappings
            transformer.AddComplexToSimpleMapping("utilize", "use");
            transformer.AddComplexToSimpleMapping("accomplish", "do");
            transformer.AddComplexToSimpleMapping("demonstrate", "show");
            transformer.AddComplexToSimpleMapping("terminate", "end");
            transformer.AddComplexToSimpleMapping("purchase", "buy");
            transformer.AddComplexToSimpleMapping("construct", "build");
            transformer.AddComplexToSimpleMapping("facilitate", "help");
            transformer.AddComplexToSimpleMapping("commence", "start");

            // Add simple-to-complex mappings
            transformer.AddSimpleToComplexMapping("make better", "improve");
            transformer.AddSimpleToComplexMapping("make worse", "deteriorate");
            transformer.AddSimpleToComplexMapping("work together", "collaborate");
            transformer.AddSimpleToComplexMapping("find out", "discover");
            transformer.AddSimpleToComplexMapping("speed up", "accelerate");
            transformer.AddSimpleToComplexMapping("slow down", "decelerate");
            transformer.AddSimpleToComplexMapping("put together", "assemble");
            transformer.AddSimpleToComplexMapping("take apart", "disassemble");

            Console.WriteLine($"Loaded {transformer.ComplexToSimpleMappingCount} complex-to-simple mappings");
            Console.WriteLine($"Loaded {transformer.SimpleToComplexMappingCount} simple-to-complex mappings");
            Console.WriteLine();

            // Demo 1: Unpack complex words to simple
            Console.WriteLine("--- Demo 1: Unpacking Complex Words ---");
            var complexText = "We need to utilize this tool to accomplish our tasks and demonstrate the results.";
            Console.WriteLine($"Original: {complexText}");
            var simplifiedText = transformer.UnpackToSimple(complexText);
            Console.WriteLine($"Simplified: {simplifiedText}");
            Console.WriteLine();

            // Demo 2: Package simple phrases to complex
            Console.WriteLine("--- Demo 2: Packaging Simple Phrases ---");
            var simpleText = "Let's work together to find out how to make better this system and speed up the process.";
            Console.WriteLine($"Original: {simpleText}");
            var complexifiedText = transformer.PackageToComplex(simpleText);
            Console.WriteLine($"Complexified: {complexifiedText}");
            Console.WriteLine();

            // Demo 3: Adapt to specific vocabulary
            Console.WriteLine("--- Demo 3: Adapting to Known Vocabulary ---");
            var knownWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "we", "need", "to", "use", "this", "tool", "do", "our", "tasks",
                "and", "show", "the", "results", "can", "help", "us", "work", "faster"
            };

            var textToAdapt = "We need to utilize this tool to accomplish our tasks and demonstrate the results.";
            Console.WriteLine($"Original: {textToAdapt}");
            Console.WriteLine($"Known vocabulary size: {knownWords.Count} words");
            var adaptedText = transformer.AdaptToVocabulary(textToAdapt, knownWords);
            Console.WriteLine($"Adapted: {adaptedText}");
            Console.WriteLine();

            // Demo 4: Bidirectional mapping
            Console.WriteLine("--- Demo 4: Bidirectional Mapping ---");
            var biTransformer = new VocabularyTransformer();
            biTransformer.AddBidirectionalMapping("investigate", "look into");
            biTransformer.AddBidirectionalMapping("comprehend", "understand");

            var text1 = "We need to investigate this issue.";
            var text2 = "We need to look into this issue.";

            Console.WriteLine($"Text 1: {text1}");
            Console.WriteLine($"Simplified: {biTransformer.UnpackToSimple(text1)}");
            Console.WriteLine();
            Console.WriteLine($"Text 2: {text2}");
            Console.WriteLine($"Complexified: {biTransformer.PackageToComplex(text2)}");
            Console.WriteLine();

            Console.WriteLine("=== Demo Complete ===");
        }
    }
}
