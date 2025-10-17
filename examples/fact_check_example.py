#!/usr/bin/env python3
"""
Example script demonstrating the Wikipedia Fact-Checking Bot

This script shows how to use the fact-checking bot to verify citations
in Wikipedia articles.
"""

import sys
import os

# Add parent directory to path to import fact_checker
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'fact-checking-bot'))

from fact_checker import WikipediaFactChecker


def example_basic_usage():
    """Example: Basic usage of the fact checker."""
    print("=" * 80)
    print("EXAMPLE 1: Basic Usage")
    print("=" * 80)
    print()

    # Create a fact checker instance
    checker = WikipediaFactChecker(verbose=True)

    # Check a Wikipedia article
    # Using a simple article with well-structured references
    article_title = "Python (programming language)"

    print(f"Checking article: {article_title}")
    print(f"Limiting to 3 citations for this example...")
    print()

    # Run fact check (limiting to 3 citations for quick demo)
    results = checker.check_article(article_title, max_citations=3)

    # Generate and print report
    report = checker.generate_report(results)
    print(report)


def example_custom_analysis():
    """Example: Custom analysis of specific citations."""
    print("\n\n")
    print("=" * 80)
    print("EXAMPLE 2: Custom Analysis")
    print("=" * 80)
    print()

    checker = WikipediaFactChecker(verbose=False)

    # Fetch an article
    article_title = "Artificial intelligence"
    print(f"Fetching article: {article_title}")

    wikitext = checker.fetch_article(article_title)

    if wikitext:
        print(f"Article fetched successfully ({len(wikitext)} characters)")

        # Extract citations and references
        citations = checker.extract_citations(wikitext)
        references = checker.extract_references(wikitext)

        print(f"Found {len(citations)} citations")
        print(f"Found {len(references)} reference definitions")
        print()

        # Show first few citations
        print("First 3 citations:")
        for i, citation in enumerate(citations[:3], 1):
            print(f"\n{i}. Text: {citation.text[:100]}...")
            print(f"   Reference: {citation.ref_name or 'inline'}")
            print(f"   Position: {citation.position}")

        # Show some reference names
        print(f"\nSample reference names: {list(references.keys())[:5]}")


def example_single_citation_check():
    """Example: Check a single citation in detail."""
    print("\n\n")
    print("=" * 80)
    print("EXAMPLE 3: Single Citation Analysis")
    print("=" * 80)
    print()

    from fact_checker import Citation, Reference

    checker = WikipediaFactChecker(verbose=True)

    # Create a sample citation (this would normally be extracted from an article)
    citation = Citation(
        text="Paris is the capital of France and its largest city",
        ref_name="france_ref",
        ref_content="",
        position=100
    )

    # Create a sample reference
    reference = Reference(
        name="france_ref",
        url="https://en.wikipedia.org/wiki/Paris",  # Using Wikipedia as reference for demo
        title="Paris - Wikipedia",
        content="Reference content"
    )

    print("Checking citation:")
    print(f"  Text: {citation.text}")
    print(f"  Reference URL: {reference.url}")
    print()

    # Verify the citation
    result = checker.verify_citation(citation, reference)

    print("Result:")
    print(f"  Status: {result.status}")
    print(f"  Similarity Score: {result.similarity_score:.2%}")
    print(f"  Details: {result.details}")


def example_similarity_testing():
    """Example: Test similarity calculation."""
    print("\n\n")
    print("=" * 80)
    print("EXAMPLE 4: Similarity Calculation")
    print("=" * 80)
    print()

    checker = WikipediaFactChecker()

    # Test different text pairs
    test_pairs = [
        (
            "The Earth orbits around the Sun",
            "The Sun is orbited by planet Earth"
        ),
        (
            "Python is a programming language",
            "Python is a type of snake"
        ),
        (
            "Machine learning is a subset of artificial intelligence",
            "AI includes machine learning as one of its subfields"
        ),
        (
            "The quick brown fox jumps over the lazy dog",
            "A fast orange cat leaps above the sleepy canine"
        ),
    ]

    for i, (text1, text2) in enumerate(test_pairs, 1):
        similarity = checker.calculate_similarity(text1, text2)
        print(f"Pair {i}:")
        print(f"  Text 1: {text1}")
        print(f"  Text 2: {text2}")
        print(f"  Similarity: {similarity:.2%}")
        print()


def main():
    """Run all examples."""
    print("Wikipedia Fact-Checking Bot - Examples")
    print()

    # Check if dependencies are installed
    try:
        import requests
        import bs4
        import mwparserfromhell
    except ImportError as e:
        print(f"Error: Missing dependency - {e}")
        print()
        print("Please install required dependencies:")
        print("  pip install -r ../fact-checking-bot/requirements.txt")
        return

    # Run examples
    try:
        # Example 1: Basic usage
        example_basic_usage()

        # Example 2: Custom analysis
        example_custom_analysis()

        # Example 3: Single citation check
        example_single_citation_check()

        # Example 4: Similarity testing
        example_similarity_testing()

        print("\n" + "=" * 80)
        print("All examples completed!")
        print("=" * 80)

    except KeyboardInterrupt:
        print("\n\nExamples interrupted by user.")
    except Exception as e:
        print(f"\nError running examples: {e}")
        import traceback
        traceback.print_exc()


if __name__ == '__main__':
    main()
