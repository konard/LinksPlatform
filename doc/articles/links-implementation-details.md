# Links Platform Implementation Details and Design Decisions

## Overview

Links Platform is an implementation of an associative memory database based on the concept of interconnected elements called "Links" or "Doublets". This document describes the core implementation details, basic rules, fundamental ideas, and the design decisions behind the current implementation.

## Core Concepts

### 1. Link as a Universal Data Structure

The fundamental building block of the Links Platform is the **Link** - a unified data type that combines both Element and Link from Simon Williams' original Associative Model of Data work.

**Key insight**: An Element or Point is a special case of a Link that references itself.

### 2. Two Primary Link Structures

The platform supports two structural variants:

#### Doublets (Untyped Links)
- Structure: **Source → Target**
- Each link contains two references:
  - **Source** (Start, Subject): The beginning of the association
  - **Target** (End, Object, Predicate): The end of the association
- Simpler structure, suitable for general-purpose associative storage
- Implementation: [Platform.Data.Doublets](https://linksplatform.github.io/Data.Doublets)

#### Triplets (Typed Links)
- Structure: **Source → Linker → Target**
- Each link contains three references:
  - **Source** (Start, Subject)
  - **Linker** (Verb, Type, Predicate): Describes the relationship type
  - **Target** (End, Object)
- Provides explicit typing of relationships
- Implementation: [Platform.Data.Triplets](https://linksplatform.github.io/Data.Triplets)

## Basic Rules and Principles

### 1. Self-Reference Rule
Links can reference themselves. This creates special types of links:
- **Point/Element**: A link where both Source and Target point to itself
- **Partial Self-Reference**: A link where only one reference points to itself

### 2. Link Uniqueness
When creating a link with specific Source, Linker (for triplets), and Target values:
- The system first searches for an existing link with identical properties
- If found, the existing link is returned (no duplicate is created)
- If not found, a new link is created
- This ensures data integrity and prevents redundancy

Implementation detail from `Link.cs:238-248`:
```csharp
Link link = TryFindExistingLink(source, linker, target);
if (link == null)
{
    link = new Link()
    {
        Source = source,
        Linker = linker,
        Target = target,
    };
}
```

### 3. Referential Integrity
- Links maintain bidirectional reference tracking
- Each link knows which other links reference it
- Three types of referrers are tracked:
  - `ReferersBySource`: Links that use this link as their Source
  - `ReferersByLinker`: Links that use this link as their Linker
  - `ReferersByTarget`: Links that use this link as their Target

### 4. Cascade Deletion
When a link is deleted:
- First, all its outgoing references (Source, Linker, Target) are cleared
- Then, all links that reference this link (referrers) are also deleted recursively
- This maintains consistency and prevents dangling references

Implementation from `Link.cs:388-396`:
```csharp
public void Delete()
{
    this.Source = null;
    this.Linker = null;
    this.Target = null;
    while (m_FirstRefererBySource != null) m_FirstRefererBySource.Delete();
    while (m_FirstRefererByLinker != null) m_FirstRefererByLinker.Delete();
    while (m_FirstRefererByTarget != null) m_FirstRefererByTarget.Delete();
}
```

## Design Decisions and Rationale

### 1. Memory-Mapped Storage
**Decision**: Use memory-mapped files for data persistence

**Rationale**:
- Provides fast, direct memory access to data
- Automatic persistence to disk
- Efficient for both small and large datasets
- Supports resizable storage through `UnitedMemoryLinks` and `SplitMemoryLinks`

**Implementation classes**:
- `FileMappedResizableDirectMemory`: For non-volatile memory storage
- `HeapResizableDirect`: For volatile memory
- `UnitedMemoryLinks`: Single memory region with resizing capability

### 2. Index-Based Access
**Decision**: Links are identified by numeric indices (UInt32 or UInt64)

**Rationale**:
- Constant-time O(1) access to any link
- Compact representation (4 or 8 bytes per reference)
- Efficient for large-scale graphs
- Supports up to 2^32 or 2^64 links per storage

### 3. Sequence Compression
**Decision**: Implement sequences as balanced binary trees of doublets

**Rationale**:
- Sequences of N elements can be represented with N-1 links
- Shared subsequences reduce storage requirements
- Example: "hello" and "help" share the prefix "hel"
- **Compression ratio** is measured as the ratio of actual links used vs. worst-case links needed
- Multiple tree structures (defined by Catalan numbers) can represent the same sequence

From `links-theory.md:369-383`:
> Any tree structure, regardless of its form but with a specific number of elements and the same elements, can represent the same sequence.

### 4. Linked List Implementation for Referrers
**Decision**: Use intrusive linked lists for tracking referrers

**Rationale**:
- No additional allocations needed for tracking
- Fast insertion/removal
- Memory-efficient: O(1) space per link
- Direct iteration over referrers without indirection

Implementation structure:
```csharp
private Link m_FirstRefererBySource;
private Link m_FirstRefererByLinker;
private Link m_FirstRefererByTarget;
private Link m_NextSiblingRefererBySource;
private Link m_NextSiblingRefererByLinker;
private Link m_NextSiblingRefererByTarget;
```

### 5. Open Source and Modular Design
**Decision**: Platform is open-source with modular architecture

**Rationale**:
- Closed-source systems were found to be limiting (e.g., Simon Williams' Sentences)
- Modularity allows using individual components independently
- Multiple language implementations (C#, C++, C, JavaScript, Python)
- Community-driven development accelerates progress
- Addresses the fundamental problem of slow knowledge sharing across languages and cultures

From the project philosophy:
> "In contrast to 'closed code', the path of development through 'open code' was chosen, which simplifies reorientation to all humanity as a whole, as opposed to a number of closed groups of people and communities."

### 6. Search Optimization Strategy
**Decision**: Search for existing links primarily by Target references

**Rationale**:
- Based on empirical performance measurements
- Can be parallelized across Source, Linker, and Target
- Target-first search chosen as the default optimization
- Future implementations may dynamically choose the optimal search path

From `Link.cs:359-377`:
```csharp
// Since Null is not a valid value, only the first search by Target will be executed.
// It is possible to search in 3 threads, and the one that finishes faster completes the operation.
// Performance results can also be tracked to decide if one pass is sufficient.
return target.ReferersByTarget.FirstOrDefault(isEqual);
```

### 7. Constants and Special Values
**Decision**: Define special constant links for common operations

**Rationale**:
- `Any`: Represents "any link" or "no constraint" in queries
- `Continue`: Used in iteration callbacks to continue processing
- `Break`: Used in iteration callbacks to stop processing
- Makes query and iteration code more readable and maintainable

Example usage:
```csharp
var any = links.Constants.Any;
var query = new Link<uint>(index: any, source: any, target: any);
links.Each((link) => {
    Console.WriteLine(links.Format(link));
    return links.Constants.Continue;
}, query);
```

### 8. Generic Type Parameters for Link Addresses
**Decision**: Support both UInt32 and UInt64 link addresses through generics

**Rationale**:
- UInt32: Up to ~4 billion links, more memory-efficient (12 bytes per doublet)
- UInt64: Up to ~18 quintillion links, for massive graphs (24 bytes per doublet)
- Generic implementation allows choosing based on use case
- Type safety ensured at compile time

## Performance Characteristics

### Time Complexity
- **Create/Read/Update/Delete single link**: O(1)
- **Search for existing link**: O(R) where R is the number of referrers by the searched field
- **Sequence operations**: Depends on compression; O(N) to O(log N) depending on structure

### Space Complexity
- **Per link storage**:
  - Doublet: 8 bytes (UInt32) or 16 bytes (UInt64) for Source and Target
  - Additional metadata managed by the storage layer
- **Sequences**: N-1 links for N elements (best case with maximum compression)
- **Memory overhead**: Minimal due to direct memory-mapped storage

### Comparison with SQLite
Independent benchmarks show Links Platform (Doublets) significantly outperforms SQLite for associative data operations, particularly for:
- High-volume insertions
- Graph traversals
- Relationship-heavy queries

## Implementation Evolution

### Historical Context
The platform has evolved through several phases:

1. **Early Implementation (2010)**: Triplet-based with Source-Linker-Target structure
2. **Simplification**: Recognition that doublets (pairs) are sufficient
3. **Modern Implementation**: Highly optimized doublet storage with optional triplet layer

### Current State
- Primary implementation: `Platform.Data.Doublets` (C#, C++)
- Modular ecosystem with ~30+ supporting libraries
- Active development with regular releases
- Production-ready for associative data storage use cases

## Future Directions

Based on the roadmap and theoretical foundations:

1. **Automatic Sequence Optimization**: Dynamic selection of optimal tree structure for sequences
2. **Distributed Storage**: Support for multi-node deployments
3. **Query Optimization**: Machine learning-driven query path selection
4. **Natural Language Integration**: Direct representation of linguistic structures
5. **Self-Modifying Algorithms**: Code represented as data within the link space

## Philosophical Foundation

The Links Platform is designed as a step toward:
- **Associative Memory Modeling**: Mimicking high-level effects of human associative memory
- **Automation of Automation**: Creating systems that can automate programming itself
- **Universal Knowledge Representation**: A common format for data and algorithms
- **AGI Foundation**: Providing the memory substrate for artificial general intelligence

From the README:
> "One of the most important goals of the project is to accelerate the development of automation to the level where it would be possible to automate automation itself. In other words, this project should allow for the implementation of a programmer bot that could create programs based on descriptions in human language."

## References

- Simon Williams: [Associative Model of Data](https://web.archive.org/web/20181219134621/http://sentences.com/docs/amd.pdf)
- Links Theory: [doc/articles/links-theory.md](links-theory.md)
- Platform.Data.Doublets: https://linksplatform.github.io/Data.Doublets
- Platform.Data.Triplets: https://linksplatform.github.io/Data.Triplets
- Main Repository: https://github.com/linksplatform
- Legacy Implementation: [28.03.2010-04.11.2010/Net/Net/Link.cs](../../28.03.2010-04.11.2010/Net/Net/Link.cs)
