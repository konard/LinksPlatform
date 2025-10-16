using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Platform.Exceptions;

namespace Platform.Examples
{
    /// <summary>
    /// Imports file system structure (directories and files) into links storage.
    /// </summary>
    public class FileSystemImporter<TLink>
    {
        private readonly IHierarchicalStorage<TLink> _storage;
        private readonly bool _includeFileContents;

        public FileSystemImporter(IHierarchicalStorage<TLink> storage, bool includeFileContents = false)
        {
            _storage = storage;
            _includeFileContents = includeFileContents;
        }

        public Task Import(string path, CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var root = _storage.CreateRoot(path);
                    if (Directory.Exists(path))
                    {
                        ProcessDirectory(path, root, token);
                    }
                    else if (File.Exists(path))
                    {
                        ProcessFile(path, root, token);
                    }
                    else
                    {
                        Console.WriteLine($"Path does not exist: {path}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToStringWithAllInnerExceptions());
                }
            }, token);
        }

        private void ProcessDirectory(string directoryPath, TLink parent, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            try
            {
                // Process subdirectories
                var directories = Directory.GetDirectories(directoryPath);
                foreach (var directory in directories)
                {
                    if (token.IsCancellationRequested) return;

                    var dirName = Path.GetFileName(directory);
                    var dirNode = _storage.CreateNode(dirName, "directory");
                    _storage.AttachToParent(dirNode, parent);
                    ProcessDirectory(directory, dirNode, token);
                }

                // Process files
                var files = Directory.GetFiles(directoryPath);
                foreach (var file in files)
                {
                    if (token.IsCancellationRequested) return;

                    ProcessFile(file, parent, token);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied: {directoryPath} - {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing directory {directoryPath}: {ex.Message}");
            }
        }

        private void ProcessFile(string filePath, TLink parent, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            try
            {
                var fileName = Path.GetFileName(filePath);
                var fileNode = _storage.CreateNode(fileName, "file");
                _storage.AttachToParent(fileNode, parent);

                // Add file metadata
                var fileInfo = new FileInfo(filePath);
                var sizeNode = _storage.CreateNode($"Size: {fileInfo.Length} bytes", "metadata");
                _storage.AttachToParent(sizeNode, fileNode);

                var modifiedNode = _storage.CreateNode($"Modified: {fileInfo.LastWriteTime}", "metadata");
                _storage.AttachToParent(modifiedNode, fileNode);

                // Optionally include file contents for text files
                if (_includeFileContents && IsTextFile(filePath))
                {
                    var content = File.ReadAllText(filePath);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var contentNode = _storage.CreateValueNode(content);
                        _storage.AttachToParent(contentNode, fileNode);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
            }
        }

        private bool IsTextFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".txt" => true,
                ".cs" => true,
                ".json" => true,
                ".xml" => true,
                ".md" => true,
                ".html" => true,
                ".css" => true,
                ".js" => true,
                ".ts" => true,
                ".yaml" => true,
                ".yml" => true,
                ".config" => true,
                ".ini" => true,
                _ => false
            };
        }
    }
}
