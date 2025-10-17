# Friend Projects Finder

A tool to find GitHub projects that have similar functions or code patterns as LinksPlatform libraries. These "friend projects" could potentially benefit from using LinksPlatform libraries to reduce code duplication.

## Purpose

This tool helps identify projects on GitHub that:
- Implement similar functionality to LinksPlatform libraries
- Use similar code patterns or data structures
- Could reduce their codebase by adopting LinksPlatform libraries

## Requirements

- Python 3.6+
- GitHub CLI (`gh`) installed and authenticated
- Internet connection for GitHub API access

## Installation

1. Ensure GitHub CLI is installed:
```bash
gh --version
```

2. Authenticate with GitHub (if not already done):
```bash
gh auth login
```

3. Make the script executable:
```bash
chmod +x tools/friend_projects_finder.py
```

## Usage

### Quick Start - Analyze LinksPlatform Organization

```bash
./tools/friend_projects_finder.py --org linksplatform --analyze
```

This will:
1. Analyze common patterns in LinksPlatform libraries
2. Search GitHub for similar code
3. Generate a report of potential friend projects

### Search for Specific Terms

```bash
./tools/friend_projects_finder.py --search-terms "Range,ILinks,Doublets"
```

### Save Report to File

```bash
./tools/friend_projects_finder.py --org linksplatform --analyze --output report.txt
```

### Customize Search Parameters

```bash
./tools/friend_projects_finder.py \
  --org linksplatform \
  --analyze \
  --language csharp \
  --max-results 100 \
  --output friend_projects_report.txt
```

## Command Line Options

- `--language LANG` - Programming language to search (default: csharp)
- `--search-terms TERMS` - Comma-separated list of terms to search for
- `--org ORG` - Organization to analyze (e.g., linksplatform)
- `--analyze` - Analyze organization and find friend projects
- `--max-results N` - Maximum results per search (default: 50)
- `--output FILE` - Output file for report (default: stdout)

## Examples

### Example 1: Find projects using Range pattern
```bash
./tools/friend_projects_finder.py --search-terms "struct Range"
```

### Example 2: Find projects with ILinks interface
```bash
./tools/friend_projects_finder.py --search-terms "interface ILinks"
```

### Example 3: Full analysis with report
```bash
./tools/friend_projects_finder.py \
  --org linksplatform \
  --analyze \
  --max-results 100 \
  --output reports/$(date +%Y%m%d)_friend_projects.txt
```

## Output Format

The tool generates a report showing:
- Total number of potential friend projects found
- For each project:
  - Repository name
  - Number of pattern matches
  - Specific files with matches
  - URLs to the matching code

Example output:
```
================================================================================
Friend Projects Finder - Report
================================================================================

Found 15 potential friend projects

## someuser/example-project
   Total matches: 5

   - Range struct usage: 3 match(es)
     * src/Models/Range.cs
       https://github.com/someuser/example-project/blob/main/src/Models/Range.cs
   - Memory management: 2 match(es)
     * src/Core/Memory.cs
       https://github.com/someuser/example-project/blob/main/src/Core/Memory.cs
```

## Common Patterns Detected

The tool searches for these LinksPlatform patterns:

1. **Range struct usage** - Projects using Range with Minimum/Maximum fields
2. **ILinks interface** - Projects implementing link-based data structures
3. **Doublets pattern** - Projects using doublet data structures
4. **Memory management** - Projects with custom memory management (IMemory, pointers)
5. **Synchronization wrapper** - Projects using thread-safe synchronization patterns

## Limitations

- GitHub API rate limits apply (typically 5,000 requests/hour for authenticated users)
- The tool uses GitHub CLI which respects these limits
- Results are limited by GitHub code search capabilities
- Pattern matching is based on text search, not semantic analysis

## Future Enhancements

Potential improvements for the tool:
- AST-based code analysis for more accurate matching
- Function signature comparison
- Code similarity scoring
- Integration with static analysis tools
- Support for more programming languages
- Automated pull request generation for suggesting library adoption

## Contributing

To add new search patterns, modify the `extract_common_patterns` method in `friend_projects_finder.py`:

```python
common_patterns = [
    {
        "name": "Your Pattern Name",
        "pattern": "regex or string pattern",
        "description": "Description of what to find"
    },
    # ... more patterns
]
```

## License

This tool is part of the LinksPlatform project. See the repository's LICENSE file for details.
