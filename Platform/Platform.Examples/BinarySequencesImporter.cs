using System;
using System.IO;
using System.Collections.Generic;
using Platform.Data.Doublets;
using Platform.Data;

namespace Platform.Examples
{
    /// <summary>
    /// Imports sequences from a binary file format with two main sections:
    /// 1. Links section for duplicate data (reusable subsequences)
    /// 2. Sequences section for unique uncompressable sequences
    /// </summary>
    public class BinarySequencesImporter
    {
        protected SynchronizedLinks<ulong> _links;
        protected List<ulong> _importedReusableLinks;

        public void Import(SynchronizedLinks<ulong> links, string path)
        {
            _links = links;
            _importedReusableLinks = new List<ulong>();

            using (var file = File.OpenRead(path))
            using (var reader = new BinaryReader(file))
            {
                ReadBinaryFormat(reader);
            }
        }

        protected void ReadBinaryFormat(BinaryReader reader)
        {
            // Read and validate file header
            if (reader.ReadByte() != 'B' ||
                reader.ReadByte() != 'S' ||
                reader.ReadByte() != 'E' ||
                reader.ReadByte() != 'Q')
            {
                throw new InvalidDataException("Invalid file format: missing BSEQ magic bytes");
            }

            var version = reader.ReadUInt16();
            if (version != 1)
            {
                throw new InvalidDataException($"Unsupported format version: {version}");
            }

            // Section 1: Links section (duplicate/reusable data)
            ReadLinksSection(reader);

            // Section 2: Sequences section (unique uncompressable sequences)
            ReadSequencesSection(reader);
        }

        protected void ReadLinksSection(BinaryReader reader)
        {
            // Validate section header
            if (reader.ReadByte() != 'L' ||
                reader.ReadByte() != 'I' ||
                reader.ReadByte() != 'N' ||
                reader.ReadByte() != 'K')
            {
                throw new InvalidDataException("Invalid links section header");
            }

            // Read number of reusable links
            var count = ReadVariableLength(reader);

            // Read each reusable link
            for (ulong i = 0; i < count; i++)
            {
                var source = ReadVariableLength(reader);
                var target = ReadVariableLength(reader);

                // Resolve references if needed
                var resolvedSource = ResolveLinkReference(source);
                var resolvedTarget = ResolveLinkReference(target);

                // Create the link in the data store
                var linkIndex = _links.GetOrCreate(resolvedSource, resolvedTarget);
                _importedReusableLinks.Add(linkIndex);
            }
        }

        protected void ReadSequencesSection(BinaryReader reader)
        {
            // Validate section header
            if (reader.ReadByte() != 'S' ||
                reader.ReadByte() != 'E' ||
                reader.ReadByte() != 'Q' ||
                reader.ReadByte() != 'S')
            {
                throw new InvalidDataException("Invalid sequences section header");
            }

            // Read number of unique sequences
            var count = ReadVariableLength(reader);

            // Read each unique sequence
            for (ulong i = 0; i < count; i++)
            {
                var source = ReadVariableLength(reader);
                var target = ReadVariableLength(reader);

                // Resolve references if needed
                var resolvedSource = ResolveLinkReference(source);
                var resolvedTarget = ResolveLinkReference(target);

                // Create the link in the data store
                _links.GetOrCreate(resolvedSource, resolvedTarget);
            }
        }

        protected ulong ResolveLinkReference(ulong value)
        {
            // Check if high bit is set (indicates reference to links section)
            if ((value & 0x8000000000000000UL) != 0)
            {
                var index = value & 0x7FFFFFFFFFFFFFFFUL;
                if (index >= (ulong)_importedReusableLinks.Count)
                {
                    throw new InvalidDataException($"Invalid link reference: {index}");
                }
                return _importedReusableLinks[(int)index];
            }
            // Otherwise it's a direct link value
            return value;
        }

        /// <summary>
        /// Reads a variable-length encoded unsigned integer.
        /// Uses continuation bit scheme: if high bit is 1, more bytes follow.
        /// </summary>
        protected ulong ReadVariableLength(BinaryReader reader)
        {
            ulong result = 0;
            int shift = 0;
            byte b;

            do
            {
                b = reader.ReadByte();
                result |= (ulong)(b & 0x7F) << shift;
                shift += 7;

                if (shift > 63)
                {
                    throw new InvalidDataException("Variable length integer is too large");
                }
            } while ((b & 0x80) != 0);

            return result;
        }
    }
}
