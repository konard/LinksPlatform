using System;

namespace Platform.Examples
{
    /// <summary>
    /// Provides common binary file format schemas
    /// </summary>
    public static class CommonBinarySchemas
    {
        public static BinarySchema PngSchema()
        {
            var schema = new BinarySchema(
                "PNG Image",
                "Portable Network Graphics image format",
                ".png",
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } // PNG signature
            );

            schema.AddField(new BinaryField("Signature", "hex", 0, 8, "PNG file signature"));
            schema.AddField(new BinaryField("IHDRLength", "uint32", 8, 4, "IHDR chunk length"));
            schema.AddField(new BinaryField("IHDRType", "ascii", 12, 4, "IHDR chunk type"));
            schema.AddField(new BinaryField("Width", "uint32", 16, 4, "Image width"));
            schema.AddField(new BinaryField("Height", "uint32", 20, 4, "Image height"));
            schema.AddField(new BinaryField("BitDepth", "uint8", 24, 1, "Bit depth"));
            schema.AddField(new BinaryField("ColorType", "uint8", 25, 1, "Color type"));
            schema.AddField(new BinaryField("Compression", "uint8", 26, 1, "Compression method"));
            schema.AddField(new BinaryField("Filter", "uint8", 27, 1, "Filter method"));
            schema.AddField(new BinaryField("Interlace", "uint8", 28, 1, "Interlace method"));

            return schema;
        }

        public static BinarySchema BmpSchema()
        {
            var schema = new BinarySchema(
                "BMP Image",
                "Bitmap image format",
                ".bmp",
                new byte[] { 0x42, 0x4D } // "BM"
            );

            schema.AddField(new BinaryField("Signature", "ascii", 0, 2, "BMP signature (BM)"));
            schema.AddField(new BinaryField("FileSize", "uint32", 2, 4, "File size in bytes"));
            schema.AddField(new BinaryField("Reserved1", "uint16", 6, 2, "Reserved"));
            schema.AddField(new BinaryField("Reserved2", "uint16", 8, 2, "Reserved"));
            schema.AddField(new BinaryField("DataOffset", "uint32", 10, 4, "Offset to image data"));
            schema.AddField(new BinaryField("HeaderSize", "uint32", 14, 4, "DIB header size"));
            schema.AddField(new BinaryField("Width", "int32", 18, 4, "Image width"));
            schema.AddField(new BinaryField("Height", "int32", 22, 4, "Image height"));
            schema.AddField(new BinaryField("Planes", "uint16", 26, 2, "Color planes"));
            schema.AddField(new BinaryField("BitsPerPixel", "uint16", 28, 2, "Bits per pixel"));

            return schema;
        }

        public static BinarySchema PdfSchema()
        {
            var schema = new BinarySchema(
                "PDF Document",
                "Portable Document Format",
                ".pdf",
                new byte[] { 0x25, 0x50, 0x44, 0x46 } // "%PDF"
            );

            schema.AddField(new BinaryField("Header", "ascii", 0, 8, "PDF header"));
            schema.AddField(new BinaryField("Version", "ascii", 5, 3, "PDF version (e.g., 1.4)"));

            return schema;
        }

        public static BinarySchema ZipSchema()
        {
            var schema = new BinarySchema(
                "ZIP Archive",
                "ZIP compressed archive format",
                ".zip",
                new byte[] { 0x50, 0x4B, 0x03, 0x04 } // PK signature
            );

            schema.AddField(new BinaryField("Signature", "hex", 0, 4, "ZIP signature (PK)"));
            schema.AddField(new BinaryField("Version", "uint16", 4, 2, "Version needed to extract"));
            schema.AddField(new BinaryField("Flags", "uint16", 6, 2, "General purpose bit flag"));
            schema.AddField(new BinaryField("Compression", "uint16", 8, 2, "Compression method"));
            schema.AddField(new BinaryField("ModTime", "uint16", 10, 2, "Last mod file time"));
            schema.AddField(new BinaryField("ModDate", "uint16", 12, 2, "Last mod file date"));
            schema.AddField(new BinaryField("CRC32", "uint32", 14, 4, "CRC-32"));
            schema.AddField(new BinaryField("CompressedSize", "uint32", 18, 4, "Compressed size"));
            schema.AddField(new BinaryField("UncompressedSize", "uint32", 22, 4, "Uncompressed size"));
            schema.AddField(new BinaryField("FileNameLength", "uint16", 26, 2, "File name length"));
            schema.AddField(new BinaryField("ExtraFieldLength", "uint16", 28, 2, "Extra field length"));

            return schema;
        }

        public static BinarySchema ElfSchema()
        {
            var schema = new BinarySchema(
                "ELF Executable",
                "Executable and Linkable Format",
                ".elf",
                new byte[] { 0x7F, 0x45, 0x4C, 0x46 } // 0x7F "ELF"
            );

            schema.AddField(new BinaryField("Magic", "hex", 0, 4, "ELF magic number"));
            schema.AddField(new BinaryField("Class", "uint8", 4, 1, "32/64-bit (1=32-bit, 2=64-bit)"));
            schema.AddField(new BinaryField("Data", "uint8", 5, 1, "Endianness (1=little, 2=big)"));
            schema.AddField(new BinaryField("Version", "uint8", 6, 1, "ELF version"));
            schema.AddField(new BinaryField("OSABI", "uint8", 7, 1, "OS/ABI identification"));
            schema.AddField(new BinaryField("ABIVersion", "uint8", 8, 1, "ABI version"));
            schema.AddField(new BinaryField("Padding", "hex", 9, 7, "Padding bytes"));
            schema.AddField(new BinaryField("Type", "uint16", 16, 2, "Object file type"));
            schema.AddField(new BinaryField("Machine", "uint16", 18, 2, "Machine architecture"));

            return schema;
        }

        public static BinarySchema GetSchemaByExtension(string extension)
        {
            switch (extension?.ToLowerInvariant())
            {
                case ".png": return PngSchema();
                case ".bmp": return BmpSchema();
                case ".pdf": return PdfSchema();
                case ".zip": return ZipSchema();
                case ".elf": return ElfSchema();
                default: return null;
            }
        }

        public static BinarySchema DetectSchema(byte[] data)
        {
            var schemas = new[] { PngSchema(), BmpSchema(), PdfSchema(), ZipSchema(), ElfSchema() };

            foreach (var schema in schemas)
            {
                if (schema.IsMatch(data))
                {
                    return schema;
                }
            }

            return null;
        }
    }
}
