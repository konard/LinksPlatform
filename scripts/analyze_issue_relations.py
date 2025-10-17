#!/usr/bin/env python3
"""
Analyze and extract relations between GitHub issues.

This script scans through GitHub issues to identify:
1. Explicit dependencies (Related to, Fixes, etc.)
2. Referenced issues
3. Common patterns and groupings
"""

import json
import re
import sys
from collections import defaultdict
from typing import Dict, List, Set, Tuple


def extract_issue_references(text: str) -> Set[int]:
    """Extract all issue numbers referenced in text."""
    if not text:
        return set()

    # Pattern 1: https://github.com/Konard/LinksPlatform/issues/NNN
    url_pattern = r'https://github\.com/(?:Konard|linksplatform)/LinksPlatform/issues/(\d+)'
    # Pattern 2: #NNN
    hash_pattern = r'(?:^|[^\w])#(\d+)(?:[^\w]|$)'

    refs = set()
    refs.update(int(m) for m in re.findall(url_pattern, text))
    refs.update(int(m) for m in re.findall(hash_pattern, text))

    return refs


def analyze_dependency_type(text: str, issue_num: int) -> str:
    """Determine the type of dependency based on keywords."""
    if not text:
        return "unknown"

    text_lower = text.lower()

    # Create a mapping of dependency types to keywords
    dependency_patterns = {
        "required_by": [r"will require.*#?" + str(issue_num), r"requires.*#?" + str(issue_num)],
        "related_to": [r"related to.*#?" + str(issue_num)],
        "fixes": [r"fixes.*#?" + str(issue_num)],
        "based_on": [r"based on.*#?" + str(issue_num), r"can be based on.*#?" + str(issue_num)],
        "mentioned": []  # default if no specific pattern matches
    }

    for dep_type, patterns in dependency_patterns.items():
        for pattern in patterns:
            if re.search(pattern, text_lower):
                return dep_type

    return "mentioned"


def load_issues_data(filepath: str) -> List[Dict]:
    """Load issues from JSON file."""
    with open(filepath, 'r') as f:
        content = f.read().strip()
        try:
            # Try to load as a single JSON array first
            data = json.loads(content)
            if isinstance(data, list):
                return data
            else:
                return [data]
        except json.JSONDecodeError:
            # Fall back to line-by-line parsing (JSONL format)
            issues = []
            for line in content.split('\n'):
                line = line.strip()
                if line:
                    try:
                        issue = json.loads(line)
                        issues.append(issue)
                    except json.JSONDecodeError:
                        continue
            return issues


def analyze_relations(issues: List[Dict]) -> Dict:
    """Analyze all relations between issues."""
    relations = defaultdict(lambda: defaultdict(set))
    issue_refs_count = defaultdict(int)
    labels_map = defaultdict(list)

    for issue in issues:
        issue_num = issue.get('number')
        title = issue.get('title', '')
        body = issue.get('body', '')
        labels = issue.get('labels', [])

        # Track labels (handle both string and dict formats)
        for label in labels:
            if isinstance(label, dict):
                label_name = label.get('name', str(label))
            else:
                label_name = str(label)
            labels_map[label_name].append(issue_num)

        # Extract references
        refs = extract_issue_references(body)

        for ref in refs:
            dep_type = analyze_dependency_type(body, ref)
            relations[issue_num][ref] = dep_type
            issue_refs_count[ref] += 1

    return {
        'relations': dict(relations),
        'referenced_count': dict(issue_refs_count),
        'labels': dict(labels_map)
    }


def generate_markdown_report(issues: List[Dict], analysis: Dict) -> str:
    """Generate a markdown report of issue relations."""
    report = []

    report.append("# GitHub Issues Relations Analysis")
    report.append("")
    report.append("This document reveals the relationships and dependencies between issues in the LinksPlatform repository.")
    report.append("")

    # Section 1: Most Referenced Issues
    report.append("## Most Referenced Issues")
    report.append("")
    report.append("These issues are referenced by other issues and may be foundational:")
    report.append("")
    report.append("| Issue | References | Title |")
    report.append("|-------|------------|-------|")

    # Create issue number to title mapping
    issue_map = {i['number']: i['title'] for i in issues}

    sorted_refs = sorted(analysis['referenced_count'].items(), key=lambda x: x[1], reverse=True)
    for issue_num, count in sorted_refs[:20]:
        title = issue_map.get(issue_num, "Unknown")
        report.append(f"| #{issue_num} | {count} | {title} |")

    report.append("")

    # Section 2: Issues with Dependencies
    report.append("## Issues with Explicit Dependencies")
    report.append("")

    relations = analysis['relations']
    if relations:
        for issue_num in sorted(relations.keys(), reverse=True):
            deps = relations[issue_num]
            if deps:
                title = issue_map.get(issue_num, "Unknown")
                report.append(f"### #{issue_num}: {title}")
                report.append("")
                for ref, dep_type in sorted(deps.items()):
                    ref_title = issue_map.get(ref, "Unknown")
                    report.append(f"- **{dep_type.replace('_', ' ').title()}**: #{ref} - {ref_title}")
                report.append("")
    else:
        report.append("No explicit dependencies found in issue bodies.")
        report.append("")

    # Section 3: Issues Grouped by Labels
    report.append("## Issues Grouped by Labels")
    report.append("")

    labels = analysis['labels']
    if labels:
        for label, issue_nums in sorted(labels.items()):
            report.append(f"### {label}")
            report.append("")
            for issue_num in sorted(issue_nums, reverse=True):
                title = issue_map.get(issue_num, "Unknown")
                report.append(f"- #{issue_num}: {title}")
            report.append("")
    else:
        report.append("No labeled issues found.")
        report.append("")

    # Section 4: Dependency Graph (Mermaid)
    report.append("## Dependency Graph")
    report.append("")
    report.append("```mermaid")
    report.append("graph TD")

    # Add nodes and edges for issues with dependencies
    added_nodes = set()
    for issue_num in sorted(relations.keys(), reverse=True)[:30]:  # Limit to top 30 for readability
        deps = relations[issue_num]
        if deps:
            if issue_num not in added_nodes:
                title = issue_map.get(issue_num, "Unknown")[:40]  # Truncate for display
                report.append(f"    I{issue_num}[#{issue_num}: {title}]")
                added_nodes.add(issue_num)

            for ref in deps:
                if ref not in added_nodes:
                    ref_title = issue_map.get(ref, "Unknown")[:40]
                    report.append(f"    I{ref}[#{ref}: {ref_title}]")
                    added_nodes.add(ref)
                report.append(f"    I{issue_num} --> I{ref}")

    report.append("```")
    report.append("")

    # Section 5: Implementation Priority Suggestions
    report.append("## Suggested Implementation Priority")
    report.append("")
    report.append("Based on dependency analysis, these issues should be prioritized:")
    report.append("")

    # Issues that are referenced most = highest priority
    priority_issues = sorted_refs[:10]
    report.append("### High Priority (Foundational Issues)")
    report.append("")
    for issue_num, count in priority_issues:
        title = issue_map.get(issue_num, "Unknown")
        report.append(f"1. #{issue_num}: {title} (referenced {count} times)")
    report.append("")

    report.append("### Medium Priority (Dependent Issues)")
    report.append("")
    # Issues that reference others but aren't referenced much themselves
    for issue_num in sorted(relations.keys(), reverse=True)[:10]:
        deps = relations[issue_num]
        if deps and issue_num not in [i for i, _ in priority_issues]:
            title = issue_map.get(issue_num, "Unknown")
            report.append(f"- #{issue_num}: {title} (depends on {len(deps)} other issues)")
    report.append("")

    return "\n".join(report)


def main():
    if len(sys.argv) < 2:
        print("Usage: python analyze_issue_relations.py <issues_json_file>")
        sys.exit(1)

    issues_file = sys.argv[1]

    # Load and analyze issues
    issues = load_issues_data(issues_file)
    print(f"Loaded {len(issues)} issues")

    analysis = analyze_relations(issues)
    print(f"Found {len(analysis['relations'])} issues with dependencies")
    print(f"Found {len(analysis['referenced_count'])} referenced issues")

    # Generate report
    report = generate_markdown_report(issues, analysis)

    # Write to file
    output_file = "ISSUE_RELATIONS.md"
    with open(output_file, 'w') as f:
        f.write(report)

    print(f"\nReport saved to {output_file}")

    # Also print summary to stdout
    print("\n=== SUMMARY ===")
    print(f"Total issues analyzed: {len(issues)}")
    print(f"Issues with dependencies: {len(analysis['relations'])}")
    print(f"Most referenced issue: #{max(analysis['referenced_count'].items(), key=lambda x: x[1])[0]} ({max(analysis['referenced_count'].values())} references)")


if __name__ == "__main__":
    main()
