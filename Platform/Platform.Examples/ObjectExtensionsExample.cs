using System;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Demonstrates the usage of object extensions with the global context system.
    /// </summary>
    public class ObjectExtensionsExample
    {
        /// <summary>
        /// Example demonstrating ToDateTime extension with context-based format.
        /// </summary>
        public static void ToDateTimeExample()
        {
            // Set the default format in the global context
            Global.GetCurrentContext().Set("ToDateTime.format", "dd-MM-yyyy");

            // Now ToDateTime can use the context format
            var dateString = "25-12-2024";
            var date = dateString.ToDateTime();

            Console.WriteLine($"Parsed date: {date}");

            // You can also override the context format
            var usDate = "12/25/2024";
            var date2 = usDate.ToDateTime("MM/dd/yyyy");

            Console.WriteLine($"Parsed US date: {date2}");
        }

        /// <summary>
        /// Example demonstrating ToLink extension with context-based Links instance.
        /// Note: This is a conceptual example. In real usage, you would initialize
        /// a Links instance (e.g., SynchronizedLinks&lt;ulong&gt;) and set it in the context.
        /// </summary>
        public static void ToLinkExampleConcept()
        {
            // Conceptual example:
            // Global.GetCurrentContext().Set("ToLink.links", yourLinksInstance);
            // var obj = "example string";
            // var link = obj.ToLink(); // Uses context Links instance
            // var link2 = obj.ToLink(explicitLinksInstance); // Uses explicit Links instance

            Console.WriteLine("ToLink example: Set 'ToLink.links' in context before calling ToLink()");
        }

        /// <summary>
        /// Runs all examples.
        /// </summary>
        public static void RunAllExamples()
        {
            Console.WriteLine("=== ToDateTime Example ===");
            ToDateTimeExample();

            Console.WriteLine("\n=== ToLink Example ===");
            ToLinkExampleConcept();
        }
    }
}
