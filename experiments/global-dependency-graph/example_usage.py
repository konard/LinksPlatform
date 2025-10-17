"""
Example usage of the Global Dependency Graph system.

This script demonstrates how the system tracks text fragments across different
sources and automatically reorganizes when earlier sources are discovered.
"""

from datetime import datetime, timedelta
from global_dependency_graph import GlobalDependencyGraph
import json


def example_basic_usage():
    """Demonstrate basic fragment tracking."""
    print("=" * 60)
    print("Example 1: Basic Fragment Tracking")
    print("=" * 60)

    graph = GlobalDependencyGraph()

    # Simulate discovering text fragments from different repositories
    # at different times

    # Repository A creates some code
    print("\n1. Repository A creates original code...")
    node1 = graph.add_fragment(
        content="""def calculate_sum(numbers):
    total = 0
    for num in numbers:
        total += num
    return total""",
        source="github.com/userA/repo-a/utils.py",
        timestamp=datetime(2023, 1, 1),
        line_start=10,
        line_end=14,
        metadata={"commit": "abc123"}
    )
    print(f"   Added fragment: {node1.fragment.hash[:16]}...")

    # Repository B copies the code
    print("\n2. Repository B copies the code (not a fork on GitHub)...")
    node2 = graph.add_fragment(
        content="""def calculate_sum(numbers):
    total = 0
    for num in numbers:
        total += num
    return total""",
        source="github.com/userB/repo-b/helpers.py",
        timestamp=datetime(2023, 2, 1),
        line_start=5,
        line_end=9,
        metadata={"commit": "def456"}
    )
    print(f"   Added fragment: {node2.fragment.hash[:16]}...")
    print(f"   Detected as copy from: {node1.fragment.source}")

    # Repository C modifies the code
    print("\n3. Repository C modifies the code slightly...")
    node3 = graph.add_fragment(
        content="""def calculate_sum(numbers):
    result = 0
    for num in numbers:
        result += num
    return result""",
        source="github.com/userC/repo-c/math_utils.py",
        timestamp=datetime(2023, 3, 1),
        line_start=20,
        line_end=24,
        metadata={"commit": "ghi789"}
    )
    print(f"   Added fragment: {node3.fragment.hash[:16]}...")
    print(f"   Detected as modified version")

    # Print network visualization
    print("\n" + "=" * 60)
    print("Fork Network Visualization:")
    print("=" * 60)
    print(graph.visualize_network(node1.fragment.hash))

    # Print statistics
    print("\n" + "=" * 60)
    print("Graph Statistics:")
    print("=" * 60)
    stats = graph.get_statistics()
    for key, value in stats.items():
        print(f"   {key}: {value}")


def example_earlier_source_discovery():
    """Demonstrate dynamic reorganization when an earlier source is found."""
    print("\n\n" + "=" * 60)
    print("Example 2: Dynamic Reorganization with Earlier Source")
    print("=" * 60)

    graph = GlobalDependencyGraph()

    # We first discover the code in Repository B
    print("\n1. First discovery: Repository B (2023-02-01)...")
    node_b = graph.add_fragment(
        content="const API_KEY = 'your-api-key-here';",
        source="github.com/userB/project-b/config.js",
        timestamp=datetime(2023, 2, 1)
    )
    print(f"   Added fragment: {node_b.fragment.hash[:16]}...")
    print(f"   Currently appears to be the original source")

    # Then Repository C copies it
    print("\n2. Repository C copies the code (2023-03-01)...")
    node_c = graph.add_fragment(
        content="const API_KEY = 'your-api-key-here';",
        source="github.com/userC/project-c/settings.js",
        timestamp=datetime(2023, 3, 1)
    )
    print(f"   Added fragment: {node_c.fragment.hash[:16]}...")
    print(f"   Linked as child of Repository B")

    print("\n" + "-" * 60)
    print("Current network:")
    print("-" * 60)
    print(graph.visualize_network(node_b.fragment.hash, max_depth=2))

    # Now we discover it was actually in Repository A even earlier!
    print("\n" + "=" * 60)
    print("3. DISCOVERY: Found earlier source in Repository A (2023-01-01)!")
    print("=" * 60)
    node_a = graph.add_fragment(
        content="const API_KEY = 'your-api-key-here';",
        source="github.com/userA/original-repo/constants.js",
        timestamp=datetime(2023, 1, 1)
    )
    print(f"   Added fragment: {node_a.fragment.hash[:16]}...")
    print(f"   Graph automatically reorganized!")

    print("\n" + "-" * 60)
    print("Reorganized network:")
    print("-" * 60)
    print(graph.visualize_network(node_a.fragment.hash, max_depth=2))

    # Print the fork chain
    print("\n" + "-" * 60)
    print("Fork chain from earliest to latest:")
    print("-" * 60)
    chain = node_c.get_fork_chain()
    for i, node in enumerate(chain):
        print(f"   {i+1}. [{node.fragment.timestamp.date()}] {node.fragment.source}")


def example_cross_repository_tracking():
    """Demonstrate tracking the same text fragment across many repositories."""
    print("\n\n" + "=" * 60)
    print("Example 3: Cross-Repository Fragment Tracking")
    print("=" * 60)

    graph = GlobalDependencyGraph()

    # A common code snippet appears in many repositories
    common_code = "import React from 'react';\nimport ReactDOM from 'react-dom';"

    repositories = [
        ("github.com/user1/react-app", datetime(2023, 1, 15)),
        ("github.com/user2/my-website", datetime(2023, 2, 10)),
        ("github.com/user3/dashboard", datetime(2023, 3, 5)),
        ("github.com/user4/portfolio", datetime(2023, 4, 20)),
        ("stackoverflow.com/questions/12345", datetime(2022, 12, 1)),  # Earlier!
    ]

    print("\nAdding fragments from different sources...")
    for source, timestamp in repositories:
        node = graph.add_fragment(
            content=common_code,
            source=source,
            timestamp=timestamp
        )
        print(f"   [{timestamp.date()}] {source}")

    # Find the network
    print("\n" + "=" * 60)
    print("Complete Fork Network:")
    print("=" * 60)

    # Get any node to visualize
    any_node = list(graph.nodes.values())[0]
    print(graph.visualize_network(any_node.fragment.hash, max_depth=5))

    # Show statistics
    print("\n" + "=" * 60)
    print("Network Statistics:")
    print("=" * 60)
    stats = graph.get_statistics()
    for key, value in stats.items():
        print(f"   {key}: {value}")


def example_export_for_visualization():
    """Demonstrate exporting graph data for visualization tools."""
    print("\n\n" + "=" * 60)
    print("Example 4: Export for Visualization")
    print("=" * 60)

    graph = GlobalDependencyGraph()

    # Add some fragments
    code_snippet = "function hello() { console.log('Hello, World!'); }"

    sources = [
        ("repo-a/main.js", datetime(2023, 1, 1)),
        ("repo-b/app.js", datetime(2023, 2, 1)),
        ("repo-c/index.js", datetime(2023, 3, 1)),
    ]

    for source, timestamp in sources:
        graph.add_fragment(code_snippet, source, timestamp)

    # Export the graph
    export_data = graph.export_graph_data()

    print("\nExported graph data (JSON format):")
    print(json.dumps(export_data, indent=2))

    print("\nThis data can be used with:")
    print("   - D3.js for interactive web visualizations")
    print("   - Gephi for network analysis")
    print("   - Cytoscape for biological network visualization")
    print("   - Any graph visualization library")


def main():
    """Run all examples."""
    print("\n" + "=" * 60)
    print("GLOBAL DEPENDENCY GRAPH - DEMONSTRATION")
    print("=" * 60)
    print("\nThis system tracks text fragments across repositories and")
    print("creates a dynamic dependency graph that reorganizes when")
    print("earlier sources are discovered.")

    example_basic_usage()
    example_earlier_source_discovery()
    example_cross_repository_tracking()
    example_export_for_visualization()

    print("\n" + "=" * 60)
    print("END OF DEMONSTRATION")
    print("=" * 60)


if __name__ == "__main__":
    main()
