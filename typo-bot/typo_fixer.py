#!/usr/bin/env python3
"""
TypoBot - Automated typo detection and fixing bot for GitHub repositories.

This bot searches for common typos across GitHub repositories and creates
pull requests to fix them automatically.
"""

import os
import sys
import json
import argparse
import subprocess
import re
from pathlib import Path
from typing import List, Dict, Tuple
import time


class TypoBot:
    """Main class for typo detection and fixing bot."""

    def __init__(self, config_path: str = "typos.json", dry_run: bool = False):
        """
        Initialize the TypoBot.

        Args:
            config_path: Path to the typos configuration file
            dry_run: If True, don't create PRs, just report findings
        """
        self.config_path = config_path
        self.dry_run = dry_run
        self.typos = self.load_typos()
        self.github_token = os.environ.get("GITHUB_TOKEN", "")

    def load_typos(self) -> List[Dict[str, str]]:
        """Load typo definitions from configuration file."""
        try:
            with open(self.config_path, 'r', encoding='utf-8') as f:
                data = json.load(f)
                return data.get('typos', [])
        except FileNotFoundError:
            print(f"Warning: Config file {self.config_path} not found. Using defaults.")
            return [
                {
                    "incorrect": "внезависимости",
                    "correct": "независимости",
                    "description": "Russian word for 'independence' - 'вне' prefix is incorrect"
                }
            ]

    def search_github(self, query: str, max_results: int = 100) -> List[str]:
        """
        Search GitHub for repositories containing the query.

        Args:
            query: Search query
            max_results: Maximum number of results to return

        Returns:
            List of repository full names (owner/repo)
        """
        print(f"Searching GitHub for: {query}")

        try:
            # Use gh CLI to search code
            cmd = [
                "gh", "search", "code",
                query,
                "--limit", str(max_results),
                "--json", "repository"
            ]

            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                check=True
            )

            if not result.stdout.strip():
                print(f"No results found for '{query}'")
                return []

            data = json.loads(result.stdout)
            repos = []
            seen = set()

            for item in data:
                repo_info = item.get('repository', {})
                full_name = repo_info.get('fullName', '')

                if full_name and full_name not in seen:
                    repos.append(full_name)
                    seen.add(full_name)

            print(f"Found {len(repos)} repositories with '{query}'")
            return repos

        except subprocess.CalledProcessError as e:
            print(f"Error searching GitHub: {e}")
            print(f"Stderr: {e.stderr}")
            return []
        except json.JSONDecodeError as e:
            print(f"Error parsing search results: {e}")
            return []

    def clone_repo(self, repo_full_name: str, target_dir: str) -> bool:
        """
        Clone a repository.

        Args:
            repo_full_name: Repository full name (owner/repo)
            target_dir: Directory to clone into

        Returns:
            True if successful, False otherwise
        """
        try:
            cmd = ["gh", "repo", "clone", repo_full_name, target_dir]
            subprocess.run(cmd, check=True, capture_output=True)
            return True
        except subprocess.CalledProcessError as e:
            print(f"Error cloning {repo_full_name}: {e}")
            return False

    def find_typos_in_file(self, file_path: Path, typo_dict: Dict[str, str]) -> List[Tuple[int, str]]:
        """
        Find typos in a file.

        Args:
            file_path: Path to the file
            typo_dict: Dictionary with 'incorrect' and 'correct' keys

        Returns:
            List of (line_number, line_content) tuples containing typos
        """
        matches = []
        incorrect = typo_dict['incorrect']

        try:
            with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
                for line_num, line in enumerate(f, 1):
                    if incorrect in line:
                        matches.append((line_num, line.rstrip()))
        except Exception as e:
            print(f"Error reading {file_path}: {e}")

        return matches

    def fix_typos_in_file(self, file_path: Path, typo_dict: Dict[str, str]) -> int:
        """
        Fix typos in a file.

        Args:
            file_path: Path to the file
            typo_dict: Dictionary with 'incorrect' and 'correct' keys

        Returns:
            Number of replacements made
        """
        incorrect = typo_dict['incorrect']
        correct = typo_dict['correct']

        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()

            new_content = content.replace(incorrect, correct)

            if new_content != content:
                with open(file_path, 'w', encoding='utf-8') as f:
                    f.write(new_content)

                count = content.count(incorrect)
                return count

        except Exception as e:
            print(f"Error fixing typos in {file_path}: {e}")

        return 0

    def scan_repository(self, repo_path: str, typo_dict: Dict[str, str]) -> Dict[str, List[Tuple[int, str]]]:
        """
        Scan repository for typos.

        Args:
            repo_path: Path to repository
            typo_dict: Dictionary with 'incorrect' and 'correct' keys

        Returns:
            Dictionary mapping file paths to list of matches
        """
        results = {}
        repo_pathobj = Path(repo_path)

        # File extensions to check (text files)
        text_extensions = {
            '.md', '.txt', '.rst', '.asciidoc', '.adoc',
            '.py', '.js', '.ts', '.java', '.c', '.cpp', '.h', '.hpp',
            '.cs', '.rb', '.go', '.rs', '.php', '.html', '.css',
            '.json', '.yaml', '.yml', '.xml', '.toml', '.ini',
            '.sh', '.bash', '.zsh', '.fish'
        }

        for file_path in repo_pathobj.rglob('*'):
            # Skip directories and hidden files
            if file_path.is_dir() or file_path.name.startswith('.'):
                continue

            # Skip if not a text file
            if file_path.suffix.lower() not in text_extensions and not file_path.suffix == '':
                continue

            matches = self.find_typos_in_file(file_path, typo_dict)
            if matches:
                rel_path = file_path.relative_to(repo_pathobj)
                results[str(rel_path)] = matches

        return results

    def create_pull_request(self, repo_path: str, repo_full_name: str,
                          typo_dict: Dict[str, str], files_changed: int) -> bool:
        """
        Create a pull request with typo fixes.

        Args:
            repo_path: Path to local repository
            repo_full_name: Full name of repository (owner/repo)
            typo_dict: Dictionary with typo information
            files_changed: Number of files changed

        Returns:
            True if successful, False otherwise
        """
        incorrect = typo_dict['incorrect']
        correct = typo_dict['correct']
        description = typo_dict.get('description', 'Typo fix')

        branch_name = f"fix-typo-{incorrect.replace(' ', '-')}"

        try:
            # Create and checkout new branch
            subprocess.run(
                ["git", "-C", repo_path, "checkout", "-b", branch_name],
                check=True,
                capture_output=True
            )

            # Add all changes
            subprocess.run(
                ["git", "-C", repo_path, "add", "-A"],
                check=True,
                capture_output=True
            )

            # Commit changes
            commit_msg = f"Fix typo: '{incorrect}' → '{correct}'\n\n{description}"
            subprocess.run(
                ["git", "-C", repo_path, "commit", "-m", commit_msg],
                check=True,
                capture_output=True
            )

            # Push to fork (requires proper setup)
            subprocess.run(
                ["git", "-C", repo_path, "push", "-u", "origin", branch_name],
                check=True,
                capture_output=True
            )

            # Create PR
            pr_title = f"Fix typo: '{incorrect}' → '{correct}'"
            pr_body = f"""## Summary
This PR fixes a typo found in {files_changed} file(s).

**Incorrect:** `{incorrect}`
**Correct:** `{correct}`

{description}

---
🤖 This PR was created automatically by [TypoBot](https://github.com/konard/LinksPlatform/issues/471)
"""

            subprocess.run(
                ["gh", "pr", "create",
                 "--repo", repo_full_name,
                 "--title", pr_title,
                 "--body", pr_body],
                check=True,
                capture_output=True,
                cwd=repo_path
            )

            print(f"✓ Created PR for {repo_full_name}")
            return True

        except subprocess.CalledProcessError as e:
            print(f"Error creating PR for {repo_full_name}: {e}")
            return False

    def process_repository(self, repo_full_name: str, typo_dict: Dict[str, str]) -> bool:
        """
        Process a single repository: clone, fix typos, create PR.

        Args:
            repo_full_name: Repository full name (owner/repo)
            typo_dict: Dictionary with typo information

        Returns:
            True if successful, False otherwise
        """
        print(f"\nProcessing: {repo_full_name}")

        # Create temp directory for cloning
        temp_dir = f"/tmp/typobot_{repo_full_name.replace('/', '_')}"

        try:
            # Clone repository
            if not self.clone_repo(repo_full_name, temp_dir):
                return False

            # Scan for typos
            results = self.scan_repository(temp_dir, typo_dict)

            if not results:
                print(f"  No typos found in {repo_full_name}")
                return False

            print(f"  Found typos in {len(results)} file(s):")
            for file_path, matches in results.items():
                print(f"    - {file_path}: {len(matches)} occurrence(s)")

            if self.dry_run:
                print(f"  [DRY RUN] Would create PR for {repo_full_name}")
                return True

            # Fix typos
            total_fixes = 0
            for file_path in results.keys():
                full_path = Path(temp_dir) / file_path
                fixes = self.fix_typos_in_file(full_path, typo_dict)
                total_fixes += fixes

            print(f"  Fixed {total_fixes} occurrence(s)")

            # Create PR
            return self.create_pull_request(temp_dir, repo_full_name, typo_dict, len(results))

        finally:
            # Cleanup
            if os.path.exists(temp_dir):
                subprocess.run(["rm", "-rf", temp_dir], capture_output=True)

    def run(self, max_repos: int = 10, search_query: str = None):
        """
        Run the typo bot.

        Args:
            max_repos: Maximum number of repositories to process
            search_query: Optional custom search query (otherwise uses first typo)
        """
        print("=" * 60)
        print("TypoBot - Automated Typo Fixer")
        print("=" * 60)

        if not self.typos:
            print("Error: No typos configured!")
            return

        # Use first typo for search if no custom query provided
        typo_dict = self.typos[0]
        query = search_query or typo_dict['incorrect']

        print(f"\nConfiguration:")
        print(f"  Typo: '{typo_dict['incorrect']}' → '{typo_dict['correct']}'")
        print(f"  Description: {typo_dict.get('description', 'N/A')}")
        print(f"  Max repositories: {max_repos}")
        print(f"  Dry run: {self.dry_run}")

        # Search for repositories
        repos = self.search_github(query, max_results=max_repos * 2)

        if not repos:
            print("\nNo repositories found!")
            return

        # Limit to max_repos
        repos = repos[:max_repos]

        print(f"\nWill process {len(repos)} repository(ies)")

        # Process each repository
        successful = 0
        failed = 0

        for i, repo in enumerate(repos, 1):
            print(f"\n[{i}/{len(repos)}] ", end="")

            if self.process_repository(repo, typo_dict):
                successful += 1
            else:
                failed += 1

            # Rate limiting - be nice to GitHub
            if i < len(repos):
                time.sleep(2)

        # Summary
        print("\n" + "=" * 60)
        print("Summary:")
        print(f"  Successful: {successful}")
        print(f"  Failed: {failed}")
        print(f"  Total: {len(repos)}")
        print("=" * 60)


def main():
    """Main entry point."""
    parser = argparse.ArgumentParser(
        description='TypoBot - Automated typo detection and fixing for GitHub'
    )
    parser.add_argument(
        '--config',
        default='typos.json',
        help='Path to typos configuration file (default: typos.json)'
    )
    parser.add_argument(
        '--dry-run',
        action='store_true',
        help='Dry run mode - search and report but don\'t create PRs'
    )
    parser.add_argument(
        '--max-repos',
        type=int,
        default=10,
        help='Maximum number of repositories to process (default: 10)'
    )
    parser.add_argument(
        '--search',
        help='Custom search query (default: use first typo from config)'
    )

    args = parser.parse_args()

    bot = TypoBot(config_path=args.config, dry_run=args.dry_run)
    bot.run(max_repos=args.max_repos, search_query=args.search)


if __name__ == '__main__':
    main()
