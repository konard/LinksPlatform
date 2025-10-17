using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using LibGit2Sharp;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    public class GitRepositoryExporter<TLink>
    {
        private readonly ILinks<TLink> _links;

        public GitRepositoryExporter(ILinks<TLink> links)
        {
            _links = links;
        }

        public void Export(TLink repository, string outputPath, string authorName, string authorEmail)
        {
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }
            Directory.CreateDirectory(outputPath);

            Repository.Init(outputPath);

            using (var repo = new Repository(outputPath))
            {
                var files = GetFilesFromRepository(repository);

                foreach (var (filePath, fileContent) in files)
                {
                    var fullPath = Path.Combine(outputPath, filePath);
                    var directory = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    File.WriteAllText(fullPath, fileContent);
                }

                Commands.Stage(repo, "*");

                var signature = new Signature(authorName, authorEmail, DateTimeOffset.Now);
                repo.Commit("Export from Links storage", signature, signature);
            }
        }

        private List<(string path, string content)> GetFilesFromRepository(TLink repository)
        {
            var files = new List<(string, string)>();

            // This is a simplified implementation
            // In a real scenario, we would need to traverse the links structure
            // to find all files associated with the repository

            return files;
        }
    }
}
