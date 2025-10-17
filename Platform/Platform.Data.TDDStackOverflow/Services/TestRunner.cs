using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Platform.Data.TDDStackOverflow.Models;

namespace Platform.Data.TDDStackOverflow.Services
{
    /// <summary>
    /// Executes language-agnostic tests using CLI input/output
    /// </summary>
    public class TestRunner
    {
        private readonly string _workingDirectory;

        public TestRunner(string workingDirectory = null)
        {
            _workingDirectory = workingDirectory ?? Path.GetTempPath();
        }

        /// <summary>
        /// Runs all tests in a test suite against a solution
        /// </summary>
        public async Task<TestResult> RunTestsAsync(Solution solution, TestSuite testSuite)
        {
            var result = new TestResult
            {
                SolutionId = solution.Id,
                StartTime = DateTime.UtcNow
            };

            // Write solution code to file
            var tempFile = Path.Combine(_workingDirectory, solution.FileName);
            try
            {
                File.WriteAllText(tempFile, solution.Code);

                // Run each test case
                foreach (var testCase in testSuite.TestCases)
                {
                    var testCaseResult = await RunSingleTestAsync(solution, testCase, testSuite.TimeoutMilliseconds);
                    result.TestCaseResults.Add(testCaseResult);
                }

                // Calculate metrics
                result.EndTime = DateTime.UtcNow;
                result.TotalExecutionTime = (result.EndTime.Value - result.StartTime).TotalMilliseconds;
                result.TestsPassed = result.TestCaseResults.FindAll(r => r.Passed).Count;
                result.TestsFailed = result.TestCaseResults.Count - result.TestsPassed;
                result.AllTestsPassed = result.TestsFailed == 0;
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { /* Ignore cleanup errors */ }
                }
            }

            return result;
        }

        /// <summary>
        /// Runs a single test case
        /// </summary>
        private async Task<TestCaseResult> RunSingleTestAsync(Solution solution, TestCase testCase, int timeoutMs)
        {
            var result = new TestCaseResult
            {
                TestCase = testCase,
                StartTime = DateTime.UtcNow
            };

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = GetShellCommand(),
                        Arguments = GetShellArguments(solution.ExecutionCommand),
                        WorkingDirectory = _workingDirectory,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
                process.ErrorDataReceived += (sender, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Write input
                if (!string.IsNullOrEmpty(testCase.Input))
                {
                    process.StandardInput.Write(testCase.Input);
                    process.StandardInput.Flush();
                }
                process.StandardInput.Close();

                // Wait for completion with timeout
                var completed = process.WaitForExit(timeoutMs);

                if (!completed)
                {
                    process.Kill();
                    result.Passed = false;
                    result.ErrorMessage = $"Test timed out after {timeoutMs}ms";
                }
                else
                {
                    result.ActualOutput = outputBuilder.ToString().Trim();
                    result.ErrorOutput = errorBuilder.ToString().Trim();
                    result.ExitCode = process.ExitCode;

                    // Compare output
                    result.Passed = CompareOutput(testCase.ExpectedOutput, result.ActualOutput) && result.ExitCode == 0;

                    if (!result.Passed && string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        result.ErrorMessage = result.ExitCode != 0
                            ? $"Process exited with code {result.ExitCode}"
                            : "Output mismatch";
                    }
                }

                result.EndTime = DateTime.UtcNow;
                result.ExecutionTime = (result.EndTime.Value - result.StartTime).TotalMilliseconds;
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.ErrorMessage = ex.Message;
                result.EndTime = DateTime.UtcNow;
                result.ExecutionTime = (result.EndTime.Value - result.StartTime).TotalMilliseconds;
            }

            return result;
        }

        private bool CompareOutput(string expected, string actual)
        {
            if (expected == null && actual == null) return true;
            if (expected == null || actual == null) return false;

            // Normalize line endings and trim
            var normalizedExpected = expected.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
            var normalizedActual = actual.Replace("\r\n", "\n").Replace("\r", "\n").Trim();

            return normalizedExpected == normalizedActual;
        }

        private string GetShellCommand()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT ? "cmd.exe" : "/bin/sh";
        }

        private string GetShellArguments(string command)
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT ? $"/c {command}" : $"-c \"{command}\"";
        }
    }

    /// <summary>
    /// Result of running all tests for a solution
    /// </summary>
    public class TestResult
    {
        public ulong SolutionId { get; set; }
        public System.Collections.Generic.List<TestCaseResult> TestCaseResults { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double TotalExecutionTime { get; set; }
        public int TestsPassed { get; set; }
        public int TestsFailed { get; set; }
        public bool AllTestsPassed { get; set; }
        public long PeakMemoryUsage { get; set; }

        public TestResult()
        {
            TestCaseResults = new System.Collections.Generic.List<TestCaseResult>();
        }
    }

    /// <summary>
    /// Result of running a single test case
    /// </summary>
    public class TestCaseResult
    {
        public TestCase TestCase { get; set; }
        public bool Passed { get; set; }
        public string ActualOutput { get; set; }
        public string ErrorOutput { get; set; }
        public string ErrorMessage { get; set; }
        public int ExitCode { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double ExecutionTime { get; set; }
    }
}
