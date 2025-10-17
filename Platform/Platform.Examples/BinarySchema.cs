using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Examples
{
    /// <summary>
    /// Represents a field in a binary schema
    /// </summary>
    public class BinaryField
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Offset { get; set; }
        public int Size { get; set; }
        public string Description { get; set; }
        public BinaryField[] SubFields { get; set; }

        public BinaryField(string name, string type, int offset, int size, string description = null, BinaryField[] subFields = null)
        {
            Name = name;
            Type = type;
            Offset = offset;
            Size = size;
            Description = description;
            SubFields = subFields;
        }
    }

    /// <summary>
    /// Represents a schema for binary file structure
    /// </summary>
    public class BinarySchema
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string FileExtension { get; set; }
        public byte[] MagicBytes { get; set; }
        public List<BinaryField> Fields { get; set; }

        public BinarySchema(string name, string description = null, string fileExtension = null, byte[] magicBytes = null)
        {
            Name = name;
            Description = description;
            FileExtension = fileExtension;
            MagicBytes = magicBytes;
            Fields = new List<BinaryField>();
        }

        public void AddField(BinaryField field)
        {
            Fields.Add(field);
        }

        public bool IsMatch(byte[] data)
        {
            if (MagicBytes == null || MagicBytes.Length == 0)
            {
                return false;
            }
            if (data.Length < MagicBytes.Length)
            {
                return false;
            }
            return MagicBytes.SequenceEqual(data.Take(MagicBytes.Length));
        }
    }
}
