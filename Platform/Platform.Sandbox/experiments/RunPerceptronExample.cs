using System;

namespace Platform.Sandbox.Experiments
{
    /// <summary>
    /// Helper class to run perceptron examples.
    /// This can be used to demonstrate the perceptron neural network
    /// implementation created for issue #33.
    ///
    /// To run this example:
    /// 1. Uncomment the call in Program.cs: PerceptronExamples.RunAllExamples();
    /// 2. Build and run the Platform.Sandbox project
    /// </summary>
    public static class RunPerceptronExample
    {
        public static void Run()
        {
            PerceptronExamples.RunAllExamples();

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
