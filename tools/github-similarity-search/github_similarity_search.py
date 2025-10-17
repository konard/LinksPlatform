#!/usr/bin/env python3
"""
GitHub Similarity Search Tool

Finds similar organizations, repositories, issues, or files on GitHub based on word matching.
Each word found in both items counts as rank. Items are sorted by number of similarities (rank).

Usage:
    python github_similarity_search.py --type <org|repo|issue|file> --query "search terms" [--target "comparison terms"]
"""

import argparse
import json
import re
import sys
from collections import Counter
from typing import List, Dict, Tuple, Set
import subprocess


class GitHubSimilaritySearch:
    """GitHub similarity search using word-based ranking algorithm."""

    def __init__(self, github_token: str = None):
        """
        Initialize the search tool.

        Args:
            github_token: GitHub personal access token (optional, uses gh CLI auth if not provided)
        """
        self.github_token = github_token

    def tokenize(self, text: str) -> Set[str]:
        """
        Tokenize text into words, converting to lowercase and removing special characters.

        Args:
            text: Input text to tokenize

        Returns:
            Set of unique words
        """
        if not text:
            return set()

        # Convert to lowercase and split by non-alphanumeric characters
        words = re.findall(r'\b[a-z0-9]+\b', text.lower())

        # Filter out very short words (likely not meaningful)
        return set(word for word in words if len(word) > 1)

    def calculate_similarity(self, text1: str, text2: str) -> int:
        """
        Calculate similarity rank between two texts based on common words.

        Args:
            text1: First text
            text2: Second text

        Returns:
            Number of common words (rank)
        """
        words1 = self.tokenize(text1)
        words2 = self.tokenize(text2)

        # Count common words
        return len(words1.intersection(words2))

    def search_organizations(self, query: str, limit: int = 30) -> List[Dict]:
        """
        Search for GitHub organizations.

        Args:
            query: Search query
            limit: Maximum number of results

        Returns:
            List of organization data
        """
        try:
            # Note: gh search doesn't support organizations directly, so we search repos and extract unique orgs
            cmd = ['gh', 'search', 'repos', query, '--limit', str(limit),
                   '--json', 'owner,description,url']
            result = subprocess.run(cmd, capture_output=True, text=True, check=True)
            data = json.loads(result.stdout)

            # Extract unique organizations from repository results
            orgs_dict = {}
            for item in data:
                owner = item.get('owner', {})
                login = owner.get('login', '')
                # Try to determine if it's an organization (not perfect, but reasonable heuristic)
                if login and login not in orgs_dict:
                    orgs_dict[login] = {
                        'login': login,
                        'description': item.get('description', ''),  # Using repo description as proxy
                        'url': f"https://github.com/{login}",
                        'type': 'organization'
                    }

            return list(orgs_dict.values())
        except subprocess.CalledProcessError as e:
            print(f"Error searching organizations: {e.stderr}", file=sys.stderr)
            return []
        except json.JSONDecodeError as e:
            print(f"Error parsing JSON response: {e}", file=sys.stderr)
            return []

    def search_repositories(self, query: str, limit: int = 30) -> List[Dict]:
        """
        Search for GitHub repositories.

        Args:
            query: Search query
            limit: Maximum number of results

        Returns:
            List of repository data
        """
        try:
            cmd = ['gh', 'search', 'repos', query, '--limit', str(limit),
                   '--json', 'fullName,description,url,language,stargazersCount']
            result = subprocess.run(cmd, capture_output=True, text=True, check=True)
            data = json.loads(result.stdout)

            repos = []
            for item in data:
                repos.append({
                    'name': item.get('fullName', ''),
                    'description': item.get('description', ''),
                    'url': item.get('url', ''),
                    'language': item.get('language', ''),
                    'stars': item.get('stargazersCount', 0),
                    'type': 'repository'
                })

            return repos
        except subprocess.CalledProcessError as e:
            print(f"Error searching repositories: {e.stderr}", file=sys.stderr)
            return []
        except json.JSONDecodeError as e:
            print(f"Error parsing JSON response: {e}", file=sys.stderr)
            return []

    def search_issues(self, query: str, limit: int = 30) -> List[Dict]:
        """
        Search for GitHub issues.

        Args:
            query: Search query
            limit: Maximum number of results

        Returns:
            List of issue data
        """
        try:
            cmd = ['gh', 'search', 'issues', query, '--limit', str(limit),
                   '--json', 'title,body,url,state,labels']
            result = subprocess.run(cmd, capture_output=True, text=True, check=True)
            data = json.loads(result.stdout)

            issues = []
            for item in data:
                issues.append({
                    'title': item.get('title', ''),
                    'body': item.get('body', ''),
                    'url': item.get('url', ''),
                    'state': item.get('state', ''),
                    'labels': [label.get('name', '') for label in item.get('labels', [])],
                    'type': 'issue'
                })

            return issues
        except subprocess.CalledProcessError as e:
            print(f"Error searching issues: {e.stderr}", file=sys.stderr)
            return []
        except json.JSONDecodeError as e:
            print(f"Error parsing JSON response: {e}", file=sys.stderr)
            return []

    def search_code(self, query: str, limit: int = 30) -> List[Dict]:
        """
        Search for files/code on GitHub.

        Args:
            query: Search query
            limit: Maximum number of results

        Returns:
            List of file/code data
        """
        try:
            cmd = ['gh', 'search', 'code', query, '--limit', str(limit),
                   '--json', 'path,repository,url']
            result = subprocess.run(cmd, capture_output=True, text=True, check=True)
            data = json.loads(result.stdout)

            files = []
            for item in data:
                path = item.get('path', '')
                repo_data = item.get('repository', {})
                repo_name = repo_data.get('fullName', '') if isinstance(repo_data, dict) else str(repo_data)

                files.append({
                    'name': path.split('/')[-1] if path else '',
                    'path': path,
                    'repository': repo_name,
                    'url': item.get('url', ''),
                    'type': 'file'
                })

            return files
        except subprocess.CalledProcessError as e:
            print(f"Error searching code: {e.stderr}", file=sys.stderr)
            return []
        except json.JSONDecodeError as e:
            print(f"Error parsing JSON response: {e}", file=sys.stderr)
            return []

    def get_item_text(self, item: Dict) -> str:
        """
        Extract searchable text from an item based on its type.

        Args:
            item: Item data

        Returns:
            Combined text for similarity comparison
        """
        item_type = item.get('type', '')

        if item_type == 'organization':
            return f"{item.get('login', '')} {item.get('description', '')}"
        elif item_type == 'repository':
            return f"{item.get('name', '')} {item.get('description', '')} {item.get('language', '')}"
        elif item_type == 'issue':
            labels = ' '.join(item.get('labels', []))
            return f"{item.get('title', '')} {item.get('body', '')} {labels}"
        elif item_type == 'file':
            return f"{item.get('name', '')} {item.get('path', '')} {item.get('repository', '')}"

        return ''

    def rank_by_similarity(self, items: List[Dict], target_text: str) -> List[Tuple[Dict, int]]:
        """
        Rank items by similarity to target text.

        Args:
            items: List of items to rank
            target_text: Text to compare against

        Returns:
            List of (item, rank) tuples sorted by rank (descending)
        """
        ranked = []

        for item in items:
            item_text = self.get_item_text(item)
            rank = self.calculate_similarity(item_text, target_text)
            ranked.append((item, rank))

        # Sort by rank (descending), then by item text (for consistency)
        ranked.sort(key=lambda x: (-x[1], self.get_item_text(x[0])))

        return ranked

    def format_result(self, item: Dict, rank: int) -> str:
        """
        Format a single result for display.

        Args:
            item: Item data
            rank: Similarity rank

        Returns:
            Formatted string
        """
        item_type = item.get('type', '')

        if item_type == 'organization':
            return f"[Rank: {rank}] Organization: {item.get('login', '')} - {item.get('description', 'N/A')}\n  URL: {item.get('url', '')}"
        elif item_type == 'repository':
            stars = item.get('stars', 0)
            lang = item.get('language', 'N/A')
            return f"[Rank: {rank}] Repository: {item.get('name', '')} ({lang}, ⭐ {stars})\n  Description: {item.get('description', 'N/A')}\n  URL: {item.get('url', '')}"
        elif item_type == 'issue':
            state = item.get('state', 'unknown')
            labels = ', '.join(item.get('labels', []))
            return f"[Rank: {rank}] Issue: {item.get('title', '')} [{state}]\n  Labels: {labels if labels else 'N/A'}\n  URL: {item.get('url', '')}"
        elif item_type == 'file':
            return f"[Rank: {rank}] File: {item.get('path', '')} in {item.get('repository', '')}\n  URL: {item.get('url', '')}"

        return f"[Rank: {rank}] Unknown type: {item}"

    def search_and_rank(self, search_type: str, query: str, target: str = None, limit: int = 30, top_n: int = 10) -> None:
        """
        Search and rank results by similarity.

        Args:
            search_type: Type of search (org, repo, issue, file)
            query: Search query
            target: Target text for similarity comparison (defaults to query)
            limit: Maximum number of results to fetch
            top_n: Number of top results to display
        """
        if target is None:
            target = query

        print(f"Searching for {search_type}s matching: '{query}'")
        print(f"Ranking by similarity to: '{target}'")
        print("-" * 80)

        # Perform search based on type
        if search_type == 'org':
            items = self.search_organizations(query, limit)
        elif search_type == 'repo':
            items = self.search_repositories(query, limit)
        elif search_type == 'issue':
            items = self.search_issues(query, limit)
        elif search_type == 'file':
            items = self.search_code(query, limit)
        else:
            print(f"Unknown search type: {search_type}", file=sys.stderr)
            return

        if not items:
            print("No results found.")
            return

        print(f"Found {len(items)} items. Ranking by similarity...\n")

        # Rank by similarity
        ranked_items = self.rank_by_similarity(items, target)

        # Display top N results
        for i, (item, rank) in enumerate(ranked_items[:top_n], 1):
            print(f"{i}. {self.format_result(item, rank)}")
            print()

        if len(ranked_items) > top_n:
            print(f"... and {len(ranked_items) - top_n} more results with lower similarity ranks.")


def main():
    """Main entry point."""
    parser = argparse.ArgumentParser(
        description='GitHub Similarity Search - Find similar organizations, repositories, issues, or files',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # Search for repositories similar to a query
  python github_similarity_search.py --type repo --query "machine learning"

  # Search for organizations similar to a specific description
  python github_similarity_search.py --type org --query "open source" --target "artificial intelligence research"

  # Search for issues related to a topic
  python github_similarity_search.py --type issue --query "bug authentication"

  # Search for files with specific content
  python github_similarity_search.py --type file --query "dockerfile python"
        """
    )

    parser.add_argument(
        '--type',
        choices=['org', 'repo', 'issue', 'file'],
        required=True,
        help='Type of item to search for'
    )

    parser.add_argument(
        '--query',
        required=True,
        help='Search query to find items'
    )

    parser.add_argument(
        '--target',
        help='Target text for similarity comparison (defaults to query)'
    )

    parser.add_argument(
        '--limit',
        type=int,
        default=30,
        help='Maximum number of results to fetch (default: 30)'
    )

    parser.add_argument(
        '--top',
        type=int,
        default=10,
        help='Number of top results to display (default: 10)'
    )

    args = parser.parse_args()

    # Check if gh CLI is available
    try:
        subprocess.run(['gh', '--version'], capture_output=True, check=True)
    except (subprocess.CalledProcessError, FileNotFoundError):
        print("Error: GitHub CLI (gh) is not installed or not in PATH.", file=sys.stderr)
        print("Please install it from: https://cli.github.com/", file=sys.stderr)
        sys.exit(1)

    # Create search instance and run
    search = GitHubSimilaritySearch()
    search.search_and_rank(
        search_type=args.type,
        query=args.query,
        target=args.target,
        limit=args.limit,
        top_n=args.top
    )


if __name__ == '__main__':
    main()
