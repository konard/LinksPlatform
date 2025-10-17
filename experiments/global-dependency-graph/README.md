# Global Data Dependency Graph

A proof-of-concept implementation for tracking text fragments across repositories and creating a dynamic dependency graph.

## Overview

This system addresses [Issue #303](https://github.com/konard/LinksPlatform/issues/303) by providing a global data dependency graph that tracks how text fragments (code snippets, documentation, etc.) are copied, modified, and reused across different repositories and sources.

### Key Features

1. **Fragment Tracking**: Identifies and tracks text fragments using content-based hashing
2. **Cross-Repository Detection**: Detects when code is copied between repositories, even without GitHub forks
3. **Dynamic Reorganization**: Automatically reorganizes the graph when earlier sources are discovered
4. **Similarity Detection**: Identifies modified versions of fragments
5. **Fork Networks**: Visualizes the complete network of fragment relationships

## Architecture

The system consists of three main components:

### 1. TextFragment (`text_fragment.py`)

Represents a fragment of text with:
- Content-based SHA-256 hashing for unique identification
- Source information (repository, file, line numbers)
- Timestamp for chronological tracking
- Similarity scoring for detecting modifications

### 2. DependencyNode (`dependency_node.py`)

Represents a node in the dependency graph:
- Contains a TextFragment
- Tracks parent-child relationships (earlier/later versions)
- Provides traversal methods (ancestors, descendants)
- Identifies fork chains and earliest sources

### 3. GlobalDependencyGraph (`global_dependency_graph.py`)

Manages the entire dependency graph:
- Adds fragments and establishes relationships
- Automatically reorganizes when earlier sources are found
- Provides visualization and export capabilities
- Tracks statistics across the entire network

## How It Works

### Basic Flow

1. **Add Fragment**: When a text fragment is discovered, it's added to the graph with its source and timestamp
2. **Hash Generation**: A SHA-256 hash is calculated from the normalized content
3. **Relationship Detection**: The system searches for identical or similar fragments
4. **Parent-Child Assignment**: Relationships are established based on timestamps (earlier = parent)
5. **Dynamic Reorganization**: If a fragment with an earlier timestamp is found, the graph reorganizes

### Example Scenario

```
1. Repository B uses code (2023-02-01)
   └─ Repository C copies it (2023-03-01)

2. Later, we discover Repository A had it first (2023-01-01)

3. Graph automatically reorganizes:
   Repository A (2023-01-01) [ORIGINAL]
   └─ Repository B (2023-02-01)
      └─ Repository C (2023-03-01)
```

## Usage

### Basic Example

```python
from datetime import datetime
from global_dependency_graph import GlobalDependencyGraph

# Create a graph
graph = GlobalDependencyGraph()

# Add fragments
node1 = graph.add_fragment(
    content="def hello():\n    print('Hello, World!')",
    source="github.com/user1/repo-a/main.py",
    timestamp=datetime(2023, 1, 1)
)

node2 = graph.add_fragment(
    content="def hello():\n    print('Hello, World!')",
    source="github.com/user2/repo-b/app.py",
    timestamp=datetime(2023, 2, 1)
)

# Visualize the network
print(graph.visualize_network(node1.fragment.hash))

# Get statistics
stats = graph.get_statistics()
print(f"Total fragments: {stats['total_fragments']}")
```

### Running Examples

```bash
cd experiments/global-dependency-graph
python example_usage.py
```

This will run demonstrations of:
1. Basic fragment tracking
2. Dynamic reorganization with earlier source discovery
3. Cross-repository tracking
4. Export for visualization tools

## Use Cases

### 1. Code Provenance Tracking

Track where code snippets originated and how they spread across projects:
- Identify original authors
- Understand code reuse patterns
- Detect copy-paste without attribution

### 2. Knowledge Diffusion

Track how knowledge spreads across documentation:
- Find original sources of information
- Understand information flow between projects
- Detect documentation duplicates

### 3. License Compliance

Identify code that may have licensing implications:
- Track GPL code being copied
- Find attribution requirements
- Ensure license compliance

### 4. Plagiarism Detection

Detect unauthorized copying:
- Academic papers
- Code submissions
- Documentation

### 5. Version Control Enhancement

Extend Git's capabilities:
- Track copies across repositories
- Visualize global fork networks
- Understand code evolution

## Implementation Details

### Hash-Based Identification

- Uses SHA-256 for content identification
- Normalizes whitespace and line endings
- Enables fast exact-match lookups

### Similarity Detection

Current implementation uses simple character-set similarity. For production, consider:
- Levenshtein distance for edit distance
- Jaccard similarity for token-based comparison
- TF-IDF with cosine similarity for semantic similarity
- MinHash for efficient large-scale similarity detection

### Graph Storage

Current implementation uses in-memory storage. For production, consider:
- Graph databases (Neo4j, ArangoDB)
- Time-series databases for temporal queries
- Distributed storage for scalability

### Performance Optimizations

For large-scale deployment:
- Implement MinHash for O(1) similarity checks
- Use bloom filters for existence checks
- Index by timestamp for temporal queries
- Shard by content hash for distributed processing

## Future Enhancements

1. **Web Crawler Integration**
   - Automatically discover fragments across the internet
   - GitHub API integration for repository scanning
   - Stack Overflow and documentation site crawling

2. **Advanced Similarity Detection**
   - AST-based comparison for code
   - Semantic similarity using embeddings
   - Language-specific normalization

3. **Visualization Tools**
   - Interactive web interface
   - Timeline views
   - Heat maps of fragment spread

4. **API and Services**
   - REST API for fragment queries
   - GitHub App for automatic tracking
   - Browser extension for highlighting fragment origins

5. **Machine Learning**
   - Predict likely sources for fragments
   - Cluster similar fragments
   - Anomaly detection for unusual patterns

## Comparison with Git

| Feature | Git | Global Dependency Graph |
|---------|-----|------------------------|
| Tracks within repository | ✓ | ✓ |
| Tracks across repositories | ✗ | ✓ |
| Requires explicit forks | ✓ | ✗ |
| Detects copy-paste | Limited (-C flag) | ✓ |
| Dynamic reorganization | ✗ | ✓ |
| Works on any text | ✗ | ✓ |
| Decentralized | ✓ | Could be |

## Related Concepts

- **Git Blame**: Tracks line-by-line authorship within a repository
- **GitHub Network Graph**: Visualizes repository forks
- **Dependency Graphs**: Track software dependencies
- **OmniBOR**: Build dependency tracking with reproducible identifiers
- **Software Heritage**: Archive of all public source code

## Contributing

This is a proof-of-concept implementation. Areas for contribution:
1. Performance optimizations
2. Additional similarity algorithms
3. Visualization tools
4. Integration with existing version control systems
5. Test coverage and documentation

## License

Part of the LinksPlatform project. See repository LICENSE for details.

## References

- [Issue #303](https://github.com/konard/LinksPlatform/issues/303) - Original issue
- [Git Blame Documentation](https://git-scm.com/docs/git-blame)
- [GitHub Network Graph](https://docs.github.com/en/repositories/viewing-activity-and-data-for-your-repository/understanding-connections-between-repositories)
- [OmniBOR](https://omnibor.io/) - Build dependency tracking
