using System;
using System.Linq;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the MatrixEditor.
    /// Provides interactive editing of multi-dimensional matrices with support for
    /// fixed-length lines, square selections, and symbols/links per cell.
    /// </summary>
    public class MatrixEditorCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            Console.WriteLine("=== Matrix Editor ===");
            Console.WriteLine("A multi-dimensional matrix editor with support for:");
            Console.WriteLine("- Fixed-length lines");
            Console.WriteLine("- Square (rectangular) selections");
            Console.WriteLine("- Symbols or links in cells");
            Console.WriteLine("- Multiple dimensions");
            Console.WriteLine();

            try
            {
                // Default: Create a 2D matrix for text editing
                int rows = 10;
                int cols = 20;

                // Parse command line arguments if provided
                if (args.Length >= 2)
                {
                    if (int.TryParse(args[0], out int r) && int.TryParse(args[1], out int c))
                    {
                        rows = r;
                        cols = c;
                    }
                }

                Console.WriteLine($"Creating a {rows}x{cols} matrix (0 = empty cell)");
                var matrix = new MatrixEditor<int>(rows, cols);

                Console.WriteLine("\nCommands:");
                Console.WriteLine("  set <row> <col> <value>  - Set a cell value");
                Console.WriteLine("  get <row> <col>          - Get a cell value");
                Console.WriteLine("  select <r1> <c1> <r2> <c2> - Get square selection");
                Console.WriteLine("  clear <r1> <c1> <r2> <c2>  - Clear square region");
                Console.WriteLine("  clearall                 - Clear entire matrix");
                Console.WriteLine("  print                    - Print the matrix");
                Console.WriteLine("  example                  - Run examples");
                Console.WriteLine("  quit                     - Exit");
                Console.WriteLine();

                while (true)
                {
                    Console.Write("> ");
                    var input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        continue;
                    }

                    var parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var command = parts[0].ToLower();

                    try
                    {
                        switch (command)
                        {
                            case "set":
                                if (parts.Length >= 4)
                                {
                                    int row = int.Parse(parts[1]);
                                    int col = int.Parse(parts[2]);
                                    int value = int.Parse(parts[3]);
                                    matrix[row, col] = value;
                                    Console.WriteLine($"Set [{row}, {col}] = {value}");
                                }
                                else
                                {
                                    Console.WriteLine("Usage: set <row> <col> <value>");
                                }
                                break;

                            case "get":
                                if (parts.Length >= 3)
                                {
                                    int row = int.Parse(parts[1]);
                                    int col = int.Parse(parts[2]);
                                    int value = matrix[row, col];
                                    Console.WriteLine($"[{row}, {col}] = {value}");
                                }
                                else
                                {
                                    Console.WriteLine("Usage: get <row> <col>");
                                }
                                break;

                            case "select":
                                if (parts.Length >= 5)
                                {
                                    int r1 = int.Parse(parts[1]);
                                    int c1 = int.Parse(parts[2]);
                                    int r2 = int.Parse(parts[3]);
                                    int c2 = int.Parse(parts[4]);
                                    var selection = matrix.GetSquareSelection(new[] { r1, c1 }, new[] { r2, c2 });
                                    Console.WriteLine($"Selection [{r1}, {c1}] to [{r2}, {c2}]:");
                                    Console.WriteLine(selection);
                                }
                                else
                                {
                                    Console.WriteLine("Usage: select <r1> <c1> <r2> <c2>");
                                }
                                break;

                            case "clear":
                                if (parts.Length >= 5)
                                {
                                    int r1 = int.Parse(parts[1]);
                                    int c1 = int.Parse(parts[2]);
                                    int r2 = int.Parse(parts[3]);
                                    int c2 = int.Parse(parts[4]);
                                    matrix.ClearSquareRegion(new[] { r1, c1 }, new[] { r2, c2 });
                                    Console.WriteLine($"Cleared region [{r1}, {c1}] to [{r2}, {c2}]");
                                }
                                else
                                {
                                    Console.WriteLine("Usage: clear <r1> <c1> <r2> <c2>");
                                }
                                break;

                            case "clearall":
                                matrix.Clear();
                                Console.WriteLine("Matrix cleared.");
                                break;

                            case "print":
                                Console.WriteLine(matrix);
                                break;

                            case "example":
                                RunExamples();
                                break;

                            case "quit":
                            case "exit":
                                Console.WriteLine("Goodbye!");
                                return;

                            default:
                                Console.WriteLine($"Unknown command: {command}");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void RunExamples()
        {
            Console.WriteLine("\n=== Running Examples ===\n");

            // Example 1: 2D matrix with text
            Console.WriteLine("Example 1: 2D Matrix (5x10)");
            var matrix2D = new MatrixEditor<int>(5, 10);

            // Set some values
            matrix2D[0, 0] = 72;  // 'H'
            matrix2D[0, 1] = 69;  // 'E'
            matrix2D[0, 2] = 76;  // 'L'
            matrix2D[0, 3] = 76;  // 'L'
            matrix2D[0, 4] = 79;  // 'O'

            Console.WriteLine("Initial matrix:");
            Console.WriteLine(matrix2D);
            Console.WriteLine();

            // Example 2: Square selection
            Console.WriteLine("Example 2: Square Selection");
            var selection = matrix2D.GetSquareSelection(new[] { 0, 0 }, new[] { 0, 4 });
            Console.WriteLine("Selection [0,0] to [0,4]:");
            Console.WriteLine(selection);
            Console.WriteLine();

            // Example 3: 3D matrix
            Console.WriteLine("Example 3: 3D Matrix (3x3x3)");
            var matrix3D = new MatrixEditor<int>(3, 3, 3);
            matrix3D[0, 0, 0] = 1;
            matrix3D[1, 1, 1] = 2;
            matrix3D[2, 2, 2] = 3;
            Console.WriteLine($"Matrix dimensions: {string.Join("x", matrix3D.Dimensions)}");
            Console.WriteLine($"[0,0,0] = {matrix3D[0, 0, 0]}");
            Console.WriteLine($"[1,1,1] = {matrix3D[1, 1, 1]}");
            Console.WriteLine($"[2,2,2] = {matrix3D[2, 2, 2]}");
            Console.WriteLine();

            // Example 4: 4D matrix
            Console.WriteLine("Example 4: 4D Matrix (2x2x2x2)");
            var matrix4D = new MatrixEditor<int>(2, 2, 2, 2);
            matrix4D[0, 0, 0, 0] = 100;
            matrix4D[1, 1, 1, 1] = 200;
            Console.WriteLine($"Matrix dimensions: {string.Join("x", matrix4D.Dimensions)}");
            Console.WriteLine($"Total cells: {matrix4D.TotalSize}");
            Console.WriteLine($"[0,0,0,0] = {matrix4D[0, 0, 0, 0]}");
            Console.WriteLine($"[1,1,1,1] = {matrix4D[1, 1, 1, 1]}");
            Console.WriteLine();

            Console.WriteLine("=== Examples Complete ===\n");
        }
    }
}
