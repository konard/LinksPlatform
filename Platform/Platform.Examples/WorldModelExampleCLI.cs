using System;
using System.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the World Model Example.
    /// </summary>
    /// <remarks>
    /// Provides an interactive demonstration of the world model framework
    /// that combines physical and knowledge domains.
    /// </remarks>
    public static class WorldModelExampleCLI
    {
        /// <summary>
        /// Runs the world model example demonstration.
        /// </summary>
        /// <param name="args">Command-line arguments (optional database path).</param>
        public static void Run(string[] args)
        {
            var databasePath = args.Length > 0 ? args[0] : Path.GetTempFileName();

            Console.WriteLine($"Using database: {databasePath}");
            Console.WriteLine("Initializing Links Platform associative memory...\n");

            using (var memory = new HeapResizableDirectMemory())
            using (var links = new UnitedMemoryLinks<ulong>(memory))
            {
                var example = new WorldModelExample(links);

                try
                {
                    example.RunAllDemos();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError during execution: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
