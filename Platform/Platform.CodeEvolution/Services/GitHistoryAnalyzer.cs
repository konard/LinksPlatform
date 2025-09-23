using LibGit2Sharp;
using Platform.CodeEvolution.Models;
using System.Text.RegularExpressions;

namespace Platform.CodeEvolution.Services;

public class GitHistoryAnalyzer : IDisposable
{
    private readonly Repository _repository;
    private readonly CodeAnalyzer _codeAnalyzer;

    public GitHistoryAnalyzer(string repositoryPath)
    {
        _repository = new Repository(repositoryPath);
        _codeAnalyzer = new CodeAnalyzer();
    }

    public async Task<List<CodeChange>> AnalyzeHistoryAsync(string? branchName = null, int maxCommits = 1000)
    {
        var changes = new List<CodeChange>();
        var branch = branchName != null ? _repository.Branches[branchName] : _repository.Head;

        if (branch == null)
        {
            throw new ArgumentException($"Branch '{branchName}' not found");
        }

        var commits = branch.Commits.Take(maxCommits).ToList();

        for (int i = 0; i < commits.Count - 1; i++)
        {
            var currentCommit = commits[i];
            var previousCommit = commits[i + 1];

            var diff = _repository.Diff.Compare<TreeChanges>(previousCommit.Tree, currentCommit.Tree);

            foreach (var change in diff)
            {
                if (IsSourceCodeFile(change.Path))
                {
                    var codeChanges = await AnalyzeFileChangeAsync(change, currentCommit, previousCommit);
                    changes.AddRange(codeChanges);
                }
            }
        }

        return changes;
    }

    private async Task<List<CodeChange>> AnalyzeFileChangeAsync(TreeEntryChanges change, Commit currentCommit, Commit previousCommit)
    {
        var changes = new List<CodeChange>();

        try
        {
            var oldContent = string.Empty;
            var newContent = string.Empty;

            if (change.OldOid != null && change.OldOid != ObjectId.Zero)
            {
                var oldBlob = _repository.Lookup<Blob>(change.OldOid);
                oldContent = oldBlob?.GetContentText() ?? string.Empty;
            }

            if (change.Oid != null && change.Oid != ObjectId.Zero)
            {
                var newBlob = _repository.Lookup<Blob>(change.Oid);
                newContent = newBlob?.GetContentText() ?? string.Empty;
            }

            var language = GetLanguageFromExtension(change.Path);
            var dependencies = await _codeAnalyzer.ExtractDependenciesAsync(newContent, language);

            if (change.Status == ChangeKind.Modified || change.Status == ChangeKind.Added)
            {
                var functionChanges = await _codeAnalyzer.AnalyzeFunctionChangesAsync(oldContent, newContent, language);

                foreach (var funcChange in functionChanges)
                {
                    changes.Add(new CodeChange
                    {
                        CommitSha = currentCommit.Sha,
                        FileName = change.Path,
                        FunctionName = funcChange.FunctionName,
                        BeforeCode = funcChange.BeforeCode,
                        AfterCode = funcChange.AfterCode,
                        ChangeDate = currentCommit.Author.When.DateTime,
                        Author = currentCommit.Author.Name,
                        Language = language,
                        Dependencies = dependencies,
                        LineNumber = funcChange.LineNumber,
                        Type = GetChangeType(change.Status),
                        CommitMessage = currentCommit.MessageShort
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error analyzing file change {change.Path}: {ex.Message}");
        }

        return changes;
    }

    private bool IsSourceCodeFile(string path)
    {
        var supportedExtensions = new[] { ".cs", ".js", ".ts", ".py", ".java", ".cpp", ".c", ".h", ".hpp", ".go", ".rs", ".php", ".rb", ".swift", ".kt" };
        return supportedExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    private string GetLanguageFromExtension(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".cs" => "C#",
            ".js" => "JavaScript",
            ".ts" => "TypeScript",
            ".py" => "Python",
            ".java" => "Java",
            ".cpp" or ".cc" => "C++",
            ".c" => "C",
            ".h" or ".hpp" => "C/C++ Header",
            ".go" => "Go",
            ".rs" => "Rust",
            ".php" => "PHP",
            ".rb" => "Ruby",
            ".swift" => "Swift",
            ".kt" => "Kotlin",
            _ => "Unknown"
        };
    }

    private ChangeType GetChangeType(ChangeKind status)
    {
        return status switch
        {
            ChangeKind.Added => ChangeType.Added,
            ChangeKind.Modified => ChangeType.Modified,
            ChangeKind.Deleted => ChangeType.Deleted,
            ChangeKind.Renamed => ChangeType.Renamed,
            _ => ChangeType.Modified
        };
    }

    public void Dispose()
    {
        _repository?.Dispose();
    }
}