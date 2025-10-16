using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Imports links from Universal Links String Format.
    /// Uses two base symbols: space (link part separator) and newline (link separator).
    /// This format can represent pair links, triple links, and sequences of any size.
    /// </summary>
    /// <remarks>
    /// Format specification:
    /// - Space character (' ') separates parts within a link (source and target)
    /// - Newline character ('\n') separates different links
    ///
    /// Example input:
    /// 1 2
    /// 2 3
    /// 3 1
    ///
    /// This creates or updates three links: (1→2), (2→3), (3→1)
    /// </remarks>
    public class UniversalLinksStringFormatImporter
    {
        private readonly SynchronizedLinks<ulong> _links;
        private readonly bool _useLineNumbersAsIndices;
        private readonly Dictionary<ulong, ulong> _lineNumberToAddress = new Dictionary<ulong, ulong>();

        /// <summary>
        /// Creates a new Universal Links String Format importer.
        /// </summary>
        /// <param name="links">The links store to import into.</param>
        /// <param name="useLineNumbersAsIndices">Whether the format uses line numbers as link references.</param>
        public UniversalLinksStringFormatImporter(SynchronizedLinks<ulong> links, bool useLineNumbersAsIndices = false)
        {
            _links = links;
            _useLineNumbersAsIndices = useLineNumbersAsIndices;
        }

        /// <summary>
        /// Imports links from a file in Universal Links String Format.
        /// </summary>
        /// <param name="path">The file path to import from.</param>
        /// <param name="cancellationToken">Cancellation token for the import operation.</param>
        public void Import(string path, CancellationToken cancellationToken)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File not found: {path}");
            }

            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            {
                ulong lineNumber = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    lineNumber++;

                    // Skip empty lines
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    // Parse the line: "source target"
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2)
                    {
                        throw new FormatException($"Invalid format at line {lineNumber}: expected 'source target', got '{line}'");
                    }

                    if (!ulong.TryParse(parts[0], out ulong source))
                    {
                        throw new FormatException($"Invalid source value at line {lineNumber}: '{parts[0]}'");
                    }

                    if (!ulong.TryParse(parts[1], out ulong target))
                    {
                        throw new FormatException($"Invalid target value at line {lineNumber}: '{parts[1]}'");
                    }

                    // Convert line numbers to actual addresses if needed
                    if (_useLineNumbersAsIndices)
                    {
                        if (_lineNumberToAddress.TryGetValue(source, out ulong sourceAddress))
                        {
                            source = sourceAddress;
                        }
                        if (_lineNumberToAddress.TryGetValue(target, out ulong targetAddress))
                        {
                            target = targetAddress;
                        }
                    }

                    // Create or update the link
                    var linkAddress = _links.GetOrCreate(source, target);

                    // Store line number mapping if needed
                    if (_useLineNumbersAsIndices)
                    {
                        _lineNumberToAddress[lineNumber] = linkAddress;
                    }
                }
            }
        }
    }
}
