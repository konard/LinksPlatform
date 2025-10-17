using System;
using System.Threading.Tasks;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.LinksCloud.Core;
using Platform.Data.LinksCloud.Models;

namespace Platform.Examples.LinksCloud
{
    /// <summary>
    /// Simple example demonstrating LinksCloud usage.
    /// </summary>
    public class SimpleExample
    {
        public static async Task RunAsync()
        {
            Console.WriteLine("=== LinksCloud Simple Example ===\n");

            // Create an in-memory Links database
            using var links = new UnitedMemoryLinks<ulong>();

            // Create a LinksCloud node
            using var node = new LinksCloudNode(links, "ExampleNode", 9000);

            // Start the node
            Console.WriteLine("Starting LinksCloud node...");
            await node.StartAsync();
            Console.WriteLine("Node started!\n");

            // Create some knowledge entries
            Console.WriteLine("Creating knowledge entries...");
            var entry1Id = await node.CreateKnowledgeEntryAsync(
                "LinksPlatform",
                "A platform for working with associative data using doublets",
                "platform", "database"
            );
            Console.WriteLine($"Created entry 1 with ID: {entry1Id}");

            var entry2Id = await node.CreateKnowledgeEntryAsync(
                "Doublets",
                "A fundamental data structure consisting of two references",
                "data-structure", "core"
            );
            Console.WriteLine($"Created entry 2 with ID: {entry2Id}\n");

            // Search for knowledge
            Console.WriteLine("Searching for 'platform'...");
            var searchResults = await node.SearchKnowledgeAsync("platform");
            Console.WriteLine($"Found {searchResults.Count} result(s):");
            foreach (var entry in searchResults)
            {
                Console.WriteLine($"  - {entry.Title}: {entry.Content}");
            }
            Console.WriteLine();

            // Submit a computational task
            Console.WriteLine("Submitting a computational task...");
            var task = new ComputeTask
            {
                Name = "Example Computation",
                TaskType = "analysis",
                Priority = 1
            };
            var taskId = await node.SubmitComputeTaskAsync(task);
            Console.WriteLine($"Task submitted with ID: {taskId}\n");

            // Wait a bit for task to process
            await Task.Delay(2000);

            // Check task status
            var taskStatus = await node.GetTaskStatusAsync(taskId);
            if (taskStatus != null)
            {
                Console.WriteLine($"Task status: {taskStatus.Status}");
                if (taskStatus.Status == TaskStatus.Completed)
                {
                    Console.WriteLine("Task completed successfully!");
                }
            }

            Console.WriteLine("\nStopping node...");
            await node.StopAsync();
            Console.WriteLine("Node stopped.");
        }

        public static void Main(string[] args)
        {
            RunAsync().Wait();
        }
    }
}
