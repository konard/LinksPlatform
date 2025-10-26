using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Platform.Numbers;
using Platform.Data.Doublets;
using Platform.Converters;

namespace Platform.Examples
{
    /// <summary>
    /// Implements compact snapshot storage using tiered address spaces.
    /// Links are reordered by usage frequency and stored using minimal space:
    /// - First 256 most frequent links: byte addresses (2 bytes per link)
    /// - Next 65,536 links: ushort addresses (4 bytes per link)
    /// - Next 4,294,967,296 links: uint addresses (8 bytes per link)
    /// - Remaining links: ulong addresses (16 bytes per link)
    /// </summary>
    /// <typeparam name="TLink">The type used to represent links</typeparam>
    public class CompactSnapshotStorage<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly Comparer<TLink> _comparer;

        // Address space boundaries
        private const int ByteAddressSpace = 256;
        private const int UShortAddressSpace = 65_536;
        private const long UIntAddressSpace = 4_294_967_296L;

        public CompactSnapshotStorage(ILinks<TLink> links)
        {
            _links = links;
            _comparer = Comparer<TLink>.Default;
        }

        /// <summary>
        /// Counts the usage frequency of each link in the storage.
        /// A link is "used" when it appears as Source or Target of another link.
        /// </summary>
        /// <returns>Dictionary mapping link addresses to their usage counts</returns>
        public Dictionary<TLink, long> CountLinkUsages()
        {
            var usageCounts = new Dictionary<TLink, long>();
            var linkAddresses = new List<TLink>();

            // Collect all link addresses
            _links.Each(link =>
            {
                linkAddresses.Add(_links.GetIndex(link));
                return _links.Constants.Continue;
            });

            // Count usages (how many times each link is referenced)
            foreach (var linkAddress in linkAddresses)
            {
                var source = _links.GetSource(linkAddress);
                var target = _links.GetTarget(linkAddress);

                // Count source usage
                if (!usageCounts.ContainsKey(source))
                    usageCounts[source] = 0;
                usageCounts[source]++;

                // Count target usage
                if (!usageCounts.ContainsKey(target))
                    usageCounts[target] = 0;
                usageCounts[target]++;
            }

            return usageCounts;
        }

        /// <summary>
        /// Reorders links by usage frequency (most used first).
        /// </summary>
        /// <param name="usageCounts">Usage frequency for each link</param>
        /// <returns>Ordered list of link addresses</returns>
        public List<TLink> ReorderLinksByUsage(Dictionary<TLink, long> usageCounts)
        {
            return usageCounts
                .OrderByDescending(kvp => kvp.Value)
                .ThenBy(kvp => kvp.Key, _comparer)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        /// <summary>
        /// Saves a compact snapshot to a stream.
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        public void SaveSnapshot(Stream stream)
        {
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                // Count link usages
                var usageCounts = CountLinkUsages();

                // Reorder links by usage
                var orderedLinks = ReorderLinksByUsage(usageCounts);

                // Write header
                WriteHeader(writer, orderedLinks.Count);

                // Write links in tiered format
                WriteLinksInTieredFormat(writer, orderedLinks);
            }
        }

        /// <summary>
        /// Loads a compact snapshot from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        public void LoadSnapshot(Stream stream)
        {
            using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                // Read header
                var totalLinks = ReadHeader(reader);

                // Read links in tiered format
                ReadLinksInTieredFormat(reader, totalLinks);
            }
        }

        private void WriteHeader(BinaryWriter writer, int totalLinks)
        {
            // Magic number for file format identification
            writer.Write("LCSS".ToCharArray()); // Links Compact Snapshot Storage

            // Version
            writer.Write((byte)1);

            // Total number of links
            writer.Write(totalLinks);
        }

        private int ReadHeader(BinaryReader reader)
        {
            // Read and validate magic number
            var magic = new string(reader.ReadChars(4));
            if (magic != "LCSS")
                throw new InvalidDataException("Invalid snapshot file format");

            // Read version
            var version = reader.ReadByte();
            if (version != 1)
                throw new InvalidDataException($"Unsupported snapshot version: {version}");

            // Read total links count
            return reader.ReadInt32();
        }

        private void WriteLinksInTieredFormat(BinaryWriter writer, List<TLink> orderedLinks)
        {
            var addressMapping = new Dictionary<TLink, TLink>();
            var zero = default(TLink);

            // Create address mapping (old address -> new compact address)
            for (int i = 0; i < orderedLinks.Count; i++)
            {
                var oldAddress = orderedLinks[i];
                var newAddress = Arithmetic.Add(zero, (TLink)(object)i);
                addressMapping[oldAddress] = newAddress;
            }

            // Write byte address space (first 256 links)
            int byteSpaceCount = System.Math.Min(orderedLinks.Count, ByteAddressSpace);
            writer.Write(byteSpaceCount);
            for (int i = 0; i < byteSpaceCount; i++)
            {
                WriteLinkInByteSpace(writer, orderedLinks[i], addressMapping);
            }

            // Write ushort address space (next 65,536 links)
            int ushortSpaceCount = System.Math.Min(System.Math.Max(0, orderedLinks.Count - ByteAddressSpace), UShortAddressSpace);
            writer.Write(ushortSpaceCount);
            for (int i = ByteAddressSpace; i < ByteAddressSpace + ushortSpaceCount; i++)
            {
                WriteLinkInUShortSpace(writer, orderedLinks[i], addressMapping);
            }

            // Write uint address space (next 4,294,967,296 links)
            long uintSpaceCount = System.Math.Min(System.Math.Max(0L, orderedLinks.Count - ByteAddressSpace - UShortAddressSpace), UIntAddressSpace);
            writer.Write((int)uintSpaceCount);
            for (long i = ByteAddressSpace + UShortAddressSpace; i < ByteAddressSpace + UShortAddressSpace + uintSpaceCount; i++)
            {
                WriteLinkInUIntSpace(writer, orderedLinks[(int)i], addressMapping);
            }

            // Write ulong address space (remaining links)
            long ulongSpaceCount = System.Math.Max(0L, orderedLinks.Count - ByteAddressSpace - UShortAddressSpace - uintSpaceCount);
            writer.Write(ulongSpaceCount);
            for (long i = ByteAddressSpace + UShortAddressSpace + uintSpaceCount; i < orderedLinks.Count; i++)
            {
                WriteLinkInULongSpace(writer, orderedLinks[(int)i], addressMapping);
            }
        }

        private void WriteLinkInByteSpace(BinaryWriter writer, TLink linkAddress, Dictionary<TLink, TLink> addressMapping)
        {
            var source = _links.GetSource(linkAddress);
            var target = _links.GetTarget(linkAddress);

            // Map to new addresses
            var newSource = addressMapping.ContainsKey(source) ? addressMapping[source] : source;
            var newTarget = addressMapping.ContainsKey(target) ? addressMapping[target] : target;

            // Write as bytes (1 byte each for source and target)
            writer.Write(ConvertToByte(newSource));
            writer.Write(ConvertToByte(newTarget));
        }

        private void WriteLinkInUShortSpace(BinaryWriter writer, TLink linkAddress, Dictionary<TLink, TLink> addressMapping)
        {
            var source = _links.GetSource(linkAddress);
            var target = _links.GetTarget(linkAddress);

            var newSource = addressMapping.ContainsKey(source) ? addressMapping[source] : source;
            var newTarget = addressMapping.ContainsKey(target) ? addressMapping[target] : target;

            // Write as ushorts (2 bytes each)
            writer.Write(ConvertToUShort(newSource));
            writer.Write(ConvertToUShort(newTarget));
        }

        private void WriteLinkInUIntSpace(BinaryWriter writer, TLink linkAddress, Dictionary<TLink, TLink> addressMapping)
        {
            var source = _links.GetSource(linkAddress);
            var target = _links.GetTarget(linkAddress);

            var newSource = addressMapping.ContainsKey(source) ? addressMapping[source] : source;
            var newTarget = addressMapping.ContainsKey(target) ? addressMapping[target] : target;

            // Write as uints (4 bytes each)
            writer.Write(ConvertToUInt(newSource));
            writer.Write(ConvertToUInt(newTarget));
        }

        private void WriteLinkInULongSpace(BinaryWriter writer, TLink linkAddress, Dictionary<TLink, TLink> addressMapping)
        {
            var source = _links.GetSource(linkAddress);
            var target = _links.GetTarget(linkAddress);

            var newSource = addressMapping.ContainsKey(source) ? addressMapping[source] : source;
            var newTarget = addressMapping.ContainsKey(target) ? addressMapping[target] : target;

            // Write as ulongs (8 bytes each)
            writer.Write(ConvertToULong(newSource));
            writer.Write(ConvertToULong(newTarget));
        }

        private void ReadLinksInTieredFormat(BinaryReader reader, int totalLinks)
        {
            // Read byte address space
            int byteSpaceCount = reader.ReadInt32();
            for (int i = 0; i < byteSpaceCount; i++)
            {
                ReadLinkInByteSpace(reader);
            }

            // Read ushort address space
            int ushortSpaceCount = reader.ReadInt32();
            for (int i = 0; i < ushortSpaceCount; i++)
            {
                ReadLinkInUShortSpace(reader);
            }

            // Read uint address space
            int uintSpaceCount = reader.ReadInt32();
            for (int i = 0; i < uintSpaceCount; i++)
            {
                ReadLinkInUIntSpace(reader);
            }

            // Read ulong address space
            long ulongSpaceCount = reader.ReadInt64();
            for (long i = 0; i < ulongSpaceCount; i++)
            {
                ReadLinkInULongSpace(reader);
            }
        }

        private void ReadLinkInByteSpace(BinaryReader reader)
        {
            var source = reader.ReadByte();
            var target = reader.ReadByte();

            var sourceLink = ConvertFromByte(source);
            var targetLink = ConvertFromByte(target);

            _links.GetOrCreate(sourceLink, targetLink);
        }

        private void ReadLinkInUShortSpace(BinaryReader reader)
        {
            var source = reader.ReadUInt16();
            var target = reader.ReadUInt16();

            var sourceLink = ConvertFromUShort(source);
            var targetLink = ConvertFromUShort(target);

            _links.GetOrCreate(sourceLink, targetLink);
        }

        private void ReadLinkInUIntSpace(BinaryReader reader)
        {
            var source = reader.ReadUInt32();
            var target = reader.ReadUInt32();

            var sourceLink = ConvertFromUInt(source);
            var targetLink = ConvertFromUInt(target);

            _links.GetOrCreate(sourceLink, targetLink);
        }

        private void ReadLinkInULongSpace(BinaryReader reader)
        {
            var source = reader.ReadUInt64();
            var target = reader.ReadUInt64();

            var sourceLink = ConvertFromULong(source);
            var targetLink = ConvertFromULong(target);

            _links.GetOrCreate(sourceLink, targetLink);
        }

        // Conversion helpers
        private byte ConvertToByte(TLink value)
        {
            return (byte)(object)value;
        }

        private ushort ConvertToUShort(TLink value)
        {
            return (ushort)(object)value;
        }

        private uint ConvertToUInt(TLink value)
        {
            return (uint)(object)value;
        }

        private ulong ConvertToULong(TLink value)
        {
            return (ulong)(object)value;
        }

        private TLink ConvertFromByte(byte value)
        {
            return (TLink)(object)value;
        }

        private TLink ConvertFromUShort(ushort value)
        {
            return (TLink)(object)value;
        }

        private TLink ConvertFromUInt(uint value)
        {
            return (TLink)(object)value;
        }

        private TLink ConvertFromULong(ulong value)
        {
            return (TLink)(object)value;
        }
    }
}
