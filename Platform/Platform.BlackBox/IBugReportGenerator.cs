using System;
using System.Collections.Generic;

namespace Platform.BlackBox
{
    /// <summary>
    /// Interface for generating bug reports from activity logs.
    /// </summary>
    public interface IBugReportGenerator
    {
        /// <summary>
        /// Generates a bug report from the current activity log.
        /// </summary>
        BugReport Generate(IEnumerable<ActivityEvent> events, string title = null, string description = null);

        /// <summary>
        /// Exports the bug report to a string format (markdown, JSON, etc.).
        /// </summary>
        string Export(BugReport report, BugReportFormat format = BugReportFormat.Markdown);
    }

    /// <summary>
    /// Supported bug report export formats.
    /// </summary>
    public enum BugReportFormat
    {
        Markdown,
        Json,
        PlainText
    }
}
