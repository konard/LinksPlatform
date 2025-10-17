using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Platform.Examples
{
    /// <summary>
    /// Represents a difference between two binary files
    /// </summary>
    public class BinaryDifference
    {
        public long Offset { get; set; }
        public byte OldValue { get; set; }
        public byte NewValue { get; set; }

        public BinaryDifference(long offset, byte oldValue, byte newValue)
        {
            Offset = offset;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public override string ToString()
        {
            return $"Offset 0x{Offset:X8}: 0x{OldValue:X2} -> 0x{NewValue:X2} ({OldValue} -> {NewValue})";
        }
    }

    /// <summary>
    /// Compares and finds differences between binary files
    /// </summary>
    public class BinaryDiff
    {
        public List<BinaryDifference> CompareBinaryFiles(string file1Path, string file2Path)
        {
            if (!File.Exists(file1Path))
            {
                throw new FileNotFoundException($"File not found: {file1Path}");
            }
            if (!File.Exists(file2Path))
            {
                throw new FileNotFoundException($"File not found: {file2Path}");
            }

            var data1 = File.ReadAllBytes(file1Path);
            var data2 = File.ReadAllBytes(file2Path);

            return CompareBinaryData(data1, data2);
        }

        public List<BinaryDifference> CompareBinaryData(byte[] data1, byte[] data2)
        {
            var differences = new List<BinaryDifference>();
            var minLength = Math.Min(data1.Length, data2.Length);

            for (long i = 0; i < minLength; i++)
            {
                if (data1[i] != data2[i])
                {
                    differences.Add(new BinaryDifference(i, data1[i], data2[i]));
                }
            }

            // Handle size differences
            if (data1.Length != data2.Length)
            {
                var longerData = data1.Length > data2.Length ? data1 : data2;
                var isFirst = data1.Length > data2.Length;

                for (long i = minLength; i < longerData.Length; i++)
                {
                    if (isFirst)
                    {
                        differences.Add(new BinaryDifference(i, longerData[i], 0));
                    }
                    else
                    {
                        differences.Add(new BinaryDifference(i, 0, longerData[i]));
                    }
                }
            }

            return differences;
        }

        public Dictionary<string, object> CompareWithSchema(string file1Path, string file2Path, BinarySchema schema)
        {
            var data1 = File.ReadAllBytes(file1Path);
            var data2 = File.ReadAllBytes(file2Path);

            var mapper = new BinaryMapper(schema);
            var map1 = mapper.MapData(data1);
            var map2 = mapper.MapData(data2);

            var result = new Dictionary<string, object>
            {
                ["Schema"] = schema.Name,
                ["File1Size"] = data1.Length,
                ["File2Size"] = data2.Length,
                ["SizeDifference"] = data2.Length - data1.Length
            };

            var fieldDifferences = new Dictionary<string, object>();
            var fields1 = map1["Fields"] as Dictionary<string, object>;
            var fields2 = map2["Fields"] as Dictionary<string, object>;

            if (fields1 != null && fields2 != null)
            {
                foreach (var fieldName in fields1.Keys.Union(fields2.Keys))
                {
                    var value1 = fields1.ContainsKey(fieldName) ? fields1[fieldName] : null;
                    var value2 = fields2.ContainsKey(fieldName) ? fields2[fieldName] : null;

                    if (!Equals(value1, value2))
                    {
                        fieldDifferences[fieldName] = new Dictionary<string, object>
                        {
                            ["Old"] = value1,
                            ["New"] = value2
                        };
                    }
                }
            }

            result["FieldDifferences"] = fieldDifferences;
            result["ByteDifferences"] = CompareBinaryData(data1, data2);

            return result;
        }

        public string FormatDifferences(List<BinaryDifference> differences, int maxDifferences = 100)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Total differences: {differences.Count}");
            sb.AppendLine();

            var displayCount = Math.Min(differences.Count, maxDifferences);
            for (int i = 0; i < displayCount; i++)
            {
                sb.AppendLine(differences[i].ToString());
            }

            if (differences.Count > maxDifferences)
            {
                sb.AppendLine($"... and {differences.Count - maxDifferences} more differences");
            }

            return sb.ToString();
        }
    }
}
