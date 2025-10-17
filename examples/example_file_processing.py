#!/usr/bin/env python3
"""
Example of processing markdown files with WikiAnnotator.
"""

from pathlib import Path
import sys
sys.path.insert(0, str(Path(__file__).parent.parent))

from wiki_annotator import WikiAnnotator


def main():
    # Create a sample markdown file
    sample_path = Path(__file__).parent / 'sample_readme.md'

    sample_content = """# My Project

A modular framework for building scalable applications.

## Features

- Built with modern framework architecture
- Integrated database and DBMS support
- Compatible with any programming language
- Works with popular IDE and text editor
- Extensive library collection

## Architecture

This project uses a modular design pattern. The framework core
provides the foundation, while libraries add functionality.

## Database

The DBMS implementation supports various database backends.
"""

    # Write sample file
    with open(sample_path, 'w', encoding='utf-8') as f:
        f.write(sample_content)

    print("Original file:")
    print("=" * 60)
    print(sample_content)

    # Annotate the file
    annotator = WikiAnnotator(language='en')
    annotated_path = Path(__file__).parent / 'sample_readme_annotated.md'
    annotator.annotate_file(sample_path, annotated_path)

    # Read and display annotated content
    with open(annotated_path, 'r', encoding='utf-8') as f:
        annotated_content = f.read()

    print("\nAnnotated file:")
    print("=" * 60)
    print(annotated_content)

    print(f"\nFiles created:")
    print(f"  Original: {sample_path}")
    print(f"  Annotated: {annotated_path}")


if __name__ == '__main__':
    main()
