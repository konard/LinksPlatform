#!/usr/bin/env python3
"""
Code Preference Extractor

This tool analyzes published code from a developer to extract their personal coding preferences.
This can be useful for adapting other developer's code to reader's personal preferences,
making the code look similar to the reader-developer's own code and improving reading efficiency.

Features:
- Extracts indentation style (spaces vs tabs, indentation size)
- Detects naming conventions (camelCase, PascalCase, snake_case, etc.)
- Analyzes brace placement style
- Detects line length preferences
- Identifies comment style patterns
- Extracts using/import organization patterns
- Analyzes blank line usage patterns
- Detects string quote preferences

Usage:
    python3 code_preference_extractor.py <path_to_code_directory> [--output <output_file>]
    python3 code_preference_extractor.py --help
"""

import os
import re
import json
import argparse
from collections import Counter, defaultdict
from pathlib import Path
from typing import Dict, List, Any, Optional


class CodePreferenceExtractor:
    """Extract coding style preferences from source code files."""

    def __init__(self):
        self.stats = {
            'indentation': defaultdict(int),
            'naming_conventions': {
                'classes': defaultdict(int),
                'methods': defaultdict(int),
                'variables': defaultdict(int),
                'constants': defaultdict(int),
            },
            'brace_style': defaultdict(int),
            'line_lengths': [],
            'comment_styles': defaultdict(int),
            'using_patterns': defaultdict(int),
            'blank_lines': defaultdict(int),
            'string_quotes': defaultdict(int),
            'files_analyzed': 0,
            'total_lines': 0,
        }

    def analyze_directory(self, directory: str, extensions: Optional[List[str]] = None) -> Dict[str, Any]:
        """Analyze all code files in a directory."""
        if extensions is None:
            extensions = ['.cs', '.py', '.java', '.js', '.ts', '.cpp', '.c', '.h', '.hpp']

        directory_path = Path(directory)
        if not directory_path.exists():
            raise ValueError(f"Directory does not exist: {directory}")

        for root, _, files in os.walk(directory):
            for file in files:
                if any(file.endswith(ext) for ext in extensions):
                    file_path = os.path.join(root, file)
                    try:
                        self.analyze_file(file_path)
                    except Exception as e:
                        print(f"Warning: Could not analyze {file_path}: {e}")

        return self.get_preferences()

    def analyze_file(self, file_path: str) -> None:
        """Analyze a single code file."""
        try:
            with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read()
                lines = content.split('\n')
        except Exception as e:
            raise Exception(f"Failed to read file: {e}")

        self.stats['files_analyzed'] += 1
        self.stats['total_lines'] += len(lines)

        # Analyze each aspect
        self._analyze_indentation(lines)
        self._analyze_naming_conventions(content)
        self._analyze_brace_style(lines)
        self._analyze_line_lengths(lines)
        self._analyze_comment_styles(lines)
        self._analyze_using_patterns(lines)
        self._analyze_blank_lines(lines)
        self._analyze_string_quotes(content)

    def _analyze_indentation(self, lines: List[str]) -> None:
        """Detect indentation style and size."""
        for line in lines:
            if line and line[0] in (' ', '\t'):
                # Count leading whitespace
                indent = len(line) - len(line.lstrip())
                if line[0] == '\t':
                    self.stats['indentation']['tabs'] += 1
                elif line[0] == ' ':
                    # Detect common indentation sizes (2, 4, 8 spaces)
                    if indent % 8 == 0:
                        self.stats['indentation']['spaces_8'] += 1
                    elif indent % 4 == 0:
                        self.stats['indentation']['spaces_4'] += 1
                    elif indent % 2 == 0:
                        self.stats['indentation']['spaces_2'] += 1
                    else:
                        self.stats['indentation']['spaces_other'] += 1

    def _analyze_naming_conventions(self, content: str) -> None:
        """Detect naming convention patterns."""
        # Class names (PascalCase, camelCase, snake_case)
        class_pattern = r'\bclass\s+([A-Za-z_][A-Za-z0-9_]*)'
        for match in re.finditer(class_pattern, content):
            name = match.group(1)
            self.stats['naming_conventions']['classes'][self._get_naming_style(name)] += 1

        # Method/Function names
        method_pattern = r'\b(?:public|private|protected|static|void|async|internal)?\s*(?:[A-Za-z_][A-Za-z0-9_<>,\[\]]*\s+)?([A-Za-z_][A-Za-z0-9_]*)\s*\('
        for match in re.finditer(method_pattern, content):
            name = match.group(1)
            if name not in ['if', 'while', 'for', 'switch', 'using', 'catch']:
                self.stats['naming_conventions']['methods'][self._get_naming_style(name)] += 1

        # Variable names (approximation)
        variable_pattern = r'\b(?:var|int|long|string|bool|double|float|byte|uint|ulong|const)\s+([A-Za-z_][A-Za-z0-9_]*)'
        for match in re.finditer(variable_pattern, content):
            name = match.group(1)
            self.stats['naming_conventions']['variables'][self._get_naming_style(name)] += 1

    def _get_naming_style(self, name: str) -> str:
        """Determine the naming style of an identifier."""
        if name.isupper():
            return 'SCREAMING_SNAKE_CASE' if '_' in name else 'UPPERCASE'
        elif '_' in name:
            return 'snake_case'
        elif name[0].isupper():
            return 'PascalCase'
        elif name[0].islower() and any(c.isupper() for c in name[1:]):
            return 'camelCase'
        else:
            return 'lowercase'

    def _analyze_brace_style(self, lines: List[str]) -> None:
        """Detect brace placement style (K&R, Allman, etc.)."""
        for i, line in enumerate(lines):
            stripped = line.strip()
            if stripped.endswith('{'):
                # Check if opening brace is on same line as statement
                if stripped != '{':
                    self.stats['brace_style']['same_line'] += 1
                else:
                    self.stats['brace_style']['next_line'] += 1

    def _analyze_line_lengths(self, lines: List[str]) -> None:
        """Analyze typical line lengths."""
        for line in lines:
            # Ignore blank lines and very short lines
            if len(line.strip()) > 10:
                self.stats['line_lengths'].append(len(line.rstrip()))

    def _analyze_comment_styles(self, lines: List[str]) -> None:
        """Detect comment style preferences."""
        for line in lines:
            stripped = line.strip()
            if stripped.startswith('//'):
                self.stats['comment_styles']['single_line_slashes'] += 1
            elif stripped.startswith('/*'):
                self.stats['comment_styles']['multi_line_start'] += 1
            elif stripped.startswith('///'):
                self.stats['comment_styles']['documentation_slashes'] += 1
            elif stripped.startswith('#'):
                self.stats['comment_styles']['hash'] += 1

    def _analyze_using_patterns(self, lines: List[str]) -> None:
        """Analyze using/import statement organization."""
        using_block = []
        in_using_block = False

        for line in lines:
            stripped = line.strip()
            if stripped.startswith('using ') or stripped.startswith('import '):
                using_block.append(stripped)
                in_using_block = True
            elif in_using_block and stripped == '':
                # End of using block
                if len(using_block) > 1:
                    # Check if sorted
                    if using_block == sorted(using_block):
                        self.stats['using_patterns']['sorted'] += 1
                    else:
                        self.stats['using_patterns']['unsorted'] += 1
                using_block = []
                in_using_block = False
            elif in_using_block and not (stripped.startswith('using ') or stripped.startswith('import ')):
                in_using_block = False
                using_block = []

    def _analyze_blank_lines(self, lines: List[str]) -> None:
        """Analyze blank line usage patterns."""
        blank_count = 0
        for line in lines:
            if line.strip() == '':
                blank_count += 1
            else:
                if blank_count > 0:
                    self.stats['blank_lines'][f'{blank_count}_lines'] += 1
                blank_count = 0

    def _analyze_string_quotes(self, content: str) -> None:
        """Detect string quote preferences."""
        single_quotes = len(re.findall(r"'[^']*'", content))
        double_quotes = len(re.findall(r'"[^"]*"', content))

        self.stats['string_quotes']['single'] = single_quotes
        self.stats['string_quotes']['double'] = double_quotes

    def get_preferences(self) -> Dict[str, Any]:
        """Compile and return the extracted preferences."""
        preferences = {
            'summary': {
                'files_analyzed': self.stats['files_analyzed'],
                'total_lines': self.stats['total_lines'],
            },
            'indentation': self._get_indentation_preference(),
            'naming_conventions': self._get_naming_preferences(),
            'brace_style': self._get_brace_preference(),
            'line_length': self._get_line_length_preference(),
            'comment_style': self._get_comment_preference(),
            'using_organization': self._get_using_preference(),
            'blank_lines': self._get_blank_line_preference(),
            'string_quotes': self._get_quote_preference(),
        }
        return preferences

    def _get_indentation_preference(self) -> Dict[str, Any]:
        """Get the preferred indentation style."""
        if not self.stats['indentation']:
            return {'style': 'unknown', 'confidence': 0}

        total = sum(self.stats['indentation'].values())
        if self.stats['indentation']['tabs'] > total / 2:
            return {
                'style': 'tabs',
                'confidence': round(self.stats['indentation']['tabs'] / total * 100, 2)
            }
        else:
            # Find most common space indentation
            space_counts = {k: v for k, v in self.stats['indentation'].items() if k.startswith('spaces_')}
            if space_counts:
                most_common = max(space_counts.items(), key=lambda x: x[1])
                size = most_common[0].replace('spaces_', '')
                return {
                    'style': f'spaces_{size}',
                    'size': int(size) if size.isdigit() else 'variable',
                    'confidence': round(most_common[1] / total * 100, 2)
                }
        return {'style': 'mixed', 'confidence': 0}

    def _get_naming_preferences(self) -> Dict[str, Any]:
        """Get the preferred naming conventions."""
        result = {}
        for category, counts in self.stats['naming_conventions'].items():
            if counts:
                most_common = max(counts.items(), key=lambda x: x[1])
                total = sum(counts.values())
                result[category] = {
                    'preferred_style': most_common[0],
                    'confidence': round(most_common[1] / total * 100, 2),
                    'distribution': dict(counts)
                }
        return result

    def _get_brace_preference(self) -> Dict[str, Any]:
        """Get the preferred brace placement style."""
        if not self.stats['brace_style']:
            return {'style': 'unknown', 'confidence': 0}

        total = sum(self.stats['brace_style'].values())
        same_line = self.stats['brace_style']['same_line']
        return {
            'style': 'same_line' if same_line > total / 2 else 'next_line',
            'confidence': round(max(same_line, total - same_line) / total * 100, 2)
        }

    def _get_line_length_preference(self) -> Dict[str, Any]:
        """Get typical line length preferences."""
        if not self.stats['line_lengths']:
            return {'average': 0, 'median': 0, 'max': 0}

        lengths = sorted(self.stats['line_lengths'])
        return {
            'average': round(sum(lengths) / len(lengths), 2),
            'median': lengths[len(lengths) // 2],
            'p95': lengths[int(len(lengths) * 0.95)] if lengths else 0,
            'max': max(lengths),
        }

    def _get_comment_preference(self) -> Dict[str, Any]:
        """Get preferred comment style."""
        if not self.stats['comment_styles']:
            return {'style': 'unknown', 'confidence': 0}

        most_common = max(self.stats['comment_styles'].items(), key=lambda x: x[1])
        total = sum(self.stats['comment_styles'].values())
        return {
            'preferred_style': most_common[0],
            'confidence': round(most_common[1] / total * 100, 2),
            'distribution': dict(self.stats['comment_styles'])
        }

    def _get_using_preference(self) -> Dict[str, Any]:
        """Get using/import organization preference."""
        total = sum(self.stats['using_patterns'].values())
        if total == 0:
            return {'organized': 'unknown', 'confidence': 0}

        sorted_count = self.stats['using_patterns']['sorted']
        return {
            'organized': 'sorted' if sorted_count > total / 2 else 'unsorted',
            'confidence': round(max(sorted_count, total - sorted_count) / total * 100, 2)
        }

    def _get_blank_line_preference(self) -> Dict[str, Any]:
        """Get blank line usage preference."""
        if not self.stats['blank_lines']:
            return {'typical': 'unknown'}

        most_common = max(self.stats['blank_lines'].items(), key=lambda x: x[1])
        return {
            'typical': most_common[0],
            'distribution': dict(sorted(self.stats['blank_lines'].items()))
        }

    def _get_quote_preference(self) -> Dict[str, Any]:
        """Get string quote preference."""
        single = self.stats['string_quotes']['single']
        double = self.stats['string_quotes']['double']
        total = single + double

        if total == 0:
            return {'style': 'unknown', 'confidence': 0}

        preferred = 'single' if single > double else 'double'
        confidence = round(max(single, double) / total * 100, 2)
        return {
            'preferred': preferred,
            'confidence': confidence
        }


def main():
    parser = argparse.ArgumentParser(
        description='Extract personal coding preferences from published code.',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__
    )
    parser.add_argument(
        'directory',
        help='Path to the directory containing code to analyze'
    )
    parser.add_argument(
        '-o', '--output',
        help='Output file path (JSON format). If not specified, prints to stdout.'
    )
    parser.add_argument(
        '-e', '--extensions',
        nargs='+',
        help='File extensions to analyze (e.g., .cs .py .java). Default: common extensions'
    )
    parser.add_argument(
        '--pretty',
        action='store_true',
        help='Pretty-print the JSON output'
    )

    args = parser.parse_args()

    print(f"Analyzing code in: {args.directory}")

    extractor = CodePreferenceExtractor()
    preferences = extractor.analyze_directory(args.directory, args.extensions)

    # Format output
    json_indent = 2 if args.pretty else None
    output_json = json.dumps(preferences, indent=json_indent)

    if args.output:
        with open(args.output, 'w') as f:
            f.write(output_json)
        print(f"\nPreferences extracted and saved to: {args.output}")
    else:
        print("\n" + "="*80)
        print("EXTRACTED CODE PREFERENCES")
        print("="*80)
        print(output_json)

    # Print summary
    print("\n" + "="*80)
    print("SUMMARY")
    print("="*80)
    print(f"Files analyzed: {preferences['summary']['files_analyzed']}")
    print(f"Total lines: {preferences['summary']['total_lines']}")
    print(f"\nIndentation: {preferences['indentation']['style']} "
          f"(confidence: {preferences['indentation'].get('confidence', 0)}%)")

    if 'classes' in preferences['naming_conventions']:
        print(f"Class naming: {preferences['naming_conventions']['classes']['preferred_style']} "
              f"(confidence: {preferences['naming_conventions']['classes']['confidence']}%)")

    print(f"Brace style: {preferences['brace_style']['style']} "
          f"(confidence: {preferences['brace_style']['confidence']}%)")
    print(f"Average line length: {preferences['line_length']['average']} characters")


if __name__ == '__main__':
    main()
