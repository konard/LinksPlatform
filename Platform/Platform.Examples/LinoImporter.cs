using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Platform.Exceptions;
using Platform.IO;

namespace Platform.Examples
{
    /// <summary>
    /// Imports Lino (Links Notation) files into Links platform storage.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public class LinoImporter<TLink>
    {
        private readonly ILinoStorage<TLink> _storage;

        public LinoImporter(ILinoStorage<TLink> storage) => _storage = storage;

        /// <summary>
        /// Imports a Lino file into the storage.
        /// </summary>
        /// <param name="file">Path to the Lino file.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>A task representing the import operation.</returns>
        public Task Import(string file, CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    ConsoleHelpers.Debug("Starting Lino import from: {0}", file);

                    if (!File.Exists(file))
                    {
                        throw new FileNotFoundException($"Lino file not found: {file}");
                    }

                    var content = File.ReadAllText(file);
                    var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    int lineNumber = 0;
                    foreach (var line in lines)
                    {
                        if (token.IsCancellationRequested)
                        {
                            ConsoleHelpers.Debug("Import cancelled at line {0}", lineNumber);
                            return;
                        }

                        lineNumber++;
                        var trimmedLine = line.Trim();

                        // Skip empty lines and comments
                        if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                        {
                            continue;
                        }

                        try
                        {
                            ParseAndStoreLine(trimmedLine);

                            if (lineNumber % 100 == 0)
                            {
                                ConsoleHelpers.Debug("Processed {0} lines", lineNumber);
                            }
                        }
                        catch (Exception ex)
                        {
                            ConsoleHelpers.Debug("Error parsing line {0}: {1}", lineNumber, ex.Message);
                        }
                    }

                    Console.WriteLine($"Lino import completed. Processed {lineNumber} lines.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToStringWithAllInnerExceptions());
                }
            }, token);
        }

        private void ParseAndStoreLine(string line)
        {
            // Simple Lino parser implementation
            // Format examples:
            // 1. "identifier (value1 value2 value3)" - named link with values
            // 2. "(value1 value2)" - anonymous link
            // 3. "value1 value2" - simple doublet
            // 4. "identifier: targetIdentifier" - reference to existing link

            string id = null;
            string[] values;

            // Check for reference syntax (id: target)
            if (line.Contains(":") && !line.Contains("("))
            {
                var parts = line.Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    id = parts[0].Trim();
                    var targetId = parts[1].Trim();
                    var target = _storage.GetLink(targetId);
                    _storage.CreateReference(id, target);
                    return;
                }
            }

            // Check for parenthetical notation
            if (line.Contains("(") && line.Contains(")"))
            {
                var openParen = line.IndexOf('(');
                var closeParen = line.LastIndexOf(')');

                // Extract identifier if present
                if (openParen > 0)
                {
                    id = line.Substring(0, openParen).Trim();
                }

                // Extract values from parentheses
                var valuesString = line.Substring(openParen + 1, closeParen - openParen - 1);
                values = SplitValues(valuesString);
            }
            else
            {
                // Simple space-separated values
                values = SplitValues(line);
            }

            if (values.Length > 0)
            {
                _storage.CreateLink(id, values);
            }
        }

        private string[] SplitValues(string text)
        {
            // Split by whitespace but preserve quoted strings
            var result = new System.Collections.Generic.List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();

            foreach (var c in text)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
            {
                result.Add(current.ToString());
            }

            return result.ToArray();
        }
    }
}
