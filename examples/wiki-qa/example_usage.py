#!/usr/bin/env python3
"""
Example usage of the Wiki-QA dataset.

This script demonstrates how to load and use the wiki-qa dataset
for various purposes including AI training, search, and analysis.
"""

import json
from typing import List, Dict, Any
from pathlib import Path


class WikiQADataset:
    """Handler for Wiki-QA dataset operations."""

    def __init__(self, dataset_path: str = "qa-dataset.json"):
        """Load the dataset from JSON file."""
        self.dataset_path = Path(dataset_path)
        with open(self.dataset_path, 'r', encoding='utf-8') as f:
            self.data = json.load(f)

        self.metadata = self.data.get('metadata', {})
        self.categories = {cat['id']: cat for cat in self.data.get('categories', [])}
        self.qa_pairs = self.data.get('qa_pairs', [])

    def search_by_keyword(self, keyword: str) -> List[Dict[str, Any]]:
        """Search Q&A pairs by keyword in question or answer."""
        keyword_lower = keyword.lower()
        results = []

        for qa in self.qa_pairs:
            if (keyword_lower in qa['question'].lower() or
                keyword_lower in qa['answer'].lower() or
                keyword_lower in ' '.join(qa.get('tags', [])).lower()):
                results.append(qa)

        return results

    def get_by_category(self, category_id: str) -> List[Dict[str, Any]]:
        """Get all Q&A pairs in a specific category."""
        return [qa for qa in self.qa_pairs if qa['category'] == category_id]

    def get_by_tag(self, tag: str) -> List[Dict[str, Any]]:
        """Get all Q&A pairs with a specific tag."""
        return [qa for qa in self.qa_pairs if tag in qa.get('tags', [])]

    def get_high_confidence(self) -> List[Dict[str, Any]]:
        """Get all high-confidence Q&A pairs."""
        return [qa for qa in self.qa_pairs if qa.get('confidence') == 'high']

    def export_for_training(self, output_path: str = "training_data.jsonl"):
        """Export dataset in JSONL format for ML training."""
        with open(output_path, 'w', encoding='utf-8') as f:
            for qa in self.qa_pairs:
                training_example = {
                    'question': qa['question'],
                    'answer': qa['answer'],
                    'category': qa['category'],
                    'tags': qa.get('tags', [])
                }
                f.write(json.dumps(training_example) + '\n')

        print(f"Exported {len(self.qa_pairs)} examples to {output_path}")

    def statistics(self) -> Dict[str, Any]:
        """Generate statistics about the dataset."""
        total_pairs = len(self.qa_pairs)
        category_counts = {}
        confidence_counts = {'high': 0, 'medium': 0, 'low': 0}

        for qa in self.qa_pairs:
            category = qa['category']
            category_counts[category] = category_counts.get(category, 0) + 1

            confidence = qa.get('confidence', 'medium')
            confidence_counts[confidence] = confidence_counts.get(confidence, 0) + 1

        return {
            'total_qa_pairs': total_pairs,
            'total_categories': len(self.categories),
            'category_distribution': category_counts,
            'confidence_distribution': confidence_counts,
            'dataset_version': self.metadata.get('version', 'unknown')
        }

    def print_qa(self, qa: Dict[str, Any], show_metadata: bool = True):
        """Pretty print a Q&A pair."""
        print(f"\n{'='*80}")
        print(f"Q: {qa['question']}")
        print(f"\nA: {qa['answer']}")

        if show_metadata:
            print(f"\nCategory: {self.categories.get(qa['category'], {}).get('name', qa['category'])}")
            print(f"Tags: {', '.join(qa.get('tags', []))}")
            print(f"Confidence: {qa.get('confidence', 'N/A')}")
            if qa.get('references'):
                print(f"References: {', '.join(qa['references'])}")
        print('='*80)


def main():
    """Demonstrate dataset usage."""
    print("Wiki-QA Dataset Example Usage\n")

    # Load dataset
    dataset = WikiQADataset("qa-dataset.json")

    # Show statistics
    print("Dataset Statistics:")
    stats = dataset.statistics()
    print(f"  Version: {stats['dataset_version']}")
    print(f"  Total Q&A pairs: {stats['total_qa_pairs']}")
    print(f"  Categories: {stats['total_categories']}")
    print(f"  By category: {stats['category_distribution']}")
    print(f"  By confidence: {stats['confidence_distribution']}")

    # Search by keyword
    print("\n\nSearching for 'doublets':")
    results = dataset.search_by_keyword('doublets')
    for qa in results:
        dataset.print_qa(qa, show_metadata=False)

    # Get by category
    print("\n\nLinks Platform category:")
    lp_questions = dataset.get_by_category('links-platform')
    print(f"Found {len(lp_questions)} questions in this category")

    # Get high confidence answers
    print("\n\nHigh confidence Q&A pairs:")
    high_conf = dataset.get_high_confidence()
    print(f"Found {len(high_conf)} high-confidence answers")

    # Export for training
    print("\n\nExporting for training...")
    dataset.export_for_training("training_data.jsonl")

    print("\n\nExample complete!")


if __name__ == "__main__":
    main()
