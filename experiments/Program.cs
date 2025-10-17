using System;

namespace Platform.Experiments
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Issue #311: Comparing UInt64Links and Links<ulong>");
            Console.WriteLine("=".PadRight(60, '='));
            Console.WriteLine();

            try
            {
                // Note about the simple demo
                Console.WriteLine("Note: The simplified demo uses dynamic operations which are extremely slow.");
                Console.WriteLine("This exaggerates the performance difference beyond what's realistic.");
                Console.WriteLine("Skipping to focus on actual Links Platform benchmark...");
                // SimpleGenericVsSpecializedDemo.Run();

                Console.WriteLine();
                Console.WriteLine("=".PadRight(60, '='));
                Console.WriteLine();

                // Run the full comparison (if Platform.Data.Doublets is available)
                UInt64LinksVsGenericLinksComparison.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine();
                Console.WriteLine("Note: Some tests require Platform.Data.Doublets package.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Comparison complete. See GenericVsNonGenericComparisonBenchmark.md");
            Console.WriteLine("for detailed analysis and recommendations.");
        }
    }
}
