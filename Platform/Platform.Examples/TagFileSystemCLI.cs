using System;
using System.IO;
using System.Linq;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Decorators;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the Tag File System.
    /// Provides interactive commands to manage tags and files in a tag-based file system.
    /// </summary>
    public class TagFileSystemCLI : ICommandLineInterface
    {
        public void Run(string[] args)
        {
            var dbPath = args.Length > 0 ? args[0] : "tag-filesystem.links";

            Console.WriteLine("=== Tag File System ===");
            Console.WriteLine($"Database: {dbPath}");
            Console.WriteLine();

            using (var memoryAdapter = new UInt64UnitedMemoryLinks(dbPath))
            using (var linksDecorator = new UInt64Links(memoryAdapter))
            {
                var links = new SynchronizedLinks<ulong>(linksDecorator);
                var tfs = new TagFileSystem(links);

                Console.WriteLine("Commands:");
                Console.WriteLine("  add-file <path>                    - Add a file to the system");
                Console.WriteLine("  add-tag <file-path> <tag>          - Add a tag to a file");
                Console.WriteLine("  tag-inherit <parent> <child>       - Create tag inheritance");
                Console.WriteLine("  list-tags <file-path>              - List all tags for a file");
                Console.WriteLine("  find <tag1> [tag2...]              - Find files with all specified tags");
                Console.WriteLine("  list-files <tag>                   - List all files with a tag");
                Console.WriteLine("  get-parents <tag>                  - Get parent tags");
                Console.WriteLine("  get-children <tag>                 - Get child tags");
                Console.WriteLine("  stats                              - Show database statistics");
                Console.WriteLine("  help                               - Show this help");
                Console.WriteLine("  exit                               - Exit");
                Console.WriteLine();

                while (true)
                {
                    Console.Write("> ");
                    var input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        continue;
                    }

                    var parts = ParseCommand(input);
                    var command = parts[0].ToLower();

                    try
                    {
                        switch (command)
                        {
                            case "add-file":
                                if (parts.Length < 2)
                                {
                                    Console.WriteLine("Usage: add-file <path>");
                                    break;
                                }
                                var filePath = string.Join(" ", parts.Skip(1));
                                var file = tfs.GetOrCreateFile(filePath);
                                Console.WriteLine($"File added/found: {filePath} (Link: {file})");
                                break;

                            case "add-tag":
                                if (parts.Length < 3)
                                {
                                    Console.WriteLine("Usage: add-tag <file-path> <tag>");
                                    break;
                                }
                                var filePathForTag = parts[1];
                                var tagName = string.Join(" ", parts.Skip(2));
                                tfs.AddTagToFile(filePathForTag, tagName);
                                Console.WriteLine($"Tag '{tagName}' added to '{filePathForTag}'");
                                break;

                            case "tag-inherit":
                                if (parts.Length < 3)
                                {
                                    Console.WriteLine("Usage: tag-inherit <parent> <child>");
                                    break;
                                }
                                var parentTag = parts[1];
                                var childTag = string.Join(" ", parts.Skip(2));
                                tfs.CreateTagInheritance(parentTag, childTag);
                                Console.WriteLine($"Tag inheritance created: '{childTag}' inherits from '{parentTag}'");
                                break;

                            case "list-tags":
                                if (parts.Length < 2)
                                {
                                    Console.WriteLine("Usage: list-tags <file-path>");
                                    break;
                                }
                                var filePathForList = string.Join(" ", parts.Skip(1));
                                var fileLink = tfs.GetFile(filePathForList);
                                if (fileLink == links.Constants.Null)
                                {
                                    Console.WriteLine($"File not found: {filePathForList}");
                                    break;
                                }
                                var tags = tfs.GetFileTags(fileLink);
                                Console.WriteLine($"Tags for '{filePathForList}':");
                                foreach (var tag in tags)
                                {
                                    var tagNameStr = tfs.GetTagName(tag);
                                    Console.WriteLine($"  - {tagNameStr} (Link: {tag})");
                                }
                                Console.WriteLine($"Total: {tags.Count} tags");
                                break;

                            case "find":
                                if (parts.Length < 2)
                                {
                                    Console.WriteLine("Usage: find <tag1> [tag2...]");
                                    break;
                                }
                                var searchTags = parts.Skip(1).ToArray();
                                var foundFiles = tfs.GetFilesWithAllTags(searchTags);
                                Console.WriteLine($"Files with tags [{string.Join(", ", searchTags)}]:");
                                foreach (var foundFile in foundFiles)
                                {
                                    var path = tfs.GetFilePath(foundFile);
                                    Console.WriteLine($"  - {path} (Link: {foundFile})");
                                }
                                Console.WriteLine($"Total: {foundFiles.Count} files");
                                break;

                            case "list-files":
                                if (parts.Length < 2)
                                {
                                    Console.WriteLine("Usage: list-files <tag>");
                                    break;
                                }
                                var tagForList = string.Join(" ", parts.Skip(1));
                                var filesWithTag = tfs.GetFilesWithTag(tagForList);
                                Console.WriteLine($"Files with tag '{tagForList}':");
                                foreach (var fileWithTag in filesWithTag)
                                {
                                    var path = tfs.GetFilePath(fileWithTag);
                                    Console.WriteLine($"  - {path} (Link: {fileWithTag})");
                                }
                                Console.WriteLine($"Total: {filesWithTag.Count} files");
                                break;

                            case "get-parents":
                                if (parts.Length < 2)
                                {
                                    Console.WriteLine("Usage: get-parents <tag>");
                                    break;
                                }
                                var childTagForParents = string.Join(" ", parts.Skip(1));
                                var childTagLink = tfs.GetTag(childTagForParents);
                                if (childTagLink == links.Constants.Null)
                                {
                                    Console.WriteLine($"Tag not found: {childTagForParents}");
                                    break;
                                }
                                var parentTags = tfs.GetParentTags(childTagLink);
                                Console.WriteLine($"Parent tags of '{childTagForParents}':");
                                foreach (var parent in parentTags)
                                {
                                    var parentName = tfs.GetTagName(parent);
                                    Console.WriteLine($"  - {parentName} (Link: {parent})");
                                }
                                Console.WriteLine($"Total: {parentTags.Count} parents");
                                break;

                            case "get-children":
                                if (parts.Length < 2)
                                {
                                    Console.WriteLine("Usage: get-children <tag>");
                                    break;
                                }
                                var parentTagForChildren = string.Join(" ", parts.Skip(1));
                                var parentTagLink = tfs.GetTag(parentTagForChildren);
                                if (parentTagLink == links.Constants.Null)
                                {
                                    Console.WriteLine($"Tag not found: {parentTagForChildren}");
                                    break;
                                }
                                var childTags = tfs.GetChildTags(parentTagLink);
                                Console.WriteLine($"Child tags of '{parentTagForChildren}':");
                                foreach (var child in childTags)
                                {
                                    var childName = tfs.GetTagName(child);
                                    Console.WriteLine($"  - {childName} (Link: {child})");
                                }
                                Console.WriteLine($"Total: {childTags.Count} children");
                                break;

                            case "stats":
                                Console.WriteLine("Database Statistics:");
                                var totalLinks = links.Count();
                                Console.WriteLine($"  Total links: {totalLinks}");
                                break;

                            case "help":
                                Console.WriteLine("Commands:");
                                Console.WriteLine("  add-file <path>                    - Add a file to the system");
                                Console.WriteLine("  add-tag <file-path> <tag>          - Add a tag to a file");
                                Console.WriteLine("  tag-inherit <parent> <child>       - Create tag inheritance");
                                Console.WriteLine("  list-tags <file-path>              - List all tags for a file");
                                Console.WriteLine("  find <tag1> [tag2...]              - Find files with all specified tags");
                                Console.WriteLine("  list-files <tag>                   - List all files with a tag");
                                Console.WriteLine("  get-parents <tag>                  - Get parent tags");
                                Console.WriteLine("  get-children <tag>                 - Get child tags");
                                Console.WriteLine("  stats                              - Show database statistics");
                                Console.WriteLine("  help                               - Show this help");
                                Console.WriteLine("  exit                               - Exit");
                                break;

                            case "exit":
                            case "quit":
                                Console.WriteLine("Goodbye!");
                                return;

                            default:
                                Console.WriteLine($"Unknown command: {command}. Type 'help' for available commands.");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }

                    Console.WriteLine();
                }
            }
        }

        private string[] ParseCommand(string input)
        {
            return input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
