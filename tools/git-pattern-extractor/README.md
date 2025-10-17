# Git Pattern Extractor

A tool to extract, merge, split, and replay change patterns from Git history. This tool analyzes Git commits to identify patterns of changes that can be reproduced in other similar repositories.

## Features

- **Extract**: Extract change patterns from Git commit history
- **Merge**: Combine multiple patterns into one
- **Split**: Break apart patterns by author, file type, or commit type
- **Replay**: Apply patterns to other repositories (with dry-run support)

## Installation

No installation required - just Python 3.6+ and Git.

```bash
# Make the script executable
chmod +x git_pattern_extractor.py
```

## Usage

### Extract Patterns

Extract patterns from the current repository:

```bash
./git_pattern_extractor.py extract --output pattern.json
```

Extract patterns from a specific commit range:

```bash
./git_pattern_extractor.py extract --range HEAD~10..HEAD --output recent_pattern.json
```

Extract patterns from a different repository:

```bash
./git_pattern_extractor.py extract --repo /path/to/repo --output pattern.json
```

### Merge Patterns

Combine multiple patterns:

```bash
./git_pattern_extractor.py merge pattern1.json pattern2.json pattern3.json --output merged_pattern.json
```

### Split Patterns

Split a pattern by author:

```bash
./git_pattern_extractor.py split pattern.json --by author --output-dir split_patterns/
```

Split by file type:

```bash
./git_pattern_extractor.py split pattern.json --by file_type --output-dir split_patterns/
```

Split by commit type (conventional commits):

```bash
./git_pattern_extractor.py split pattern.json --by commit_type --output-dir split_patterns/
```

### Replay Patterns

Dry-run (preview changes):

```bash
./git_pattern_extractor.py replay pattern.json --target /path/to/target/repo
```

Apply changes (use with caution):

```bash
./git_pattern_extractor.py replay pattern.json --target /path/to/target/repo --no-dry-run
```

## Pattern Structure

The extracted pattern JSON contains:

- `repository`: Source repository path
- `extracted_at`: Timestamp of extraction
- `commit_range`: Range of commits analyzed
- `total_commits`: Number of commits in pattern
- `commits`: Array of commit objects with:
  - `hash`: Commit SHA
  - `author`: Author name and email
  - `timestamp`: Unix timestamp
  - `date`: ISO format date
  - `subject`: Commit message
  - `files_changed`: List of changed files
  - `diff`: Full diff content
- `patterns`: Analyzed patterns including:
  - `file_co_changes`: Files that frequently change together
  - `commit_message_patterns`: Common commit message types
  - `file_type_changes`: Distribution of file types changed
  - `author_patterns`: Which authors change which files

## Example Workflow

1. **Extract a pattern from your main repository**:
   ```bash
   ./git_pattern_extractor.py extract --output main_pattern.json
   ```

2. **Split the pattern by feature area**:
   ```bash
   ./git_pattern_extractor.py split main_pattern.json --by commit_type --output-dir features/
   ```

3. **Replay a specific pattern to a similar repository**:
   ```bash
   ./git_pattern_extractor.py replay features/pattern_commit_type_feat.json --target ../similar-repo
   ```

## Use Cases

- **Repository Synchronization**: Extract patterns from one repository and apply them to similar repositories
- **Refactoring Analysis**: Identify files that change together for better code organization
- **Team Workflow Analysis**: Understand team patterns and file ownership
- **Change Template Creation**: Create reusable change templates from successful patterns
- **Multi-Repo Management**: Maintain consistency across multiple similar repositories

## Requirements

- Python 3.6 or higher
- Git command-line tool
- A Git repository to analyze

## Limitations

- The replay functionality currently supports dry-run mode only
- Pattern application requires manual review for file path mapping
- Works best with repositories that follow conventional commit messages

## Contributing

This tool is part of the LinksPlatform project. For contributions, please follow the repository's contributing guidelines.
