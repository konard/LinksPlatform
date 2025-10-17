using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Doublets;
using Platform.Data;

namespace Platform.Examples
{
    /// <summary>
    /// Exports sequences to a binary file format with two main sections:
    /// 1. Links section for duplicate data (reusable subsequences)
    /// 2. Sequences section for unique uncompressable sequences
    /// </summary>
    public class BinarySequencesExporter
    {
        protected SynchronizedLinks<ulong> _links;
        protected Dictionary<ulong, ulong> _linkToIndex;
        protected List<ulong> _reusableLinks;
        protected List<ulong> _uniqueSequences;

        public void Export(SynchronizedLinks<ulong> links, string path)
        {
            _links = links;
            _linkToIndex = new Dictionary<ulong, ulong>();
            _reusableLinks = new List<ulong>();
            _uniqueSequences = new List<ulong>();

            // Analyze links to determine which are reusable and which are unique
            AnalyzeLinks();

            // Write to binary file
            using (var file = File.Create(path))
            using (var writer = new BinaryWriter(file))
            {
                WriteBinaryFormat(writer);
            }
        }

        protected void AnalyzeLinks()
        {
            var linkUsageCount = new Dictionary<ulong, int>();

            // Count how many times each link is referenced
            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                var source = link[_links.Constants.SourcePart];
                var target = link[_links.Constants.TargetPart];

                // Count references
                if (!linkUsageCount.ContainsKey(source))
                    linkUsageCount[source] = 0;
                linkUsageCount[source]++;

                if (!linkUsageCount.ContainsKey(target))
                    linkUsageCount[target] = 0;
                linkUsageCount[target]++;

                return _links.Constants.Continue;
            });

            // Classify links based on usage
            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];

                // Links referenced more than once are reusable
                if (linkUsageCount.ContainsKey(linkIndex) && linkUsageCount[linkIndex] > 1)
                {
                    if (!_linkToIndex.ContainsKey(linkIndex))
                    {
                        _linkToIndex[linkIndex] = (ulong)_reusableLinks.Count;
                        _reusableLinks.Add(linkIndex);
                    }
                }
                else if (!_links.IsPartialPoint(linkIndex))
                {
                    _uniqueSequences.Add(linkIndex);
                }

                return _links.Constants.Continue;
            });
        }

        protected void WriteBinaryFormat(BinaryWriter writer)
        {
            // File header
            writer.Write((byte)'B');  // Magic bytes: "BSEQ"
            writer.Write((byte)'S');
            writer.Write((byte)'E');
            writer.Write((byte)'Q');
            writer.Write((ushort)1);  // Format version

            // Section 1: Links section (duplicate/reusable data)
            WriteLinksSection(writer);

            // Section 2: Sequences section (unique uncompressable sequences)
            WriteSequencesSection(writer);
        }

        protected void WriteLinksSection(BinaryWriter writer)
        {
            // Section header
            writer.Write((byte)'L');
            writer.Write((byte)'I');
            writer.Write((byte)'N');
            writer.Write((byte)'K');

            // Number of reusable links
            WriteVariableLength(writer, (ulong)_reusableLinks.Count);

            // Write each reusable link
            foreach (var linkIndex in _reusableLinks)
            {
                var link = _links.GetLink(linkIndex);
                var source = link[_links.Constants.SourcePart];
                var target = link[_links.Constants.TargetPart];

                // Write source and target as variable length values
                WriteVariableLength(writer, GetLinkReference(source));
                WriteVariableLength(writer, GetLinkReference(target));
            }
        }

        protected void WriteSequencesSection(BinaryWriter writer)
        {
            // Section header
            writer.Write((byte)'S');
            writer.Write((byte)'E');
            writer.Write((byte)'Q');
            writer.Write((byte)'S');

            // Number of unique sequences
            WriteVariableLength(writer, (ulong)_uniqueSequences.Count);

            // Write each unique sequence
            foreach (var linkIndex in _uniqueSequences)
            {
                var link = _links.GetLink(linkIndex);
                var source = link[_links.Constants.SourcePart];
                var target = link[_links.Constants.TargetPart];

                // Write source and target as variable length values
                WriteVariableLength(writer, GetLinkReference(source));
                WriteVariableLength(writer, GetLinkReference(target));
            }
        }

        protected ulong GetLinkReference(ulong linkIndex)
        {
            // If link is in reusable section, return its index with a flag
            if (_linkToIndex.ContainsKey(linkIndex))
            {
                return _linkToIndex[linkIndex] | 0x8000000000000000UL; // Set high bit to indicate reference
            }
            // Otherwise return the raw link value
            return linkIndex;
        }

        /// <summary>
        /// Writes a variable-length encoded unsigned integer.
        /// Uses continuation bit scheme: if high bit is 1, more bytes follow.
        /// </summary>
        protected void WriteVariableLength(BinaryWriter writer, ulong value)
        {
            while (value >= 0x80)
            {
                writer.Write((byte)((value & 0x7F) | 0x80));
                value >>= 7;
            }
            writer.Write((byte)value);
        }
    }
}
