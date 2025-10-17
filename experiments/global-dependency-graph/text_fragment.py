"""
TextFragment: Represents a fragment of text with its origin and metadata.

This module provides a way to identify and track text fragments across different
sources (repositories, files, etc.) using content-based hashing.
"""

import hashlib
from datetime import datetime
from typing import Optional, Set
from dataclasses import dataclass, field


@dataclass
class TextFragment:
    """
    Represents a text fragment with its metadata and origin information.

    Attributes:
        content: The actual text content
        source: Origin identifier (e.g., repository URL, file path)
        timestamp: When this fragment was first discovered
        line_start: Starting line number in the source
        line_end: Ending line number in the source
        metadata: Additional metadata about the fragment
    """
    content: str
    source: str
    timestamp: datetime
    line_start: Optional[int] = None
    line_end: Optional[int] = None
    metadata: dict = field(default_factory=dict)

    def __post_init__(self):
        """Calculate the content hash after initialization."""
        self._hash = self._calculate_hash()

    def _calculate_hash(self) -> str:
        """
        Calculate a SHA-256 hash of the normalized content.

        Normalization includes:
        - Stripping leading/trailing whitespace
        - Normalizing line endings

        Returns:
            Hexadecimal hash string
        """
        normalized = self.content.strip().replace('\r\n', '\n')
        return hashlib.sha256(normalized.encode('utf-8')).hexdigest()

    @property
    def hash(self) -> str:
        """Get the content hash of this fragment."""
        return self._hash

    def similarity_score(self, other: 'TextFragment') -> float:
        """
        Calculate similarity between this and another fragment.

        Uses a simple approach based on common substrings.
        For production, consider using more sophisticated algorithms like:
        - Levenshtein distance
        - Jaccard similarity
        - Cosine similarity with TF-IDF

        Args:
            other: Another TextFragment to compare with

        Returns:
            Similarity score between 0.0 and 1.0
        """
        if self.hash == other.hash:
            return 1.0

        # Simple character-level similarity
        set1 = set(self.content.lower())
        set2 = set(other.content.lower())

        if not set1 or not set2:
            return 0.0

        intersection = len(set1.intersection(set2))
        union = len(set1.union(set2))

        return intersection / union if union > 0 else 0.0

    def is_modified_version(self, other: 'TextFragment', threshold: float = 0.8) -> bool:
        """
        Check if this fragment is a modified version of another.

        Args:
            other: Another TextFragment to compare with
            threshold: Similarity threshold (0.0 to 1.0)

        Returns:
            True if similarity exceeds threshold
        """
        return self.similarity_score(other) >= threshold

    def __eq__(self, other) -> bool:
        """Two fragments are equal if they have the same content hash."""
        if not isinstance(other, TextFragment):
            return False
        return self.hash == other.hash

    def __hash__(self) -> int:
        """Use the content hash for set/dict operations."""
        return int(self._hash[:16], 16)

    def __repr__(self) -> str:
        """String representation of the fragment."""
        preview = self.content[:50] + '...' if len(self.content) > 50 else self.content
        return f"TextFragment(hash={self.hash[:8]}..., source={self.source}, content='{preview}')"
