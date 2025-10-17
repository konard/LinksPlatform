#!/usr/bin/env python3
"""
Basic example of using the WikiAnnotator class.
"""

from pathlib import Path
import sys
sys.path.insert(0, str(Path(__file__).parent.parent))

from wiki_annotator import WikiAnnotator


def main():
    # Example 1: Basic usage with English Wikipedia
    print("Example 1: Basic English annotation")
    print("-" * 50)

    text = """
# Software Development

This project uses a modern framework and multiple libraries.
The database is a high-performance DBMS with modular architecture.
    """

    annotator = WikiAnnotator(language='en')
    annotated = annotator.annotate(text)
    print(annotated)
    print()

    # Example 2: Russian Wikipedia
    print("Example 2: Russian annotation")
    print("-" * 50)

    text_ru = """
# Разработка ПО

Это модульный фреймворк с поддержкой СУБД.
Включает библиотеку и транслятор кода.
    """

    annotator_ru = WikiAnnotator(language='ru')
    annotated_ru = annotator_ru.annotate(text_ru)
    print(annotated_ru)
    print()

    # Example 3: Custom terms
    print("Example 3: Custom terms")
    print("-" * 50)

    text_custom = """
# Cloud Infrastructure

Deploy using Docker containers and Kubernetes orchestration.
Connect services via REST API endpoints.
    """

    custom_terms = {
        'Docker': 'Docker_(software)',
        'Kubernetes': 'Kubernetes',
        'REST API': 'Representational_state_transfer',
    }

    annotator_custom = WikiAnnotator(language='en', custom_terms=custom_terms)
    annotated_custom = annotator_custom.annotate(text_custom)
    print(annotated_custom)
    print()

    # Example 4: Preserve existing links
    print("Example 4: Preserve existing links")
    print("-" * 50)

    text_links = """
# Documentation

This [framework](https://myframework.com) is a powerful library.
The framework supports multiple databases.
    """

    annotator_links = WikiAnnotator(language='en')
    annotated_links = annotator_links.annotate(text_links)
    print(annotated_links)
    print("Note: First 'framework' link is preserved, second one gets annotated")


if __name__ == '__main__':
    main()
