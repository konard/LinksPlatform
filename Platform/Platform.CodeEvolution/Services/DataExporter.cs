using Platform.CodeEvolution.Models;
using Newtonsoft.Json;
using System.Text;

namespace Platform.CodeEvolution.Services;

public class DataExporter
{
    public async Task ExportToJsonAsync(string filePath, object data)
    {
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task ExportChangeFrequenciesToCsvAsync(string filePath, List<ChangeFrequency> frequencies)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Pattern,FromCode,ToCode,Count,Language,Dependencies,Confidence,FirstSeen,LastSeen,Authors");

        foreach (var freq in frequencies)
        {
            csv.AppendLine($"\"{EscapeCsv(freq.Pattern)}\"," +
                          $"\"{EscapeCsv(freq.FromCode)}\"," +
                          $"\"{EscapeCsv(freq.ToCode)}\"," +
                          $"{freq.Count}," +
                          $"\"{freq.Language}\"," +
                          $"\"{string.Join(";", freq.Dependencies)}\"," +
                          $"{freq.Confidence:F3}," +
                          $"{freq.FirstSeen:yyyy-MM-dd}," +
                          $"{freq.LastSeen:yyyy-MM-dd}," +
                          $"\"{string.Join(";", freq.Authors)}\"");
        }

        await File.WriteAllTextAsync(filePath, csv.ToString());
    }

    public async Task ExportRecommendationsToHtmlAsync(string filePath, List<CodeRecommendation> recommendations)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("<title>Code Evolution Recommendations</title>");
        html.AppendLine("<style>");
        html.AppendLine(@"
            body { font-family: Arial, sans-serif; margin: 20px; }
            .recommendation { border: 1px solid #ddd; margin: 10px 0; padding: 15px; border-radius: 5px; }
            .title { font-size: 18px; font-weight: bold; color: #333; }
            .description { margin: 10px 0; color: #666; }
            .code { background: #f5f5f5; padding: 10px; margin: 5px 0; border-radius: 3px; font-family: monospace; }
            .before { border-left: 3px solid #ff6b6b; }
            .after { border-left: 3px solid #51cf66; }
            .metadata { font-size: 12px; color: #888; margin-top: 10px; }
            .confidence { font-weight: bold; }
            .high-confidence { color: #51cf66; }
            .medium-confidence { color: #ffd43b; }
            .low-confidence { color: #ff6b6b; }
        ");
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("<h1>Code Evolution Recommendations</h1>");

        foreach (var rec in recommendations)
        {
            html.AppendLine("<div class=\"recommendation\">");
            html.AppendLine($"<div class=\"title\">{EscapeHtml(rec.Title)}</div>");
            html.AppendLine($"<div class=\"description\">{EscapeHtml(rec.Description)}</div>");

            if (!string.IsNullOrEmpty(rec.BeforeCode))
            {
                html.AppendLine("<div class=\"code before\">");
                html.AppendLine("<strong>Before:</strong><br>");
                html.AppendLine($"<pre>{EscapeHtml(rec.BeforeCode)}</pre>");
                html.AppendLine("</div>");
            }

            if (!string.IsNullOrEmpty(rec.AfterCode))
            {
                html.AppendLine("<div class=\"code after\">");
                html.AppendLine("<strong>After:</strong><br>");
                html.AppendLine($"<pre>{EscapeHtml(rec.AfterCode)}</pre>");
                html.AppendLine("</div>");
            }

            var confidenceClass = rec.Confidence > 0.7 ? "high-confidence" :
                                 rec.Confidence > 0.4 ? "medium-confidence" : "low-confidence";

            html.AppendLine("<div class=\"metadata\">");
            html.AppendLine($"<span class=\"confidence {confidenceClass}\">Confidence: {rec.Confidence:P}</span> | ");
            html.AppendLine($"Frequency: {rec.Frequency} | ");
            html.AppendLine($"Language: {rec.Language} | ");
            html.AppendLine($"Type: {rec.Type}");

            if (rec.Dependencies.Count > 0)
            {
                html.AppendLine($" | Dependencies: {string.Join(", ", rec.Dependencies)}");
            }

            html.AppendLine("</div>");

            if (rec.Alternatives.Count > 0)
            {
                html.AppendLine("<div style=\"margin-top: 10px;\">");
                html.AppendLine("<strong>Alternatives:</strong>");
                foreach (var alt in rec.Alternatives)
                {
                    html.AppendLine($"<div style=\"margin-left: 20px; padding: 5px; border-left: 2px solid #ddd;\">");
                    html.AppendLine($"<div>{EscapeHtml(alt.Title)}</div>");
                    if (!string.IsNullOrEmpty(alt.AfterCode))
                    {
                        html.AppendLine($"<pre style=\"font-size: 12px;\">{EscapeHtml(alt.AfterCode)}</pre>");
                    }
                    html.AppendLine("</div>");
                }
                html.AppendLine("</div>");
            }

            html.AppendLine("</div>");
        }

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        await File.WriteAllTextAsync(filePath, html.ToString());
    }

    public async Task<string> GenerateReportAsync(Dictionary<string, object> statistics, List<ChangeFrequency> topChanges, List<CodeRecommendation> recommendations)
    {
        var report = new StringBuilder();
        report.AppendLine("# Code Evolution Analysis Report");
        report.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();

        report.AppendLine("## Statistics");
        foreach (var stat in statistics)
        {
            report.AppendLine($"- **{stat.Key}**: {stat.Value}");
        }
        report.AppendLine();

        report.AppendLine("## Top Change Patterns");
        foreach (var change in topChanges.Take(10))
        {
            report.AppendLine($"### {change.Pattern}");
            report.AppendLine($"- **Frequency**: {change.Count}");
            report.AppendLine($"- **Language**: {change.Language}");
            report.AppendLine($"- **Confidence**: {change.Confidence:P}");
            report.AppendLine($"- **Authors**: {change.Authors.Count}");
            report.AppendLine();
        }

        report.AppendLine("## Top Recommendations");
        foreach (var rec in recommendations.Take(5))
        {
            report.AppendLine($"### {rec.Title}");
            report.AppendLine($"{rec.Description}");
            report.AppendLine($"- **Confidence**: {rec.Confidence:P}");
            report.AppendLine($"- **Frequency**: {rec.Frequency}");
            report.AppendLine($"- **Language**: {rec.Language}");
            report.AppendLine();
        }

        return report.ToString();
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "");
    }

    private string EscapeHtml(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Replace("&", "&amp;")
                   .Replace("<", "&lt;")
                   .Replace(">", "&gt;")
                   .Replace("\"", "&quot;")
                   .Replace("'", "&#39;");
    }
}