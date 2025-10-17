#!/usr/bin/env python3
"""
Example demonstrating the basic words classification algorithm
with a more comprehensive dictionary dataset
"""

import sys
import os

# Add parent directory to path to import the algorithm
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'experiments'))

from basic_words_finder import BasicWordsFinder


def create_sample_dictionary():
    """Create a more comprehensive sample dictionary for demonstration"""
    finder = BasicWordsFinder()

    # Core existential/philosophical words (likely basic)
    finder.add_definition("be", "to exist or to have reality")
    finder.add_definition("exist", "to be real or to have being")
    finder.add_definition("being", "the state of existing or living")
    finder.add_definition("have", "to possess or to hold")
    finder.add_definition("is", "third person singular present of be")

    # Action verbs with potential cycles
    finder.add_definition("do", "to perform or to carry out an action")
    finder.add_definition("perform", "to do or to carry out")
    finder.add_definition("make", "to create or to cause to exist")
    finder.add_definition("create", "to make or to bring into being")
    finder.add_definition("cause", "to make happen or to bring about")

    # Movement verbs
    finder.add_definition("move", "to change position or place")
    finder.add_definition("go", "to move or to travel")
    finder.add_definition("come", "to move toward or to arrive")
    finder.add_definition("arrive", "to come to a destination")

    # State and change
    finder.add_definition("become", "to begin to be or to grow to be")
    finder.add_definition("change", "to make or to become different")
    finder.add_definition("grow", "to increase or to become larger")

    # Perception and cognition (potentially basic)
    finder.add_definition("see", "to perceive with the eyes")
    finder.add_definition("perceive", "to become aware of through the senses")
    finder.add_definition("know", "to be aware of or to have knowledge")
    finder.add_definition("think", "to have thoughts or to use the mind")
    finder.add_definition("feel", "to perceive through touch or to experience emotion")

    # Time concepts
    finder.add_definition("happen", "to occur or to take place")
    finder.add_definition("occur", "to happen or to come about")
    finder.add_definition("begin", "to start or to come into being")
    finder.add_definition("start", "to begin or to commence")
    finder.add_definition("end", "to finish or to come to a conclusion")

    # Space and place
    finder.add_definition("place", "a particular position or location")
    finder.add_definition("position", "a place or location")
    finder.add_definition("location", "a particular place or position")

    # Clearly compound words - concrete objects
    finder.add_definition("table", "a piece of furniture with a flat top and legs")
    finder.add_definition("chair", "a piece of furniture for sitting")
    finder.add_definition("furniture", "movable objects used to make a room suitable")
    finder.add_definition("room", "a space enclosed by walls")
    finder.add_definition("wall", "a vertical structure that encloses an area")
    finder.add_definition("door", "a movable barrier used to close an opening")
    finder.add_definition("window", "an opening in a wall with glass")
    finder.add_definition("glass", "a hard transparent material")

    # Simple compound words
    finder.add_definition("book", "a written work bound together")
    finder.add_definition("pen", "a writing instrument")
    finder.add_definition("write", "to form letters or words on a surface")
    finder.add_definition("read", "to look at and understand written words")

    # More complex concepts
    finder.add_definition("understand", "to know the meaning or to comprehend")
    finder.add_definition("comprehend", "to understand or to grasp mentally")
    finder.add_definition("learn", "to gain knowledge or skill")
    finder.add_definition("knowledge", "information and understanding acquired through learning")
    finder.add_definition("information", "facts or knowledge communicated")

    # Living things
    finder.add_definition("live", "to be alive or to exist")
    finder.add_definition("alive", "living or having life")
    finder.add_definition("life", "the condition of living beings")
    finder.add_definition("organism", "a living thing")
    finder.add_definition("animal", "a living organism that can move")
    finder.add_definition("plant", "a living organism that grows in soil")

    return finder


def main():
    print("=" * 70)
    print("BASIC WORDS CLASSIFICATION - Extended Dictionary Example")
    print("=" * 70)
    print()
    print("This example demonstrates the algorithm with a more comprehensive")
    print("dictionary containing ~50 words from various semantic categories.")
    print()

    finder = create_sample_dictionary()

    # Classify words
    basic_words, compound_words = finder.classify_words()
    word_refs = finder.word_references

    print(f"Total words analyzed: {len(finder.dictionary)}")
    print(f"Basic words found: {len(basic_words)}")
    print(f"Compound words found: {len(compound_words)}")
    print()

    # Analyze strongly connected components
    sccs = finder.find_strongly_connected_components()
    non_trivial_sccs = [scc for scc in sccs if len(scc) > 1]

    print("-" * 70)
    print(f"STRONGLY CONNECTED COMPONENTS: {len(non_trivial_sccs)}")
    print("-" * 70)
    for i, component in enumerate(non_trivial_sccs, 1):
        print(f"\nComponent {i} ({len(component)} words): {sorted(component)}")
        print("  Definitions forming the cycle:")
        for word in sorted(component):
            refs = word_refs.get(word, set()) & component
            if refs:
                print(f"    {word} -> {sorted(refs)}")

    print()
    print("-" * 70)
    print("BASIC WORDS (detailed analysis)")
    print("-" * 70)

    # Group basic words by category
    categories = {
        "Existential": ["be", "exist", "being", "is", "have"],
        "Action": ["do", "perform", "make", "create", "cause"],
        "Movement": ["move", "go", "come", "arrive"],
        "Change": ["become", "change", "grow"],
        "Time": ["happen", "occur", "begin", "start", "end"],
        "Space": ["place", "position", "location"],
        "Perception": ["see", "perceive", "know", "think", "feel"],
        "Life": ["live", "alive", "life"],
        "Understanding": ["understand", "comprehend", "learn"],
    }

    for category, words in categories.items():
        basic_in_category = [w for w in words if w in basic_words]
        if basic_in_category:
            print(f"\n{category}:")
            for word in basic_in_category:
                refs = sorted(word_refs.get(word, set()))
                self_ref = "(SELF-REF)" if word in word_refs.get(word, set()) else ""
                print(f"  {word:15} -> {refs} {self_ref}")

    print()
    print("-" * 70)
    print("COMPOUND WORDS (sample - objects and concrete concepts)")
    print("-" * 70)

    # Show some example compound words
    compound_examples = [w for w in compound_words if w in [
        "table", "chair", "book", "pen", "door", "window",
        "furniture", "room", "wall", "glass", "animal", "plant"
    ]]

    for word in sorted(compound_examples):
        refs = sorted(word_refs.get(word, set()))
        print(f"  {word:15} -> {refs}")

    print()
    print("=" * 70)
    print("OBSERVATIONS:")
    print("=" * 70)
    print("""
1. Basic words tend to form tight circular dependencies with each other
2. Words like "be", "exist", "do", "have" are fundamental and self-referential
3. Many basic words relate to abstract concepts: existence, action, perception
4. Compound words (tables, chairs, books) can be fully explained using basic words
5. The algorithm successfully separates abstract foundational concepts from
   concrete objects and derivative concepts
    """)


if __name__ == "__main__":
    main()
