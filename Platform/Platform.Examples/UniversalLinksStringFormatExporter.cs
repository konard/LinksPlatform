using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// Exports links in Universal Links String Format.
    /// Uses two base symbols: space (link part separator) and newline (link separator).
    /// This format can represent pair links, triple links, and sequences of any size.
    /// </summary>
    /// <remarks>
    /// Format specification:
    /// - Space character (' ') separates parts within a link (source and target)
    /// - Newline character ('\n') separates different links
    ///
    /// Example:
    /// 1 2
    /// 2 3
    /// 3 1
    ///
    /// This represents three links: (1→2), (2→3), (3→1)
    ///
    /// The format is scalable:
    /// - For pair links (doublets): "source target"
    /// - For sequences: links can reference other links, building complex structures
    /// - Can be scaled down to Unicode-like single reference sequences
    /// </remarks>
    public class UniversalLinksStringFormatExporter
    {
        protected SynchronizedLinks<ulong> _links;
        protected bool _unicodeMapped;
        protected bool _convertUnicodeLinksToCharacters;
        protected bool _referenceByLines;
        protected HashSet<ulong> _visited;
        protected Dictionary<ulong, ulong> _addressToLineNumber = new Dictionary<ulong, ulong>();
        protected ulong _linksCounter;
        protected ulong _linesCounter;

        /// <summary>
        /// Exports links to Universal Links String Format.
        /// </summary>
        /// <param name="links">The links store to export from.</param>
        /// <param name="path">The file path to export to.</param>
        /// <param name="unicodeMapped">Whether Unicode characters are mapped to links.</param>
        /// <param name="convertUnicodeLinksToCharacters">Whether to convert Unicode link indices to their character representations.</param>
        /// <param name="referenceByLines">Whether to reference links by their line numbers instead of addresses.</param>
        /// <param name="cancellationToken">Cancellation token for the export operation.</param>
        public void Export(SynchronizedLinks<ulong> links, string path, bool unicodeMapped, bool convertUnicodeLinksToCharacters, bool referenceByLines, CancellationToken cancellationToken)
        {
            _links = links;
            _unicodeMapped = unicodeMapped;
            _convertUnicodeLinksToCharacters = convertUnicodeLinksToCharacters;
            _referenceByLines = referenceByLines;
            _visited = new HashSet<ulong>();
            using (var file = File.OpenWrite(path))
            using (var writer = new StreamWriter(file, Encoding.UTF8))
            {
                _linksCounter = 1;
                _linesCounter = 0;
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
                    return _links.Constants.Continue;
                });
            }
        }

        protected bool Visit(ulong linkIndex)
        {
            var result = _visited.Add(linkIndex);
            if (result)
            {
                _addressToLineNumber.Add(linkIndex, _linesCounter + 1);
            }
            return result;
        }

        protected virtual void WriteLink(StreamWriter writer, IList<ulong> link)
        {
            // Universal Links String Format: "source target\n"
            // Space is the link part separator, newline is the link separator
            writer.Write(FormatLink(link[_links.Constants.SourcePart]));
            writer.Write(' '); // Link part separator
            writer.WriteLine(FormatLink(link[_links.Constants.TargetPart]));
            _linesCounter++;
        }

        protected string FormatLink(ulong link)
        {
            if (_unicodeMapped && _convertUnicodeLinksToCharacters && link <= UnicodeMap.MapSize)
            {
                var character = UnicodeMap.FromLinkToChar(link);
                // For special characters that might interfere with format, use link index
                if (character == ' ' || character == '\n' || character == '\r')
                {
                    if (_referenceByLines)
                    {
                        link = _addressToLineNumber[link];
                    }
                    return link.ToString();
                }
                return character.ToString();
            }
            if (_referenceByLines)
            {
                link = _addressToLineNumber[link];
            }
            return link.ToString();
        }
    }
}
