using System;

namespace LinksPlatform.Experiments
{
    /// <summary>
    /// Simple program to run the Price History Event Mapping example
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                PriceHistoryEventMapping.RunExample();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            if (Environment.UserInteractive && !Console.IsInputRedirected)
            {
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
