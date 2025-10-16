using System;

namespace Platform.Sandbox.Experiments
{
    /// <summary>
    /// Demonstrates various perceptron learning examples.
    /// These examples show how a simple perceptron can learn
    /// basic logical operations and simple classification tasks.
    /// </summary>
    public static class PerceptronExamples
    {
        /// <summary>
        /// Demonstrates learning the AND logical operation.
        /// The AND function is linearly separable, perfect for perceptron learning.
        /// </summary>
        public static void LearnAndGate()
        {
            Console.WriteLine("=== Learning AND Gate ===\n");

            // Training data for AND gate
            double[][] inputs = new double[][]
            {
                new double[] { 0, 0 },  // 0 AND 0 = 0
                new double[] { 0, 1 },  // 0 AND 1 = 0
                new double[] { 1, 0 },  // 1 AND 0 = 0
                new double[] { 1, 1 }   // 1 AND 1 = 1
            };

            int[] labels = new int[] { 0, 0, 0, 1 };

            // Create and train perceptron
            var perceptron = new SimplePerceptron(inputCount: 2, learningRate: 0.1);
            Console.WriteLine("Initial weights:");
            perceptron.PrintWeights();
            Console.WriteLine();

            perceptron.Train(inputs, labels, epochs: 100, verbose: true);

            Console.WriteLine("\nFinal weights:");
            perceptron.PrintWeights();

            // Test the trained perceptron
            Console.WriteLine("\nTesting AND gate:");
            for (int i = 0; i < inputs.Length; i++)
            {
                int prediction = perceptron.Predict(inputs[i]);
                Console.WriteLine($"  {inputs[i][0]} AND {inputs[i][1]} = {prediction} (expected: {labels[i]})");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates learning the OR logical operation.
        /// </summary>
        public static void LearnOrGate()
        {
            Console.WriteLine("=== Learning OR Gate ===\n");

            // Training data for OR gate
            double[][] inputs = new double[][]
            {
                new double[] { 0, 0 },  // 0 OR 0 = 0
                new double[] { 0, 1 },  // 0 OR 1 = 1
                new double[] { 1, 0 },  // 1 OR 0 = 1
                new double[] { 1, 1 }   // 1 OR 1 = 1
            };

            int[] labels = new int[] { 0, 1, 1, 1 };

            // Create and train perceptron
            var perceptron = new SimplePerceptron(inputCount: 2, learningRate: 0.1);
            perceptron.Train(inputs, labels, epochs: 100, verbose: true);

            Console.WriteLine("\nFinal weights:");
            perceptron.PrintWeights();

            // Test the trained perceptron
            Console.WriteLine("\nTesting OR gate:");
            for (int i = 0; i < inputs.Length; i++)
            {
                int prediction = perceptron.Predict(inputs[i]);
                Console.WriteLine($"  {inputs[i][0]} OR {inputs[i][1]} = {prediction} (expected: {labels[i]})");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates learning the NAND logical operation.
        /// </summary>
        public static void LearnNandGate()
        {
            Console.WriteLine("=== Learning NAND Gate ===\n");

            // Training data for NAND gate
            double[][] inputs = new double[][]
            {
                new double[] { 0, 0 },  // NOT (0 AND 0) = 1
                new double[] { 0, 1 },  // NOT (0 AND 1) = 1
                new double[] { 1, 0 },  // NOT (1 AND 0) = 1
                new double[] { 1, 1 }   // NOT (1 AND 1) = 0
            };

            int[] labels = new int[] { 1, 1, 1, 0 };

            // Create and train perceptron
            var perceptron = new SimplePerceptron(inputCount: 2, learningRate: 0.1);
            perceptron.Train(inputs, labels, epochs: 100, verbose: true);

            Console.WriteLine("\nFinal weights:");
            perceptron.PrintWeights();

            // Test the trained perceptron
            Console.WriteLine("\nTesting NAND gate:");
            for (int i = 0; i < inputs.Length; i++)
            {
                int prediction = perceptron.Predict(inputs[i]);
                Console.WriteLine($"  NOT ({inputs[i][0]} AND {inputs[i][1]}) = {prediction} (expected: {labels[i]})");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates that XOR cannot be learned by a single perceptron.
        /// XOR is not linearly separable, which is a fundamental limitation
        /// of the perceptron. This limitation led to the development of
        /// multi-layer neural networks.
        /// </summary>
        public static void TryLearnXorGate()
        {
            Console.WriteLine("=== Attempting to Learn XOR Gate (Will Fail) ===\n");
            Console.WriteLine("Note: XOR is NOT linearly separable.");
            Console.WriteLine("A single perceptron cannot learn this function.\n");

            // Training data for XOR gate
            double[][] inputs = new double[][]
            {
                new double[] { 0, 0 },  // 0 XOR 0 = 0
                new double[] { 0, 1 },  // 0 XOR 1 = 1
                new double[] { 1, 0 },  // 1 XOR 0 = 1
                new double[] { 1, 1 }   // 1 XOR 1 = 0
            };

            int[] labels = new int[] { 0, 1, 1, 0 };

            // Create and train perceptron
            var perceptron = new SimplePerceptron(inputCount: 2, learningRate: 0.1);
            perceptron.Train(inputs, labels, epochs: 1000, verbose: true);

            Console.WriteLine("\nFinal weights:");
            perceptron.PrintWeights();

            // Test the trained perceptron
            Console.WriteLine("\nTesting XOR gate:");
            int correctCount = 0;
            for (int i = 0; i < inputs.Length; i++)
            {
                int prediction = perceptron.Predict(inputs[i]);
                bool correct = prediction == labels[i];
                if (correct) correctCount++;
                Console.WriteLine($"  {inputs[i][0]} XOR {inputs[i][1]} = {prediction} (expected: {labels[i]}) {(correct ? "✓" : "✗")}");
            }

            double accuracy = (double)correctCount / inputs.Length * 100;
            Console.WriteLine($"\nAccuracy: {accuracy:F2}% (Maximum achievable: 75% for XOR with single perceptron)");
            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates learning a simple 2D point classification.
        /// Classifies points as being above or below a line.
        /// </summary>
        public static void LearnLinearSeparation()
        {
            Console.WriteLine("=== Learning Linear Separation ===");
            Console.WriteLine("Task: Classify points as above or below the line y = x\n");

            // Training data: points and their classification
            // Label 1: above line (y > x), Label 0: below line (y <= x)
            double[][] inputs = new double[][]
            {
                new double[] { 0, 0 },   // Below
                new double[] { 1, 0 },   // Below
                new double[] { 0, 1 },   // Above
                new double[] { 2, 1 },   // Below
                new double[] { 1, 2 },   // Above
                new double[] { 3, 2 },   // Below
                new double[] { 2, 3 },   // Above
                new double[] { 4, 4 },   // On line (we'll call it below)
                new double[] { 3, 5 },   // Above
                new double[] { 5, 3 }    // Below
            };

            int[] labels = new int[] { 0, 0, 1, 0, 1, 0, 1, 0, 1, 0 };

            // Create and train perceptron
            var perceptron = new SimplePerceptron(inputCount: 2, learningRate: 0.1);
            perceptron.Train(inputs, labels, epochs: 100, verbose: true);

            Console.WriteLine("\nFinal weights:");
            perceptron.PrintWeights();

            // Test with new points
            Console.WriteLine("\nTesting with training data:");
            double accuracy = perceptron.Test(inputs, labels);
            Console.WriteLine($"Training Accuracy: {accuracy:F2}%");

            Console.WriteLine("\nTesting with new points:");
            double[][] testPoints = new double[][]
            {
                new double[] { 1, 1 },   // On line
                new double[] { 2, 4 },   // Above
                new double[] { 4, 2 },   // Below
                new double[] { 5, 5 }    // On line
            };

            foreach (var point in testPoints)
            {
                int prediction = perceptron.Predict(point);
                string position = prediction == 1 ? "above" : "below/on";
                Console.WriteLine($"  Point ({point[0]}, {point[1]}): {position} the line");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Runs all perceptron examples.
        /// </summary>
        public static void RunAllExamples()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║    Simple Perceptron - Neural Network Examples           ║");
            Console.WriteLine("║    Issue #33: Neural Network Example                      ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("This demonstrates the simplest form of a neural network:");
            Console.WriteLine("the Perceptron, invented by Frank Rosenblatt in 1957.");
            Console.WriteLine();

            LearnAndGate();
            Console.WriteLine(new string('─', 60));
            Console.WriteLine();

            LearnOrGate();
            Console.WriteLine(new string('─', 60));
            Console.WriteLine();

            LearnNandGate();
            Console.WriteLine(new string('─', 60));
            Console.WriteLine();

            TryLearnXorGate();
            Console.WriteLine(new string('─', 60));
            Console.WriteLine();

            LearnLinearSeparation();

            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║    Key Takeaways:                                          ║");
            Console.WriteLine("║    1. Perceptrons can learn linearly separable patterns   ║");
            Console.WriteLine("║    2. They cannot learn XOR (not linearly separable)      ║");
            Console.WriteLine("║    3. Multi-layer networks are needed for complex tasks   ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        }
    }
}
