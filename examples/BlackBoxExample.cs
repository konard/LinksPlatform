using System;
using System.Threading;
using Platform.BlackBox;

namespace Platform.BlackBox.Examples
{
    /// <summary>
    /// Example demonstrating how to use the BlackBox for activity recording and bug report generation.
    /// </summary>
    public class BlackBoxExample
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Platform.BlackBox Example ===");
            Console.WriteLine();

            // Get the singleton instance
            var blackBox = BlackBox.Instance;

            Console.WriteLine("Simulating application activity...");
            Console.WriteLine();

            // Simulate normal application activity
            SimulateApplicationActivity(blackBox);

            Console.WriteLine();
            Console.WriteLine($"Total events recorded: {blackBox.EventCount}");
            Console.WriteLine();

            // Generate and save a bug report
            Console.WriteLine("Press any key to generate bug report...");
            Console.ReadKey();
            Console.WriteLine();

            var reportPath = blackBox.GenerateAndSaveBugReport(
                title: "Example Bug Report",
                description: "This is an automatically generated bug report demonstrating the BlackBox functionality."
            );

            Console.WriteLine($"Bug report saved to: {reportPath}");
            Console.WriteLine();

            // Display the report content
            var report = blackBox.GenerateBugReport();
            var reportContent = new BugReportGenerator().Export(report, BugReportFormat.Markdown);

            Console.WriteLine("=== Bug Report Preview ===");
            Console.WriteLine(reportContent);
        }

        private static void SimulateApplicationActivity(BlackBox blackBox)
        {
            // Record application startup
            blackBox.Record("SystemEvent", "Application", "Application started", "Info");

            // Simulate user actions
            blackBox.Record("UserAction", "UI.MainWindow", "User opened main window", "Info");
            Thread.Sleep(100);

            blackBox.Record("UserAction", "UI.Menu", "User clicked File menu", "Info");
            Thread.Sleep(50);

            blackBox.Record("UserAction", "UI.FileDialog", "User opened file dialog", "Info");
            Thread.Sleep(200);

            // Simulate some processing
            blackBox.Record("SystemEvent", "FileProcessor", "Started processing file: example.txt", "Info");
            Thread.Sleep(150);

            blackBox.Record("SystemEvent", "FileProcessor", "File processed successfully", "Info");

            // Simulate a warning
            blackBox.Record("Warning", "MemoryManager", "Memory usage approaching 80% threshold", "Warning");
            Thread.Sleep(100);

            // Simulate an error scenario
            try
            {
                SimulateErrorScenario();
            }
            catch (Exception ex)
            {
                blackBox.RecordException(ex, "DataProcessor", "Error during data processing");
            }

            // More activity after the error
            blackBox.Record("SystemEvent", "ErrorHandler", "Error handled gracefully, continuing operation", "Info");
            Thread.Sleep(50);

            blackBox.Record("UserAction", "UI.MainWindow", "User clicked Save button", "Info");
            Thread.Sleep(100);

            // Another warning
            blackBox.Record("Warning", "NetworkManager", "Network latency detected: 250ms", "Warning");

            // Final events
            blackBox.Record("SystemEvent", "Application", "Autosave completed", "Info");
        }

        private static void SimulateErrorScenario()
        {
            // Simulate a database connection error
            throw new InvalidOperationException("Database connection failed: Connection timeout after 30 seconds");
        }
    }
}
