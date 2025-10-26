# Recursive Paths: Visual Diagrams and Examples

This document provides visual representations of recursive paths in links network structures.

## 1. Basic Path Representations

### 1.1 Linear Sequence (Regular Path)

**String representation:**
```
"A -> B -> C -> D"
```

**Visual as array:**
```
┌───┬───┬───┬───┐
│ A │ B │ C │ D │
└───┴───┴───┴───┘
```

**As doublet links (Ladder/List style):**
```
      ┌───────┐
      │   D   │
      └───────┘
          ↑
    ┌─────────┐
    │  (C,D)  │
    └─────────┘
        ↑
  ┌───────────┐
  │ (B,(C,D)) │
  └───────────┘
      ↑
┌─────────────────┐
│ (A,(B,(C,D)))  │  ← Final path link
└─────────────────┘

Structure: A -> (B -> (C -> D))
Height: 3
Links needed: 3 (one per connection)
```

### 1.2 Balanced Binary Path

**As doublet links (Balanced style):**
```
        ┌─────────────────┐
        │   ((A,B),(C,D)) │  ← Final path link
        └─────────────────┘
              ↙       ↘
        ┌────────┐   ┌────────┐
        │ (A,B)  │   │ (C,D)  │
        └────────┘   └────────┘
          ↙   ↘       ↙   ↘
        ┌──┐ ┌──┐  ┌──┐ ┌──┐
        │A │ │B │  │C │ │D │
        └──┘ └──┘  └──┘ └──┘

Structure: ((A,B),(C,D))
Height: 2
Links needed: 3 (one per pair)
```

**Comparison:**
- Both represent the sequence [A, B, C, D]
- Ladder style has height 3, balanced style has height 2
- Different traversal properties but same sequential meaning

## 2. Materialized Path as Recursive Links

### 2.1 Single Path Example

**Traditional materialized path:**
```
/animals/mammals/primates/humans
```

**As nested links (right-heavy tree):**
```
                    ┌──────────────────────────────────┐
                    │ (animals,(mammals,(primates,    │
                    │                    humans)))    │
                    └──────────────────────────────────┘
                                    ↓
              ┌──────────┐    ┌─────────────────────────┐
              │ animals  │    │ (mammals,(primates,     │
              │          │    │            humans))     │
              └──────────┘    └─────────────────────────┘
                                        ↓
                        ┌──────────┐    ┌────────────────────┐
                        │ mammals  │    │ (primates,humans)  │
                        └──────────┘    └────────────────────┘
                                                ↓
                                    ┌──────────┐  ┌──────────┐
                                    │ primates │  │  humans  │
                                    └──────────┘  └──────────┘
```

**As nested links (balanced tree):**
```
                ┌────────────────────────────────────┐
                │ ((animals,mammals),               │
                │     (primates,humans))            │
                └────────────────────────────────────┘
                        ↙                    ↘
        ┌────────────────────┐       ┌────────────────────┐
        │ (animals,mammals)  │       │ (primates,humans)  │
        └────────────────────┘       └────────────────────┘
              ↙         ↘                  ↙         ↘
        ┌─────────┐ ┌─────────┐    ┌─────────┐ ┌─────────┐
        │animals  │ │mammals  │    │primates │ │ humans  │
        └─────────┘ └─────────┘    └─────────┘ └─────────┘
```

### 2.2 Multiple Paths with Shared Prefixes

**Traditional materialized paths:**
```
/animals/mammals/primates/humans
/animals/mammals/primates/chimpanzees
/animals/mammals/dogs
/animals/reptiles/snakes
```

**As recursive links network with reuse:**
```
                                        [humans_path]
                                              ↓
                                    ┌──────────────────┐
                                    │ (primates_path, │
                                    │     humans)     │
                                    └──────────────────┘
                                              ↓
        [chimps_path]                [primates_path]
              ↓                             ↓
    ┌──────────────────┐          ┌──────────────────┐
    │ (primates_path, │          │ (mammals_path,  │
    │  chimpanzees)   │          │    primates)    │
    └──────────────────┘          └──────────────────┘
                                          ↓
    [dogs_path]                    [mammals_path]  ← SHARED!
         ↓                                ↓
┌──────────────────┐             ┌──────────────────┐
│ (mammals_path,  │             │ (animals_path,  │
│     dogs)       │             │    mammals)     │
└──────────────────┘             └──────────────────┘
                                         ↓
    [snakes_path]                 [animals_path]  ← SHARED!
         ↓                               ↓
┌──────────────────┐             ┌──────────────────┐
│ (reptiles_path, │             │ (root, animals) │
│     snakes)     │             └──────────────────┘
└──────────────────┘
         ↓
  [reptiles_path]
         ↓
┌──────────────────┐
│ (animals_path,  │
│   reptiles)     │
└──────────────────┘
```

**Storage comparison:**

Traditional (string-based):
```
String 1: "/animals/mammals/primates/humans"      (32 chars)
String 2: "/animals/mammals/primates/chimpanzees" (39 chars)
String 3: "/animals/mammals/dogs"                 (22 chars)
String 4: "/animals/reptiles/snakes"              (25 chars)
Total: 118 characters

Redundancy:
- "/animals/" appears 4 times
- "/animals/mammals/" appears 3 times
```

Recursive links network:
```
Links created:
1. (root, animals)          = animals_path
2. (animals_path, mammals)  = mammals_path    ← REUSED 3 times!
3. (animals_path, reptiles) = reptiles_path
4. (mammals_path, primates) = primates_path   ← REUSED 2 times!
5. (mammals_path, dogs)     = dogs_path
6. (primates_path, humans)  = humans_path
7. (primates_path, chimps)  = chimps_path
8. (reptiles_path, snakes)  = snakes_path

Total: 8 unique links (plus leaf data: animals, mammals, reptiles, primates, dogs, humans, chimps, snakes)

Key benefit: Common path segments stored ONCE, referenced MANY times
```

## 3. Recursive Path Properties

### 3.1 Self-Referential Paths (Cycles)

**Cyclic structure:**
```
        ┌─────────┐
    ┌───│  Node A │←──┐
    │   └─────────┘   │
    ↓                 │
┌─────────┐      ┌─────────┐
│  Node B │─────→│  Node C │
└─────────┘      └─────────┘

Link definitions:
A = (B, "data_a")
B = (C, "data_b")
C = (A, "data_c")  ← Creates cycle back to A

Path from A: A → B → C → A → B → C → ...
```

**Use case:** Representing circular dependencies, state machines, or infinite sequences.

### 3.2 Multiple Representation of Same Sequence

**Sequence: [A, B, C, D, E, F, G, H]**

**Representation 1: Fully left-heavy (ladder)**
```
A → (B → (C → (D → (E → (F → (G → H))))))

Height: 7
```

**Representation 2: Fully balanced**
```
        ┌──────────────────────┐
        │    Full Tree Root    │
        └──────────────────────┘
              ↙            ↘
    ┌──────────────┐    ┌──────────────┐
    │ ((A,B),(C,D))│    │ ((E,F),(G,H))│
    └──────────────┘    └──────────────┘
        ↙      ↘            ↙      ↘
    ┌──────┐ ┌──────┐  ┌──────┐ ┌──────┐
    │(A,B) │ │(C,D) │  │(E,F) │ │(G,H) │
    └──────┘ └──────┘  └──────┘ └──────┘

Height: 3
```

**Representation 3: Mixed**
```
            Root
           ↙    ↘
    ((A,B),C)    ((D,E),(F,(G,H)))
        ↙  ↘          ↙    ↘
     (A,B)  C      (D,E)  (F,(G,H))
                          ↙    ↘
                         F    (G,H)

Height: 4
```

All three represent the SAME sequence but with different structural properties.

## 4. Practical Example: File System

### 4.1 Directory Structure

**Traditional file system tree:**
```
/
├── home/
│   ├── alice/
│   │   ├── documents/
│   │   │   └── report.pdf
│   │   └── pictures/
│   │       └── photo.jpg
│   └── bob/
│       └── documents/
│           └── notes.txt
└── etc/
    └── config.txt
```

### 4.2 As Recursive Links Network

**Link structure:**
```
root = (system, "/")
home = (root, "home")
alice = (home, "alice")
bob = (home, "bob")
etc = (root, "etc")

alice_docs = (alice, "documents")
alice_pics = (alice, "pictures")
bob_docs = (bob, "documents")

report = (alice_docs, "report.pdf")
photo = (alice_pics, "photo.jpg")
notes = (bob_docs, "notes.txt")
config = (etc, "config.txt")
```

**Graph visualization:**
```
                        [root]
                          │
                ┌─────────┴─────────┐
                ↓                   ↓
              [home]               [etc]
                │                   │
        ┌───────┴────────┐          ↓
        ↓                ↓      [config.txt]
     [alice]           [bob]
        │                │
    ┌───┴───┐            ↓
    ↓       ↓      [bob/documents]
[alice/   [alice/        │
 docs]    pics]          ↓
   │        │       [notes.txt]
   ↓        ↓
[report] [photo]
 .pdf]    .jpg]
```

**Benefits:**
1. Path to report.pdf: `root → home → alice → documents → report.pdf`
2. Path to notes.txt: `root → home → bob → documents → notes.txt`
3. Both share `root → home` (stored once)
4. Can query "all files in home" by traversing from `home` link
5. Can find "all documents folders" by searching for links ending in "documents"

## 5. Compression Through Reuse

### 5.1 Without Reuse (Independent Paths)

**Three paths stored independently:**
```
Path 1: /root/a/b/c    → Links: (root,a), (a,b), (b,c)          = 3 links
Path 2: /root/a/b/d    → Links: (root,a), (a,b), (b,d)          = 3 links
Path 3: /root/a/e      → Links: (root,a), (a,e)                 = 2 links

Total: 8 links (with duplicates!)
```

### 5.2 With Reuse (Shared Network)

**Three paths sharing common prefixes:**
```
Unique links:
1. (root, a)     ← Used by all three paths
2. (a, b)        ← Used by paths 1 and 2
3. (a, e)        ← Used by path 3
4. (b, c)        ← Used by path 1
5. (b, d)        ← Used by path 2

Total: 5 links (3 links saved = 37.5% compression)

Network structure:
            (root,a) ← SHARED
               ↓
         ┌─────┴─────┐
         ↓           ↓
      (a,b)       (a,e)
    ← SHARED
    ↓
 ┌──┴──┐
 ↓     ↓
(b,c) (b,d)
```

**Compression ratio increases with:**
- More paths sharing common prefixes
- Longer common prefixes
- More structured/organized data

## 6. Query Patterns

### 6.1 Find All Descendants

**Query:** "Find all paths under `/animals/mammals/`"

**In recursive links:**
```
1. Find the mammals_path link: (animals_path, mammals)
2. Search for all links where SOURCE = mammals_path
3. Results:
   - (mammals_path, primates) = primates_path
   - (mammals_path, dogs) = dogs_path
4. Recursively repeat for found links to get full subtree
```

### 6.2 Find Common Ancestor

**Query:** "Find common ancestor of 'humans' and 'dogs'"

**In recursive links:**
```
1. Trace humans backwards:
   humans ← primates_path ← mammals_path ← animals_path ← root

2. Trace dogs backwards:
   dogs ← mammals_path ← animals_path ← root

3. Find first common link in both traces:
   mammals_path ← COMMON ANCESTOR

Result: Both are under mammals
```

### 6.3 Find All Paths Containing Element

**Query:** "Find all complete paths containing 'mammals'"

**In recursive links:**
```
1. Find all links that reference mammals:
   - mammals_path = (animals_path, mammals)

2. Find all links that reference mammals_path:
   - primates_path = (mammals_path, primates)
   - dogs_path = (mammals_path, dogs)

3. Continue tracing to leaf nodes:
   - humans_path = (primates_path, humans)
   - chimps_path = (primates_path, chimpanzees)

Results (complete paths):
   - root → animals → mammals → primates → humans
   - root → animals → mammals → primates → chimpanzees
   - root → animals → mammals → dogs
```

## 7. Summary

**Key visual insights:**

1. **Same sequence, different structures**: A sequence can be represented as balanced trees, left-heavy trees, or right-heavy trees - all equivalent in meaning.

2. **Automatic deduplication**: Common sub-paths become shared links in the network, automatically reducing storage.

3. **Graph properties**: What starts as simple paths becomes a directed acyclic graph (DAG) when paths share structure, or a general graph when cycles exist.

4. **Flexibility**: The recursive nature allows representing trees, paths, sequences, and arbitrary graph structures using the same fundamental "doublet link" primitive.

5. **Query power**: Graph traversal operations (forward, backward, searching) enable rich queries on the path structure.

The recursive path representation transforms static hierarchical data into a dynamic, queryable network structure with built-in compression and semantic richness.
