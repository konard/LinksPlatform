using System;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Examples;

/// <summary>
/// Example demonstrating the Tag File System functionality.
/// This shows how to create a tag-based file system where:
/// - Files are stored in a flat collection
/// - Files can have multiple tags
/// - Tags can inherit from other tags (implementing path-like structures)
/// </summary>
namespace TagFileSystemExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Tag File System Example ===\n");

            // Create or open the database file
            var dbPath = "example-tagfs.links";
            using (var memory = new FileMappedResizableDirectMemory(dbPath))
            using (var links = new UnitedMemoryLinks<ulong>(memory))
            {
                var tfs = new TagFileSystem(links);

                Console.WriteLine("1. Creating files...");
                tfs.GetOrCreateFile("/documents/report.pdf");
                tfs.GetOrCreateFile("/images/photo1.jpg");
                tfs.GetOrCreateFile("/images/photo2.jpg");
                tfs.GetOrCreateFile("/projects/code.cs");
                Console.WriteLine("   Files created.\n");

                Console.WriteLine("2. Adding tags to files...");
                tfs.AddTagToFile("/documents/report.pdf", "work");
                tfs.AddTagToFile("/documents/report.pdf", "important");
                tfs.AddTagToFile("/documents/report.pdf", "2023");

                tfs.AddTagToFile("/images/photo1.jpg", "vacation");
                tfs.AddTagToFile("/images/photo1.jpg", "2023");

                tfs.AddTagToFile("/images/photo2.jpg", "vacation");
                tfs.AddTagToFile("/images/photo2.jpg", "family");

                tfs.AddTagToFile("/projects/code.cs", "work");
                tfs.AddTagToFile("/projects/code.cs", "programming");
                Console.WriteLine("   Tags added.\n");

                Console.WriteLine("3. Creating tag inheritance (path-like structure)...");
                // Create hierarchical tag structure: personal -> vacation -> summer
                tfs.CreateTagInheritance("personal", "vacation");
                tfs.CreateTagInheritance("vacation", "summer");
                tfs.AddTagToFile("/images/photo1.jpg", "summer");
                Console.WriteLine("   Tag hierarchy: personal <- vacation <- summer\n");

                Console.WriteLine("4. Finding files with tag 'work':");
                var workFiles = tfs.GetFilesWithTag("work");
                foreach (var file in workFiles)
                {
                    Console.WriteLine($"   - {tfs.GetFilePath(file)}");
                }
                Console.WriteLine();

                Console.WriteLine("5. Finding files with tag 'vacation':");
                var vacationFiles = tfs.GetFilesWithTag("vacation");
                foreach (var file in vacationFiles)
                {
                    Console.WriteLine($"   - {tfs.GetFilePath(file)}");
                }
                Console.WriteLine();

                Console.WriteLine("6. Finding files with both 'work' AND 'important' tags:");
                var importantWorkFiles = tfs.GetFilesWithAllTags("work", "important");
                foreach (var file in importantWorkFiles)
                {
                    Console.WriteLine($"   - {tfs.GetFilePath(file)}");
                }
                Console.WriteLine();

                Console.WriteLine("7. Finding files with tag '2023':");
                var files2023 = tfs.GetFilesWithTag("2023");
                foreach (var file in files2023)
                {
                    Console.WriteLine($"   - {tfs.GetFilePath(file)}");
                }
                Console.WriteLine();

                Console.WriteLine("8. Listing tags for '/images/photo1.jpg':");
                var photo1 = tfs.GetFile("/images/photo1.jpg");
                var photo1Tags = tfs.GetFileTags(photo1);
                foreach (var tag in photo1Tags)
                {
                    Console.WriteLine($"   - {tfs.GetTagName(tag)}");
                }
                Console.WriteLine();

                Console.WriteLine("9. Getting parent tags of 'vacation':");
                var vacationTag = tfs.GetTag("vacation");
                var vacationParents = tfs.GetParentTags(vacationTag);
                foreach (var parent in vacationParents)
                {
                    Console.WriteLine($"   - {tfs.GetTagName(parent)}");
                }
                Console.WriteLine();

                Console.WriteLine("10. Getting child tags of 'vacation':");
                var vacationChildren = tfs.GetChildTags(vacationTag);
                foreach (var child in vacationChildren)
                {
                    Console.WriteLine($"   - {tfs.GetTagName(child)}");
                }
                Console.WriteLine();

                Console.WriteLine($"Total links in database: {links.Count()}");
                Console.WriteLine("\nExample completed successfully!");
                Console.WriteLine($"Database saved to: {dbPath}");
            }
        }
    }
}
