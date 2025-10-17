using System;
using Platform.Examples;

namespace Platform.Sandbox
{
    /// <summary>
    /// Test suite for MatrixEditor functionality
    /// </summary>
    public static class MatrixEditorTest
    {
        public static void Run()
        {
            Console.WriteLine("=== MatrixEditor Test Suite ===\n");

            try
            {
                Test1DMatrix();
                Test2DMatrix();
                Test3DMatrix();
                Test4DMatrix();
                TestSquareSelection();
                TestSetSquareSelection();
                TestClearOperations();
                TestWithDifferentTypes();
                TestEdgeCases();

                Console.WriteLine("\n=== All MatrixEditor Tests Passed ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n=== Test Failed: {ex.Message} ===");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void Test1DMatrix()
        {
            Console.WriteLine("Test 1: 1D Matrix (Fixed-length line)");
            var matrix = new MatrixEditor<int>(10);

            // Test setting values
            matrix[0] = 1;
            matrix[5] = 5;
            matrix[9] = 9;

            AssertEqual(matrix[0], 1, "[0]");
            AssertEqual(matrix[5], 5, "[5]");
            AssertEqual(matrix[9], 9, "[9]");
            AssertEqual(matrix[3], 0, "[3] (should be 0 by default)");

            Console.WriteLine("  ✓ 1D matrix tests passed");
            Console.WriteLine();
        }

        private static void Test2DMatrix()
        {
            Console.WriteLine("Test 2: 2D Matrix (Fixed-length lines)");
            var matrix = new MatrixEditor<int>(5, 5);

            // Set diagonal
            for (int i = 0; i < 5; i++)
            {
                matrix[i, i] = i + 1;
            }

            // Verify diagonal
            for (int i = 0; i < 5; i++)
            {
                AssertEqual(matrix[i, i], i + 1, $"[{i}, {i}]");
            }

            // Verify non-diagonal is zero
            AssertEqual(matrix[0, 1], 0, "[0, 1] (should be 0)");
            AssertEqual(matrix[1, 0], 0, "[1, 0] (should be 0)");

            Console.WriteLine("  ✓ 2D matrix tests passed");
            Console.WriteLine();
        }

        private static void Test3DMatrix()
        {
            Console.WriteLine("Test 3: 3D Matrix");
            var matrix = new MatrixEditor<int>(3, 3, 3);

            matrix[0, 0, 0] = 1;
            matrix[1, 1, 1] = 2;
            matrix[2, 2, 2] = 3;
            matrix[0, 1, 2] = 4;

            AssertEqual(matrix[0, 0, 0], 1, "[0, 0, 0]");
            AssertEqual(matrix[1, 1, 1], 2, "[1, 1, 1]");
            AssertEqual(matrix[2, 2, 2], 3, "[2, 2, 2]");
            AssertEqual(matrix[0, 1, 2], 4, "[0, 1, 2]");
            AssertEqual(matrix[0, 0, 1], 0, "[0, 0, 1] (should be 0)");

            Console.WriteLine("  ✓ 3D matrix tests passed");
            Console.WriteLine();
        }

        private static void Test4DMatrix()
        {
            Console.WriteLine("Test 4: 4D Matrix");
            var matrix = new MatrixEditor<int>(2, 2, 2, 2);

            matrix[0, 0, 0, 0] = 100;
            matrix[1, 1, 1, 1] = 200;
            matrix[0, 1, 0, 1] = 300;

            AssertEqual(matrix[0, 0, 0, 0], 100, "[0, 0, 0, 0]");
            AssertEqual(matrix[1, 1, 1, 1], 200, "[1, 1, 1, 1]");
            AssertEqual(matrix[0, 1, 0, 1], 300, "[0, 1, 0, 1]");
            AssertEqual(matrix.TotalSize, 16, "TotalSize");

            Console.WriteLine("  ✓ 4D matrix tests passed");
            Console.WriteLine();
        }

        private static void TestSquareSelection()
        {
            Console.WriteLine("Test 5: Square Selection (GetSquareSelection)");
            var matrix = new MatrixEditor<int>(5, 5);

            // Fill a 3x3 area
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    matrix[i, j] = (i + 1) * 10 + (j + 1);
                }
            }

            // Get a 2x2 selection
            var selection = matrix.GetSquareSelection(new[] { 0, 0 }, new[] { 1, 1 });

            AssertEqual(selection[0, 0], 11, "selection[0, 0]");
            AssertEqual(selection[0, 1], 12, "selection[0, 1]");
            AssertEqual(selection[1, 0], 21, "selection[1, 0]");
            AssertEqual(selection[1, 1], 22, "selection[1, 1]");

            var dims = selection.Dimensions;
            AssertEqual(dims[0], 2, "selection.Dimensions[0]");
            AssertEqual(dims[1], 2, "selection.Dimensions[1]");

            Console.WriteLine("  ✓ Square selection tests passed");
            Console.WriteLine();
        }

        private static void TestSetSquareSelection()
        {
            Console.WriteLine("Test 6: Set Square Selection (SetSquareSelection)");
            var matrix = new MatrixEditor<int>(5, 5);

            // Create a 2x2 pattern
            var pattern = new MatrixEditor<int>(2, 2);
            pattern[0, 0] = 99;
            pattern[0, 1] = 88;
            pattern[1, 0] = 77;
            pattern[1, 1] = 66;

            // Set the pattern at different positions
            matrix.SetSquareSelection(new[] { 0, 0 }, pattern);
            matrix.SetSquareSelection(new[] { 3, 3 }, pattern);

            // Verify first position
            AssertEqual(matrix[0, 0], 99, "[0, 0]");
            AssertEqual(matrix[0, 1], 88, "[0, 1]");
            AssertEqual(matrix[1, 0], 77, "[1, 0]");
            AssertEqual(matrix[1, 1], 66, "[1, 1]");

            // Verify second position
            AssertEqual(matrix[3, 3], 99, "[3, 3]");
            AssertEqual(matrix[3, 4], 88, "[3, 4]");
            AssertEqual(matrix[4, 3], 77, "[4, 3]");
            AssertEqual(matrix[4, 4], 66, "[4, 4]");

            // Verify unaffected cells are still zero
            AssertEqual(matrix[2, 2], 0, "[2, 2] (should be 0)");

            Console.WriteLine("  ✓ Set square selection tests passed");
            Console.WriteLine();
        }

        private static void TestClearOperations()
        {
            Console.WriteLine("Test 7: Clear Operations");
            var matrix = new MatrixEditor<int>(5, 5);

            // Fill matrix
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    matrix[i, j] = 1;
                }
            }

            // Clear a region
            matrix.ClearSquareRegion(new[] { 1, 1 }, new[] { 3, 3 });

            // Verify cleared region
            AssertEqual(matrix[2, 2], 0, "[2, 2] (should be cleared)");
            AssertEqual(matrix[1, 1], 0, "[1, 1] (should be cleared)");
            AssertEqual(matrix[3, 3], 0, "[3, 3] (should be cleared)");

            // Verify non-cleared cells
            AssertEqual(matrix[0, 0], 1, "[0, 0] (should not be cleared)");
            AssertEqual(matrix[4, 4], 1, "[4, 4] (should not be cleared)");

            // Clear all
            matrix.Clear();
            AssertEqual(matrix[0, 0], 0, "[0, 0] (should be cleared)");
            AssertEqual(matrix[4, 4], 0, "[4, 4] (should be cleared)");

            Console.WriteLine("  ✓ Clear operations tests passed");
            Console.WriteLine();
        }

        private static void TestWithDifferentTypes()
        {
            Console.WriteLine("Test 8: Different Types");

            // Test with char (for text editor)
            var charMatrix = new MatrixEditor<char>(3, 3);
            charMatrix[0, 0] = 'A';
            charMatrix[1, 1] = 'B';
            charMatrix[2, 2] = 'C';

            AssertEqual(charMatrix[0, 0], 'A', "char[0, 0]");
            AssertEqual(charMatrix[1, 1], 'B', "char[1, 1]");
            AssertEqual(charMatrix[2, 2], 'C', "char[2, 2]");
            AssertEqual(charMatrix[0, 1], '\0', "char[0, 1] (should be null char)");

            // Test with ulong (for link IDs)
            var ulongMatrix = new MatrixEditor<ulong>(2, 2);
            ulongMatrix[0, 0] = 1000000UL;
            ulongMatrix[1, 1] = 2000000UL;

            AssertEqual(ulongMatrix[0, 0], 1000000UL, "ulong[0, 0]");
            AssertEqual(ulongMatrix[1, 1], 2000000UL, "ulong[1, 1]");
            AssertEqual(ulongMatrix[0, 1], 0UL, "ulong[0, 1] (should be 0)");

            Console.WriteLine("  ✓ Different types tests passed");
            Console.WriteLine();
        }

        private static void TestEdgeCases()
        {
            Console.WriteLine("Test 9: Edge Cases");

            // Test minimum dimensions
            var tiny = new MatrixEditor<int>(1);
            tiny[0] = 42;
            AssertEqual(tiny[0], 42, "1D tiny matrix");

            var tiny2D = new MatrixEditor<int>(1, 1);
            tiny2D[0, 0] = 43;
            AssertEqual(tiny2D[0, 0], 43, "2D tiny matrix");

            // Test large dimensions count
            var multiDim = new MatrixEditor<int>(2, 2, 2, 2, 2);
            multiDim[0, 0, 0, 0, 0] = 5;
            multiDim[1, 1, 1, 1, 1] = 10;
            AssertEqual(multiDim[0, 0, 0, 0, 0], 5, "5D matrix[0,0,0,0,0]");
            AssertEqual(multiDim[1, 1, 1, 1, 1], 10, "5D matrix[1,1,1,1,1]");
            AssertEqual(multiDim.TotalSize, 32, "5D matrix TotalSize");

            Console.WriteLine("  ✓ Edge cases tests passed");
            Console.WriteLine();
        }

        private static void AssertEqual<T>(T actual, T expected, string message)
        {
            if (!actual.Equals(expected))
            {
                throw new Exception($"Assertion failed for {message}: expected {expected}, got {actual}");
            }
        }
    }
}
