using System;

namespace Platform.Sandbox.Experiments
{
    /// <summary>
    /// Runner for Copy-On-Write database examples
    /// </summary>
    public static class RunCopyOnWriteExamples
    {
        public static void Run()
        {
            try
            {
                Console.WriteLine("Copy-On-Write Database Implementation");
                Console.WriteLine("=====================================\n");
                Console.WriteLine("This implementation demonstrates the Copy-On-Write principle");
                Console.WriteLine("as described in issue #603.\n");
                Console.WriteLine("Key features:");
                Console.WriteLine("- Two copies of data (one writable, one readable)");
                Console.WriteLine("- Multiple concurrent readers");
                Console.WriteLine("- Single writer with transaction support");
                Console.WriteLine("- Pointer swap after transaction write");
                Console.WriteLine("- Wait for readers before swapping");
                Console.WriteLine("- Transaction replication to second copy");
                Console.WriteLine("- Optimization: Apply pending transactions while waiting\n");
                Console.WriteLine(new string('=', 70) + "\n");

                // Run all examples
                CopyOnWriteDatabaseExample.Run();

                Console.WriteLine("\n" + new string('=', 70) + "\n");

                // Run advanced example
                CopyOnWriteDatabaseExample.AdvancedOptimizationExample();

                Console.WriteLine("\n" + new string('=', 70));
                Console.WriteLine("All examples completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
