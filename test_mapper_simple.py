#!/usr/bin/env python3

import os
import subprocess
import re
from collections import defaultdict

def simple_issue_to_code_mapper(codebase_root, issue_text, max_results=10):
    """
    Simple Python implementation to test the concept before relying on C# compilation
    """

    # Extract words from issue text
    words = re.findall(r'\b[a-zA-Z][a-zA-Z0-9_]*\b', issue_text.lower())
    words = [w for w in words if len(w) > 2]  # Filter short words
    words = list(dict.fromkeys(words))  # Remove duplicates while preserving order

    print(f"Extracted words: {words[:10]}")  # Show first 10 words

    # Find source files
    source_extensions = ['.cs', '.js', '.ts', '.py', '.cpp', '.h']
    source_files = []

    for root, dirs, files in os.walk(codebase_root):
        # Skip common excluded directories
        dirs[:] = [d for d in dirs if d not in ['bin', 'obj', 'packages', 'node_modules', '.git', '.vs']]

        for file in files:
            if any(file.endswith(ext) for ext in source_extensions):
                source_files.append(os.path.join(root, file))

    print(f"Found {len(source_files)} source files")

    # Score files
    file_scores = {}

    for file_path in source_files:
        try:
            with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read().lower()
                lines = content.split('\n')

            matched_words = []
            line_matches = []

            for i, line in enumerate(lines):
                line_matched_words = []
                for word in words:
                    if word in line:
                        if word not in matched_words:
                            matched_words.append(word)
                        line_matched_words.append(word)

                if line_matched_words:
                    line_matches.append((i + 1, line.strip(), line_matched_words))

            if matched_words:
                # Calculate score
                word_ratio = len(matched_words) / len(words)
                sequence_length = calculate_max_sequence(words, matched_words)
                score = (word_ratio * 100) + (sequence_length * 10)

                file_scores[file_path] = {
                    'score': score,
                    'matched_words': matched_words,
                    'line_matches': line_matches,
                    'sequence_length': sequence_length
                }
        except Exception as e:
            # Skip files that can't be read
            continue

    # Sort by score and return top results
    sorted_files = sorted(file_scores.items(), key=lambda x: x[1]['score'], reverse=True)
    return sorted_files[:max_results], len(source_files), words

def calculate_max_sequence(original_words, matched_words):
    """Calculate the maximum sequence length of consecutive words"""
    if len(matched_words) < 2:
        return len(matched_words)

    max_sequence = 1
    current_sequence = 1

    # Check for consecutive words in the original order
    matched_set = set(matched_words)
    for i in range(len(original_words) - 1):
        if original_words[i] in matched_set and original_words[i + 1] in matched_set:
            current_sequence += 1
        else:
            max_sequence = max(max_sequence, current_sequence)
            current_sequence = 1

    return max(max_sequence, current_sequence)

def test_mapper():
    print("Simple Issue to Code Mapper Test")
    print("================================")
    print()

    # Test with current directory
    codebase_root = "."

    # Test case 1: Search functionality
    print("Test 1: Search functionality issue")
    print("-" * 40)
    issue1 = "The search function is not working properly when indexing files"
    results1, total_files, words1 = simple_issue_to_code_mapper(codebase_root, issue1, 5)

    print(f"Results for '{issue1}':")
    print(f"Total files scanned: {total_files}")
    print(f"Matches found: {len(results1)}")
    print()

    for i, (file_path, data) in enumerate(results1):
        rel_path = os.path.relpath(file_path, codebase_root)
        print(f"{i+1}. {rel_path}")
        print(f"   Score: {data['score']:.2f}")
        print(f"   Sequence Length: {data['sequence_length']}")
        print(f"   Matched Words: {', '.join(data['matched_words'][:5])}")
        if data['line_matches']:
            line_num, line_content, _ = data['line_matches'][0]
            print(f"   Example (line {line_num}): {line_content[:60]}...")
        print()

    print("\n" + "="*60 + "\n")

    # Test case 2: Actual issue #681
    print("Test 2: Actual issue #681")
    print("-" * 40)
    issue2 = "We can use any mentions of UI text or functions or stack traces from all of files descriptions for the issue to find exact code that might be related to the problem First we start by searching each word case insensitive and get the instant index of all source code files"
    results2, total_files2, words2 = simple_issue_to_code_mapper(codebase_root, issue2, 5)

    print(f"Results for issue #681:")
    print(f"Total files scanned: {total_files2}")
    print(f"Matches found: {len(results2)}")
    print()

    for i, (file_path, data) in enumerate(results2):
        rel_path = os.path.relpath(file_path, codebase_root)
        print(f"{i+1}. {rel_path}")
        print(f"   Score: {data['score']:.2f}")
        print(f"   Sequence Length: {data['sequence_length']}")
        print(f"   Matched Words: {', '.join(data['matched_words'][:5])}")
        if data['line_matches']:
            line_num, line_content, _ = data['line_matches'][0]
            print(f"   Example (line {line_num}): {line_content[:60]}...")
        print()

    print("\nComparison commands:")
    print(f"GitHub Search: gh search code --owner konard {' '.join(words2[:3])}")
    print(f"Web Search: site:github.com/konard/LinksPlatform {' '.join(words2[:4])}")

if __name__ == "__main__":
    test_mapper()