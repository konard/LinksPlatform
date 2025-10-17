#!/usr/bin/env python3
"""
Basic Words Finder - Algorithm to classify words as basic or compound

A basic word is defined as a word that cannot be explained by other basic words
without referencing itself. This means its definition is inherently self-referential.

Algorithm:
1. Parse dictionary definitions for all words
2. Build a directed graph where edges represent "word A is used in definition of word B"
3. Identify strongly connected components (cycles) in the graph
4. Words in cycles (especially self-loops) are candidates for basic words
5. Words that can be fully explained without cycles are compound words
"""

from collections import defaultdict, deque
from typing import Dict, List, Set, Tuple
import re


class BasicWordsFinder:
    def __init__(self):
        self.dictionary: Dict[str, str] = {}
        self.word_references: Dict[str, Set[str]] = defaultdict(set)

    def add_definition(self, word: str, definition: str):
        """Add a word and its definition to the dictionary"""
        word = word.lower().strip()
        self.dictionary[word] = definition.lower()

    def extract_words_from_definition(self, definition: str) -> Set[str]:
        """Extract meaningful words from a definition (excluding common stopwords)"""
        # Simple word extraction - split by non-alphabetic characters
        words = re.findall(r'\b[a-z]+\b', definition.lower())

        # Filter out very common stopwords that don't carry meaning
        # Note: We keep words like "be", "exist", "have" etc. as they might be basic words
        stopwords = {
            'a', 'an', 'the', 'was', 'were', 'been',
            'do', 'does', 'did', 'will', 'would', 'could',
            'should', 'may', 'might', 'must', 'can', 'to', 'of', 'in', 'for',
            'on', 'at', 'by', 'with', 'from', 'as', 'that', 'which', 'who',
            'or', 'and', 'but', 'if', 'than', 'when', 'where', 'how', 'what',
            'there', 'here', 'this', 'these', 'those', 'it', 'its', 'itself',
            'one', 'ones', 'any', 'some', 'such', 'no', 'not', 'only', 'own',
            'same', 'so', 'too', 'very'
        }

        return {w for w in words if w not in stopwords and w in self.dictionary}

    def build_reference_graph(self):
        """Build a directed graph of word references in definitions"""
        for word, definition in self.dictionary.items():
            referenced_words = self.extract_words_from_definition(definition)
            self.word_references[word] = referenced_words

    def find_self_referential_words(self) -> Set[str]:
        """Find words that reference themselves in their definitions"""
        self_referential = set()
        for word, references in self.word_references.items():
            if word in references:
                self_referential.add(word)
        return self_referential

    def find_strongly_connected_components(self) -> List[Set[str]]:
        """Find strongly connected components using Tarjan's algorithm"""
        index_counter = [0]
        stack = []
        lowlinks = {}
        index = {}
        on_stack = defaultdict(bool)
        sccs = []

        def strongconnect(node):
            index[node] = index_counter[0]
            lowlinks[node] = index_counter[0]
            index_counter[0] += 1
            stack.append(node)
            on_stack[node] = True

            # Consider successors of node
            for successor in self.word_references.get(node, []):
                if successor not in index:
                    strongconnect(successor)
                    lowlinks[node] = min(lowlinks[node], lowlinks[successor])
                elif on_stack[successor]:
                    lowlinks[node] = min(lowlinks[node], index[successor])

            # If node is a root node, pop the stack and generate an SCC
            if lowlinks[node] == index[node]:
                component = set()
                while True:
                    w = stack.pop()
                    on_stack[w] = False
                    component.add(w)
                    if w == node:
                        break
                sccs.append(component)

        for node in self.dictionary.keys():
            if node not in index:
                strongconnect(node)

        return sccs

    def classify_words(self) -> Tuple[Set[str], Set[str]]:
        """
        Classify words as basic or compound

        Returns:
            Tuple of (basic_words, compound_words)
        """
        self.build_reference_graph()

        # Find self-referential words
        self_referential = self.find_self_referential_words()

        # Find strongly connected components (cyclic dependencies)
        sccs = self.find_strongly_connected_components()

        # Words in cycles (SCCs with size > 1 or self-loops) are basic
        basic_words = set()

        # Add self-referential words
        basic_words.update(self_referential)

        # Add words from non-trivial strongly connected components
        for component in sccs:
            if len(component) > 1:
                basic_words.update(component)

        # Words that can be defined without cycles are compound
        compound_words = set(self.dictionary.keys()) - basic_words

        return basic_words, compound_words

    def analyze_word_depth(self, word: str, visited: Set[str] = None) -> int:
        """
        Calculate the depth of a word's definition tree
        Returns -1 if there's a cycle (indicating a basic word)
        """
        if visited is None:
            visited = set()

        if word in visited:
            return -1  # Cycle detected

        if word not in self.dictionary:
            return 0  # External word

        visited.add(word)
        max_depth = 0

        for referenced_word in self.word_references.get(word, []):
            depth = self.analyze_word_depth(referenced_word, visited.copy())
            if depth == -1:
                return -1  # Cycle in definition chain
            max_depth = max(max_depth, depth + 1)

        return max_depth


def main():
    # Example: Small English dictionary subset
    finder = BasicWordsFinder()

    # Add example definitions
    # "be" is a classic basic word - it's hard to define without using itself
    finder.add_definition("be", "to exist or live")
    finder.add_definition("exist", "to be real or present")
    finder.add_definition("real", "actually existing or happening")
    finder.add_definition("present", "existing or happening now")
    finder.add_definition("happen", "to take place or occur")
    finder.add_definition("occur", "to happen or take place")
    finder.add_definition("place", "a particular position, point, or area")
    finder.add_definition("live", "to be alive or to exist")
    finder.add_definition("alive", "living or having life")
    finder.add_definition("life", "the condition that distinguishes living organisms from dead organisms")

    # Some compound words
    finder.add_definition("table", "a piece of furniture with a flat top and legs")
    finder.add_definition("furniture", "movable objects that make a room suitable for living or working")
    finder.add_definition("chair", "a piece of furniture for one person to sit on")
    finder.add_definition("sit", "to rest with the body supported by the buttocks")
    finder.add_definition("rest", "to cease work or movement in order to relax")

    # Classify words
    basic_words, compound_words = finder.classify_words()

    print("=" * 60)
    print("BASIC WORDS ANALYSIS")
    print("=" * 60)
    print(f"\nTotal words in dictionary: {len(finder.dictionary)}")
    print(f"Basic words: {len(basic_words)}")
    print(f"Compound words: {len(compound_words)}")

    print("\n" + "-" * 60)
    print("BASIC WORDS (self-referential or in cyclic definitions):")
    print("-" * 60)
    for word in sorted(basic_words):
        refs = finder.word_references.get(word, set())
        self_ref = "(SELF-REFERENTIAL)" if word in refs else ""
        print(f"  {word:15} -> {sorted(refs)} {self_ref}")

    print("\n" + "-" * 60)
    print("COMPOUND WORDS (can be defined without cycles):")
    print("-" * 60)
    for word in sorted(compound_words):
        refs = finder.word_references.get(word, set())
        print(f"  {word:15} -> {sorted(refs)}")

    # Analyze strongly connected components
    print("\n" + "-" * 60)
    print("STRONGLY CONNECTED COMPONENTS (cyclic groups):")
    print("-" * 60)
    sccs = finder.find_strongly_connected_components()
    for i, component in enumerate(sccs):
        if len(component) > 1:
            print(f"  Component {i+1}: {sorted(component)}")


if __name__ == "__main__":
    main()
