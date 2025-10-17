# GitHub Similarity Search

A tool to find similar organizations, repositories, issues, or files on GitHub based on word matching. Each word found in both items counts as rank. Items are sorted by number of similarities (rank).

## Features

- **Multiple search types**: Search for organizations, repositories, issues, or files
- **Word-based ranking**: Calculates similarity based on common words between items
- **Flexible querying**: Search with one query and rank against different target text
- **Customizable results**: Control the number of results fetched and displayed

## Prerequisites

- Python 3.6 or later
- [GitHub CLI (gh)](https://cli.github.com/) installed and authenticated

## Installation

1. Ensure GitHub CLI is installed and authenticated:
   ```bash
   gh auth login
   ```

2. Make the script executable (optional):
   ```bash
   chmod +x github_similarity_search.py
   ```

## Usage

### Basic syntax

```bash
python github_similarity_search.py --type <org|repo|issue|file> --query "search terms" [--target "comparison terms"]
```

### Options

- `--type`: Type of item to search for (required)
  - `org`: Organizations
  - `repo`: Repositories
  - `issue`: Issues and pull requests
  - `file`: Code/files

- `--query`: Search query to find items (required)

- `--target`: Target text for similarity comparison (optional, defaults to query)

- `--limit`: Maximum number of results to fetch (default: 30)

- `--top`: Number of top results to display (default: 10)

### Examples

#### Search for repositories similar to "machine learning"
```bash
python github_similarity_search.py --type repo --query "machine learning"
```

#### Search for organizations and rank by similarity to specific text
```bash
python github_similarity_search.py --type org --query "open source" --target "artificial intelligence research"
```

#### Search for issues related to authentication bugs
```bash
python github_similarity_search.py --type issue --query "bug authentication"
```

#### Search for Dockerfiles in Python projects
```bash
python github_similarity_search.py --type file --query "dockerfile python"
```

#### Get more results
```bash
python github_similarity_search.py --type repo --query "data science" --limit 50 --top 20
```

## How it works

1. **Search**: The tool uses GitHub CLI to search for items matching the query
2. **Tokenization**: Each item's text (name, description, labels, etc.) is tokenized into words
3. **Similarity calculation**: The number of common words between the item and target text is counted
4. **Ranking**: Items are sorted by similarity rank (number of common words) in descending order
5. **Display**: Top N results are displayed with their rank and relevant information

## Algorithm

The similarity ranking algorithm:
1. Converts all text to lowercase
2. Extracts words (alphanumeric sequences)
3. Filters out very short words (1 character)
4. Counts common words between two texts
5. Uses the count as the similarity rank

## Limitations

- Requires GitHub CLI authentication
- Subject to GitHub API rate limits
- Code search may have additional restrictions (requires specific query format)
- Similarity is based purely on word matching, not semantic understanding

## Contributing

This tool is part of the [LinksPlatform](https://github.com/konard/LinksPlatform) project. Contributions are welcome!

## License

See the main repository [LICENSE](../../LICENSE) file.
