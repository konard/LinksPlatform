using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Platform.Examples
{
    /// <summary>
    /// Infers binary schema from multiple files
    /// </summary>
    public class SchemaInference
    {
        public BinarySchema InferSchemaFromFiles(string[] filePaths, string schemaName = "InferredSchema")
        {
            if (filePaths == null || filePaths.Length == 0)
            {
                throw new ArgumentException("At least one file is required for schema inference");
            }

            var dataList = filePaths.Select(File.ReadAllBytes).ToArray();
            return InferSchemaFromData(dataList, schemaName);
        }

        public BinarySchema InferSchemaFromData(byte[][] dataList, string schemaName = "InferredSchema")
        {
            if (dataList == null || dataList.Length == 0)
            {
                throw new ArgumentException("At least one data array is required for schema inference");
            }

            var schema = new BinarySchema(schemaName);

            // Find common magic bytes (header)
            var magicBytes = FindCommonPrefix(dataList);
            if (magicBytes != null && magicBytes.Length > 0)
            {
                schema.MagicBytes = magicBytes;
                schema.AddField(new BinaryField("MagicBytes", "hex", 0, magicBytes.Length, "File signature"));
            }

            // Find common file size
            var sizes = dataList.Select(d => d.Length).ToArray();
            var minSize = sizes.Min();
            var maxSize = sizes.Max();

            if (minSize == maxSize)
            {
                schema.Description = $"Fixed size format ({minSize} bytes)";
            }
            else
            {
                schema.Description = $"Variable size format ({minSize}-{maxSize} bytes)";
            }

            // Analyze byte patterns for fixed positions
            var fixedPositions = FindFixedBytePositions(dataList);
            int fieldIndex = 0;
            int currentOffset = magicBytes?.Length ?? 0;

            foreach (var position in fixedPositions.OrderBy(kvp => kvp.Key))
            {
                if (position.Key < currentOffset) continue;

                var offset = position.Key;
                var value = position.Value;

                // Try to group consecutive fixed bytes
                var groupSize = 1;
                while (fixedPositions.ContainsKey(offset + groupSize))
                {
                    groupSize++;
                }

                schema.AddField(new BinaryField(
                    $"Field_{fieldIndex}",
                    InferType(value, groupSize),
                    offset,
                    groupSize,
                    $"Fixed value at offset {offset}"
                ));

                fieldIndex++;
                currentOffset = offset + groupSize;
            }

            // Analyze variable positions
            var variableRegions = FindVariableRegions(dataList, fixedPositions);
            foreach (var region in variableRegions.OrderBy(r => r.Offset))
            {
                schema.AddField(new BinaryField(
                    $"VariableField_{fieldIndex}",
                    "hex",
                    region.Offset,
                    region.Size,
                    $"Variable data region ({region.Size} bytes)"
                ));
                fieldIndex++;
            }

            return schema;
        }

        private byte[] FindCommonPrefix(byte[][] dataList)
        {
            if (dataList.Length == 0) return null;

            var minLength = dataList.Min(d => d.Length);
            var prefixLength = 0;

            for (int i = 0; i < minLength; i++)
            {
                var firstByte = dataList[0][i];
                if (dataList.All(d => d[i] == firstByte))
                {
                    prefixLength = i + 1;
                }
                else
                {
                    break;
                }
            }

            if (prefixLength == 0) return null;

            var prefix = new byte[prefixLength];
            Array.Copy(dataList[0], prefix, prefixLength);
            return prefix;
        }

        private Dictionary<int, byte> FindFixedBytePositions(byte[][] dataList)
        {
            var fixedPositions = new Dictionary<int, byte>();
            var minLength = dataList.Min(d => d.Length);

            for (int i = 0; i < minLength; i++)
            {
                var firstByte = dataList[0][i];
                if (dataList.All(d => d[i] == firstByte))
                {
                    fixedPositions[i] = firstByte;
                }
            }

            return fixedPositions;
        }

        private class VariableRegion
        {
            public int Offset { get; set; }
            public int Size { get; set; }
        }

        private List<VariableRegion> FindVariableRegions(byte[][] dataList, Dictionary<int, byte> fixedPositions)
        {
            var regions = new List<VariableRegion>();
            var minLength = dataList.Min(d => d.Length);

            int regionStart = -1;
            for (int i = 0; i < minLength; i++)
            {
                if (!fixedPositions.ContainsKey(i))
                {
                    if (regionStart == -1)
                    {
                        regionStart = i;
                    }
                }
                else
                {
                    if (regionStart != -1)
                    {
                        regions.Add(new VariableRegion
                        {
                            Offset = regionStart,
                            Size = i - regionStart
                        });
                        regionStart = -1;
                    }
                }
            }

            // Handle region extending to end
            if (regionStart != -1)
            {
                regions.Add(new VariableRegion
                {
                    Offset = regionStart,
                    Size = minLength - regionStart
                });
            }

            return regions;
        }

        private string InferType(byte value, int size)
        {
            if (size == 1) return "uint8";
            if (size == 2) return "uint16";
            if (size == 4) return "uint32";
            if (size == 8) return "uint64";
            return "hex";
        }
    }
}
