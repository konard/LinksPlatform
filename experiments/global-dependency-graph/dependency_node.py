"""
DependencyNode: Represents a node in the global dependency graph.

Each node contains a text fragment and tracks its relationships (forks, copies)
to other fragments.
"""

from typing import Set, List, Optional
from datetime import datetime
from dataclasses import dataclass, field
from text_fragment import TextFragment


@dataclass
class DependencyNode:
    """
    A node in the dependency graph representing a text fragment and its relationships.

    Attributes:
        fragment: The TextFragment this node represents
        parents: Nodes that this fragment was derived from (earlier versions)
        children: Nodes derived from this fragment (later forks/copies)
        fork_type: Type of relationship ('direct_copy', 'modified', 'original')
    """
    fragment: TextFragment
    parents: Set['DependencyNode'] = field(default_factory=set)
    children: Set['DependencyNode'] = field(default_factory=set)
    fork_type: str = 'original'

    def add_parent(self, parent: 'DependencyNode', fork_type: str = 'modified'):
        """
        Add a parent node (an earlier version of this content).

        Args:
            parent: The parent DependencyNode
            fork_type: Type of fork relationship
        """
        self.parents.add(parent)
        parent.children.add(self)
        self.fork_type = fork_type

    def add_child(self, child: 'DependencyNode', fork_type: str = 'modified'):
        """
        Add a child node (a later version derived from this content).

        Args:
            child: The child DependencyNode
            fork_type: Type of fork relationship
        """
        self.children.add(child)
        child.parents.add(self)
        child.fork_type = fork_type

    def is_root(self) -> bool:
        """Check if this node is a root (has no parents)."""
        return len(self.parents) == 0

    def is_leaf(self) -> bool:
        """Check if this node is a leaf (has no children)."""
        return len(self.children) == 0

    def get_ancestors(self) -> Set['DependencyNode']:
        """
        Get all ancestor nodes (recursive parent traversal).

        Returns:
            Set of all ancestor nodes
        """
        ancestors = set()
        to_visit = list(self.parents)

        while to_visit:
            node = to_visit.pop()
            if node not in ancestors:
                ancestors.add(node)
                to_visit.extend(node.parents)

        return ancestors

    def get_descendants(self) -> Set['DependencyNode']:
        """
        Get all descendant nodes (recursive child traversal).

        Returns:
            Set of all descendant nodes
        """
        descendants = set()
        to_visit = list(self.children)

        while to_visit:
            node = to_visit.pop()
            if node not in descendants:
                descendants.add(node)
                to_visit.extend(node.children)

        return descendants

    def find_earliest_ancestor(self) -> 'DependencyNode':
        """
        Find the earliest known source of this content.

        Returns:
            The root node with the earliest timestamp
        """
        ancestors = self.get_ancestors()
        if not ancestors:
            return self

        # Find the ancestor with the earliest timestamp
        earliest = min(
            ancestors,
            key=lambda node: node.fragment.timestamp,
            default=self
        )

        return earliest if earliest.fragment.timestamp < self.fragment.timestamp else self

    def get_fork_chain(self) -> List['DependencyNode']:
        """
        Get the chain of forks from earliest to this node.

        Returns:
            List of nodes from root to current
        """
        chain = []
        current = self

        # Build chain from current to root
        while current:
            chain.append(current)
            # Get the earliest parent
            if current.parents:
                current = min(current.parents, key=lambda p: p.fragment.timestamp)
            else:
                current = None

        # Reverse to get root to current
        chain.reverse()
        return chain

    def __eq__(self, other) -> bool:
        """Two nodes are equal if they contain the same fragment."""
        if not isinstance(other, DependencyNode):
            return False
        return self.fragment == other.fragment

    def __hash__(self) -> int:
        """Hash based on the fragment."""
        return hash(self.fragment)

    def __repr__(self) -> str:
        """String representation of the node."""
        return (
            f"DependencyNode(fragment={self.fragment.hash[:8]}..., "
            f"parents={len(self.parents)}, children={len(self.children)}, "
            f"type={self.fork_type})"
        )
