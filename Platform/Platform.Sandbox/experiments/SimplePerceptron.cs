using System;

namespace Platform.Sandbox.Experiments
{
    /// <summary>
    /// Simple perceptron implementation for binary classification.
    /// This is one of the simplest forms of neural networks, demonstrating
    /// basic machine learning concepts as suggested in issue #33.
    ///
    /// Based on the classic perceptron algorithm:
    /// - Learns linearly separable patterns
    /// - Uses step activation function
    /// - Updates weights based on prediction errors
    ///
    /// References:
    /// - https://en.wikipedia.org/wiki/Perceptron
    /// - "ЭВМ видит мир" (Александров В., Горский Н.Д.)
    /// </summary>
    public class SimplePerceptron
    {
        private double[] _weights;
        private double _bias;
        private readonly double _learningRate;
        private readonly System.Random _random;

        /// <summary>
        /// Creates a new perceptron with the specified number of inputs and learning rate.
        /// </summary>
        /// <param name="inputCount">Number of input features</param>
        /// <param name="learningRate">Learning rate (typically 0.01 to 0.1)</param>
        public SimplePerceptron(int inputCount, double learningRate = 0.1)
        {
            _weights = new double[inputCount];
            _learningRate = learningRate;
            _random = new System.Random(42); // Fixed seed for reproducibility

            // Initialize weights with small random values
            for (int i = 0; i < _weights.Length; i++)
            {
                _weights[i] = _random.NextDouble() * 0.2 - 0.1; // Range: -0.1 to 0.1
            }
            _bias = _random.NextDouble() * 0.2 - 0.1;
        }

        /// <summary>
        /// Activation function: Heaviside step function.
        /// Returns 1 if input >= 0, otherwise returns 0.
        /// </summary>
        private int Activation(double sum)
        {
            return sum >= 0 ? 1 : 0;
        }

        /// <summary>
        /// Makes a prediction for the given input.
        /// </summary>
        /// <param name="inputs">Input feature vector</param>
        /// <returns>Binary prediction (0 or 1)</returns>
        public int Predict(double[] inputs)
        {
            if (inputs.Length != _weights.Length)
            {
                throw new ArgumentException($"Expected {_weights.Length} inputs, got {inputs.Length}");
            }

            double sum = _bias;
            for (int i = 0; i < inputs.Length; i++)
            {
                sum += inputs[i] * _weights[i];
            }

            return Activation(sum);
        }

        /// <summary>
        /// Trains the perceptron on a single example.
        /// Updates weights when prediction differs from expected output.
        /// </summary>
        /// <param name="inputs">Input feature vector</param>
        /// <param name="expectedOutput">Expected output (0 or 1)</param>
        /// <returns>True if prediction was correct, false otherwise</returns>
        public bool Train(double[] inputs, int expectedOutput)
        {
            int prediction = Predict(inputs);
            int error = expectedOutput - prediction;

            if (error != 0)
            {
                // Update weights: w = w + learningRate * error * input
                for (int i = 0; i < _weights.Length; i++)
                {
                    _weights[i] += _learningRate * error * inputs[i];
                }
                _bias += _learningRate * error;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Trains the perceptron on a dataset for multiple epochs.
        /// </summary>
        /// <param name="trainingData">Array of input vectors</param>
        /// <param name="labels">Array of expected outputs</param>
        /// <param name="epochs">Number of training iterations</param>
        /// <param name="verbose">If true, prints training progress</param>
        public void Train(double[][] trainingData, int[] labels, int epochs = 100, bool verbose = false)
        {
            if (trainingData.Length != labels.Length)
            {
                throw new ArgumentException("Training data and labels must have the same length");
            }

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                int correctCount = 0;

                for (int i = 0; i < trainingData.Length; i++)
                {
                    if (Train(trainingData[i], labels[i]))
                    {
                        correctCount++;
                    }
                }

                if (verbose && (epoch % 10 == 0 || epoch == epochs - 1))
                {
                    double accuracy = (double)correctCount / trainingData.Length * 100;
                    Console.WriteLine($"Epoch {epoch + 1}/{epochs}: Accuracy = {accuracy:F2}%");
                }

                // Early stopping if all predictions are correct
                if (correctCount == trainingData.Length)
                {
                    if (verbose)
                    {
                        Console.WriteLine($"Perfect accuracy reached at epoch {epoch + 1}!");
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Prints the current weights and bias of the perceptron.
        /// </summary>
        public void PrintWeights()
        {
            Console.WriteLine("Perceptron Weights:");
            for (int i = 0; i < _weights.Length; i++)
            {
                Console.WriteLine($"  w[{i}] = {_weights[i]:F4}");
            }
            Console.WriteLine($"  bias = {_bias:F4}");
        }

        /// <summary>
        /// Tests the perceptron accuracy on a test dataset.
        /// </summary>
        /// <param name="testData">Array of input vectors</param>
        /// <param name="labels">Array of expected outputs</param>
        /// <returns>Accuracy as a percentage (0-100)</returns>
        public double Test(double[][] testData, int[] labels)
        {
            if (testData.Length != labels.Length)
            {
                throw new ArgumentException("Test data and labels must have the same length");
            }

            int correctCount = 0;
            for (int i = 0; i < testData.Length; i++)
            {
                int prediction = Predict(testData[i]);
                if (prediction == labels[i])
                {
                    correctCount++;
                }
            }

            return (double)correctCount / testData.Length * 100;
        }
    }
}
