namespace Platform.CodeEvolution.Models;

public class CodeChange
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CommitSha { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FunctionName { get; set; } = string.Empty;
    public string BeforeCode { get; set; } = string.Empty;
    public string AfterCode { get; set; } = string.Empty;
    public DateTime ChangeDate { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = new();
    public int LineNumber { get; set; }
    public ChangeType Type { get; set; }
    public string CommitMessage { get; set; } = string.Empty;
}

public enum ChangeType
{
    Added,
    Modified,
    Deleted,
    Renamed,
    Moved
}