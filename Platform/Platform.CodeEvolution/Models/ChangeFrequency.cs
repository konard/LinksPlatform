namespace Platform.CodeEvolution.Models;

public class ChangeFrequency
{
    public string Pattern { get; set; } = string.Empty;
    public string FromCode { get; set; } = string.Empty;
    public string ToCode { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<string> CommitShas { get; set; } = new();
    public string Language { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = new();
    public double Confidence { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public List<string> Authors { get; set; } = new();
}

public class ChangeChain
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public List<CodeChange> Changes { get; set; } = new();
    public string FinalCode { get; set; } = string.Empty;
    public double Quality { get; set; }
    public bool IsComplete { get; set; }
    public List<ChangeChain> Alternatives { get; set; } = new();
}