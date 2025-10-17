using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Platform.BlackBox
{
    /// <summary>
    /// Default implementation of bug report generator.
    /// </summary>
    public class BugReportGenerator : IBugReportGenerator
    {
        public BugReport Generate(IEnumerable<ActivityEvent> events, string title = null, string description = null)
        {
            var eventList = events?.ToList() ?? new List<ActivityEvent>();

            var report = new BugReport
            {
                Title = title ?? "Automated Bug Report",
                Description = description ?? "This bug report was automatically generated from activity logs.",
                ActivityLog = eventList
            };

            // Collect system information
            report.SystemInfo["Platform"] = Environment.OSVersion.Platform.ToString();
            report.SystemInfo["OSVersion"] = Environment.OSVersion.VersionString;
            report.SystemInfo["MachineName"] = Environment.MachineName;
            report.SystemInfo["ProcessorCount"] = Environment.ProcessorCount.ToString();
            report.SystemInfo["CLR_Version"] = Environment.Version.ToString();
            report.SystemInfo["Is64BitOS"] = Environment.Is64BitOperatingSystem.ToString();
            report.SystemInfo["Is64BitProcess"] = Environment.Is64BitProcess.ToString();
            report.SystemInfo["WorkingSet"] = $"{Environment.WorkingSet / 1024 / 1024} MB";

            // Calculate statistics
            report.Statistics.TotalEvents = eventList.Count;
            report.Statistics.ErrorCount = eventList.Count(e => e.Severity == "Error" || e.Severity == "Critical");
            report.Statistics.WarningCount = eventList.Count(e => e.Severity == "Warning");
            report.Statistics.ExceptionCount = eventList.Count(e => e.EventType == "Exception");

            if (eventList.Any())
            {
                report.Statistics.FirstEventTime = eventList.Min(e => e.Timestamp);
                report.Statistics.LastEventTime = eventList.Max(e => e.Timestamp);
                report.Statistics.TimeSpanCovered = report.Statistics.LastEventTime.Value - report.Statistics.FirstEventTime.Value;
            }

            return report;
        }

        public string Export(BugReport report, BugReportFormat format = BugReportFormat.Markdown)
        {
            return format switch
            {
                BugReportFormat.Markdown => ExportAsMarkdown(report),
                BugReportFormat.Json => ExportAsJson(report),
                BugReportFormat.PlainText => ExportAsPlainText(report),
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };
        }

        private string ExportAsMarkdown(BugReport report)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"# {report.Title}");
            sb.AppendLine();
            sb.AppendLine($"**Generated:** {report.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}");
            sb.AppendLine();

            sb.AppendLine("## Description");
            sb.AppendLine(report.Description);
            sb.AppendLine();

            sb.AppendLine("## Statistics");
            sb.AppendLine($"- **Total Events:** {report.Statistics.TotalEvents}");
            sb.AppendLine($"- **Errors:** {report.Statistics.ErrorCount}");
            sb.AppendLine($"- **Warnings:** {report.Statistics.WarningCount}");
            sb.AppendLine($"- **Exceptions:** {report.Statistics.ExceptionCount}");
            if (report.Statistics.FirstEventTime.HasValue)
            {
                sb.AppendLine($"- **Time Range:** {report.Statistics.FirstEventTime:yyyy-MM-dd HH:mm:ss} - {report.Statistics.LastEventTime:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"- **Duration:** {report.Statistics.TimeSpanCovered}");
            }
            sb.AppendLine();

            sb.AppendLine("## System Information");
            foreach (var kvp in report.SystemInfo.OrderBy(x => x.Key))
            {
                sb.AppendLine($"- **{kvp.Key}:** {kvp.Value}");
            }
            sb.AppendLine();

            sb.AppendLine("## Activity Log");
            sb.AppendLine();

            if (report.ActivityLog.Any())
            {
                // Group by severity and show errors first
                var errorEvents = report.ActivityLog.Where(e => e.Severity == "Error" || e.Severity == "Critical").ToList();
                var warningEvents = report.ActivityLog.Where(e => e.Severity == "Warning").ToList();
                var infoEvents = report.ActivityLog.Where(e => e.Severity == "Info").ToList();

                if (errorEvents.Any())
                {
                    sb.AppendLine("### Errors and Critical Issues");
                    foreach (var evt in errorEvents)
                    {
                        AppendEventMarkdown(sb, evt);
                    }
                    sb.AppendLine();
                }

                if (warningEvents.Any())
                {
                    sb.AppendLine("### Warnings");
                    foreach (var evt in warningEvents)
                    {
                        AppendEventMarkdown(sb, evt);
                    }
                    sb.AppendLine();
                }

                sb.AppendLine("### Full Timeline");
                foreach (var evt in report.ActivityLog.OrderBy(e => e.Timestamp))
                {
                    sb.AppendLine($"- `{evt.Timestamp:HH:mm:ss.fff}` **[{evt.Severity}]** [{evt.EventType}] {evt.Source}: {evt.Description}");
                }
            }
            else
            {
                sb.AppendLine("*No activity events recorded.*");
            }

            return sb.ToString();
        }

        private void AppendEventMarkdown(StringBuilder sb, ActivityEvent evt)
        {
            sb.AppendLine($"#### {evt.Timestamp:yyyy-MM-dd HH:mm:ss.fff} - {evt.Source}");
            sb.AppendLine($"- **Type:** {evt.EventType}");
            sb.AppendLine($"- **Description:** {evt.Description}");
            if (!string.IsNullOrEmpty(evt.Metadata))
            {
                sb.AppendLine($"- **Metadata:** {evt.Metadata}");
            }
            if (!string.IsNullOrEmpty(evt.StackTrace))
            {
                sb.AppendLine("- **Stack Trace:**");
                sb.AppendLine("```");
                sb.AppendLine(evt.StackTrace);
                sb.AppendLine("```");
            }
            sb.AppendLine();
        }

        private string ExportAsJson(BugReport report)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(report, options);
        }

        private string ExportAsPlainText(BugReport report)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"BUG REPORT: {report.Title}");
            sb.AppendLine($"Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}");
            sb.AppendLine(new string('=', 80));
            sb.AppendLine();

            sb.AppendLine("DESCRIPTION:");
            sb.AppendLine(report.Description);
            sb.AppendLine();

            sb.AppendLine("STATISTICS:");
            sb.AppendLine($"  Total Events: {report.Statistics.TotalEvents}");
            sb.AppendLine($"  Errors: {report.Statistics.ErrorCount}");
            sb.AppendLine($"  Warnings: {report.Statistics.WarningCount}");
            sb.AppendLine($"  Exceptions: {report.Statistics.ExceptionCount}");
            if (report.Statistics.FirstEventTime.HasValue)
            {
                sb.AppendLine($"  Time Range: {report.Statistics.FirstEventTime:yyyy-MM-dd HH:mm:ss} - {report.Statistics.LastEventTime:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"  Duration: {report.Statistics.TimeSpanCovered}");
            }
            sb.AppendLine();

            sb.AppendLine("SYSTEM INFORMATION:");
            foreach (var kvp in report.SystemInfo.OrderBy(x => x.Key))
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
            sb.AppendLine();

            sb.AppendLine("ACTIVITY LOG:");
            sb.AppendLine(new string('-', 80));
            foreach (var evt in report.ActivityLog.OrderBy(e => e.Timestamp))
            {
                sb.AppendLine(evt.ToString());
                if (!string.IsNullOrEmpty(evt.StackTrace))
                {
                    sb.AppendLine($"  Stack Trace: {evt.StackTrace}");
                }
            }

            return sb.ToString();
        }
    }
}
