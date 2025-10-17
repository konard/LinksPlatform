// Example usage of MatrixEditor for various scenarios
// This demonstrates real-world applications of the matrix editor

using System;
using Platform.Examples;

namespace MatrixEditorExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== MatrixEditor Usage Examples ===\n");

            Example1_TextEditor();
            Example2_SpriteEditor();
            Example3_VoxelEditor();
            Example4_LinkStorage();

            Console.WriteLine("\n=== All Examples Completed ===");
        }

        /// <summary>
        /// Example 1: Simple text editor with fixed-length lines
        /// </summary>
        static void Example1_TextEditor()
        {
            Console.WriteLine("Example 1: Text Editor (10 lines, 80 chars per line)");

            var textEditor = new MatrixEditor<char>(10, 80);

            // Write some text
            string text = "Hello, World!";
            for (int i = 0; i < text.Length; i++)
            {
                textEditor[0, i] = text[i];
            }

            // Copy a selection to another line
            var selection = textEditor.GetSquareSelection(new[] { 0, 0 }, new[] { 0, 12 });
            textEditor.SetSquareSelection(new[] { 2, 5 }, selection);

            Console.WriteLine("  Line 0: " + GetLine(textEditor, 0, 20));
            Console.WriteLine("  Line 2: " + GetLine(textEditor, 2, 20));
            Console.WriteLine();
        }

        /// <summary>
        /// Example 2: 2D sprite editor (like a pixel art editor)
        /// </summary>
        static void Example2_SpriteEditor()
        {
            Console.WriteLine("Example 2: Sprite Editor (8x8 pixels)");

            var sprite = new MatrixEditor<int>(8, 8);

            // Draw a simple smiley face
            // 1 = black, 0 = white
            sprite[2, 2] = 1; // left eye
            sprite[2, 5] = 1; // right eye
            sprite[5, 2] = 1; // mouth left
            sprite[5, 3] = 1;
            sprite[5, 4] = 1;
            sprite[5, 5] = 1; // mouth right

            Console.WriteLine("  Sprite:");
            PrintSprite(sprite);
            Console.WriteLine();

            // Copy eye to make it bigger
            var eyeSelection = sprite.GetSquareSelection(new[] { 2, 2 }, new[] { 2, 2 });
            sprite.SetSquareSelection(new[] { 3, 2 }, eyeSelection);
            sprite.SetSquareSelection(new[] { 3, 5 }, eyeSelection);

            Console.WriteLine("  Sprite with bigger eyes:");
            PrintSprite(sprite);
            Console.WriteLine();
        }

        /// <summary>
        /// Example 3: 3D voxel editor (like Minecraft)
        /// </summary>
        static void Example3_VoxelEditor()
        {
            Console.WriteLine("Example 3: Voxel Editor (4x4x4 space)");

            var voxels = new MatrixEditor<int>(4, 4, 4);

            // Build a simple structure
            // 0 = air, 1 = stone, 2 = wood, 3 = glass
            for (int x = 0; x < 4; x++)
            {
                for (int z = 0; z < 4; z++)
                {
                    voxels[x, 0, z] = 1; // stone floor
                }
            }

            // Add walls
            for (int y = 1; y < 3; y++)
            {
                voxels[0, y, 0] = 2; // corner post
                voxels[3, y, 0] = 2;
                voxels[0, y, 3] = 2;
                voxels[3, y, 3] = 2;
            }

            // Add glass roof
            for (int x = 0; x < 4; x++)
            {
                for (int z = 0; z < 4; z++)
                {
                    voxels[x, 3, z] = 3;
                }
            }

            Console.WriteLine($"  Total blocks placed: {CountNonZero(voxels)}");
            Console.WriteLine($"  Stone blocks (1): {CountValue(voxels, 1)}");
            Console.WriteLine($"  Wood blocks (2): {CountValue(voxels, 2)}");
            Console.WriteLine($"  Glass blocks (3): {CountValue(voxels, 3)}");
            Console.WriteLine();
        }

        /// <summary>
        /// Example 4: Multi-dimensional link storage
        /// Links represented as integers (link IDs)
        /// </summary>
        static void Example4_LinkStorage()
        {
            Console.WriteLine("Example 4: Link Storage (5x5 matrix)");

            var linkMatrix = new MatrixEditor<ulong>(5, 5);

            // Store link IDs in a pattern
            linkMatrix[0, 0] = 1001;
            linkMatrix[0, 1] = 1002;
            linkMatrix[1, 0] = 2001;
            linkMatrix[1, 1] = 2002;

            // Create a subgraph selection
            var subgraph = linkMatrix.GetSquareSelection(new[] { 0, 0 }, new[] { 1, 1 });

            Console.WriteLine("  Link matrix:");
            Console.WriteLine($"    [0, 0] = {linkMatrix[0, 0]}");
            Console.WriteLine($"    [0, 1] = {linkMatrix[0, 1]}");
            Console.WriteLine($"    [1, 0] = {linkMatrix[1, 0]}");
            Console.WriteLine($"    [1, 1] = {linkMatrix[1, 1]}");
            Console.WriteLine();

            Console.WriteLine("  Subgraph selection:");
            Console.WriteLine($"    [0, 0] = {subgraph[0, 0]}");
            Console.WriteLine($"    [0, 1] = {subgraph[0, 1]}");
            Console.WriteLine($"    [1, 0] = {subgraph[1, 0]}");
            Console.WriteLine($"    [1, 1] = {subgraph[1, 1]}");
            Console.WriteLine();

            // Replicate the pattern elsewhere
            linkMatrix.SetSquareSelection(new[] { 3, 3 }, subgraph);
            Console.WriteLine($"  Replicated at [3, 3]: {linkMatrix[3, 3]}");
            Console.WriteLine($"  Replicated at [4, 4]: {linkMatrix[4, 4]}");
            Console.WriteLine();
        }

        // Helper methods

        static string GetLine(MatrixEditor<char> matrix, int row, int length)
        {
            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = matrix[row, i];
                if (chars[i] == '\0')
                    chars[i] = ' ';
            }
            return new string(chars);
        }

        static void PrintSprite(MatrixEditor<int> sprite)
        {
            var dims = sprite.Dimensions;
            for (int i = 0; i < dims[0]; i++)
            {
                Console.Write("    ");
                for (int j = 0; j < dims[1]; j++)
                {
                    Console.Write(sprite[i, j] == 0 ? "." : "#");
                }
                Console.WriteLine();
            }
        }

        static int CountNonZero(MatrixEditor<int> matrix)
        {
            int count = 0;
            var dims = matrix.Dimensions;
            if (dims.Length == 3)
            {
                for (int x = 0; x < dims[0]; x++)
                {
                    for (int y = 0; y < dims[1]; y++)
                    {
                        for (int z = 0; z < dims[2]; z++)
                        {
                            if (matrix[x, y, z] != 0)
                                count++;
                        }
                    }
                }
            }
            return count;
        }

        static int CountValue(MatrixEditor<int> matrix, int value)
        {
            int count = 0;
            var dims = matrix.Dimensions;
            if (dims.Length == 3)
            {
                for (int x = 0; x < dims[0]; x++)
                {
                    for (int y = 0; y < dims[1]; y++)
                    {
                        for (int z = 0; z < dims[2]; z++)
                        {
                            if (matrix[x, y, z] == value)
                                count++;
                        }
                    }
                }
            }
            return count;
        }
    }
}
