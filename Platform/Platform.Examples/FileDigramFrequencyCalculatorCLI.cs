using System;
using System.IO;
using Platform.IO;

namespace Platform.Examples
{
    public class FileDigramFrequencyCalculatorCLI : ICommandLineInterface
    {
        public void Run(string[] args)
        {
            var fileToAnalyze = ConsoleHelpers.GetOrReadArgument(0, "File to analyze", args);
            var outputFile = ConsoleHelpers.GetOrReadArgument(1, "Output file (optional, press Enter to skip)", args);

            if (!File.Exists(fileToAnalyze))
            {
                Console.WriteLine("Entered file to analyze does not exist.");
            }
            else
            {
                using (var cancellation = new ConsoleCancellation())
                {
                    Console.WriteLine("Press CTRL+C to stop.");
                    var calculator = new FileDigramFrequencyCalculator();
                    calculator.CalculateSync(fileToAnalyze, cancellation.Token);

                    calculator.PrintTopDigrams(20);

                    if (!string.IsNullOrWhiteSpace(outputFile))
                    {
                        calculator.SaveToFile(outputFile);
                    }
                }
            }
        }
    }
}
