using System;
using System.IO;
using System.Threading;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Packs links database into a compact binary format for backup and transfer.
    /// File format: [Header][Link Records]
    /// Header: 1 byte format version, 1 byte bytes per index
    /// Link Records: repeated [Index][Source][Target] triplets
    /// </summary>
    public class LinksPacker
    {
        private const byte CurrentFormatVersion = 1;
        private const byte PairsFormat = 2; // Indicates doublets (pairs)

        protected SynchronizedLinks<ulong> _links;

        /// <summary>
        /// Packs the links database into a compact binary file.
        /// </summary>
        /// <param name="links">The links database to pack</param>
        /// <param name="path">Output file path</param>
        /// <param name="bytesPerIndex">Number of bytes per index (1-8), 0 for automatic detection</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public void Pack(SynchronizedLinks<ulong> links, string path, byte bytesPerIndex, CancellationToken cancellationToken)
        {
            _links = links;

            // Auto-detect bytes per index if not specified
            if (bytesPerIndex == 0)
            {
                bytesPerIndex = DetectBytesPerIndex();
            }

            // Validate bytes per index
            if (bytesPerIndex < 1 || bytesPerIndex > 8)
            {
                throw new ArgumentException("Bytes per index must be between 1 and 8", nameof(bytesPerIndex));
            }

            using (var file = File.Create(path))
            using (var writer = new BinaryWriter(file))
            {
                // Write header
                writer.Write(CurrentFormatVersion);
                writer.Write(PairsFormat);
                writer.Write(bytesPerIndex);

                // Write total count (for validation/progress)
                var totalCount = _links.Count();
                WriteUInt64(writer, totalCount, bytesPerIndex);

                // Write all links
                _links.Each(link =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return _links.Constants.Break;
                    }

                    var index = link[_links.Constants.IndexPart];
                    var source = link[_links.Constants.SourcePart];
                    var target = link[_links.Constants.TargetPart];

                    // Write: Index, Source, Target
                    WriteUInt64(writer, index, bytesPerIndex);
                    WriteUInt64(writer, source, bytesPerIndex);
                    WriteUInt64(writer, target, bytesPerIndex);

                    return _links.Constants.Continue;
                });
            }
        }

        /// <summary>
        /// Detects the minimum number of bytes needed to store all indices.
        /// </summary>
        private byte DetectBytesPerIndex()
        {
            ulong maxIndex = 0;

            _links.Each(link =>
            {
                var index = link[_links.Constants.IndexPart];
                if (index > maxIndex)
                {
                    maxIndex = index;
                }
                return _links.Constants.Continue;
            });

            // Determine bytes needed
            if (maxIndex <= byte.MaxValue) return 1;
            if (maxIndex <= ushort.MaxValue) return 2;
            if (maxIndex <= 0xFFFFFF) return 3;
            if (maxIndex <= uint.MaxValue) return 4;
            if (maxIndex <= 0xFFFFFFFFFF) return 5;
            if (maxIndex <= 0xFFFFFFFFFFFF) return 6;
            if (maxIndex <= 0xFFFFFFFFFFFFFF) return 7;
            return 8;
        }

        /// <summary>
        /// Writes a UInt64 value using the specified number of bytes.
        /// </summary>
        private void WriteUInt64(BinaryWriter writer, ulong value, byte bytesPerIndex)
        {
            for (int i = 0; i < bytesPerIndex; i++)
            {
                writer.Write((byte)(value & 0xFF));
                value >>= 8;
            }
        }
    }
}
