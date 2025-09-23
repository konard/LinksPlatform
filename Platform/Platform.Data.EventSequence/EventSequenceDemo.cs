using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Platform.Data.EventSequence
{
    // Core event record with UUIDv7 for timestamp-based ordering
    public class EventRecord
    {
        public Guid EventId { get; set; }
        public string EventType { get; set; }
        public string Message { get; set; }
        public string ProcessId { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{ProcessId}] [{EventType}] {Message} (ID: {EventId})";
        }
    }

    // Simple in-memory event store with file persistence
    public class EventStore : IDisposable
    {
        private readonly List<EventRecord> _events = new List<EventRecord>();
        private readonly object _lock = new object();
        private readonly string _filePath;

        public EventStore(string filePath = "events.json")
        {
            _filePath = filePath;
            LoadEvents();
        }

        public void StoreEvent(string eventType, string message, string processId = null, Guid? eventId = null)
        {
            var evt = new EventRecord
            {
                EventId = eventId ?? CreateUUIDv7(),
                EventType = eventType,
                Message = message,
                ProcessId = processId ?? $"{Process.GetCurrentProcess().Id}:{Process.GetCurrentProcess().ProcessName}",
                Timestamp = DateTime.UtcNow
            };

            lock (_lock)
            {
                _events.Add(evt);
                SaveEvents();
            }

            Console.WriteLine($"[EVENT] {evt}");
        }

        public IEnumerable<EventRecord> GetEvents(string eventType = null, string processId = null, DateTime? fromTime = null, DateTime? toTime = null)
        {
            lock (_lock)
            {
                return _events
                    .Where(e => eventType == null || e.EventType == eventType)
                    .Where(e => processId == null || e.ProcessId == processId)
                    .Where(e => fromTime == null || e.Timestamp >= fromTime)
                    .Where(e => toTime == null || e.Timestamp <= toTime)
                    .OrderBy(e => e.Timestamp)
                    .ToList();
            }
        }

        public IEnumerable<EventRecord> GetEventsByTimeRange(DateTime from, DateTime to) => GetEvents(fromTime: from, toTime: to);
        public IEnumerable<EventRecord> GetEventsByProcess(string processId) => GetEvents(processId: processId);
        public IEnumerable<EventRecord> GetEventsByType(string eventType) => GetEvents(eventType: eventType);

        private void LoadEvents()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    var events = JsonSerializer.Deserialize<List<EventRecord>>(json);
                    if (events != null)
                    {
                        _events.AddRange(events);
                    }
                }
            }
            catch
            {
                // Ignore load errors
            }
        }

        private void SaveEvents()
        {
            try
            {
                var json = JsonSerializer.Serialize(_events, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }

        // UUIDv7 implementation with embedded timestamp
        private static Guid CreateUUIDv7()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var timestampBytes = BitConverter.GetBytes(timestamp);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(timestampBytes, 0, 6);

            var randomBytes = new byte[10];
            new Random().NextBytes(randomBytes);

            var uuidBytes = new byte[16];
            Array.Copy(timestampBytes, 0, uuidBytes, 0, 6);
            Array.Copy(randomBytes, 0, uuidBytes, 6, 10);

            // Set version (7) and variant bits
            uuidBytes[6] = (byte)((uuidBytes[6] & 0x0F) | 0x70);
            uuidBytes[8] = (byte)((uuidBytes[8] & 0x3F) | 0x80);

            return new Guid(uuidBytes);
        }

        public void Dispose()
        {
            SaveEvents();
        }
    }

    // Command wrapper that logs all command executions
    public class EventCommandWrapper : IDisposable
    {
        private readonly EventStore _eventStore;
        private readonly string _processName;

        public EventCommandWrapper(EventStore eventStore, string processName = null)
        {
            _eventStore = eventStore;
            _processName = processName ?? "CommandWrapper";
        }

        public CommandResult ExecuteCommand(string command, string arguments = "")
        {
            var commandId = Guid.NewGuid();
            var fullCommand = string.IsNullOrEmpty(arguments) ? command : $"{command} {arguments}";

            _eventStore.StoreEvent("Command.Start", $"Starting: {fullCommand}", _processName, commandId);

            var result = new CommandResult
            {
                Command = fullCommand,
                CommandId = commandId,
                StartTime = DateTime.UtcNow
            };

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                result.StandardOutput = process.StandardOutput.ReadToEnd();
                result.StandardError = process.StandardError.ReadToEnd();
                process.WaitForExit();

                result.ExitCode = process.ExitCode;
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;

                if (!string.IsNullOrEmpty(result.StandardOutput))
                    _eventStore.StoreEvent("Command.Output", result.StandardOutput, _processName, commandId);
                if (!string.IsNullOrEmpty(result.StandardError))
                    _eventStore.StoreEvent("Command.Error", result.StandardError, _processName, commandId);

                _eventStore.StoreEvent("Command.Complete",
                    $"Completed with exit code {result.ExitCode} in {result.Duration.TotalMilliseconds:F0}ms",
                    _processName, commandId);

                return result;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                result.ExitCode = -1;
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;

                _eventStore.StoreEvent("Command.Exception", $"Failed: {ex.Message}", _processName, commandId);
                return result;
            }
        }

        public void LogInfo(string message) => _eventStore.StoreEvent("Info", message, _processName);
        public void LogWarning(string message) => _eventStore.StoreEvent("Warning", message, _processName);
        public void LogError(string message) => _eventStore.StoreEvent("Error", message, _processName);

        public void Dispose() { }
    }

    public class CommandResult
    {
        public Guid CommandId { get; set; }
        public string Command { get; set; }
        public int ExitCode { get; set; }
        public string StandardOutput { get; set; }
        public string StandardError { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public Exception Exception { get; set; }

        public bool IsSuccess => ExitCode == 0 && Exception == null;
    }

    // Demo program
    public class EventSequenceDemo
    {
        public static void RunDemo(string[] args)
        {
            Console.WriteLine("Event Sequence System - Issue #684 Solution");
            Console.WriteLine("==========================================");
            Console.WriteLine();

            using var eventStore = new EventStore("event_sequence_demo.json");
            using var commandWrapper = new EventCommandWrapper(eventStore, "DemoProcess");

            // Demonstrate basic event logging
            Console.WriteLine("1. Basic Event Logging:");
            eventStore.StoreEvent("Application.Start", "Demo application started", "MainProcess");
            eventStore.StoreEvent("Database.Connect", "Connected to event database", "MainProcess");
            commandWrapper.LogInfo("System initialized successfully");
            commandWrapper.LogWarning("This is a warning message");

            Console.WriteLine();

            // Demonstrate command execution with automatic event logging
            Console.WriteLine("2. Command Execution with Event Tracking:");

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var result1 = commandWrapper.ExecuteCommand("cmd", "/c echo Hello from Windows Command");
                var result2 = commandWrapper.ExecuteCommand("cmd", "/c echo Current Time: %TIME%");
            }
            else
            {
                var result1 = commandWrapper.ExecuteCommand("echo", "Hello from Unix Command");
                var result2 = commandWrapper.ExecuteCommand("date");
            }

            Console.WriteLine();

            // Demonstrate parallel process simulation
            Console.WriteLine("3. Parallel Process Simulation:");
            Parallel.Invoke(
                () => SimulateWorker(eventStore, "Worker1", 3),
                () => SimulateWorker(eventStore, "Worker2", 2),
                () => SimulateBackground(eventStore, "BackgroundTask", 4)
            );

            Console.WriteLine();

            // Query and display events
            Console.WriteLine("4. Event Querying and Analysis:");
            var allEvents = eventStore.GetEvents().ToList();
            Console.WriteLine($"Total events recorded: {allEvents.Count}");

            var commandEvents = eventStore.GetEventsByType("Command.Start").ToList();
            Console.WriteLine($"Commands executed: {commandEvents.Count}");

            var recentEvents = eventStore.GetEventsByTimeRange(
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow).ToList();
            Console.WriteLine($"Events in last minute: {recentEvents.Count}");

            Console.WriteLine();
            Console.WriteLine("5. Complete Event Timeline (Last 15 events):");
            Console.WriteLine("".PadRight(80, '='));

            foreach (var evt in allEvents.TakeLast(15))
            {
                Console.WriteLine($"  {evt}");
            }

            Console.WriteLine();
            Console.WriteLine("Event Sequence System demonstration completed!");
            Console.WriteLine($"All events saved to: {Path.GetFullPath("event_sequence_demo.json")}");
            Console.WriteLine();
            Console.WriteLine("This system provides:");
            Console.WriteLine("- Single database for all process events");
            Console.WriteLine("- UUIDv7 timestamps for perfect chronological ordering");
            Console.WriteLine("- Automatic command wrapping and logging");
            Console.WriteLine("- Rich querying capabilities");
            Console.WriteLine("- Full automation of debugging through event sequences");
        }

        private static void SimulateWorker(EventStore eventStore, string workerName, int tasks)
        {
            eventStore.StoreEvent("Worker.Start", $"Worker {workerName} started", workerName);

            for (int i = 1; i <= tasks; i++)
            {
                System.Threading.Thread.Sleep(100);
                eventStore.StoreEvent("Task.Execute", $"Executing task {i}/{tasks}", workerName);
            }

            eventStore.StoreEvent("Worker.Complete", $"Worker {workerName} completed {tasks} tasks", workerName);
        }

        private static void SimulateBackground(EventStore eventStore, string taskName, int cycles)
        {
            eventStore.StoreEvent("Background.Start", $"Background task {taskName} started", taskName);

            for (int i = 1; i <= cycles; i++)
            {
                System.Threading.Thread.Sleep(75);
                eventStore.StoreEvent("Background.Cycle", $"Background cycle {i} completed", taskName);
            }

            eventStore.StoreEvent("Background.Stop", $"Background task {taskName} stopped", taskName);
        }
    }
}