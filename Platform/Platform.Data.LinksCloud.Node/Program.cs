using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.LinksCloud.Core;
using Platform.Data.LinksCloud.Models;
using Platform.IO;

namespace Platform.Data.LinksCloud.Node
{
    /// <summary>
    /// LinksCloud Node - A decentralized computational network with knowledge encyclopedia.
    /// </summary>
    public class Program
    {
        private static LinksCloudNode? _node;

        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== LinksCloud Node ===");
            Console.WriteLine("Decentralized computational network with knowledge encyclopedia");
            Console.WriteLine();

            // Parse arguments
            var nodeName = args.Length > 0 ? args[0] : $"Node-{Environment.MachineName}";
            var port = args.Length > 1 ? int.Parse(args[1]) : 9000;

            // Initialize Links database (in-memory for this example)
            using var links = new UnitedMemoryLinks<ulong>();

            // Create and start the LinksCloud node
            _node = new LinksCloudNode(links, nodeName, port);

            Console.WriteLine($"Starting LinksCloud node: {nodeName} on port {port}...");
            await _node.StartAsync();
            Console.WriteLine("Node started successfully!");
            Console.WriteLine();

            // Show help
            ShowHelp();

            // Command loop
            using var cancellation = new ConsoleCancellation();
            _ = Task.Run(async () =>
            {
                while (!cancellation.Token.IsCancellationRequested)
                {
                    try
                    {
                        Console.Write("> ");
                        var input = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(input))
                        {
                            continue;
                        }

                        await ProcessCommandAsync(input.Trim());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            });

            cancellation.Wait();

            Console.WriteLine("\nStopping LinksCloud node...");
            await _node.StopAsync();
            Console.WriteLine("Node stopped.");
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("  help                          - Show this help message");
            Console.WriteLine("  status                        - Show node status");
            Console.WriteLine("  peers                         - List connected peers");
            Console.WriteLine("  connect <address> <port>      - Connect to a peer");
            Console.WriteLine("  create <title> <content>      - Create knowledge entry");
            Console.WriteLine("  search <query>                - Search knowledge entries");
            Console.WriteLine("  list                          - List all knowledge entries");
            Console.WriteLine("  task <name> <type>            - Submit a computational task");
            Console.WriteLine("  tasks                         - List my tasks");
            Console.WriteLine("  exit                          - Exit the application");
            Console.WriteLine();
        }

        private static async Task ProcessCommandAsync(string input)
        {
            if (_node == null)
            {
                return;
            }

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return;
            }

            var command = parts[0].ToLower();

            switch (command)
            {
                case "help":
                    ShowHelp();
                    break;

                case "status":
                    ShowStatus();
                    break;

                case "peers":
                    ShowPeers();
                    break;

                case "connect":
                    if (parts.Length >= 3)
                    {
                        await ConnectToPeerAsync(parts[1], int.Parse(parts[2]));
                    }
                    else
                    {
                        Console.WriteLine("Usage: connect <address> <port>");
                    }
                    break;

                case "create":
                    if (parts.Length >= 3)
                    {
                        var title = parts[1];
                        var content = string.Join(" ", parts, 2, parts.Length - 2);
                        await CreateKnowledgeEntryAsync(title, content);
                    }
                    else
                    {
                        Console.WriteLine("Usage: create <title> <content>");
                    }
                    break;

                case "search":
                    if (parts.Length >= 2)
                    {
                        var query = string.Join(" ", parts, 1, parts.Length - 1);
                        await SearchKnowledgeAsync(query);
                    }
                    else
                    {
                        Console.WriteLine("Usage: search <query>");
                    }
                    break;

                case "list":
                    await ListKnowledgeEntriesAsync();
                    break;

                case "task":
                    if (parts.Length >= 3)
                    {
                        var taskName = parts[1];
                        var taskType = parts[2];
                        await SubmitTaskAsync(taskName, taskType);
                    }
                    else
                    {
                        Console.WriteLine("Usage: task <name> <type>");
                    }
                    break;

                case "tasks":
                    await ListTasksAsync();
                    break;

                case "exit":
                    Environment.Exit(0);
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}. Type 'help' for available commands.");
                    break;
            }
        }

        private static void ShowStatus()
        {
            if (_node == null)
            {
                return;
            }

            Console.WriteLine($"Node ID: {_node.LocalNode.NodeId}");
            Console.WriteLine($"Name: {_node.LocalNode.Name}");
            Console.WriteLine($"Port: {_node.LocalNode.Port}");
            Console.WriteLine($"Compute Capacity: {_node.LocalNode.ComputeCapacity}");
            Console.WriteLine($"Storage Capacity: {_node.LocalNode.StorageCapacity:N0} bytes");
            Console.WriteLine($"Connected Peers: {_node.NetworkManager.Peers.Count}");
        }

        private static void ShowPeers()
        {
            if (_node == null)
            {
                return;
            }

            var peers = _node.NetworkManager.Peers;
            if (peers.Count == 0)
            {
                Console.WriteLine("No peers connected.");
                return;
            }

            Console.WriteLine($"Connected Peers ({peers.Count}):");
            foreach (var peer in peers)
            {
                Console.WriteLine($"  - {peer.Name} ({peer.NodeId})");
                Console.WriteLine($"    Address: {peer.Address}:{peer.Port}");
                Console.WriteLine($"    Status: {(peer.IsOnline ? "Online" : "Offline")}");
            }
        }

        private static async Task ConnectToPeerAsync(string address, int port)
        {
            if (_node == null)
            {
                return;
            }

            Console.WriteLine($"Connecting to {address}:{port}...");
            var success = await _node.ConnectToPeerAsync(address, port);

            if (success)
            {
                Console.WriteLine("Connected successfully!");
            }
            else
            {
                Console.WriteLine("Failed to connect.");
            }
        }

        private static async Task CreateKnowledgeEntryAsync(string title, string content)
        {
            if (_node == null)
            {
                return;
            }

            Console.WriteLine($"Creating knowledge entry: {title}");
            var entryId = await _node.CreateKnowledgeEntryAsync(title, content);
            Console.WriteLine($"Entry created with ID: {entryId}");
        }

        private static async Task SearchKnowledgeAsync(string query)
        {
            if (_node == null)
            {
                return;
            }

            Console.WriteLine($"Searching for: {query}");
            var results = await _node.SearchKnowledgeAsync(query);

            if (results.Count == 0)
            {
                Console.WriteLine("No results found.");
                return;
            }

            Console.WriteLine($"Found {results.Count} result(s):");
            foreach (var entry in results)
            {
                Console.WriteLine($"  [{entry.EntryId}] {entry.Title}");
                Console.WriteLine($"    Content: {entry.Content}");
                Console.WriteLine($"    Author: {entry.AuthorNodeId}");
                Console.WriteLine($"    Created: {entry.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine();
            }
        }

        private static async Task ListKnowledgeEntriesAsync()
        {
            if (_node == null)
            {
                return;
            }

            var entries = await _node.KnowledgeStore.GetAllEntriesAsync();

            if (entries.Count == 0)
            {
                Console.WriteLine("No knowledge entries.");
                return;
            }

            Console.WriteLine($"Knowledge Entries ({entries.Count}):");
            foreach (var entry in entries)
            {
                Console.WriteLine($"  [{entry.EntryId}] {entry.Title}");
            }
        }

        private static async Task SubmitTaskAsync(string taskName, string taskType)
        {
            if (_node == null)
            {
                return;
            }

            var task = new ComputeTask
            {
                Name = taskName,
                TaskType = taskType,
                Input = new Dictionary<string, object>
                {
                    ["example"] = "data"
                }
            };

            Console.WriteLine($"Submitting task: {taskName}");
            var taskId = await _node.SubmitComputeTaskAsync(task);
            Console.WriteLine($"Task submitted with ID: {taskId}");
        }

        private static async Task ListTasksAsync()
        {
            if (_node == null)
            {
                return;
            }

            var tasks = await _node.TaskScheduler.GetMyTasksAsync();

            if (tasks.Count == 0)
            {
                Console.WriteLine("No tasks submitted.");
                return;
            }

            Console.WriteLine($"My Tasks ({tasks.Count}):");
            foreach (var task in tasks)
            {
                Console.WriteLine($"  [{task.TaskId}] {task.Name}");
                Console.WriteLine($"    Type: {task.TaskType}");
                Console.WriteLine($"    Status: {task.Status}");
                Console.WriteLine($"    Created: {task.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                if (task.CompletedAt.HasValue)
                {
                    Console.WriteLine($"    Completed: {task.CompletedAt:yyyy-MM-dd HH:mm:ss}");
                }
                Console.WriteLine();
            }
        }
    }
}
