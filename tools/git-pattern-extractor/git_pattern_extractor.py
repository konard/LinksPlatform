#!/usr/bin/env python3
"""
Git Pattern Extractor - Extract, merge, split, and replay change patterns from Git history.

This tool analyzes Git commit history to identify patterns of changes that can be
reproduced in other similar repositories.
"""

import argparse
import json
import os
import re
import subprocess
import sys
from collections import defaultdict
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Any, Optional, Set, Tuple


class GitPatternExtractor:
    """Main class for extracting patterns from Git history."""

    def __init__(self, repo_path: str = "."):
        """Initialize the extractor with a repository path."""
        self.repo_path = Path(repo_path).resolve()
        if not (self.repo_path / ".git").exists():
            raise ValueError(f"Not a git repository: {self.repo_path}")

    def _run_git_command(self, args: List[str]) -> str:
        """Run a git command and return its output."""
        try:
            result = subprocess.run(
                ["git"] + args,
                cwd=self.repo_path,
                capture_output=True,
                text=True,
                check=True
            )
            return result.stdout.strip()
        except subprocess.CalledProcessError as e:
            print(f"Git command failed: {e.stderr}", file=sys.stderr)
            raise

    def extract_commit_pattern(self, commit_range: Optional[str] = None) -> Dict[str, Any]:
        """
        Extract change patterns from a range of commits.

        Args:
            commit_range: Git commit range (e.g., "HEAD~10..HEAD", "main..feature")
                         If None, analyzes all commits.

        Returns:
            Dictionary containing extracted patterns.
        """
        # Get commit list
        range_arg = commit_range if commit_range else "--all"
        log_format = "%H|%an|%ae|%at|%s"
        log_output = self._run_git_command([
            "log",
            range_arg,
            f"--format={log_format}",
            "--no-merges"
        ])

        if not log_output:
            return {"commits": [], "patterns": {}}

        commits = []
        for line in log_output.split("\n"):
            if not line.strip():
                continue
            parts = line.split("|", 4)
            if len(parts) != 5:
                continue

            commit_hash, author_name, author_email, timestamp, subject = parts

            # Get file changes for this commit
            diff_output = self._run_git_command([
                "show",
                "--stat",
                "--format=",
                commit_hash
            ])

            files_changed = []
            for diff_line in diff_output.split("\n"):
                if "|" in diff_line:
                    file_path = diff_line.split("|")[0].strip()
                    if file_path:
                        files_changed.append(file_path)

            # Get detailed diff
            diff_detail = self._run_git_command([
                "show",
                "--format=",
                commit_hash
            ])

            commits.append({
                "hash": commit_hash,
                "author": {"name": author_name, "email": author_email},
                "timestamp": int(timestamp),
                "date": datetime.fromtimestamp(int(timestamp)).isoformat(),
                "subject": subject,
                "files_changed": files_changed,
                "diff": diff_detail
            })

        # Analyze patterns
        patterns = self._analyze_patterns(commits)

        return {
            "repository": str(self.repo_path),
            "extracted_at": datetime.now().isoformat(),
            "commit_range": commit_range or "all",
            "total_commits": len(commits),
            "commits": commits,
            "patterns": patterns
        }

    def _analyze_patterns(self, commits: List[Dict[str, Any]]) -> Dict[str, Any]:
        """Analyze commits to identify common patterns."""
        patterns = {
            "file_co_changes": defaultdict(int),
            "commit_message_patterns": defaultdict(int),
            "file_type_changes": defaultdict(int),
            "author_patterns": defaultdict(lambda: defaultdict(int)),
            "temporal_patterns": defaultdict(list)
        }

        # Analyze file co-changes
        for commit in commits:
            files = commit["files_changed"]
            if len(files) > 1:
                for i, file1 in enumerate(files):
                    for file2 in files[i+1:]:
                        pair = tuple(sorted([file1, file2]))
                        patterns["file_co_changes"][f"{pair[0]} <-> {pair[1]}"] += 1

        # Analyze commit message patterns
        for commit in commits:
            subject = commit["subject"]
            # Extract conventional commit patterns
            match = re.match(r"^(\w+)(?:\(([^)]+)\))?: (.+)$", subject)
            if match:
                commit_type, scope, _ = match.groups()
                patterns["commit_message_patterns"][commit_type] += 1

            # Extract common keywords
            keywords = re.findall(r"\b(?:add|update|fix|remove|refactor|docs|test|feat|chore)\b",
                                 subject.lower())
            for keyword in keywords:
                patterns["commit_message_patterns"][keyword] += 1

        # Analyze file type changes
        for commit in commits:
            for file_path in commit["files_changed"]:
                ext = Path(file_path).suffix
                if ext:
                    patterns["file_type_changes"][ext] += 1

        # Analyze author patterns
        for commit in commits:
            author = commit["author"]["email"]
            for file_path in commit["files_changed"]:
                patterns["author_patterns"][author][file_path] += 1

        # Convert defaultdicts to regular dicts for JSON serialization
        return {
            "file_co_changes": dict(sorted(
                patterns["file_co_changes"].items(),
                key=lambda x: x[1],
                reverse=True
            )[:20]),  # Top 20
            "commit_message_patterns": dict(patterns["commit_message_patterns"]),
            "file_type_changes": dict(patterns["file_type_changes"]),
            "author_patterns": {k: dict(v) for k, v in patterns["author_patterns"].items()},
        }

    def save_pattern(self, pattern: Dict[str, Any], output_path: str):
        """Save extracted pattern to a JSON file."""
        output_file = Path(output_path)
        output_file.parent.mkdir(parents=True, exist_ok=True)

        with open(output_file, "w") as f:
            json.dump(pattern, f, indent=2)

        print(f"Pattern saved to: {output_file}")

    @staticmethod
    def load_pattern(pattern_path: str) -> Dict[str, Any]:
        """Load a pattern from a JSON file."""
        with open(pattern_path, "r") as f:
            return json.load(f)

    @staticmethod
    def merge_patterns(pattern_paths: List[str], output_path: str):
        """
        Merge multiple patterns into a single pattern.

        Args:
            pattern_paths: List of paths to pattern JSON files
            output_path: Path to save the merged pattern
        """
        merged = {
            "merged_at": datetime.now().isoformat(),
            "source_patterns": pattern_paths,
            "commits": [],
            "patterns": {
                "file_co_changes": defaultdict(int),
                "commit_message_patterns": defaultdict(int),
                "file_type_changes": defaultdict(int),
                "author_patterns": defaultdict(lambda: defaultdict(int))
            }
        }

        for pattern_path in pattern_paths:
            pattern = GitPatternExtractor.load_pattern(pattern_path)

            # Merge commits
            merged["commits"].extend(pattern.get("commits", []))

            # Merge patterns
            for key in ["file_co_changes", "commit_message_patterns", "file_type_changes"]:
                if key in pattern.get("patterns", {}):
                    for item, count in pattern["patterns"][key].items():
                        merged["patterns"][key][item] += count

            # Merge author patterns
            if "author_patterns" in pattern.get("patterns", {}):
                for author, files in pattern["patterns"]["author_patterns"].items():
                    for file_path, count in files.items():
                        merged["patterns"]["author_patterns"][author][file_path] += count

        # Convert defaultdicts to regular dicts
        merged["patterns"] = {
            "file_co_changes": dict(merged["patterns"]["file_co_changes"]),
            "commit_message_patterns": dict(merged["patterns"]["commit_message_patterns"]),
            "file_type_changes": dict(merged["patterns"]["file_type_changes"]),
            "author_patterns": {k: dict(v) for k, v in merged["patterns"]["author_patterns"].items()}
        }

        # Save merged pattern
        with open(output_path, "w") as f:
            json.dump(merged, f, indent=2)

        print(f"Merged pattern saved to: {output_path}")

    @staticmethod
    def split_pattern(pattern_path: str, output_dir: str, split_by: str = "author"):
        """
        Split a pattern into multiple patterns based on criteria.

        Args:
            pattern_path: Path to the pattern JSON file
            output_dir: Directory to save split patterns
            split_by: Criteria to split by ("author", "file_type", "commit_type")
        """
        pattern = GitPatternExtractor.load_pattern(pattern_path)
        output_path = Path(output_dir)
        output_path.mkdir(parents=True, exist_ok=True)

        splits = defaultdict(lambda: {
            "commits": [],
            "patterns": {
                "file_co_changes": defaultdict(int),
                "commit_message_patterns": defaultdict(int),
                "file_type_changes": defaultdict(int),
            }
        })

        if split_by == "author":
            for commit in pattern.get("commits", []):
                author = commit["author"]["email"]
                splits[author]["commits"].append(commit)

        elif split_by == "file_type":
            for commit in pattern.get("commits", []):
                for file_path in commit["files_changed"]:
                    ext = Path(file_path).suffix or "no_extension"
                    splits[ext]["commits"].append(commit)

        elif split_by == "commit_type":
            for commit in pattern.get("commits", []):
                subject = commit["subject"]
                match = re.match(r"^(\w+)(?:\(([^)]+)\))?: (.+)$", subject)
                if match:
                    commit_type = match.group(1)
                else:
                    commit_type = "other"
                splits[commit_type]["commits"].append(commit)

        # Save split patterns
        for key, split_pattern in splits.items():
            safe_key = re.sub(r'[^\w\-.]', '_', key)
            output_file = output_path / f"pattern_{split_by}_{safe_key}.json"

            # Convert defaultdicts to regular dicts
            split_pattern["patterns"] = {
                k: dict(v) for k, v in split_pattern["patterns"].items()
            }
            split_pattern["split_by"] = split_by
            split_pattern["split_key"] = key
            split_pattern["split_at"] = datetime.now().isoformat()

            with open(output_file, "w") as f:
                json.dump(split_pattern, f, indent=2)

            print(f"Split pattern saved to: {output_file}")

    def replay_pattern(self, pattern_path: str, target_repo: str, dry_run: bool = True):
        """
        Replay a pattern in a target repository.

        Args:
            pattern_path: Path to the pattern JSON file
            target_repo: Path to the target repository
            dry_run: If True, only show what would be done without making changes
        """
        pattern = self.load_pattern(pattern_path)
        target_path = Path(target_repo).resolve()

        if not (target_path / ".git").exists():
            raise ValueError(f"Not a git repository: {target_path}")

        print(f"{'[DRY RUN] ' if dry_run else ''}Replaying pattern in: {target_path}")
        print(f"Total commits in pattern: {len(pattern.get('commits', []))}")

        if dry_run:
            print("\n--- Pattern Summary ---")
            print(f"File co-changes: {len(pattern.get('patterns', {}).get('file_co_changes', {}))}")
            print(f"Commit message patterns: {pattern.get('patterns', {}).get('commit_message_patterns', {})}")
            print(f"File type changes: {pattern.get('patterns', {}).get('file_type_changes', {})}")
            print("\nUse --no-dry-run to apply changes")
        else:
            print("Note: Actual replay implementation would apply changes here.")
            print("This requires careful mapping of source files to target repository structure.")


def main():
    """Main entry point for the CLI."""
    parser = argparse.ArgumentParser(
        description="Extract, merge, split, and replay change patterns from Git history"
    )

    subparsers = parser.add_subparsers(dest="command", help="Command to execute")

    # Extract command
    extract_parser = subparsers.add_parser("extract", help="Extract patterns from Git history")
    extract_parser.add_argument(
        "--repo",
        default=".",
        help="Path to Git repository (default: current directory)"
    )
    extract_parser.add_argument(
        "--range",
        help="Commit range to analyze (e.g., 'HEAD~10..HEAD', 'main..feature')"
    )
    extract_parser.add_argument(
        "--output",
        required=True,
        help="Output file path for the extracted pattern"
    )

    # Merge command
    merge_parser = subparsers.add_parser("merge", help="Merge multiple patterns")
    merge_parser.add_argument(
        "patterns",
        nargs="+",
        help="Paths to pattern JSON files to merge"
    )
    merge_parser.add_argument(
        "--output",
        required=True,
        help="Output file path for the merged pattern"
    )

    # Split command
    split_parser = subparsers.add_parser("split", help="Split a pattern")
    split_parser.add_argument(
        "pattern",
        help="Path to pattern JSON file to split"
    )
    split_parser.add_argument(
        "--by",
        choices=["author", "file_type", "commit_type"],
        default="author",
        help="Criteria to split by (default: author)"
    )
    split_parser.add_argument(
        "--output-dir",
        required=True,
        help="Output directory for split patterns"
    )

    # Replay command
    replay_parser = subparsers.add_parser("replay", help="Replay a pattern in a target repository")
    replay_parser.add_argument(
        "pattern",
        help="Path to pattern JSON file to replay"
    )
    replay_parser.add_argument(
        "--target",
        required=True,
        help="Path to target Git repository"
    )
    replay_parser.add_argument(
        "--no-dry-run",
        action="store_true",
        help="Actually apply changes (default is dry-run)"
    )

    args = parser.parse_args()

    if not args.command:
        parser.print_help()
        return 1

    try:
        if args.command == "extract":
            extractor = GitPatternExtractor(args.repo)
            pattern = extractor.extract_commit_pattern(args.range)
            extractor.save_pattern(pattern, args.output)

        elif args.command == "merge":
            GitPatternExtractor.merge_patterns(args.patterns, args.output)

        elif args.command == "split":
            GitPatternExtractor.split_pattern(args.pattern, args.output_dir, args.by)

        elif args.command == "replay":
            extractor = GitPatternExtractor()
            extractor.replay_pattern(args.pattern, args.target, dry_run=not args.no_dry_run)

        return 0

    except Exception as e:
        print(f"Error: {e}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
