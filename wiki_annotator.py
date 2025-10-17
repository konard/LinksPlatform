#!/usr/bin/env python3
"""
Wiki Annotator - Tool for annotating markdown text with Wikipedia links.

This tool can be used to enhance README files and wiki pages by automatically
adding Wikipedia links to technical terms and concepts.
"""

import re
import argparse
import sys
from typing import Dict, List, Tuple, Set
from pathlib import Path


class WikiAnnotator:
    """Annotates markdown text with Wikipedia links."""

    def __init__(self, language: str = 'en', custom_terms: Dict[str, str] = None):
        """
        Initialize the annotator.

        Args:
            language: Language code ('en' or 'ru') for Wikipedia links
            custom_terms: Dictionary mapping terms to their Wikipedia page names
        """
        self.language = language
        self.wiki_base = self._get_wiki_base(language)
        self.custom_terms = custom_terms or {}
        self.default_terms = self._get_default_terms(language)

    def _get_wiki_base(self, language: str) -> str:
        """Get Wikipedia base URL for the specified language."""
        if language == 'ru':
            return 'https://ru.wikipedia.org/wiki/'
        return 'https://en.wikipedia.org/wiki/'

    def _get_default_terms(self, language: str) -> Dict[str, str]:
        """Get default term mappings for the specified language."""
        if language == 'ru':
            return {
                'модульный': 'Модульное_программирование',
                'фреймворк': 'Фреймворк',
                'СУБД': 'Система_управления_базами_данных',
                'реализация': 'Реализация',
                'библиотека': 'Библиотека_(программирование)',
                'язык программирования': 'Язык_программирования',
                'транслятор': 'Транслятор',
            }
        else:
            return {
                'framework': 'Software_framework',
                'database': 'Database',
                'DBMS': 'Database',
                'library': 'Library_(computing)',
                'programming language': 'Programming_language',
                'implementation': 'Implementation',
                'compiler': 'Compiler',
                'IDE': 'Integrated_development_environment',
                'text editor': 'Text_editor',
                'modular': 'Modular_programming',
            }

    def _extract_existing_links(self, text: str) -> Set[str]:
        """Extract all text that is already linked in markdown."""
        # Match both [text](url) and [text][ref] style links
        inline_links = re.findall(r'\[([^\]]+)\]\([^\)]+\)', text)
        ref_links = re.findall(r'\[([^\]]+)\]\[[^\]]*\]', text)
        return set(inline_links + ref_links)

    def _is_in_code_block(self, text: str, position: int) -> bool:
        """Check if a position in text is inside a code block."""
        # Count backticks before the position
        before = text[:position]
        # Check for triple backtick code blocks
        triple_backtick_count = before.count('```')
        if triple_backtick_count % 2 == 1:
            return True
        # Check for inline code
        line_start = before.rfind('\n') + 1
        line_before = before[line_start:]
        single_backtick_count = line_before.count('`')
        return single_backtick_count % 2 == 1

    def annotate(self, text: str, terms: Dict[str, str] = None) -> str:
        """
        Annotate markdown text with Wikipedia links.

        Args:
            text: The markdown text to annotate
            terms: Optional dictionary of terms to annotate (term -> wiki_page_name)
                   If not provided, uses default terms for the language

        Returns:
            Annotated markdown text
        """
        if terms is None:
            terms = {**self.default_terms, **self.custom_terms}
        else:
            terms = {**self.default_terms, **self.custom_terms, **terms}

        # Extract already linked text to avoid double-linking
        existing_links = self._extract_existing_links(text)

        # Sort terms by length (longest first) to match longer phrases first
        sorted_terms = sorted(terms.items(), key=lambda x: len(x[0]), reverse=True)

        result = text
        for term, wiki_page in sorted_terms:
            # Skip if term is already linked
            if term in existing_links:
                continue

            # Create a pattern that matches the term but not inside existing links or code
            # Use word boundaries for whole word matching
            pattern = r'\b' + re.escape(term) + r'\b'

            # Find all matches
            matches = list(re.finditer(pattern, result, re.IGNORECASE))

            # Process matches in reverse order to maintain positions
            for match in reversed(matches):
                start, end = match.span()

                # Skip if inside code block
                if self._is_in_code_block(result, start):
                    continue

                # Check if already part of a markdown link
                before = result[max(0, start-100):start]
                after = result[end:min(len(result), end+10)]

                # Skip if inside a link: [text or ](url) or [ref]
                if before.rstrip().endswith('[') or after.lstrip().startswith('](') or after.lstrip().startswith(']'):
                    continue

                # Get the matched text (preserves original case)
                matched_text = result[start:end]

                # Create the wiki link
                wiki_url = self.wiki_base + wiki_page
                annotated = f'[{matched_text}]({wiki_url})'

                # Replace only the first occurrence at this position
                result = result[:start] + annotated + result[end:]

        return result

    def annotate_file(self, input_path: Path, output_path: Path = None,
                      terms: Dict[str, str] = None) -> None:
        """
        Annotate a markdown file with Wikipedia links.

        Args:
            input_path: Path to input markdown file
            output_path: Path to output file (if None, overwrites input)
            terms: Optional dictionary of terms to annotate
        """
        with open(input_path, 'r', encoding='utf-8') as f:
            text = f.read()

        annotated = self.annotate(text, terms)

        output_path = output_path or input_path
        with open(output_path, 'w', encoding='utf-8') as f:
            f.write(annotated)


def main():
    """Command-line interface for the wiki annotator."""
    parser = argparse.ArgumentParser(
        description='Annotate markdown text with Wikipedia links',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # Annotate a file with English Wikipedia links
  %(prog)s input.md -o output.md

  # Annotate with Russian Wikipedia links
  %(prog)s input.ru.md -o output.ru.md -l ru

  # Add custom terms
  %(prog)s input.md -o output.md -t "Docker:Docker_(software)" -t "API:API"

  # Read from stdin, write to stdout
  echo "This is a framework" | %(prog)s -
        """
    )

    parser.add_argument('input', help='Input markdown file (use "-" for stdin)')
    parser.add_argument('-o', '--output', help='Output file (default: overwrite input, use "-" for stdout)')
    parser.add_argument('-l', '--language', default='en', choices=['en', 'ru'],
                        help='Wikipedia language (default: en)')
    parser.add_argument('-t', '--term', action='append', metavar='TERM:PAGE',
                        help='Add custom term mapping (format: "term:wiki_page_name")')
    parser.add_argument('--dry-run', action='store_true',
                        help='Print annotated text without writing to file')

    args = parser.parse_args()

    # Parse custom terms
    custom_terms = {}
    if args.term:
        for term_mapping in args.term:
            if ':' not in term_mapping:
                print(f"Error: Invalid term mapping '{term_mapping}'. Use format 'term:wiki_page_name'",
                      file=sys.stderr)
                sys.exit(1)
            term, page = term_mapping.split(':', 1)
            custom_terms[term] = page

    # Create annotator
    annotator = WikiAnnotator(language=args.language, custom_terms=custom_terms)

    # Read input
    if args.input == '-':
        text = sys.stdin.read()
    else:
        input_path = Path(args.input)
        if not input_path.exists():
            print(f"Error: Input file '{args.input}' not found", file=sys.stderr)
            sys.exit(1)
        with open(input_path, 'r', encoding='utf-8') as f:
            text = f.read()

    # Annotate
    annotated = annotator.annotate(text)

    # Write output
    if args.dry_run or args.output == '-':
        print(annotated)
    elif args.output:
        output_path = Path(args.output)
        with open(output_path, 'w', encoding='utf-8') as f:
            f.write(annotated)
        print(f"Annotated text written to {args.output}")
    else:
        # Overwrite input
        if args.input == '-':
            print(annotated)
        else:
            with open(input_path, 'w', encoding='utf-8') as f:
                f.write(annotated)
            print(f"File {args.input} has been annotated")


if __name__ == '__main__':
    main()
