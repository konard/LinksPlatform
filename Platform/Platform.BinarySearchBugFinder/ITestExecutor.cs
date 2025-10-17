using System;

namespace Platform.BinarySearchBugFinder
{
    /// <summary>
    /// Interface for executing tests to verify if a bug exists
    /// </summary>
    public interface ITestExecutor
    {
        /// <summary>
        /// Runs tests and returns the result
        /// </summary>
        /// <returns>Test result indicating success or failure</returns>
        TestResult RunTests();
    }

    /// <summary>
    /// Result of test execution
    /// </summary>
    public class TestResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string Output { get; set; }
        public int ExitCode { get; set; }
    }
}
