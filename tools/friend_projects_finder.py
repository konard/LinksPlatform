#!/usr/bin/env python3
"""
Friend Projects Finder for LinksPlatform

This tool searches GitHub for projects that have similar functions or code patterns
as LinksPlatform libraries. These "friend projects" could potentially benefit from
using LinksPlatform libraries to reduce code duplication.

Usage:
    python3 friend_projects_finder.py --language csharp --search-terms "Range,ILink"
    python3 friend_projects_finder.py --org linksplatform --analyze
"""

import argparse
import json
import re
import subprocess
import sys
from collections import defaultdict
from dataclasses import dataclass
from typing import List, Dict, Set, Optional


@dataclass
class CodePattern:
    """Represents a code pattern to search for"""
    name: str
    pattern: str
    description: str
    language: str


@dataclass
class SearchResult:
    """Represents a search result from GitHub"""
    repo_name: str
    file_path: str
    matches: List[str]
    url: str


class FriendProjectsFinder:
    """Main class for finding friend projects on GitHub"""

    def __init__(self, language: str = "csharp", max_results: int = 50):
        self.language = language
        self.max_results = max_results
        self.patterns: List[CodePattern] = []

    def add_pattern(self, name: str, pattern: str, description: str):
        """Add a code pattern to search for"""
        self.patterns.append(CodePattern(
            name=name,
            pattern=pattern,
            description=description,
            language=self.language
        ))

    def search_github_code(self, query: str, language: Optional[str] = None) -> List[SearchResult]:
        """Search GitHub for code matching the query"""
        results = []
        lang = language or self.language

        # Build GitHub CLI search command
        cmd = [
            "gh", "search", "code",
            query,
            "--limit", str(self.max_results),
            "--json", "path,repository,url"
        ]

        if lang:
            cmd.extend(["--language", lang])

        try:
            result = subprocess.run(cmd, capture_output=True, text=True, check=True)
            data = json.loads(result.stdout)

            for item in data:
                repo_name = item.get("repository", {}).get("nameWithOwner", "unknown")
                file_path = item.get("path", "")
                url = item.get("url", "")

                results.append(SearchResult(
                    repo_name=repo_name,
                    file_path=file_path,
                    matches=[query],
                    url=url
                ))
        except subprocess.CalledProcessError as e:
            print(f"Error searching GitHub: {e.stderr}", file=sys.stderr)
        except json.JSONDecodeError as e:
            print(f"Error parsing search results: {e}", file=sys.stderr)

        return results

    def extract_common_patterns(self, org: str) -> List[CodePattern]:
        """Extract common patterns from an organization's repositories"""
        patterns = []

        # Common LinksPlatform patterns based on documentation
        common_patterns = [
            {
                "name": "Range struct usage",
                "pattern": "struct Range",
                "description": "Projects using Range struct with Minimum and Maximum"
            },
            {
                "name": "ILinks interface",
                "pattern": "interface ILinks",
                "description": "Projects implementing ILinks interface pattern"
            },
            {
                "name": "Doublets pattern",
                "pattern": "class.*Doublets.*ILinks",
                "description": "Projects using Doublets data structure"
            },
            {
                "name": "Memory management",
                "pattern": "IMemory.*Pointer",
                "description": "Projects with custom memory management"
            },
            {
                "name": "Synchronization wrapper",
                "pattern": "class Synchronized",
                "description": "Projects using synchronization wrappers"
            }
        ]

        for p in common_patterns:
            patterns.append(CodePattern(
                name=p["name"],
                pattern=p["pattern"],
                description=p["description"],
                language=self.language
            ))

        return patterns

    def find_friend_projects(self, search_terms: List[str]) -> Dict[str, List[SearchResult]]:
        """Find friend projects for given search terms"""
        results_by_term = defaultdict(list)

        for term in search_terms:
            print(f"Searching for: {term}", file=sys.stderr)
            results = self.search_github_code(term)
            results_by_term[term].extend(results)

        return dict(results_by_term)

    def analyze_organization(self, org: str) -> Dict[str, List[SearchResult]]:
        """Analyze an organization's common patterns and find friend projects"""
        print(f"Analyzing organization: {org}", file=sys.stderr)

        # Extract patterns from the organization
        patterns = self.extract_common_patterns(org)

        # Search for each pattern
        results_by_pattern = defaultdict(list)

        for pattern in patterns:
            print(f"  Searching for pattern: {pattern.name}", file=sys.stderr)
            results = self.search_github_code(pattern.pattern)

            # Filter out results from the same organization
            filtered_results = [
                r for r in results
                if not r.repo_name.lower().startswith(f"{org.lower()}/")
            ]

            if filtered_results:
                results_by_pattern[pattern.name].extend(filtered_results)

        return dict(results_by_pattern)

    def format_report(self, results: Dict[str, List[SearchResult]]) -> str:
        """Format results as a readable report"""
        report = []
        report.append("=" * 80)
        report.append("Friend Projects Finder - Report")
        report.append("=" * 80)
        report.append("")

        # Group by repository
        repos_by_pattern = defaultdict(lambda: defaultdict(list))

        for pattern, search_results in results.items():
            for result in search_results:
                repos_by_pattern[result.repo_name][pattern].append(result)

        # Sort repositories by number of matches
        sorted_repos = sorted(
            repos_by_pattern.items(),
            key=lambda x: sum(len(v) for v in x[1].values()),
            reverse=True
        )

        report.append(f"Found {len(sorted_repos)} potential friend projects\n")

        for repo_name, patterns in sorted_repos[:20]:  # Top 20 results
            total_matches = sum(len(v) for v in patterns.values())
            report.append(f"\n## {repo_name}")
            report.append(f"   Total matches: {total_matches}")
            report.append("")

            for pattern, matches in patterns.items():
                report.append(f"   - {pattern}: {len(matches)} match(es)")
                for match in matches[:3]:  # Show first 3 files
                    report.append(f"     * {match.file_path}")
                    report.append(f"       {match.url}")

        report.append("\n" + "=" * 80)
        report.append("End of Report")
        report.append("=" * 80)

        return "\n".join(report)


def main():
    parser = argparse.ArgumentParser(
        description="Find GitHub projects with similar code patterns"
    )
    parser.add_argument(
        "--language",
        default="csharp",
        help="Programming language to search (default: csharp)"
    )
    parser.add_argument(
        "--search-terms",
        help="Comma-separated list of terms to search for"
    )
    parser.add_argument(
        "--org",
        help="Organization to analyze (e.g., linksplatform)"
    )
    parser.add_argument(
        "--analyze",
        action="store_true",
        help="Analyze organization and find friend projects"
    )
    parser.add_argument(
        "--max-results",
        type=int,
        default=50,
        help="Maximum results per search (default: 50)"
    )
    parser.add_argument(
        "--output",
        help="Output file for report (default: stdout)"
    )

    args = parser.parse_args()

    finder = FriendProjectsFinder(
        language=args.language,
        max_results=args.max_results
    )

    if args.analyze and args.org:
        # Analyze organization mode
        results = finder.analyze_organization(args.org)
    elif args.search_terms:
        # Search terms mode
        terms = [t.strip() for t in args.search_terms.split(",")]
        results = finder.find_friend_projects(terms)
    else:
        parser.print_help()
        return 1

    # Generate report
    report = finder.format_report(results)

    if args.output:
        with open(args.output, "w") as f:
            f.write(report)
        print(f"Report saved to: {args.output}", file=sys.stderr)
    else:
        print(report)

    return 0


if __name__ == "__main__":
    sys.exit(main())
