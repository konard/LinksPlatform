using System;
using System.Collections.Generic;

namespace Platform.BlackBox
{
    /// <summary>
    /// Represents a generated bug report with all relevant context.
    /// </summary>
    public class BugReport
    {
        /// <summary>
        /// When the bug report was generated.
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// Title/summary of the issue.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Detailed description of the issue.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// System information (OS, runtime, etc.).
        /// </summary>
        public Dictionary<string, string> SystemInfo { get; set; }

        /// <summary>
        /// Activity events leading up to the issue.
        /// </summary>
        public List<ActivityEvent> ActivityLog { get; set; }

        /// <summary>
        /// Statistics about the activity log.
        /// </summary>
        public BugReportStatistics Statistics { get; set; }

        public BugReport()
        {
            GeneratedAt = DateTime.UtcNow;
            SystemInfo = new Dictionary<string, string>();
            ActivityLog = new List<ActivityEvent>();
            Statistics = new BugReportStatistics();
        }
    }

    /// <summary>
    /// Statistics about the bug report.
    /// </summary>
    public class BugReportStatistics
    {
        public int TotalEvents { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int ExceptionCount { get; set; }
        public TimeSpan TimeSpanCovered { get; set; }
        public DateTime? FirstEventTime { get; set; }
        public DateTime? LastEventTime { get; set; }
    }
}
