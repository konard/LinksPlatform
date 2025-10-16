using System;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;

namespace Platform.Sandbox
{
    /// <summary>
    /// Command-line interface for demonstrating binary number addition using Links.
    /// This provides an interactive way to test the addition implementation.
    /// </summary>
    public static class BinaryNumberAdditionCLI
    {
        public static void Run()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  Binary Number Addition using Links Platform                  ║");
            Console.WriteLine("║  Solution for Issue #82: Addition of numbers (eg. 2+2)        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            while (true)
            {
                Console.WriteLine("Choose an option:");
                Console.WriteLine("  1. Run demo (2+2 and other examples)");
                Console.WriteLine("  2. Interactive mode (enter your own numbers)");
                Console.WriteLine("  3. Exit");
                Console.Write("Your choice: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine();
                        BinaryNumberAddition.Demo();
                        break;

                    case "2":
                        Console.WriteLine();
                        RunInteractive();
                        break;

                    case "3":
                        Console.WriteLine("Goodbye!");
                        return;

                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        Console.WriteLine();
                        break;
                }
            }
        }

        private static void RunInteractive()
        {
            Console.WriteLine("=== Interactive Binary Addition ===");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Enter first number (or 'back' to return): ");
                var input1 = Console.ReadLine();

                if (input1?.ToLower() == "back")
                {
                    Console.WriteLine();
                    return;
                }

                if (!int.TryParse(input1, out int num1) || num1 < 0)
                {
                    Console.WriteLine("Please enter a valid non-negative integer.");
                    Console.WriteLine();
                    continue;
                }

                Console.Write("Enter second number: ");
                var input2 = Console.ReadLine();

                if (!int.TryParse(input2, out int num2) || num2 < 0)
                {
                    Console.WriteLine("Please enter a valid non-negative integer.");
                    Console.WriteLine();
                    continue;
                }

                Console.WriteLine();
                Console.WriteLine($"Computing {num1} + {num2} using Links...");

                try
                {
                    using (var memory = new Platform.Memory.HeapResizableDirectMemory(4 * 1024 * 1024))
                    using (var memoryManager = new Platform.Data.Doublets.Memory.United.Specific.UInt64UnitedMemoryLinks(memory))
                    using (var links = new UInt64Links(memoryManager))
                    {
                        var calculator = new BinaryNumberAddition(links);

                        var result = calculator.AddAndGetResult(num1, num2);

                        Console.WriteLine($"Result: {num1} + {num2} = {result}");
                        Console.WriteLine($"Links used: {links.Count()}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during calculation: {ex.Message}");
                }

                Console.WriteLine();
            }
        }
    }
}
