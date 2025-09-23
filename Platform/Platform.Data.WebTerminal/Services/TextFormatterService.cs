using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Platform.Data.WebTerminal.Services
{
    public class TextFormatterService : ITextFormatterService
    {
        public string FormatErrorText(string errorText)
        {
            if (string.IsNullOrWhiteSpace(errorText))
                return errorText;

            // Replace common error patterns with line breaks
            var formatted = errorText;

            // Add line breaks before common error patterns
            formatted = Regex.Replace(formatted, @"(\s*Error\s*:)", "\n$1", RegexOptions.IgnoreCase);
            formatted = Regex.Replace(formatted, @"(\s*Error\s+running)", "\n$1", RegexOptions.IgnoreCase);
            formatted = Regex.Replace(formatted, @"(\s*/bin/sh\s*:\s*\d+\s*:)", "\n$1");

            // Add line break before periods that start new sentences
            formatted = Regex.Replace(formatted, @"(\s*\.\s*)([A-Z])", "\n$1\n$2");

            // Clean up multiple spaces and newlines
            formatted = Regex.Replace(formatted, @"\s+", " ");
            formatted = Regex.Replace(formatted, @"\n\s+", "\n");
            formatted = Regex.Replace(formatted, @"\n+", "\n");

            // Split into lines and clean each line
            var lines = formatted.Split('\n')
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line))
                .ToArray();

            return string.Join("\n", lines);
        }

        public string FormatJson(string jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText))
                return jsonText;

            try
            {
                // Parse and reformat the JSON
                var parsedJson = JToken.Parse(jsonText);
                return parsedJson.ToString(Formatting.Indented);
            }
            catch (JsonReaderException)
            {
                // If it's not valid JSON, return the original text
                return jsonText;
            }
        }

        public string FormatJsonWithStringFormatting(string jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText))
                return jsonText;

            try
            {
                // First format the JSON
                var formattedJson = FormatJson(jsonText);

                // Then find and format string values that might contain error messages
                var stringPattern = @"""([^""\\]*(\\.[^""\\]*)*)""(\s*:\s*""([^""\\]*(\\.[^""\\]*)*)"")";

                return Regex.Replace(formattedJson, stringPattern, match =>
                {
                    var key = match.Groups[1].Value;
                    var value = match.Groups[4].Value;

                    // If the value looks like an error message, format it
                    if (value.Contains("Error") || value.Contains("/bin/sh") || value.Length > 100)
                    {
                        var formattedValue = FormatErrorText(value);
                        var escapedValue = JsonConvert.SerializeObject(formattedValue).Trim('"');
                        return $@"""{key}"": ""{escapedValue}""";
                    }

                    return match.Value;
                });
            }
            catch (Exception)
            {
                return jsonText;
            }
        }
    }
}