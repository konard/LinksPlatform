using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences;
using Platform.Data;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// Exports Links platform storage to Lino (Links Notation) format.
    /// </summary>
    public class LinoExporter
    {
        protected SynchronizedLinks<ulong> _links;
        protected bool _unicodeMapped;
        protected bool _convertUnicodeLinksToText;
        protected HashSet<ulong> _visited;
        protected Dictionary<ulong, string> _linkToIdentifier;
        protected ulong _linksCounter;
        protected int _identifierCounter;

        /// <summary>
        /// Exports links to a Lino file.
        /// </summary>
        /// <param name="links">The synchronized links collection.</param>
        /// <param name="path">Output file path.</param>
        /// <param name="unicodeMapped">Whether unicode mapping is used.</param>
        /// <param name="convertUnicodeLinksToText">Whether to convert unicode sequences to text.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public void Export(SynchronizedLinks<ulong> links, string path, bool unicodeMapped, bool convertUnicodeLinksToText, CancellationToken cancellationToken)
        {
            _links = links;
            _unicodeMapped = unicodeMapped;
            _convertUnicodeLinksToText = convertUnicodeLinksToText;
            _visited = new HashSet<ulong>();
            _linkToIdentifier = new Dictionary<ulong, string>();
            _identifierCounter = 0;

            using (var file = File.OpenWrite(path))
            using (var writer = new StreamWriter(file, Encoding.UTF8))
            {
                writer.WriteLine("# Lino (Links Notation) Export");
                writer.WriteLine($"# Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine();

                _linksCounter = 1;
                _links.Each(link =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return _links.Constants.Break;
                    }

                    var linkIndex = link[_links.Constants.IndexPart];
                    if (!_unicodeMapped || _linksCounter > UnicodeMap.MapSize)
                    {
                        if (Visit(linkIndex))
                        {
                            WriteLink(writer, link);
                        }
                    }
                    _linksCounter++;

                    if (_linksCounter % 1000 == 0)
                    {
                        Console.WriteLine($"Exported {_linksCounter} links");
                    }

                    return _links.Constants.Continue;
                });

                Console.WriteLine($"Lino export completed. Total links exported: {_visited.Count}");
            }
        }

        protected bool Visit(ulong linkIndex)
        {
            return _visited.Add(linkIndex);
        }

        protected virtual void WriteLink(StreamWriter writer, IList<ulong> link)
        {
            var linkIndex = link[_links.Constants.IndexPart];
            var source = link[_links.Constants.SourcePart];
            var target = link[_links.Constants.TargetPart];

            var identifier = GetOrCreateIdentifier(linkIndex);
            var sourceStr = FormatLink(source);
            var targetStr = FormatLink(target);

            // Write in Lino format
            // Format: identifier (source target)
            writer.WriteLine($"{identifier} ({sourceStr} {targetStr})");
        }

        protected string GetOrCreateIdentifier(ulong linkIndex)
        {
            if (!_linkToIdentifier.TryGetValue(linkIndex, out var id))
            {
                id = $"link{_identifierCounter++}";
                _linkToIdentifier[linkIndex] = id;
            }
            return id;
        }

        protected string FormatLink(ulong link)
        {
            // Try to convert to unicode text if enabled
            if (_unicodeMapped && _convertUnicodeLinksToText && link <= UnicodeMap.MapSize)
            {
                var character = UnicodeMap.FromLinkToChar(link);
                return EscapeString(character.ToString());
            }

            // Check if it's a unicode sequence that can be converted to text
            if (_convertUnicodeLinksToText)
            {
                var text = TryConvertToUnicodeSequence(link);
                if (!string.IsNullOrEmpty(text))
                {
                    return EscapeString(text);
                }
            }

            // Otherwise, use link identifier or reference
            if (_linkToIdentifier.TryGetValue(link, out var id))
            {
                return id;
            }
            else
            {
                var newId = $"link{_identifierCounter++}";
                _linkToIdentifier[link] = newId;
                return newId;
            }
        }

        protected string TryConvertToUnicodeSequence(ulong link)
        {
            try
            {
                var linkValue = _links.GetLink(link);
                if (linkValue == null || linkValue.Count == 0)
                {
                    return null;
                }

                var sb = new StringBuilder();
                var current = link;
                var maxDepth = 1000; // Prevent infinite loops
                var depth = 0;

                while (depth < maxDepth)
                {
                    var linkData = _links.GetLink(current);
                    if (linkData == null || linkData.Count < 3)
                    {
                        break;
                    }

                    var source = linkData[_links.Constants.SourcePart];
                    var target = linkData[_links.Constants.TargetPart];

                    // Check if source is a unicode character
                    if (source <= UnicodeMap.MapSize)
                    {
                        sb.Append(UnicodeMap.FromLinkToChar(source));
                    }

                    // Move to target
                    if (target <= UnicodeMap.MapSize)
                    {
                        sb.Append(UnicodeMap.FromLinkToChar(target));
                        break;
                    }
                    else
                    {
                        current = target;
                        depth++;
                    }
                }

                return sb.Length > 0 ? sb.ToString() : null;
            }
            catch
            {
                return null;
            }
        }

        protected string EscapeString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "\"\"";
            }

            // If text contains spaces, parentheses, or special characters, quote it
            if (text.Contains(" ") || text.Contains("(") || text.Contains(")") ||
                text.Contains(":") || text.Contains("\"") || text.Contains("\n") ||
                text.Contains("\r") || text.Contains("\t"))
            {
                return "\"" + text.Replace("\"", "\\\"") + "\"";
            }

            return text;
        }
    }
}
