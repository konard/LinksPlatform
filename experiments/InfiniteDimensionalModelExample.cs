using System;

namespace LinksPlatform.Experiments.Examples
{
    /// <summary>
    /// Demonstrates practical usage of the InfiniteDimensionalModel
    /// as described in issue #524.
    /// </summary>
    public class InfiniteDimensionalModelExample
    {
        public static void Main()
        {
            Console.WriteLine("=== Infinite Dimensional Model Examples ===\n");

            // Example 1: Basic Zero and One models as described in the issue
            BasicZeroAndOne();

            // Example 2: Sparse modifications
            SparseModifications();

            // Example 3: Vector operations
            VectorOperations();

            // Example 4: Practical use case - Image with default background
            ImageWithDefaultBackground();

            Console.WriteLine("\n=== All examples completed ===");
        }

        private static void BasicZeroAndOne()
        {
            Console.WriteLine("--- Example 1: Basic Zero and One ---");

            // Zero can be defined like so: 0, 0, 0, ...
            var zero = new InfiniteDimensionalModel<int>(0);
            Console.WriteLine($"Zero model: {zero}");
            Console.WriteLine($"  Dimension 0: {zero.GetValue(0)}");
            Console.WriteLine($"  Dimension 100: {zero.GetValue(100)}");
            Console.WriteLine($"  Dimension 1000000: {zero.GetValue(1000000)}");

            // One can be defined like so: 1, 1, 1, ...
            var one = new InfiniteDimensionalModel<int>(1);
            Console.WriteLine($"\nOne model: {one}");
            Console.WriteLine($"  Dimension 0: {one.GetValue(0)}");
            Console.WriteLine($"  Dimension 100: {one.GetValue(100)}");
            Console.WriteLine($"  Dimension 1000000: {one.GetValue(1000000)}");

            Console.WriteLine($"\nMemory efficiency: Only storing default values, not infinite dimensions!");
            Console.WriteLine($"  Zero overrides: {zero.OverrideCount}");
            Console.WriteLine($"  One overrides: {one.OverrideCount}");
            Console.WriteLine();
        }

        private static void SparseModifications()
        {
            Console.WriteLine("--- Example 2: Sparse Modifications ---");

            // Start with all zeros
            var model = new InfiniteDimensionalModel<double>(0.0);
            Console.WriteLine($"Initial model: {model}");

            // Calculations needed only on changes to values
            Console.WriteLine("\nModifying specific dimensions...");
            model.SetValue(10, 3.14);
            model.SetValue(100, 2.71);
            model.SetValue(1000, 1.41);

            Console.WriteLine($"After modifications: {model}");
            Console.WriteLine($"  Dimension 10: {model.GetValue(10)}");
            Console.WriteLine($"  Dimension 100: {model.GetValue(100)}");
            Console.WriteLine($"  Dimension 1000: {model.GetValue(1000)}");
            Console.WriteLine($"  Dimension 50 (unchanged): {model.GetValue(50)}");

            Console.WriteLine($"\nOnly {model.OverrideCount} dimensions stored, but infinite dimensions defined!");
            Console.WriteLine();
        }

        private static void VectorOperations()
        {
            Console.WriteLine("--- Example 3: Vector Operations ---");

            // Create two sparse infinite-dimensional vectors
            var vector1 = new InfiniteDimensionalModel<int>(0);
            vector1.SetValue(0, 5);
            vector1.SetValue(1, 10);
            vector1.SetValue(2, 15);

            var vector2 = new InfiniteDimensionalModel<int>(0);
            vector2.SetValue(1, 3);
            vector2.SetValue(2, 7);
            vector2.SetValue(3, 11);

            Console.WriteLine($"Vector 1: Default={vector1.DefaultValue}, Overrides={vector1.OverrideCount}");
            Console.WriteLine($"  [0]={vector1.GetValue(0)}, [1]={vector1.GetValue(1)}, [2]={vector1.GetValue(2)}, [3]={vector1.GetValue(3)}");

            Console.WriteLine($"\nVector 2: Default={vector2.DefaultValue}, Overrides={vector2.OverrideCount}");
            Console.WriteLine($"  [0]={vector2.GetValue(0)}, [1]={vector2.GetValue(1)}, [2]={vector2.GetValue(2)}, [3]={vector2.GetValue(3)}");

            // Add vectors element-wise
            var sum = vector1.OperateWith(vector2, (a, b) => a + b, 0);

            Console.WriteLine($"\nSum: Default={sum.DefaultValue}, Overrides={sum.OverrideCount}");
            Console.WriteLine($"  [0]={sum.GetValue(0)}, [1]={sum.GetValue(1)}, [2]={sum.GetValue(2)}, [3]={sum.GetValue(3)}");

            Console.WriteLine("\nNote: Only non-zero dimensions are calculated and stored!");
            Console.WriteLine();
        }

        private static void ImageWithDefaultBackground()
        {
            Console.WriteLine("--- Example 4: Image with Default Background ---");
            Console.WriteLine("Simulating a large image with a default background color...\n");

            // Imagine a huge image (e.g., 10^9 x 10^9 pixels) with white background
            // We only store the pixels that differ from white
            var image = new InfiniteDimensionalModel<string>("white");

            Console.WriteLine("Drawing a few colored pixels on white background:");
            // Draw some colored pixels (dimension index = y * width + x, for simplicity just use arbitrary indices)
            image.SetValue(100, "red");
            image.SetValue(255, "blue");
            image.SetValue(1000, "green");
            image.SetValue(5000, "red");

            Console.WriteLine($"Pixels drawn: {image.OverrideCount}");
            Console.WriteLine($"  Pixel at 100: {image.GetValue(100)}");
            Console.WriteLine($"  Pixel at 255: {image.GetValue(255)}");
            Console.WriteLine($"  Pixel at 1000: {image.GetValue(1000)}");
            Console.WriteLine($"  Pixel at 500 (background): {image.GetValue(500)}");
            Console.WriteLine($"  Pixel at 999999 (background): {image.GetValue(999999)}");

            Console.WriteLine($"\nMemory usage: Storing only {image.OverrideCount} pixels instead of infinite!");

            // Change background color
            Console.WriteLine("\nChanging background color to 'black'...");
            image.DefaultValue = "black";

            Console.WriteLine($"  Pixel at 100 (custom): {image.GetValue(100)}");
            Console.WriteLine($"  Pixel at 500 (now black): {image.GetValue(500)}");
            Console.WriteLine($"  Pixel at 999999 (now black): {image.GetValue(999999)}");

            Console.WriteLine();
        }
    }
}
