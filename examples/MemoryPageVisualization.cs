using System;
using Platform.Examples;

namespace Examples
{
    /// <summary>
    /// Example demonstrating memory page access visualization for Links database operations.
    ///
    /// This example shows how to:
    /// 1. Track memory page accesses during database operations
    /// 2. Generate visual heatmaps of memory access patterns
    /// 3. Export detailed access statistics for analysis
    /// 4. Identify memory hotspots and optimize data structures
    ///
    /// Usage:
    ///   dotnet run
    ///   dotnet run database.links 50000 output.csv
    /// </summary>
    class MemoryPageVisualizationExample
    {
        static void Main(string[] args)
        {
            var cli = new MemoryPageAccessVisualizerCLI();
            cli.Run(args);
        }
    }
}
