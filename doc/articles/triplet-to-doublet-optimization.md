# Multiple Types with Doublets: Cost Optimization

## Overview

This document describes an optimization pattern for representing multiple typed relationships between the same source and target using doublets (pairs) instead of triplets, resulting in significant space savings.

## The Pattern

### Basic Triplet Representation

A single triplet `(a b c)` where:
- `a` is the source
- `b` is the type/predicate
- `c` is the target

Can be represented as 2 doublets:
- `(a c)` - the basic relationship
- `((a b) c)` - the typed relationship

**Cost: 1 triplet = 2 doublets**

### Optimization for Multiple Types

However, when you have **multiple types between the same source and target**, the cost savings become apparent.

#### Example with 2 Types

Two triplets with different types:
```
(a t1 b)  - "a relates to b via type t1"
(a t2 b)  - "a relates to b via type t2"
```

Traditional representation: **2 triplets = 4 doublets**
- `(a b)` + `((a t1) b)` = 2 doublets for first triplet
- `(a b)` + `((a t2) b)` = 2 doublets for second triplet

Optimized representation: **3 doublets**
```
(a b)        - shared base relationship
((a b) t1)   - first type annotation
((a b) t2)   - second type annotation
```

**Space saved: 1 doublet (25% reduction)**

The key insight is that `(a b)` is **shared** across all type annotations, so it only needs to be stored once.

#### Example with 3 Types

Three triplets with different types:
```
(a t1 b)
(a t2 b)
(a t3 b)
```

Traditional representation: **3 triplets = 6 doublets**
Optimized representation: **4 doublets**
```
(a b)        - shared base relationship
((a b) t1)   - first type annotation
((a b) t2)   - second type annotation
((a b) t3)   - third type annotation
```

**Space saved: 2 doublets (33% reduction)**

## General Formula

For `n` triplets with the same source and target but different types:

- **Traditional cost**: `n × 2` doublets
- **Optimized cost**: `n + 1` doublets
- **Space saved**: `n - 1` doublets
- **Efficiency gain**: `(n - 1) / (2n)` = approaches 50% as n increases

### Space Savings Table

| Triplets (n) | Traditional | Optimized | Saved | Efficiency |
|--------------|-------------|-----------|-------|------------|
| 1            | 2           | 2         | 0     | 0%         |
| 2            | 4           | 3         | 1     | 25%        |
| 3            | 6           | 4         | 2     | 33%        |
| 4            | 8           | 5         | 3     | 37.5%      |
| 5            | 10          | 6         | 4     | 40%        |
| 10           | 20          | 11        | 9     | 45%        |
| 100          | 200         | 101       | 99    | 49.5%      |

## Implementation Considerations

### When to Use This Pattern

This optimization is most beneficial when:

1. **Multiple types exist** between the same source and target
2. **Type information is important** but not always needed in queries
3. **Storage efficiency** is a priority
4. The system supports **efficient queries on doublets**

### Data Structure

In a doublets-based system, the structure would be:

```
Link Structure:
- (a b): ulong -> (Source: a_id, Target: b_id)
- ((a b) t1): ulong -> (Source: link_id_of_(a,b), Target: t1_id)
- ((a b) t2): ulong -> (Source: link_id_of_(a,b), Target: t2_id)
```

### Query Patterns

**To find all types between a and b:**
1. Find link `(a b)`
2. Query all links where Source = `link_id_of_(a,b)`
3. The targets of those links are the types

**To check if specific type exists:**
1. Find link `(a b)`
2. Query for link `(link_id_of_(a,b), type_id)`

## Real-World Applications

### Knowledge Graphs

Modeling entities with multiple relationship types:
```
(Person1 "knows" Person2)
(Person1 "works_with" Person2)
(Person1 "lives_near" Person2)
```

Optimized as:
```
(Person1 Person2)
((Person1 Person2) "knows")
((Person1 Person2) "works_with")
((Person1 Person2) "lives_near")
```

### Type Systems

Multiple type annotations on the same relationship:
```
(Variable Type1)
(Variable Type2)  // union type
(Variable Type3)
```

### Metadata Annotations

Multiple metadata tags on relationships:
```
(Entity1 Entity2)
((Entity1 Entity2) "verified")
((Entity1 Entity2) "timestamp:2024")
((Entity1 Entity2) "confidence:0.95")
```

## Comparison with Triplet Stores

Traditional RDF/triplet stores maintain:
- `<subject, predicate, object>` tuples
- Each tuple stored separately
- Requires 3 × n storage units for n triplets

Doublet-based approach with this optimization:
- Base relationship stored once
- Type annotations reference the base relationship
- Requires n + 1 storage units for n triplets with same source/target

## Conclusion

The multiple-types-with-doublets pattern provides:
- **Linear scaling**: O(n+1) instead of O(2n) for n types
- **Approaches 50% efficiency** as the number of types grows
- **Maintains query flexibility** through link references
- **Practical space savings** in real-world scenarios with multiple relationship types

This optimization is particularly powerful in systems where:
- Relationships naturally have multiple types or aspects
- Storage efficiency directly impacts performance
- The doublets model is already being used for other benefits

## References

- Issue #588: [Multiple types with doublets at a fraction of the cost of triplet](https://github.com/konard/LinksPlatform/issues/588)
- [LinksPlatform Documentation](https://github.com/konard/LinksPlatform)
- [Data.Doublets Implementation](https://github.com/linksplatform/Data.Doublets)
