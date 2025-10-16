using System;
using System.Collections.Generic;

namespace Platform.Examples.AutoTest
{
    /// <summary>
    /// Represents the result of a test execution.
    /// </summary>
    public class TestResult
    {
        public string FunctionName { get; set; }
        public object[] InputParameters { get; set; }
        public object Output { get; set; }
        public bool Success { get; set; }
        public Exception Exception { get; set; }
        public long ExecutionTimeMs { get; set; }
        public long MemoryUsedBytes { get; set; }
        public TestType TestType { get; set; }
    }

    /// <summary>
    /// Type of test performed.
    /// </summary>
    public enum TestType
    {
        Normal,
        BoundaryMin,
        BoundaryMax,
        EdgeCase,
        RandomInput,
        NullInput,
        InvalidInput
    }

    /// <summary>
    /// Aggregate statistics for test runs.
    /// </summary>
    public class TestStatistics
    {
        public int TotalTests { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public long TotalExecutionTimeMs { get; set; }
        public Dictionary<string, int> TestsByFunction { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> FailuresByFunction { get; set; } = new Dictionary<string, int>();
    }
}
