using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Platform.Exceptions;
using Platform.IO;

namespace Platform.Examples
{
    public class GitRepositoryImporter<TLink>
    {
        private readonly ICodeStorage<TLink> _storage;

        public GitRepositoryImporter(ICodeStorage<TLink> storage) => _storage = storage;

        public Task Import(string repositoryPath, CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    if (!Directory.Exists(repositoryPath))
                    {
                        throw new DirectoryNotFoundException($"Repository path not found: {repositoryPath}");
                    }

                    var gitPath = Path.Combine(repositoryPath, ".git");
                    if (!Directory.Exists(gitPath))
                    {
                        throw new InvalidOperationException($"Not a git repository: {repositoryPath}");
                    }

                    var repoName = Path.GetFileName(repositoryPath);
                    var repository = _storage.CreateRepository(repoName);

                    using (var repo = new Repository(repositoryPath))
                    {
                        var commits = repo.Commits.QueryBy(new CommitFilter
                        {
                            SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time
                        }).ToList();

                        ConsoleHelpers.Debug("Found {0} commits", commits.Count);

                        foreach (var commit in commits)
                        {
                            if (token.IsCancellationRequested)
                            {
                                return;
                            }

                            ConsoleHelpers.Debug("Processing commit: {0}", commit.MessageShort);

                            var parent = default(TLink);
                            if (commit.Parents.Any())
                            {
                                // For simplicity, we only track the first parent
                                // In a full implementation, we would need to handle merge commits properly
                            }

                            var commitLink = _storage.CreateCommit(
                                commit.Message,
                                $"{commit.Author.Name} <{commit.Author.Email}>",
                                parent
                            );

                            _storage.AttachCommitToRepository(commitLink, repository);

                            foreach (var entry in commit.Tree)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    return;
                                }

                                ProcessTreeEntry(entry, "", commitLink);
                            }
                        }
                    }

                    ConsoleHelpers.Debug("Import completed successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToStringWithAllInnerExceptions());
                }
            }, token);
        }

        private void ProcessTreeEntry(TreeEntry entry, string basePath, TLink commit)
        {
            var entryPath = string.IsNullOrEmpty(basePath) ? entry.Name : Path.Combine(basePath, entry.Name);

            if (entry.TargetType == TreeEntryTargetType.Blob)
            {
                var blob = (Blob)entry.Target;
                var content = blob.GetContentText();
                var file = _storage.CreateFile(entryPath, content);
                _storage.AttachFileToCommit(file, commit);
            }
            else if (entry.TargetType == TreeEntryTargetType.Tree)
            {
                var tree = (Tree)entry.Target;
                var directory = _storage.CreateDirectory(entryPath);
                _storage.AttachFileToCommit(directory, commit);

                foreach (var subEntry in tree)
                {
                    ProcessTreeEntry(subEntry, entryPath, commit);
                }
            }
        }
    }
}
