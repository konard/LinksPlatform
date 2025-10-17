using System;
using Platform.Sandbox;

namespace Platform.Experiments
{
    /// <summary>
    /// Demonstrates the frequency-based pattern recognition algorithm from issue #652
    /// </summary>
    public class FrequencyBasedPatternRecognitionExample
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Frequency-Based Pattern Recognition Demonstration");
            Console.WriteLine("Based on ideas from issue #652");
            Console.WriteLine();

            // Run the example from the implementation
            FrequencyBasedPatternRecognition.RunExample();
        }
    }
}
