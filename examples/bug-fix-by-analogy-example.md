# Bug Fixing by Analogy - Usage Examples

This document demonstrates how to use the Bug Fixing by Analogy tool to find similar code snippets across GitHub.

## Overview

The tool helps developers find similar code patterns across GitHub repositories, which can be useful for:
- Identifying potential bugs by comparing with similar code
- Finding best practices and patterns
- Learning from how others solved similar problems
- Discovering edge cases that might be missing

## Usage

### Basic Usage

1. **From command line with code snippet:**
   ```bash
   dotnet run --project Platform/Platform.Examples BugFixByAnalogy "public void SaveData(string data) { File.WriteAllText(path, data); }" "C#" 5
   ```

2. **From command line with file:**
   ```bash
   dotnet run --project Platform/Platform.Examples BugFixByAnalogy "./mycode.cs" "C#" 10
   ```

3. **Interactive mode:**
   ```bash
   dotnet run --project Platform/Platform.Examples BugFixByAnalogy
   ```
   Then follow the prompts to enter your code snippet.

### Example Scenarios

#### Scenario 1: Finding potential null reference issues

**Your Code:**
```csharp
public void ProcessUser(User user)
{
    Console.WriteLine(user.Name);
    SaveToDatabase(user);
}
```

**Usage:**
```bash
dotnet run --project Platform/Platform.Examples BugFixByAnalogy "public void ProcessUser(User user) { Console.WriteLine(user.Name); }" "C#" 10
```

**What to look for in results:**
- How other developers handle null checks
- Common patterns for parameter validation
- Error handling approaches

#### Scenario 2: Async/Await patterns

**Your Code:**
```csharp
public async Task<string> FetchDataAsync()
{
    var result = await client.GetAsync(url);
    return result.Content.ReadAsStringAsync().Result;
}
```

**Usage:**
```bash
dotnet run --project Platform/Platform.Examples BugFixByAnalogy "async Task FetchDataAsync GetAsync ReadAsStringAsync" "C#" 10
```

**What to look for:**
- Proper async/await usage
- Avoiding .Result or .Wait() in async methods
- Cancellation token usage

#### Scenario 3: Resource disposal patterns

**Your Code:**
```csharp
public void ProcessFile(string path)
{
    var stream = File.OpenRead(path);
    // Process stream
    stream.Close();
}
```

**Usage:**
```bash
dotnet run --project Platform/Platform.Examples BugFixByAnalogy "File.OpenRead stream Close" "C#" 10
```

**What to look for:**
- Using statements for proper disposal
- Try-finally patterns
- IDisposable implementation

## Programmatic Usage

You can also use the `CodeSimilaritySearcher` class directly in your code:

```csharp
using Platform.Examples;

var searcher = new CodeSimilaritySearcher();

var codeSnippet = @"
public void SaveData(string data)
{
    File.WriteAllText(path, data);
}";

var results = searcher.SearchSimilarCode(codeSnippet, "C#", 10);

foreach (var result in results)
{
    Console.WriteLine(result);
}
```

## Tips for Best Results

1. **Extract key patterns:** Focus on the core logic rather than complete implementations
2. **Use meaningful identifiers:** Include function names, class names, or key method calls
3. **Specify language:** Adding language filter improves result relevance
4. **Start broad, then narrow:** Begin with general patterns, then refine based on results
5. **Compare multiple snippets:** If you find a bug, search for similar patterns in your codebase

## How It Works

1. **Term Extraction:** The tool analyzes your code snippet and extracts significant terms (keywords, identifiers, method names)
2. **GitHub Search:** Uses GitHub's code search API to find similar patterns across public repositories
3. **Result Ranking:** Returns results with similarity scores based on term matching
4. **Analysis:** You manually compare results with your code to identify potential issues or improvements

## Requirements

- GitHub CLI (`gh`) must be installed and authenticated
- Internet connection for accessing GitHub API
- .NET SDK for running the examples

## Advanced Usage

### Creating a code snippet file for complex searches

Create a file `suspicious-code.cs`:
```csharp
public class DataProcessor
{
    private List<int> _data;

    public void AddItem(int item)
    {
        _data.Add(item);
    }

    public int GetAverage()
    {
        return _data.Sum() / _data.Count;
    }
}
```

Then search:
```bash
dotnet run --project Platform/Platform.Examples BugFixByAnalogy "./suspicious-code.cs" "C#" 15
```

Look for:
- Null initialization issues
- Division by zero handling
- Thread safety in collections

## Troubleshooting

**No results found:**
- Ensure GitHub CLI is authenticated: `gh auth status`
- Try broader search terms
- Remove language-specific syntax
- Check internet connection

**Too many irrelevant results:**
- Add language filter
- Use more specific method/class names
- Reduce the number of search terms

**Authentication errors:**
- Run `gh auth login` to authenticate with GitHub
- Ensure you have a valid GitHub token

## References

- [GitHub Code Search Documentation](https://docs.github.com/en/search-github/searching-on-github/searching-code)
- [SimFix: Program Repair via Code Similarity](https://github.com/xgdsmileboy/SimFix)
- Research: "Shaping Program Repair Space with Existing Patches and Similar Code"
