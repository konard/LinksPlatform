using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Platform.Examples
{
    /// <summary>
    /// Provides functionality to convert text files to UTF-8 encoding without BOM (byte order mark).
    /// Automatically detects the source encoding and converts files recursively.
    /// </summary>
    public class Utf8Encoder
    {
        private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false);
        private static readonly string[] TextFileExtensions = { ".cs", ".txt", ".md", ".json", ".xml", ".html", ".css", ".js", ".ts", ".cpp", ".c", ".h", ".hpp", ".java", ".py", ".rb", ".go", ".rs", ".sql", ".sh", ".bat", ".ps1", ".yaml", ".yml", ".ini", ".cfg", ".conf", ".log" };

        /// <summary>
        /// Converts a single file to UTF-8 without BOM if it's a text file.
        /// </summary>
        /// <param name="filePath">Path to the file to convert.</param>
        /// <returns>True if the file was converted, false if skipped.</returns>
        public bool ConvertFile(string filePath)
        {
            if (!IsTextFile(filePath))
            {
                return false;
            }

            try
            {
                // Read the file with automatic encoding detection
                string content = ReadFileWithEncodingDetection(filePath);

                // Write back with UTF-8 without BOM
                File.WriteAllText(filePath, content, Utf8WithoutBom);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Converts all text files in a directory to UTF-8 without BOM.
        /// </summary>
        /// <param name="directoryPath">Path to the directory to process.</param>
        /// <param name="recursive">If true, processes subdirectories recursively.</param>
        /// <returns>Number of files converted.</returns>
        public int ConvertDirectory(string directoryPath, bool recursive = true)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
            }

            int convertedCount = 0;
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            var files = Directory.GetFiles(directoryPath, "*.*", searchOption)
                .Where(f => IsTextFile(f));

            foreach (var file in files)
            {
                Console.WriteLine($"Processing: {file}");
                if (ConvertFile(file))
                {
                    convertedCount++;
                    Console.WriteLine($"Converted: {file}");
                }
                else
                {
                    Console.WriteLine($"Skipped: {file}");
                }
            }

            return convertedCount;
        }

        /// <summary>
        /// Reads a file with automatic encoding detection.
        /// </summary>
        private string ReadFileWithEncodingDetection(string filePath)
        {
            // Try reading with BOM detection first
            byte[] fileBytes = File.ReadAllBytes(filePath);

            // Check for BOM
            Encoding detectedEncoding = DetectEncodingFromBOM(fileBytes);

            if (detectedEncoding != null)
            {
                return detectedEncoding.GetString(fileBytes, detectedEncoding.GetPreamble().Length, fileBytes.Length - detectedEncoding.GetPreamble().Length);
            }

            // If no BOM, try UTF-8 first
            try
            {
                var utf8Decoder = Encoding.UTF8.GetDecoder();
                utf8Decoder.Fallback = DecoderFallback.ExceptionFallback;
                var charBuffer = new char[utf8Decoder.GetCharCount(fileBytes, 0, fileBytes.Length)];
                utf8Decoder.GetChars(fileBytes, 0, fileBytes.Length, charBuffer, 0);
                return new string(charBuffer);
            }
            catch (DecoderFallbackException)
            {
                // Not valid UTF-8, try other encodings
            }

            // Try common encodings
            var encodingsToTry = new List<Encoding>
            {
                Encoding.GetEncoding("windows-1252"), // Western European (Windows)
                Encoding.GetEncoding("iso-8859-1"),   // Western European (ISO)
                Encoding.ASCII
            };

            foreach (var encoding in encodingsToTry)
            {
                try
                {
                    return encoding.GetString(fileBytes);
                }
                catch
                {
                    // Try next encoding
                }
            }

            // Fallback to UTF-8 with replacement fallback
            return Encoding.UTF8.GetString(fileBytes);
        }

        /// <summary>
        /// Detects encoding from BOM (Byte Order Mark).
        /// </summary>
        private Encoding DetectEncodingFromBOM(byte[] fileBytes)
        {
            if (fileBytes.Length < 2)
                return null;

            // UTF-32 BE
            if (fileBytes.Length >= 4 && fileBytes[0] == 0x00 && fileBytes[1] == 0x00 && fileBytes[2] == 0xFE && fileBytes[3] == 0xFF)
                return new UTF32Encoding(true, true);

            // UTF-32 LE
            if (fileBytes.Length >= 4 && fileBytes[0] == 0xFF && fileBytes[1] == 0xFE && fileBytes[2] == 0x00 && fileBytes[3] == 0x00)
                return Encoding.UTF32;

            // UTF-8
            if (fileBytes.Length >= 3 && fileBytes[0] == 0xEF && fileBytes[1] == 0xBB && fileBytes[2] == 0xBF)
                return Encoding.UTF8;

            // UTF-16 BE
            if (fileBytes[0] == 0xFE && fileBytes[1] == 0xFF)
                return Encoding.BigEndianUnicode;

            // UTF-16 LE
            if (fileBytes[0] == 0xFF && fileBytes[1] == 0xFE)
                return Encoding.Unicode;

            return null;
        }

        /// <summary>
        /// Checks if a file is a text file based on its extension.
        /// </summary>
        private bool IsTextFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return TextFileExtensions.Contains(extension);
        }
    }
}
