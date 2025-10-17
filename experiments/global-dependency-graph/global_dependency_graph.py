"""
GlobalDependencyGraph: Manages the global dependency graph of text fragments.

This class handles the dynamic graph that connects text fragments across different
sources, automatically reorganizing when earlier sources are discovered.
"""

from typing import Dict, List, Set, Optional, Tuple
from datetime import datetime
from text_fragment import TextFragment
from dependency_node import DependencyNode


class GlobalDependencyGraph:
    """
    Manages a global dependency graph of text fragments across repositories.

    This graph automatically reorganizes when earlier sources are discovered,
    tracking all copy/paste operations and modifications across different sources.
    """

    def __init__(self):
        """Initialize an empty dependency graph."""
        self.nodes: Dict[str, DependencyNode] = {}  # hash -> node
        self.sources: Dict[str, Set[DependencyNode]] = {}  # source -> nodes
        self.similarity_threshold: float = 0.8

    def add_fragment(
        self,
        content: str,
        source: str,
        timestamp: datetime,
        line_start: Optional[int] = None,
        line_end: Optional[int] = None,
        metadata: Optional[dict] = None
    ) -> DependencyNode:
        """
        Add a text fragment to the graph.

        This method:
        1. Creates a TextFragment
        2. Checks if identical or similar fragments exist
        3. Establishes parent-child relationships based on timestamps
        4. Reorganizes the graph if an earlier source is found

        Args:
            content: The text content
            source: Source identifier (repo URL, file path, etc.)
            timestamp: When this fragment was created
            line_start: Starting line number
            line_end: Ending line number
            metadata: Additional metadata

        Returns:
            The created or existing DependencyNode
        """
        fragment = TextFragment(
            content=content,
            source=source,
            timestamp=timestamp,
            line_start=line_start,
            line_end=line_end,
            metadata=metadata or {}
        )

        # Check if this exact fragment already exists
        if fragment.hash in self.nodes:
            existing_node = self.nodes[fragment.hash]
            # Update if this is from an earlier source
            if timestamp < existing_node.fragment.timestamp:
                self._reorganize_for_earlier_source(existing_node, fragment)
            return existing_node

        # Create new node
        node = DependencyNode(fragment=fragment)
        self.nodes[fragment.hash] = node

        # Track by source
        if source not in self.sources:
            self.sources[source] = set()
        self.sources[source].add(node)

        # Find and establish relationships with similar fragments
        self._establish_relationships(node)

        return node

    def _establish_relationships(self, new_node: DependencyNode):
        """
        Establish parent-child relationships for a new node.

        Searches for similar fragments and creates relationships based on:
        1. Timestamp (earlier = parent)
        2. Similarity score
        """
        for existing_hash, existing_node in self.nodes.items():
            if existing_hash == new_node.fragment.hash:
                continue

            # Check if fragments are similar
            similarity = new_node.fragment.similarity_score(existing_node.fragment)

            if similarity >= self.similarity_threshold:
                # Determine parent-child relationship based on timestamp
                if new_node.fragment.timestamp < existing_node.fragment.timestamp:
                    # New node is earlier, so it's the parent
                    existing_node.add_parent(new_node, fork_type='modified')
                else:
                    # Existing node is earlier, so it's the parent
                    new_node.add_parent(existing_node, fork_type='modified')

    def _reorganize_for_earlier_source(
        self,
        existing_node: DependencyNode,
        earlier_fragment: TextFragment
    ):
        """
        Reorganize the graph when an earlier source is discovered.

        This is a key feature: if we find a fragment that appeared earlier
        than what we thought was the original, we need to reorganize the graph.

        Args:
            existing_node: The existing node in the graph
            earlier_fragment: The newly discovered earlier fragment
        """
        # Create a new node for the earlier fragment
        earlier_node = DependencyNode(fragment=earlier_fragment)
        self.nodes[earlier_fragment.hash] = earlier_node

        # The existing node should now be a child of the earlier node
        # First, remove existing node from any parents
        old_parents = list(existing_node.parents)
        for parent in old_parents:
            parent.children.discard(existing_node)
            existing_node.parents.discard(parent)

        # Now make the earlier node the parent
        existing_node.add_parent(earlier_node, fork_type='direct_copy')

    def find_fragment_by_content(self, content: str) -> Optional[DependencyNode]:
        """
        Find a fragment by its content.

        Args:
            content: The text content to search for

        Returns:
            The DependencyNode if found, None otherwise
        """
        temp_fragment = TextFragment(
            content=content,
            source="temp",
            timestamp=datetime.now()
        )
        return self.nodes.get(temp_fragment.hash)

    def find_similar_fragments(
        self,
        content: str,
        threshold: Optional[float] = None
    ) -> List[Tuple[DependencyNode, float]]:
        """
        Find fragments similar to the given content.

        Args:
            content: The text content to search for
            threshold: Similarity threshold (uses default if not specified)

        Returns:
            List of (node, similarity_score) tuples, sorted by similarity
        """
        temp_fragment = TextFragment(
            content=content,
            source="temp",
            timestamp=datetime.now()
        )

        threshold = threshold or self.similarity_threshold
        results = []

        for node in self.nodes.values():
            similarity = temp_fragment.similarity_score(node.fragment)
            if similarity >= threshold:
                results.append((node, similarity))

        # Sort by similarity descending
        results.sort(key=lambda x: x[1], reverse=True)
        return results

    def get_fragments_from_source(self, source: str) -> Set[DependencyNode]:
        """
        Get all fragments from a specific source.

        Args:
            source: The source identifier

        Returns:
            Set of DependencyNodes from that source
        """
        return self.sources.get(source, set())

    def get_fork_network(self, fragment_hash: str) -> Set[DependencyNode]:
        """
        Get the entire fork network for a fragment.

        This includes all ancestors and descendants.

        Args:
            fragment_hash: The hash of the fragment

        Returns:
            Set of all nodes in the fork network
        """
        node = self.nodes.get(fragment_hash)
        if not node:
            return set()

        network = {node}
        network.update(node.get_ancestors())
        network.update(node.get_descendants())

        return network

    def visualize_network(self, fragment_hash: str, max_depth: int = 3) -> str:
        """
        Create a text visualization of the fork network.

        Args:
            fragment_hash: The hash of the fragment to visualize
            max_depth: Maximum depth to traverse

        Returns:
            String representation of the network
        """
        node = self.nodes.get(fragment_hash)
        if not node:
            return "Fragment not found"

        lines = []
        visited = set()

        def _visualize_node(n: DependencyNode, depth: int, prefix: str = ""):
            if depth > max_depth or n in visited:
                return
            visited.add(n)

            # Node info
            indent = "  " * depth
            lines.append(
                f"{indent}{prefix}[{n.fragment.timestamp.isoformat()}] "
                f"{n.fragment.source} ({n.fork_type})"
            )
            lines.append(f"{indent}  Hash: {n.fragment.hash[:16]}...")
            lines.append(f"{indent}  Preview: {n.fragment.content[:50]}...")

            # Visualize children
            for i, child in enumerate(sorted(n.children, key=lambda c: c.fragment.timestamp)):
                child_prefix = "└─ " if i == len(n.children) - 1 else "├─ "
                _visualize_node(child, depth + 1, child_prefix)

        # Start from earliest ancestor
        root = node.find_earliest_ancestor()
        lines.append("=== Fork Network ===")
        _visualize_node(root, 0)

        return "\n".join(lines)

    def get_statistics(self) -> dict:
        """
        Get statistics about the dependency graph.

        Returns:
            Dictionary with various statistics
        """
        roots = [node for node in self.nodes.values() if node.is_root()]
        leaves = [node for node in self.nodes.values() if node.is_leaf()]

        return {
            "total_fragments": len(self.nodes),
            "total_sources": len(self.sources),
            "root_fragments": len(roots),
            "leaf_fragments": len(leaves),
            "average_children": sum(len(n.children) for n in self.nodes.values()) / len(self.nodes) if self.nodes else 0,
            "average_parents": sum(len(n.parents) for n in self.nodes.values()) / len(self.nodes) if self.nodes else 0,
        }

    def export_graph_data(self) -> dict:
        """
        Export the graph in a format suitable for visualization tools.

        Returns:
            Dictionary with nodes and edges
        """
        nodes_data = []
        edges_data = []

        for node in self.nodes.values():
            nodes_data.append({
                "id": node.fragment.hash,
                "source": node.fragment.source,
                "timestamp": node.fragment.timestamp.isoformat(),
                "content_preview": node.fragment.content[:100],
                "fork_type": node.fork_type,
            })

            for child in node.children:
                edges_data.append({
                    "from": node.fragment.hash,
                    "to": child.fragment.hash,
                    "type": child.fork_type,
                })

        return {
            "nodes": nodes_data,
            "edges": edges_data,
        }
