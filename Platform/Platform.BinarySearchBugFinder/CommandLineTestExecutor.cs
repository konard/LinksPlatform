using System;
using System.Diagnostics;
using System.Text;

namespace Platform.BinarySearchBugFinder
{
    /// <summary>
    /// Executes tests using command line commands (e.g., dotnet test, npm test)
    /// </summary>
    public class CommandLineTestExecutor : ITestExecutor
    {
        private readonly string _command;
        private readonly string _workingDirectory;
        private readonly int _timeoutSeconds;

        public CommandLineTestExecutor(string command, string workingDirectory, int timeoutSeconds = 300)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _workingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
            _timeoutSeconds = timeoutSeconds;
        }

        public TestResult RunTests()
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{_command}\"",
                    WorkingDirectory = _workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var output = new StringBuilder();
                var error = new StringBuilder();

                using (var process = new Process())
                {
                    process.StartInfo = processStartInfo;

                    process.OutputDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                            output.AppendLine(args.Data);
                    };

                    process.ErrorDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                            error.AppendLine(args.Data);
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    bool completed = process.WaitForExit(_timeoutSeconds * 1000);

                    if (!completed)
                    {
                        process.Kill();
                        return new TestResult
                        {
                            Success = false,
                            ErrorMessage = $"Test execution timed out after {_timeoutSeconds} seconds",
                            ExitCode = -1
                        };
                    }

                    var exitCode = process.ExitCode;
                    var fullOutput = output.ToString();
                    var fullError = error.ToString();

                    return new TestResult
                    {
                        Success = exitCode == 0,
                        ErrorMessage = exitCode != 0 ? fullError : null,
                        Output = fullOutput,
                        ExitCode = exitCode
                    };
                }
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    Success = false,
                    ErrorMessage = $"Exception during test execution: {ex.Message}",
                    ExitCode = -1
                };
            }
        }
    }

    /// <summary>
    /// Executes compilation to check if code compiles successfully
    /// </summary>
    public class CompilationTestExecutor : ITestExecutor
    {
        private readonly string _projectPath;
        private readonly int _timeoutSeconds;

        public CompilationTestExecutor(string projectPath, int timeoutSeconds = 120)
        {
            _projectPath = projectPath ?? throw new ArgumentNullException(nameof(projectPath));
            _timeoutSeconds = timeoutSeconds;
        }

        public TestResult RunTests()
        {
            var command = $"dotnet build \"{_projectPath}\" --no-restore";
            var workingDir = System.IO.Path.GetDirectoryName(_projectPath);
            var executor = new CommandLineTestExecutor(command, workingDir, _timeoutSeconds);
            return executor.RunTests();
        }
    }
}
