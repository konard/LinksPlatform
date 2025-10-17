#!/usr/bin/env python3
"""
Pattern Discovery Tool for GitHub Repositories

This script analyzes commit messages, PR titles, and descriptions across
GitHub repositories to identify repeatable patterns and generate automation
scripts or regex patterns.

Usage:
    python pattern_discovery.py --owner konard --repos LinksPlatform,AntlrExample
    python pattern_discovery.py --owner konard --auto-discover
"""

import argparse
import json
import re
import subprocess
from collections import Counter, defaultdict
from typing import List, Dict, Tuple, Set
import sys


class PatternDiscovery:
    def __init__(self, owner: str):
        self.owner = owner
        self.patterns = defaultdict(list)
        self.commit_messages = []
        self.pr_titles = []

    def run_gh_command(self, command: List[str]) -> str:
        """Execute GitHub CLI command and return output"""
        try:
            result = subprocess.run(
                command,
                capture_output=True,
                text=True,
                check=True
            )
            return result.stdout.strip()
        except subprocess.CalledProcessError as e:
            print(f"Error running command: {' '.join(command)}", file=sys.stderr)
            print(f"Error: {e.stderr}", file=sys.stderr)
            return ""

    def discover_repositories(self, limit: int = 50) -> List[str]:
        """Discover repositories for the owner"""
        print(f"Discovering repositories for owner: {self.owner}")
        output = self.run_gh_command([
            'gh', 'search', 'repos',
            f'--owner', self.owner,
            '--limit', str(limit),
            '--json', 'name'
        ])

        if not output:
            return []

        try:
            repos = json.loads(output)
            repo_names = [repo['name'] for repo in repos]
            print(f"Found {len(repo_names)} repositories")
            return repo_names
        except json.JSONDecodeError:
            return []

    def fetch_commits(self, repo: str, limit: int = 50) -> List[Dict]:
        """Fetch recent commits from a repository"""
        print(f"Fetching commits from {self.owner}/{repo}")
        output = self.run_gh_command([
            'gh', 'api',
            f'repos/{self.owner}/{repo}/commits',
            '--jq', f'.[0:{limit}] | .[] | {{message: .commit.message, date: .commit.author.date, sha: .sha}}'
        ])

        if not output:
            return []

        commits = []
        for line in output.strip().split('\n'):
            if line:
                try:
                    commits.append(json.loads(line))
                except json.JSONDecodeError:
                    continue

        return commits

    def fetch_prs(self, repo: str, state: str = 'merged', limit: int = 30) -> List[Dict]:
        """Fetch pull requests from a repository"""
        print(f"Fetching PRs from {self.owner}/{repo}")
        output = self.run_gh_command([
            'gh', 'pr', 'list',
            '--repo', f'{self.owner}/{repo}',
            '--state', state,
            '--limit', str(limit),
            '--json', 'number,title,body,mergedAt'
        ])

        if not output:
            return []

        try:
            return json.loads(output)
        except json.JSONDecodeError:
            return []

    def analyze_commits(self, repos: List[str]):
        """Analyze commit messages for patterns"""
        print("\n=== Analyzing Commit Messages ===")

        for repo in repos:
            commits = self.fetch_commits(repo)
            for commit in commits:
                msg = commit['message'].split('\n')[0]  # First line only
                self.commit_messages.append({
                    'repo': repo,
                    'message': msg,
                    'date': commit.get('date', ''),
                    'sha': commit.get('sha', '')
                })

        print(f"Collected {len(self.commit_messages)} commit messages")

    def analyze_prs(self, repos: List[str]):
        """Analyze PR titles and descriptions for patterns"""
        print("\n=== Analyzing Pull Requests ===")

        for repo in repos:
            prs = self.fetch_prs(repo)
            for pr in prs:
                self.pr_titles.append({
                    'repo': repo,
                    'number': pr['number'],
                    'title': pr['title'],
                    'body': pr.get('body', '')
                })

        print(f"Collected {len(self.pr_titles)} PR titles")

    def extract_patterns(self):
        """Extract common patterns from collected data"""
        print("\n=== Extracting Patterns ===")

        # Pattern 1: Update/Bump dependencies
        dependency_pattern = re.compile(r'(update|bump|upgrade)\s+([a-zA-Z0-9._-]+)\s+(from|to)\s+(\d+\.\d+\.\d+)', re.IGNORECASE)

        # Pattern 2: Documentation updates
        doc_pattern = re.compile(r'(update|add|fix)\s+([a-zA-Z0-9._-]+\.(md|txt))', re.IGNORECASE)

        # Pattern 3: Conventional commits
        conventional_pattern = re.compile(r'^(feat|fix|docs|style|refactor|test|chore)(\([a-z]+\))?:', re.IGNORECASE)

        # Pattern 4: Issue/PR references
        reference_pattern = re.compile(r'(fixes?|closes?|resolves?)\s+#(\d+)', re.IGNORECASE)

        # Pattern 5: Co-authored commits
        coauthor_pattern = re.compile(r'Co-[Aa]uthored-[Bb]y:', re.IGNORECASE)

        # Pattern 6: Merge commits
        merge_pattern = re.compile(r'^[Mm]erge\s+(pull\s+request|branch)', re.IGNORECASE)

        patterns_found = {
            'dependency_updates': [],
            'documentation_updates': [],
            'conventional_commits': [],
            'issue_references': [],
            'coauthored_commits': [],
            'merge_commits': []
        }

        # Analyze commit messages
        for commit in self.commit_messages:
            msg = commit['message']

            # Check each pattern
            if dependency_pattern.search(msg):
                patterns_found['dependency_updates'].append(commit)

            if doc_pattern.search(msg):
                patterns_found['documentation_updates'].append(commit)

            if conventional_pattern.match(msg):
                patterns_found['conventional_commits'].append(commit)

            if reference_pattern.search(msg):
                patterns_found['issue_references'].append(commit)

            if coauthor_pattern.search(msg):
                patterns_found['coauthored_commits'].append(commit)

            if merge_pattern.match(msg):
                patterns_found['merge_commits'].append(commit)

        return patterns_found

    def generate_report(self, patterns: Dict):
        """Generate a human-readable report of discovered patterns"""
        print("\n" + "="*80)
        print("PATTERN DISCOVERY REPORT")
        print("="*80)

        print(f"\nOwner: {self.owner}")
        print(f"Total commits analyzed: {len(self.commit_messages)}")
        print(f"Total PRs analyzed: {len(self.pr_titles)}")

        print("\n" + "-"*80)
        print("DISCOVERED PATTERNS")
        print("-"*80)

        for pattern_name, matches in patterns.items():
            if matches:
                print(f"\n{pattern_name.replace('_', ' ').title()}: {len(matches)} occurrences")
                print(f"  Examples:")
                for match in matches[:3]:  # Show first 3 examples
                    print(f"    - [{match['repo']}] {match['message'][:80]}")

        # Find most common words in commit messages
        print("\n" + "-"*80)
        print("MOST COMMON KEYWORDS")
        print("-"*80)

        word_counter = Counter()
        for commit in self.commit_messages:
            words = re.findall(r'\b[a-z]{4,}\b', commit['message'].lower())
            word_counter.update(words)

        print("\nTop 20 most common words:")
        for word, count in word_counter.most_common(20):
            print(f"  {word:20} {count:4} times")

    def generate_automation_scripts(self, patterns: Dict) -> str:
        """Generate automation scripts based on discovered patterns"""
        scripts = []

        scripts.append("# Automation Scripts for Discovered Patterns\n")
        scripts.append("# Generated by pattern_discovery.py\n\n")

        # Script for dependency updates
        if patterns['dependency_updates']:
            scripts.append("## Pattern: Dependency Updates\n")
            scripts.append("```bash\n")
            scripts.append("# Example: Automated dependency update commit\n")
            scripts.append('#!/bin/bash\n')
            scripts.append('PACKAGE_NAME="$1"\n')
            scripts.append('OLD_VERSION="$2"\n')
            scripts.append('NEW_VERSION="$3"\n')
            scripts.append('\n')
            scripts.append('git add .\n')
            scripts.append('git commit -m "Bump ${PACKAGE_NAME} from ${OLD_VERSION} to ${NEW_VERSION}"\n')
            scripts.append('```\n\n')

        # Script for documentation updates
        if patterns['documentation_updates']:
            scripts.append("## Pattern: Documentation Updates\n")
            scripts.append("```bash\n")
            scripts.append("# Example: Automated documentation update commit\n")
            scripts.append('#!/bin/bash\n')
            scripts.append('DOC_FILE="$1"\n')
            scripts.append('\n')
            scripts.append('git add "${DOC_FILE}"\n')
            scripts.append('git commit -m "Update ${DOC_FILE}"\n')
            scripts.append('```\n\n')

        # Regex patterns
        scripts.append("## Regex Patterns for Matching\n\n")
        scripts.append("### Dependency Update Pattern\n")
        scripts.append("```regex\n")
        scripts.append(r"(update|bump|upgrade)\s+([a-zA-Z0-9._-]+)\s+(from|to)\s+(\d+\.\d+\.\d+)")
        scripts.append("\n```\n\n")

        scripts.append("### Documentation Update Pattern\n")
        scripts.append("```regex\n")
        scripts.append(r"(update|add|fix)\s+([a-zA-Z0-9._-]+\.(md|txt))")
        scripts.append("\n```\n\n")

        scripts.append("### Conventional Commit Pattern\n")
        scripts.append("```regex\n")
        scripts.append(r"^(feat|fix|docs|style|refactor|test|chore)(\([a-z]+\))?:")
        scripts.append("\n```\n\n")

        return ''.join(scripts)


def main():
    parser = argparse.ArgumentParser(
        description='Discover patterns in GitHub repository changes'
    )
    parser.add_argument(
        '--owner',
        required=True,
        help='GitHub repository owner/organization'
    )
    parser.add_argument(
        '--repos',
        help='Comma-separated list of repository names'
    )
    parser.add_argument(
        '--auto-discover',
        action='store_true',
        help='Automatically discover repositories'
    )
    parser.add_argument(
        '--limit',
        type=int,
        default=20,
        help='Limit number of repositories to analyze (default: 20)'
    )
    parser.add_argument(
        '--output',
        default='pattern_discovery_report.md',
        help='Output file for report (default: pattern_discovery_report.md)'
    )

    args = parser.parse_args()

    # Initialize pattern discovery
    pd = PatternDiscovery(args.owner)

    # Get list of repositories
    if args.auto_discover:
        repos = pd.discover_repositories(limit=args.limit)
    elif args.repos:
        repos = [r.strip() for r in args.repos.split(',')]
    else:
        print("Error: Either --repos or --auto-discover must be specified", file=sys.stderr)
        sys.exit(1)

    if not repos:
        print("No repositories found", file=sys.stderr)
        sys.exit(1)

    # Analyze commits and PRs
    pd.analyze_commits(repos)
    pd.analyze_prs(repos)

    # Extract patterns
    patterns = pd.extract_patterns()

    # Generate report
    pd.generate_report(patterns)

    # Generate automation scripts
    automation_scripts = pd.generate_automation_scripts(patterns)

    # Save report to file
    with open(args.output, 'w') as f:
        f.write(automation_scripts)

    print(f"\n\nAutomation scripts saved to: {args.output}")


if __name__ == '__main__':
    main()
