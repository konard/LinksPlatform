# Recursive Paths as Links Network Structure

## Overview

This document explores the concept of representing recursive paths (as in materialized path pattern) as a links network structure, rather than as a traditional tree structure. This approach demonstrates how the Links Platform can express hierarchical and path-based data using its fundamental doublet link structure.

## Background

### Materialized Path Pattern

The materialized path pattern is a common technique for storing hierarchical data in databases. It stores the complete path from root to a node as a string, typically using delimiters.

**Example of traditional materialized path:**
```
/animals/mammals/primates/humans
/animals/mammals/dogs
/animals/reptiles/snakes
```

In this representation:
- Each path is stored as a single string
- Delimiters (like `/`) separate hierarchy levels
- The complete ancestry is visible in each path
- Queries can use string operations to find descendants or ancestors

### Regular Sequence vs Recursive Sequence

**Regular Sequence (Linear Path):**
A regular sequence represents elements in a straightforward linear order:
```
[A] -> [B] -> [C] -> [D]
```

This is like a linked list or simple path where each element points to the next.

**Recursive Sequence (Hierarchical Path):**
A recursive sequence embeds structural information within itself:
```
[A] -> [[B] -> [[C] -> [D]]]
```

Each step in the path can contain another complete path structure, creating nested or hierarchical relationships.

## Recursive Paths in Links Network

### Fundamental Concept

In the Links Platform, paths can be represented using doublet links (pairs). A recursive path structure means that:

1. A path can be an atomic element
2. A path can be composed of two sub-paths
3. Sub-paths can themselves be either atomic elements or composed paths

This creates a **self-similar, fractal-like structure** where the same pattern repeats at different scales.

### Structure Examples

#### Example 1: Simple Linear Path

**Traditional representation:**
```
Path: A -> B -> C
```

**As doublet links (non-recursive, "ladder" style):**
```
Link1: (A, B)
Link2: (Link1, C)  // equivalent to ((A, B), C)
```

This is called the "ladder" or "list" approach - each new element extends the sequence.

#### Example 2: Balanced Binary Path

**As balanced doublet links:**
```
Link1: (A, B)
Link2: (C, D)
Link3: (Link1, Link2)  // equivalent to ((A, B), (C, D))
```

This creates a balanced tree structure for the sequence.

#### Example 3: Recursive Materialized Path

**Traditional materialized path:**
```
/animals/mammals/primates/humans
```

**As recursive links network:**

Instead of storing as a string, we can represent this as nested links:

```
humans_path = (
  (animals, (mammals, (primates, humans)))
)
```

Or in a more balanced form:
```
humans_path = (
  (animals, mammals),
  (primates, humans)
)
```

**Key insight:** Each link can reference other links, creating a network where:
- Leaf nodes are actual data elements (animals, mammals, primates, humans)
- Internal nodes are links that structure the path
- The same sub-paths can be reused across different paths

### Example 4: Reusing Common Sub-paths

Consider multiple paths that share common prefixes:

**Traditional materialized paths:**
```
/animals/mammals/primates/humans
/animals/mammals/primates/chimpanzees
/animals/mammals/dogs
```

**As recursive links network with reuse:**

```
mammals_path = (animals, mammals)
primates_path = (mammals_path, primates)
humans_path = (primates_path, humans)
chimps_path = (primates_path, chimpanzees)
dogs_path = (mammals_path, dogs)
```

**Advantage:** The common sub-path `(animals, mammals)` is stored only once and reused by multiple paths. Similarly, `(mammals_path, primates)` is reused.

This provides **automatic compression** - shared path segments exist as single links that are referenced multiple times.

## Recursive Path Properties

### 1. Self-Reference Capability

A recursive path can reference itself:
```
cyclic_path = (A, cyclic_path)
```

This creates cycles in the network, enabling representation of:
- Circular references
- Recursive data structures
- Graph structures with loops

### 2. Arbitrary Nesting Depth

Unlike string-based materialized paths with fixed delimiters, recursive link paths can have unlimited nesting:

```
deep_path = (
  (
    ((A, B), (C, D)),
    ((E, F), (G, H))
  ),
  (
    ((I, J), (K, L)),
    ((M, N), (O, P))
  )
)
```

### 3. Type Flexibility

In a links network, the same structure can represent:
- **Paths** (ordered sequences of locations)
- **Hierarchies** (parent-child relationships)
- **Categories** (taxonomies and classifications)
- **Decision trees** (branching choices)
- **File systems** (directories and files)

The interpretation depends on context, not on the structure itself.

## Comparison: Regular vs Recursive Sequences

### Regular Sequence (as String or Array)

**Structure:**
```
sequence = "ABCD"
sequence = [A, B, C, D]
```

**Characteristics:**
- Fixed representation
- Single dimension (linear)
- No structural sharing
- Simple to implement
- Limited expressiveness

### Recursive Sequence (as Links Network)

**Structure:**
```
sequence = (A, (B, (C, D)))
sequence = ((A, B), (C, D))
```

**Characteristics:**
- Multiple valid representations
- Multi-dimensional (hierarchical)
- Automatic structural sharing through link reuse
- More complex to implement
- Rich expressiveness

## Practical Applications

### 1. File System Paths

**Traditional:**
```
/home/user/documents/report.pdf
```

**Recursive links:**
```
home = (root, "home")
user = (home, "user")
documents = (user, "documents")
report = (documents, "report.pdf")
```

Common directory paths are stored once and reused.

### 2. Category Hierarchies

**Traditional:**
```
Electronics > Computers > Laptops > Gaming Laptops
Electronics > Computers > Desktops > Workstations
```

**Recursive links:**
```
electronics = (root, "Electronics")
computers = (electronics, "Computers")
laptops = (computers, "Laptops")
gaming_laptops = (laptops, "Gaming Laptops")
desktops = (computers, "Desktops")
workstations = (desktops, "Workstations")
```

The path `Electronics > Computers` is represented once by the `computers` link.

### 3. URL Paths

**Traditional:**
```
https://example.com/api/v1/users/123/posts/456
```

**Recursive links (path component):**
```
api_path = (root, "api")
v1_path = (api_path, "v1")
users_path = (v1_path, "users")
user_123_path = (users_path, "123")
posts_path = (user_123_path, "posts")
post_456_path = (posts_path, "456")
```

### 4. Comment Threads

**Traditional (materialized path):**
```
Comment 1: "1"
  Comment 2: "1/2"
    Comment 3: "1/2/3"
  Comment 4: "1/4"
Comment 5: "5"
```

**Recursive links:**
```
comment1 = (thread_root, comment_1_content)
comment2 = (comment1, comment_2_content)
comment3 = (comment2, comment_3_content)
comment4 = (comment1, comment_4_content)
comment5 = (thread_root, comment_5_content)
```

## Advantages of Recursive Path Representation

### 1. Space Efficiency Through Deduplication

When multiple paths share common prefixes, those prefixes are stored only once:

```
# 1000 paths starting with /animals/mammals/
# Traditional: stores "animals/mammals/" 1000 times
# Recursive links: stores (animals, mammals) link once, referenced 1000 times
```

### 2. Structural Flexibility

The same data can be organized in different ways without changing the underlying elements:

```
# Balanced:
path = ((A, B), (C, D))

# Left-heavy:
path = (A, (B, (C, D)))

# Right-heavy:
path = (((A, B), C), D)
```

All represent the same sequence [A, B, C, D] but with different structural properties for different use cases.

### 3. Semantic Richness

Links can have associated metadata without changing the basic structure:

```
path_link = (parent, child)
# can also carry: timestamps, permissions, types, weights, etc.
```

### 4. Query Capabilities

Finding all descendants of a node becomes a graph traversal:
```
# Find all paths containing "mammals"
# = Find all links that reference the mammals link
```

Finding common ancestors becomes finding shared links in path networks.

## Implementation Considerations

### 1. Compression Degree

As discussed in the Links Theory document (links-theory.md), sequences can have different "degrees of compression" based on how much structural sharing occurs.

**Highest compression:** Maximal reuse of common sub-paths
**Lowest compression:** No reuse (each path fully independent)

### 2. Balancing Strategy

Different balancing strategies affect:
- **Tree height** (affects traversal depth)
- **Reuse potential** (affects space efficiency)
- **Update complexity** (affects modification speed)

### 3. Cycle Handling

Recursive paths can create cycles:
```
A = (B, C)
B = (A, D)  # Cycle: A references B, B references A
```

This requires careful handling in traversal algorithms to avoid infinite loops.

## Conclusion

Recursive paths in a links network structure provide a powerful generalization of traditional materialized paths. By representing paths as composable links rather than flat strings, we gain:

- **Automatic deduplication** of common path segments
- **Flexible structural organization** (balanced, unbalanced, etc.)
- **Richer semantic capabilities** through link metadata
- **Graph-like expressiveness** while maintaining path semantics

This approach transforms the simple concept of a "path" into a first-class, composable structural element in a links-based data model, enabling more efficient and expressive representation of hierarchical and network data.

## References

- Materialized Path Pattern: A technique for storing hierarchical data where each node stores its complete path from root
- Links Theory (links-theory.md): Fundamental concepts of doublet links and sequence compression
- Catalan Numbers: Count of possible binary tree structures for n elements, relevant to sequence representations
