# GitHub Pattern Discovery Tool

A Python tool that analyzes commit messages, PR titles, and descriptions across GitHub repositories to identify repeatable patterns and generate automation scripts or regex patterns.

## Purpose

This tool solves issue #482: discovering patterns in repository changes that repeat across multiple repositories. It automatically:

1. Analyzes commit messages and PR titles across repositories
2. Identifies common patterns (dependency updates, documentation changes, etc.)
3. Generates automation scripts and regex patterns for repeating changes
4. Provides statistical analysis of common keywords and change types

## Features

- **Automatic Repository Discovery**: Discover all repositories for a GitHub user/organization
- **Pattern Detection**: Identifies 6 types of common patterns:
  - Dependency updates (e.g., "Bump Package from X to Y")
  - Documentation updates (e.g., "Update README.md")
  - Conventional commits (e.g., "feat:", "fix:", "docs:")
  - Issue/PR references (e.g., "Fixes #123")
  - Co-authored commits
  - Merge commits
- **Statistical Analysis**: Shows most common keywords in commit messages
- **Automation Script Generation**: Creates bash scripts and regex patterns for repeating changes

## Installation

### Prerequisites

- Python 3.6+
- GitHub CLI (`gh`) installed and authenticated
- Git repository access

### Setup

```bash
# Install GitHub CLI if not already installed
# For Ubuntu/Debian:
sudo apt install gh

# For macOS:
brew install gh

# Authenticate with GitHub
gh auth login

# Make the script executable
chmod +x pattern_discovery.py
```

## Usage

### Basic Usage

Analyze specific repositories:

```bash
python3 pattern_discovery.py --owner konard --repos LinksPlatform,AntlrExample
```

### Auto-Discovery Mode

Automatically discover and analyze all repositories for an owner:

```bash
python3 pattern_discovery.py --owner konard --auto-discover --limit 20
```

### Full Options

```bash
python3 pattern_discovery.py \
  --owner konard \
  --auto-discover \
  --limit 20 \
  --output pattern_report.md
```

### Command-Line Arguments

- `--owner` (required): GitHub repository owner/organization
- `--repos`: Comma-separated list of repository names (alternative to --auto-discover)
- `--auto-discover`: Automatically discover repositories for the owner
- `--limit`: Maximum number of repositories to analyze (default: 20)
- `--output`: Output file for automation scripts (default: pattern_discovery_report.md)

## Output

The tool generates:

1. **Console Output**: Real-time analysis report with:
   - Number of commits and PRs analyzed
   - Pattern occurrences with examples
   - Most common keywords

2. **Markdown File**: Automation scripts and regex patterns including:
   - Bash scripts for common patterns
   - Regex patterns for pattern matching
   - Ready-to-use automation templates

## Example Output

```
================================================================================
PATTERN DISCOVERY REPORT
================================================================================

Owner: konard
Total commits analyzed: 314
Total PRs analyzed: 40

--------------------------------------------------------------------------------
DISCOVERED PATTERNS
--------------------------------------------------------------------------------

Documentation Updates: 70 occurrences
  Examples:
    - [twittermatrix] Update README.md
    - [LinksPlatform] Update links-theory.md
    - [LinksPlatform] Update README.md

Dependency Updates: 4 occurrences
  Examples:
    - [LinksPlatform] Bump Platform.Data.Doublets from 0.6.8 to 0.6.10
```

## Real-World Applications

### 1. Dependency Update Automation

Use the generated regex pattern to detect dependency updates:

```regex
(update|bump|upgrade)\s+([a-zA-Z0-9._-]+)\s+(from|to)\s+(\d+\.\d+\.\d+)
```

Create automated commits:

```bash
#!/bin/bash
PACKAGE_NAME="$1"
OLD_VERSION="$2"
NEW_VERSION="$3"

git add .
git commit -m "Bump ${PACKAGE_NAME} from ${OLD_VERSION} to ${NEW_VERSION}"
```

### 2. Documentation Update Automation

Detect documentation updates:

```regex
(update|add|fix)\s+([a-zA-Z0-9._-]+\.(md|txt))
```

Automate documentation commits:

```bash
#!/bin/bash
DOC_FILE="$1"

git add "${DOC_FILE}"
git commit -m "Update ${DOC_FILE}"
```

### 3. CI/CD Integration

Integrate with CI/CD pipelines to:
- Validate commit message formats
- Auto-generate changelog entries
- Trigger specific workflows based on detected patterns

### 4. Repository Health Monitoring

Track patterns over time to:
- Measure documentation maintenance frequency
- Monitor dependency update cadence
- Identify common change types

## Detected Patterns Explained

### 1. Dependency Updates
Matches commits that update package dependencies with version numbers.

**Example**: "Bump Platform.Data.Doublets from 0.6.8 to 0.6.10"

### 2. Documentation Updates
Matches commits that modify documentation files (.md, .txt).

**Example**: "Update README.md"

### 3. Conventional Commits
Matches commits following the Conventional Commits specification.

**Example**: "feat(api): add new endpoint"

### 4. Issue References
Matches commits that reference issues or PRs with keywords like "fixes", "closes".

**Example**: "Fix authentication bug - Fixes #123"

### 5. Co-authored Commits
Matches commits with multiple authors.

**Example**: Commits containing "Co-Authored-By:"

### 6. Merge Commits
Matches pull request and branch merge commits.

**Example**: "Merge pull request #42 from user/branch"

## Technical Details

### Architecture

```
pattern_discovery.py
├── PatternDiscovery (main class)
│   ├── discover_repositories()    # Find repos via GitHub API
│   ├── fetch_commits()            # Get commit history
│   ├── fetch_prs()                # Get PR data
│   ├── analyze_commits()          # Process commits
│   ├── analyze_prs()              # Process PRs
│   ├── extract_patterns()         # Pattern matching
│   ├── generate_report()          # Console output
│   └── generate_automation_scripts() # Generate scripts
```

### API Calls

The tool uses GitHub CLI (`gh`) which handles authentication and rate limiting:

- `gh search repos`: Repository discovery
- `gh api repos/*/commits`: Fetch commit history
- `gh pr list`: Fetch pull requests

### Performance

- Processes 50 commits per repository (configurable)
- Fetches 30 PRs per repository (configurable)
- Handles 20+ repositories in under 2 minutes
- Respects GitHub API rate limits via `gh` CLI

## Extending the Tool

### Adding New Patterns

Add new pattern detection in `extract_patterns()`:

```python
# Add your custom pattern
custom_pattern = re.compile(r'your_regex_here', re.IGNORECASE)

# Check against commits
if custom_pattern.search(msg):
    patterns_found['custom_pattern'].append(commit)
```

### Custom Automation Scripts

Extend `generate_automation_scripts()` to add custom script templates:

```python
if patterns['your_pattern']:
    scripts.append("## Pattern: Your Pattern\n")
    scripts.append("```bash\n")
    scripts.append("# Your automation script here\n")
    scripts.append("```\n\n")
```

## Troubleshooting

### GitHub CLI Authentication

If you get authentication errors:

```bash
gh auth login
gh auth status
```

### Rate Limiting

If you hit GitHub API rate limits:

- Reduce `--limit` parameter
- Analyze fewer repositories
- Wait for rate limit reset (shown in error message)

### No Patterns Found

If no patterns are detected:

- Verify repositories have commit history
- Check that GitHub CLI is properly authenticated
- Ensure repositories are public or you have access

## Contributing

To contribute:

1. Fork the repository
2. Add new pattern detectors
3. Improve automation script templates
4. Submit a pull request

## License

This tool is part of the LinksPlatform project and follows the same license.

## Related Issues

- Issue #482: Discover patterns related to the same description of changes on GitHub
- PR #924: Implementation of pattern discovery tool

## Future Enhancements

Potential improvements:

- [ ] Add support for GitLab, Bitbucket
- [ ] Machine learning-based pattern detection
- [ ] Interactive mode for pattern selection
- [ ] Export to JSON/CSV formats
- [ ] Web interface for visualization
- [ ] Pattern similarity scoring
- [ ] Automatic PR generation for repeatable changes
- [ ] Integration with Dependabot patterns
- [ ] Custom pattern definition via config file
- [ ] Cross-repository change tracking
