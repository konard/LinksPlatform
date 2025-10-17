"""
Global Dependency Graph - Track text fragments across repositories.

This package provides a system for tracking text fragments (code, documentation, etc.)
across different sources and building a dynamic dependency graph that shows how
content is copied, modified, and reused.
"""

from .text_fragment import TextFragment
from .dependency_node import DependencyNode
from .global_dependency_graph import GlobalDependencyGraph

__all__ = ['TextFragment', 'DependencyNode', 'GlobalDependencyGraph']
__version__ = '0.1.0'
