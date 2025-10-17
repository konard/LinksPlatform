using System;

namespace Platform.Data.TDDStackOverflow.Models
{
    /// <summary>
    /// Represents a solution to a question
    /// </summary>
    public class Solution
    {
        public ulong Id { get; set; }

        public ulong QuestionId { get; set; }

        public string Code { get; set; }

        public string Language { get; set; }

        public string ExecutionCommand { get; set; }

        public string FileName { get; set; }

        public bool PassedAllTests { get; set; }

        public int Votes { get; set; }

        public SolutionMetrics Metrics { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastTestedAt { get; set; }

        public string SubmittedBy { get; set; }

        public Solution()
        {
            CreatedAt = DateTime.UtcNow;
            Metrics = new SolutionMetrics();
        }
    }

    /// <summary>
    /// Performance and resource usage metrics for a solution
    /// </summary>
    public class SolutionMetrics
    {
        public double AverageExecutionTimeMs { get; set; }

        public long PeakMemoryUsageBytes { get; set; }

        public int TestsPassed { get; set; }

        public int TestsFailed { get; set; }

        public int TotalTests { get; set; }

        public double SuccessRate => TotalTests > 0 ? (double)TestsPassed / TotalTests * 100 : 0;
    }
}
