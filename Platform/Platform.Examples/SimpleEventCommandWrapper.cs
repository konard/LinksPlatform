using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Platform.Examples
{
    public class SimpleEventCommandWrapper : IDisposable
    {
        private readonly SimpleEventStore _eventStore;
        private readonly string _processName;
        private volatile bool _disposed = false;

        public SimpleEventCommandWrapper(SimpleEventStore eventStore, string processName = null)
        {
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _processName = processName ?? Process.GetCurrentProcess().ProcessName;
        }

        public async Task<SimpleCommandResult> ExecuteCommandAsync(string command, string arguments = "", string workingDirectory = null)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SimpleEventCommandWrapper));

            var commandId = Guid.NewGuid();
            var fullCommand = string.IsNullOrEmpty(arguments) ? command : $"{command} {arguments}";

            // Log command start
            _eventStore.StoreEvent("Command.Start", $"Starting command: {fullCommand}", _processName, commandId);

            var result = new SimpleCommandResult
            {
                Command = fullCommand,
                CommandId = commandId,
                StartTime = DateTimeOffset.UtcNow
            };

            try
            {
                using var process = new Process();
                process.StartInfo.FileName = command;
                process.StartInfo.Arguments = arguments ?? "";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    process.StartInfo.WorkingDirectory = workingDirectory;
                }

                process.Start();

                // Read output and error
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                result.ExitCode = process.ExitCode;
                result.EndTime = DateTimeOffset.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
                result.StandardOutput = await outputTask;
                result.StandardError = await errorTask;

                // Log output if present
                if (!string.IsNullOrEmpty(result.StandardOutput))
                {
                    _eventStore.StoreEvent("Command.Output", result.StandardOutput, _processName, commandId);
                }
                if (!string.IsNullOrEmpty(result.StandardError))
                {
                    _eventStore.StoreEvent("Command.Error", result.StandardError, _processName, commandId);
                }

                // Log command completion
                var completionMessage = $"Command completed with exit code {result.ExitCode} in {result.Duration.TotalMilliseconds:F0}ms";
                _eventStore.StoreEvent("Command.Complete", completionMessage, _processName, commandId);

                return result;
            }
            catch (Exception ex)
            {
                result.EndTime = DateTimeOffset.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
                result.Exception = ex;
                result.ExitCode = -1;

                // Log command failure
                _eventStore.StoreEvent("Command.Exception", $"Command failed with exception: {ex.Message}", _processName, commandId);

                return result;
            }
        }

        public SimpleCommandResult ExecuteCommand(string command, string arguments = "", string workingDirectory = null)
        {
            return ExecuteCommandAsync(command, arguments, workingDirectory).GetAwaiter().GetResult();
        }

        public void LogEvent(string eventType, string message)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SimpleEventCommandWrapper));

            _eventStore.StoreEvent(eventType, message, _processName);
        }

        public void LogInfo(string message) => LogEvent("Info", message);
        public void LogWarning(string message) => LogEvent("Warning", message);
        public void LogError(string message) => LogEvent("Error", message);
        public void LogDebug(string message) => LogEvent("Debug", message);

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }

    public class SimpleCommandResult
    {
        public Guid CommandId { get; set; }
        public string Command { get; set; }
        public int ExitCode { get; set; }
        public string StandardOutput { get; set; }
        public string StandardError { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public Exception Exception { get; set; }

        public bool IsSuccess => ExitCode == 0 && Exception == null;

        public override string ToString()
        {
            var status = IsSuccess ? "SUCCESS" : "FAILED";
            return $"[{status}] {Command} - Exit Code: {ExitCode}, Duration: {Duration.TotalMilliseconds:F0}ms";
        }
    }
}