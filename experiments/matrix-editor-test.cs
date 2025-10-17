// Test script for MatrixEditor functionality
// This script can be used to verify the matrix editor implementation

using System;
using Platform.Examples;

namespace MatrixEditorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== MatrixEditor Test Suite ===\n");

            Test1DMatrix();
            Test2DMatrix();
            Test3DMatrix();
            TestSquareSelection();
            TestClearOperations();
            TestEdgeCases();

            Console.WriteLine("\n=== All Tests Completed ===");
        }

        static void Test1DMatrix()
        {
            Console.WriteLine("Test 1: 1D Matrix");
            var matrix = new MatrixEditor<int>(10);

            matrix[0] = 1;
            matrix[5] = 5;
            matrix[9] = 9;

            Console.WriteLine($"  [0] = {matrix[0]} (expected: 1)");
            Console.WriteLine($"  [5] = {matrix[5]} (expected: 5)");
            Console.WriteLine($"  [9] = {matrix[9]} (expected: 9)");
            Console.WriteLine($"  [3] = {matrix[3]} (expected: 0)");
            Console.WriteLine();
        }

        static void Test2DMatrix()
        {
            Console.WriteLine("Test 2: 2D Matrix");
            var matrix = new MatrixEditor<int>(5, 5);

            // Set diagonal
            for (int i = 0; i < 5; i++)
            {
                matrix[i, i] = i + 1;
            }

            Console.WriteLine("  Diagonal values:");
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"    [{i}, {i}] = {matrix[i, i]} (expected: {i + 1})");
            }
            Console.WriteLine();
        }

        static void Test3DMatrix()
        {
            Console.WriteLine("Test 3: 3D Matrix");
            var matrix = new MatrixEditor<int>(3, 3, 3);

            matrix[0, 0, 0] = 1;
            matrix[1, 1, 1] = 2;
            matrix[2, 2, 2] = 3;
            matrix[0, 1, 2] = 4;

            Console.WriteLine($"  [0, 0, 0] = {matrix[0, 0, 0]} (expected: 1)");
            Console.WriteLine($"  [1, 1, 1] = {matrix[1, 1, 1]} (expected: 2)");
            Console.WriteLine($"  [2, 2, 2] = {matrix[2, 2, 2]} (expected: 3)");
            Console.WriteLine($"  [0, 1, 2] = {matrix[0, 1, 2]} (expected: 4)");
            Console.WriteLine();
        }

        static void TestSquareSelection()
        {
            Console.WriteLine("Test 4: Square Selection");
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

            Console.WriteLine("  Selection [0,0] to [1,1]:");
            Console.WriteLine($"    [0, 0] = {selection[0, 0]} (expected: 11)");
            Console.WriteLine($"    [0, 1] = {selection[0, 1]} (expected: 12)");
            Console.WriteLine($"    [1, 0] = {selection[1, 0]} (expected: 21)");
            Console.WriteLine($"    [1, 1] = {selection[1, 1]} (expected: 22)");

            // Test setting a selection
            var newSelection = new MatrixEditor<int>(2, 2);
            newSelection[0, 0] = 99;
            newSelection[1, 1] = 88;

            matrix.SetSquareSelection(new[] { 3, 3 }, newSelection);
            Console.WriteLine($"  After SetSquareSelection at [3,3]:");
            Console.WriteLine($"    [3, 3] = {matrix[3, 3]} (expected: 99)");
            Console.WriteLine($"    [4, 4] = {matrix[4, 4]} (expected: 88)");
            Console.WriteLine();
        }

        static void TestClearOperations()
        {
            Console.WriteLine("Test 5: Clear Operations");
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

            Console.WriteLine("  After clearing region [1,1] to [3,3]:");
            Console.WriteLine($"    [0, 0] = {matrix[0, 0]} (expected: 1)");
            Console.WriteLine($"    [2, 2] = {matrix[2, 2]} (expected: 0)");
            Console.WriteLine($"    [4, 4] = {matrix[4, 4]} (expected: 1)");
            Console.WriteLine();
        }

        static void TestEdgeCases()
        {
            Console.WriteLine("Test 6: Edge Cases");

            // Test with char type
            var charMatrix = new MatrixEditor<char>(3, 3);
            charMatrix[0, 0] = 'A';
            charMatrix[1, 1] = 'B';
            charMatrix[2, 2] = 'C';

            Console.WriteLine("  Char matrix:");
            Console.WriteLine($"    [0, 0] = '{charMatrix[0, 0]}' (expected: 'A')");
            Console.WriteLine($"    [1, 1] = '{charMatrix[1, 1]}' (expected: 'B')");
            Console.WriteLine($"    [2, 2] = '{charMatrix[2, 2]}' (expected: 'C')");
            Console.WriteLine($"    [0, 1] = '{charMatrix[0, 1]}' (expected: '\\0')");
            Console.WriteLine();
        }
    }
}
