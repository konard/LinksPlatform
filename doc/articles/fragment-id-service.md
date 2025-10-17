# Fragment ID Service

## Overview

The Fragment ID Service is a deduplication system inspired by content-addressing mechanisms in IPFS and BitTorrent, adapted to work with the Links Platform's associative memory model. This service identifies and stores frequently used sequence fragments exactly once, eliminating redundancy across distributed storage systems.

## Problem Statement

In distributed file-sharing systems like IPFS and BitTorrent:
- Large files are split into chunks/pieces
- Identical content may be stored multiple times across different files
- Each system uses its own addressing scheme (CID in IPFS, piece hashes in BitTorrent)
- No universal fragment identification exists across systems

The Links Platform can serve as a universal foundation for fragment identification, allowing different systems to reference the same underlying data fragments.

## Core Concepts

### Content-Addressed Fragments

A fragment is a sequence of data with the following properties:
- **Immutable**: Content never changes
- **Content-Addressed**: Identified by hash of its content
- **Deduplicatable**: Identical fragments stored only once
- **Composable**: Larger sequences built from smaller fragments

### Fragment as Links Sequence

In Links Platform terms, a fragment is represented as a balanced sequence of doublets (pairs), where:
- Leaf nodes contain actual data (bytes, characters, or references)
- Internal nodes form the tree structure
- The root link serves as the fragment identifier

### Compression Through Deduplication

The service achieves compression by:
1. **Chunk-level deduplication**: Identical chunks stored once
2. **Subsequence sharing**: Common subsequences reused across fragments
3. **Structure sharing**: Tree nodes reused when subtrees match

## Architecture

### Fragment Storage Layer

```
Fragment Store (Links-based)
├── Raw Data Links (bytes, characters)
├── Sequence Links (balanced trees)
└── Fragment Index (hash → root link mapping)
```

### Fragment Identification

Each fragment has multiple identifiers:
1. **Links ID**: Root link address in associative memory
2. **Content Hash**: Cryptographic hash of content (SHA-256, BLAKE3)
3. **External IDs**: IPFS CID, BitTorrent piece hash, etc.

### Service Components

#### 1. Fragment Builder
- Accepts raw data sequences
- Constructs balanced link trees
- Returns fragment identifier

#### 2. Fragment Resolver
- Accepts fragment identifier
- Reconstructs original sequence
- Returns data or link sequence

#### 3. Deduplication Engine
- Detects existing fragments
- Identifies reusable subsequences
- Optimizes storage structure

#### 4. Cross-System Adapter
- Maps IPFS CIDs to Links IDs
- Maps BitTorrent hashes to Links IDs
- Provides unified fragment access

## Implementation Strategy

### Phase 1: Core Fragment Service

**Goal**: Basic fragment storage and retrieval

**Components**:
- Fragment data model in Links
- Hash-to-Link mapping
- Fragment creation API
- Fragment retrieval API

### Phase 2: Deduplication Optimization

**Goal**: Maximize storage efficiency

**Components**:
- Subsequence detection
- Balanced tree optimization
- Compression metrics tracking

### Phase 3: Cross-System Integration

**Goal**: Support IPFS and BitTorrent

**Components**:
- IPFS CID adapter
- BitTorrent piece hash adapter
- Gateway API for external systems

### Phase 4: Advanced Features

**Goal**: Production-ready service

**Components**:
- Fragment garbage collection
- Reference counting
- Performance optimization
- Distributed synchronization

## Technical Specification

### Fragment Data Model

A fragment in Links is represented as:

```
Fragment (root) → [Left Subsequence, Right Subsequence]
├── If leaf: points to [Data, Data] or [Self, Data]
└── If internal: points to [Fragment, Fragment]
```

### Hash Functions

Support multiple hash algorithms:
- **SHA-256**: Standard, widely supported
- **BLAKE3**: Fast, cryptographically secure
- **XXH3**: Ultra-fast, non-cryptographic (for internal use)

### API Design

#### Create Fragment
```
Input: byte[] data
Process:
  1. Compute content hash
  2. Check if fragment exists (hash lookup)
  3. If exists, return existing Links ID
  4. If new, build balanced sequence
  5. Store hash → Links ID mapping
  6. Return Links ID
Output: FragmentID (LinksID + ContentHash)
```

#### Retrieve Fragment
```
Input: FragmentID (LinksID or ContentHash)
Process:
  1. Resolve to Links ID if hash provided
  2. Traverse link tree
  3. Reconstruct byte sequence
Output: byte[] data
```

#### Index Fragment
```
Input: FragmentID, ExternalID (e.g., IPFS CID)
Process:
  1. Store mapping: ExternalID → LinksID
  2. Update cross-reference index
Output: Success/Failure
```

## Use Cases

### Use Case 1: IPFS Integration

**Scenario**: Store IPFS content in Links Platform

```
1. User adds file to IPFS
2. IPFS chunks file, generates CIDs
3. Fragment Service:
   - Receives chunks and CIDs
   - Stores each chunk as Links fragment
   - Maps CID → Links ID
   - Detects and reuses duplicate chunks
4. Result: Deduplicated storage, CID compatibility
```

### Use Case 2: BitTorrent Integration

**Scenario**: Serve BitTorrent pieces from Links Platform

```
1. Torrent file specifies piece hashes
2. Fragment Service:
   - Maps piece hash → Links ID
   - Retrieves fragment from Links store
   - Returns piece data
3. Result: Efficient piece serving, deduplication across torrents
```

### Use Case 3: Universal Fragment Store

**Scenario**: Multiple systems reference same fragments

```
1. Fragment stored once in Links Platform
2. Multiple identifiers point to same fragment:
   - IPFS CID: QmXXX...
   - BitTorrent hash: abc123...
   - Links ID: 42
3. Any system can retrieve via its native identifier
4. Result: Maximum deduplication, cross-system interoperability
```

## Performance Considerations

### Storage Efficiency

**Theoretical Compression**:
- Pure unique data: ~1x (no compression)
- Highly redundant data: up to 10-100x
- Real-world: typically 2-5x

**Overhead**:
- Tree structure: ~1 link per data element
- Hash index: ~32-64 bytes per fragment
- Cross-reference maps: ~64-128 bytes per external ID

### Time Complexity

**Fragment Creation**:
- Hash computation: O(n) where n = data size
- Tree construction: O(n) for balanced tree
- Deduplication check: O(1) hash lookup

**Fragment Retrieval**:
- Tree traversal: O(log n) for balanced tree
- Data reconstruction: O(n) for full sequence

### Scalability

**Storage Scaling**:
- Links Platform: billions of links per instance
- Fragment Index: hash table, millions of fragments
- Cross-reference: millions of mappings

**Performance Targets**:
- Fragment creation: < 1ms for small chunks (4KB-256KB)
- Fragment retrieval: < 1ms for small chunks
- Deduplication detection: < 0.1ms (hash lookup)

## Security Considerations

### Hash Collisions

**Risk**: Different content producing same hash

**Mitigation**:
- Use cryptographic hashes (SHA-256, BLAKE3)
- Optional content verification on retrieval
- Support multiple hash algorithms

### Content Addressing Trust

**Risk**: Malicious content with specific hash

**Mitigation**:
- Hash verification enforced
- Optional content signing
- Access control on fragment creation

## Future Enhancements

### Planned Features

1. **Incremental Hashing**: Update hashes on content modification
2. **Merkle Tree Proofs**: Verify fragment authenticity
3. **Erasure Coding**: Redundancy for fault tolerance
4. **Compression Codecs**: Additional compression before fragmentation
5. **Smart Chunking**: Content-defined chunking (rabin fingerprinting)

### Research Directions

1. **Optimal Tree Balancing**: Minimize tree height vs. deduplication
2. **Cache-Aware Structures**: Optimize for CPU cache efficiency
3. **Distributed Consensus**: Fragment IDs across multiple nodes
4. **Privacy-Preserving**: Encrypted fragments with deduplication

## Comparison with Existing Systems

### IPFS

**Similarities**:
- Content addressing
- Merkle DAG structure
- Deduplication

**Differences**:
- Links: More flexible sequence representation
- Links: Better subsequence sharing
- IPFS: Mature ecosystem, wide adoption

### BitTorrent v2

**Similarities**:
- Merkle tree for pieces
- Hash-based verification

**Differences**:
- Links: Universal fragment store across torrents
- Links: Subsequence-level deduplication
- BitTorrent: Focused on file distribution, not storage

### Git

**Similarities**:
- Content-addressed objects
- DAG structure
- Deduplication at object level

**Differences**:
- Links: Fine-grained sequence deduplication
- Links: Not tied to version control semantics
- Git: Optimized for source code versioning

## Conclusion

The Fragment ID Service leverages the Links Platform's associative memory model to create a universal, highly efficient fragment storage system. By treating fragments as balanced link sequences and maintaining cross-system identifier mappings, the service can:

1. **Eliminate redundancy**: Store identical content once
2. **Maximize sharing**: Reuse subsequences across fragments
3. **Bridge systems**: Unify IPFS, BitTorrent, and other systems
4. **Scale efficiently**: Handle billions of fragments

This positions the Links Platform as foundational infrastructure for decentralized storage and content distribution.

## References

- [IPFS Content Addressing](https://docs.ipfs.io/concepts/content-addressing/)
- [BitTorrent BEP 30: Merkle Hash Torrents](https://bittorrent.org/beps/bep_0030.html)
- [BitTorrent v2 (BEP 52)](https://bittorrent.org/beps/bep_0052.html)
- [Links Platform Theory](links-theory.md)
- [Catalan Numbers and Sequence Representations](https://en.wikipedia.org/wiki/Catalan_number)
