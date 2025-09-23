using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Unicode;
using Platform.Data.Doublets.Sequences;

namespace Platform.Examples
{
    public class SimpleEventSequenceExample : ICommandLineInterface
    {
        private const string EventDatabaseFilename = "simple_events.links";

        public void Run(params string[] args)
        {
            Console.WriteLine("Simple Event Sequence System Example");
            Console.WriteLine("===================================");

            try
            {
                // Clean up for demonstration
                if (File.Exists(EventDatabaseFilename))
                    File.Delete(EventDatabaseFilename);

                // Setup event database
                using var memoryAdapter = new UInt64UnitedMemoryLinks(EventDatabaseFilename, 8 * 1024 * 1024);
                using var links = new UInt64Links(memoryAdapter);
                var syncLinks = new SynchronizedLinks<ulong>(links);
                var unicodeMap = new UnicodeMap(syncLinks);
                unicodeMap.Init();

                var sequences = new Sequences(syncLinks, new SequencesOptions<ulong>
                {
                    UseSequenceMarker = true,
                    SequenceMarkerLink = 65537,
                    UseCompression = true
                });

                using var eventStore = new SimpleEventStore(links, sequences, unicodeMap);
                using var commandWrapper = new SimpleEventCommandWrapper(eventStore, "ExampleProcess");

                Console.WriteLine("\n1. Demonstrating basic event logging:");
                DemonstrateBasicEventLogging(eventStore);

                Console.WriteLine("\n2. Demonstrating command execution with event tracking:");
                DemonstrateCommandExecution(commandWrapper);

                Console.WriteLine("\n3. Demonstrating parallel process simulation:");
                DemonstrateParallelProcesses(eventStore);

                Console.WriteLine("\n4. Querying and displaying events:");
                DemonstrateEventQuerying(eventStore);

                Console.WriteLine("\nSimple Event Sequence System demonstration completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during demonstration: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void DemonstrateBasicEventLogging(SimpleEventStore eventStore)
        {
            // Log various types of events
            eventStore.StoreEvent("Application.Start", "Application started successfully", "MainProcess");
            eventStore.StoreEvent("Database.Connect", "Connected to main database", "MainProcess");
            eventStore.StoreEvent("User.Login", "User 'admin' logged in", "AuthProcess");
            eventStore.StoreEvent("Data.Query", "Executed query: SELECT * FROM users", "DatabaseProcess");
            eventStore.StoreEvent("Cache.Miss", "Cache miss for key 'user:123'", "CacheProcess");
            eventStore.StoreEvent("Cache.Update", "Updated cache for key 'user:123'", "CacheProcess");

            Console.WriteLine("  ✓ Logged 6 events from different processes");
        }

        private void DemonstrateCommandExecution(SimpleEventCommandWrapper commandWrapper)
        {
            // Execute some commands and track their output
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var result1 = commandWrapper.ExecuteCommand("cmd", "/c echo Hello from Windows");
                Console.WriteLine($"  ✓ Executed Windows commands, tracked outputs (Exit code: {result1.ExitCode})");
            }
            else
            {
                var result1 = commandWrapper.ExecuteCommand("echo", "Hello from Unix");
                Console.WriteLine($"  ✓ Executed Unix commands, tracked outputs (Exit code: {result1.ExitCode})");
            }

            // Log additional events
            commandWrapper.LogInfo("System health check completed");
            commandWrapper.LogWarning("Memory usage is at 85%");
            commandWrapper.LogError("Failed to connect to external service");
        }

        private void DemonstrateParallelProcesses(SimpleEventStore eventStore)
        {
            // Simulate parallel processes by logging events from multiple "processes"
            var tasks = new[]
            {
                Task.Run(() => SimulateWorkerProcess(eventStore, "Worker1", 3)),
                Task.Run(() => SimulateWorkerProcess(eventStore, "Worker2", 2)),
                Task.Run(() => SimulateBackgroundProcess(eventStore, "Background1", 2))
            };

            Task.WaitAll(tasks);
            Console.WriteLine("  ✓ Simulated parallel processes with event logging");
        }

        private void SimulateWorkerProcess(SimpleEventStore eventStore, string processName, int taskCount)
        {
            eventStore.StoreEvent("Process.Start", $"Worker process {processName} started", processName);

            for (int i = 1; i <= taskCount; i++)
            {
                System.Threading.Thread.Sleep(50); // Simulate work
                eventStore.StoreEvent("Task.Process", $"Processing task {i}/{taskCount}", processName);
            }

            eventStore.StoreEvent("Process.Complete", $"Worker process {processName} completed {taskCount} tasks", processName);
        }

        private void SimulateBackgroundProcess(SimpleEventStore eventStore, string processName, int cycles)
        {
            eventStore.StoreEvent("Background.Start", $"Background process {processName} started", processName);

            for (int i = 1; i <= cycles; i++)
            {
                System.Threading.Thread.Sleep(75); // Simulate background work
                eventStore.StoreEvent("Background.Cycle", $"Background cycle {i} completed", processName);
            }

            eventStore.StoreEvent("Background.Stop", $"Background process {processName} stopped", processName);
        }

        private void DemonstrateEventQuerying(SimpleEventStore eventStore)
        {
            var allEvents = eventStore.GetEvents().ToList();
            Console.WriteLine($"  Total events stored: {allEvents.Count}");

            // Query events by type
            var processEvents = eventStore.GetEventsByType("Process.Start").ToList();
            Console.WriteLine($"  Process start events: {processEvents.Count}");

            // Query events by process
            var worker1Events = eventStore.GetEventsByProcess("Worker1").ToList();
            Console.WriteLine($"  Worker1 events: {worker1Events.Count}");

            // Query events by time range (last 10 seconds)
            var recentEvents = eventStore.GetEventsByTimeRange(
                DateTimeOffset.UtcNow.AddSeconds(-10),
                DateTimeOffset.UtcNow).ToList();
            Console.WriteLine($"  Recent events (last 10s): {recentEvents.Count}");

            Console.WriteLine("\n  Sample events (chronological order):");
            foreach (var evt in allEvents.Take(10))
            {
                Console.WriteLine($"    {evt}");
            }
        }
    }
}