using System;
using System.IO;
using System.Threading;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Unpacks links from a compact binary format created by LinksPacker.
    /// </summary>
    public class LinksUnpacker
    {
        private const byte SupportedFormatVersion = 1;
        private const byte PairsFormat = 2;

        protected SynchronizedLinks<ulong> _links;

        /// <summary>
        /// Unpacks links from a binary file into the links database.
        /// </summary>
        /// <param name="links">The links database to populate</param>
        /// <param name="path">Input file path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of links restored</returns>
        public ulong Unpack(SynchronizedLinks<ulong> links, string path, CancellationToken cancellationToken)
        {
            _links = links;

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Pack file not found", path);
            }

            using (var file = File.OpenRead(path))
            using (var reader = new BinaryReader(file))
            {
                // Read and validate header
                var formatVersion = reader.ReadByte();
                if (formatVersion != SupportedFormatVersion)
                {
                    throw new NotSupportedException($"Format version {formatVersion} is not supported");
                }

                var formatType = reader.ReadByte();
                if (formatType != PairsFormat)
                {
                    throw new NotSupportedException($"Format type {formatType} is not supported (only pairs/doublets)");
                }

                var bytesPerIndex = reader.ReadByte();
                if (bytesPerIndex < 1 || bytesPerIndex > 8)
                {
                    throw new InvalidDataException($"Invalid bytes per index: {bytesPerIndex}");
                }

                // Read expected count
                var expectedCount = ReadUInt64(reader, bytesPerIndex);
                ulong restoredCount = 0;

                // Read and restore links
                while (file.Position < file.Length)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var index = ReadUInt64(reader, bytesPerIndex);
                    var source = ReadUInt64(reader, bytesPerIndex);
                    var target = ReadUInt64(reader, bytesPerIndex);

                    // Create or update the link
                    // Note: We use GetOrCreate to handle the case where links might already exist
                    _links.GetOrCreate(source, target);
                    restoredCount++;
                }

                // Validate count
                if (restoredCount != expectedCount)
                {
                    throw new InvalidDataException($"Expected {expectedCount} links but restored {restoredCount}");
                }

                return restoredCount;
            }
        }

        /// <summary>
        /// Reads a UInt64 value using the specified number of bytes.
        /// </summary>
        private ulong ReadUInt64(BinaryReader reader, byte bytesPerIndex)
        {
            ulong value = 0;
            for (int i = 0; i < bytesPerIndex; i++)
            {
                value |= (ulong)reader.ReadByte() << (i * 8);
            }
            return value;
        }
    }
}
