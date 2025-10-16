using System;
using System.IO;
using Platform.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// <para>
    /// Demonstrates the subscribe/unsubscribe pattern for link events using IObservable and IObserver.
    /// </para>
    /// <para></para>
    /// </summary>
    public class ObservableLinksExampleCLI : ICommandLineInterface
    {
        /// <inheritdoc/>
        public void Run(params string[] args)
        {
            // Create a temporary links database file
            var tempFile = Path.GetTempFileName();

            try
            {
                Console.WriteLine("=== Observable Links Example ===");
                Console.WriteLine("Demonstrates IObservable/IObserver pattern for link events.");
                Console.WriteLine();

                using (var links = new UInt64UnitedMemoryLinks(tempFile))
                {
                    // Wrap the links with the observable wrapper
                    var observableLinks = new ObservableLinks<ulong>(links);

                    Console.WriteLine("Creating observers...");
                    var observer1 = new LinksObserver<ulong>("Observer1");
                    var observer2 = new LinksObserver<ulong>("Observer2");

                    // Subscribe observers
                    Console.WriteLine("Subscribing Observer1 and Observer2...");
                    var subscription1 = observableLinks.Subscribe(observer1);
                    var subscription2 = observableLinks.Subscribe(observer2);
                    Console.WriteLine();

                    // Create some links
                    Console.WriteLine("Creating link 1...");
                    var link1 = observableLinks.Create();
                    Console.WriteLine();

                    Console.WriteLine("Creating link 2...");
                    var link2 = observableLinks.Create();
                    Console.WriteLine();

                    Console.WriteLine("Creating link 3 (pointing from link1 to link2)...");
                    var link3 = observableLinks.Create(link1, link2);
                    Console.WriteLine();

                    // Update a link
                    Console.WriteLine("Updating link 3 (changing target to link1)...");
                    observableLinks.Update(link3, link1, link1);
                    Console.WriteLine();

                    // Unsubscribe observer1
                    Console.WriteLine("Unsubscribing Observer1...");
                    subscription1.Dispose();
                    Console.WriteLine();

                    // Create another link (only observer2 should receive this)
                    Console.WriteLine("Creating link 4 (only Observer2 should see this)...");
                    var link4 = observableLinks.Create();
                    Console.WriteLine();

                    // Delete a link
                    Console.WriteLine("Deleting link 4...");
                    observableLinks.Delete(link4);
                    Console.WriteLine();

                    // Unsubscribe observer2
                    Console.WriteLine("Unsubscribing Observer2...");
                    subscription2.Dispose();
                    Console.WriteLine();

                    // Create one more link (no observers should receive this)
                    Console.WriteLine("Creating link 5 (no observers subscribed, no output expected)...");
                    var link5 = observableLinks.Create();
                    Console.WriteLine("Link 5 created, but no observers were notified.");
                    Console.WriteLine();

                    Console.WriteLine("=== Example completed successfully ===");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                // Clean up temporary file
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
    }
}
