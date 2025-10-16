# Links Platform Terminology

This document clarifies the various ways to interpret link structures in the Links Platform, as these interpretations can be context-dependent and may coexist in different use cases.

## Link Structure Interpretations

At the most fundamental level, there are several ways to interpret link structures. The choice of interpretation depends on the specific application and use case.

### 1. Link as a State

When interpreting a link as a state, there are multiple sub-interpretations:

#### a) Past-to-Present State
- **Source**: Reference to all previous (past) states
- **Target**: Current or final state
- **Use case**: Tracking state evolution where the link represents a cumulative history

#### b) Present-to-Future State
- **Source**: Current or first state
- **Target**: Reference to all next (future) states
- **Use case**: Planning or prediction systems where the link represents potential futures

#### c) Sequence of States
- A sequence of states with one designated as the current state
- **Use case**: State machines, temporal databases

### 2. Link as a Pure Connection

A link is never a state itself, but rather a pure fact of connection between objects in a concrete direction (from Source to Target).

- **Characteristics**:
  - Directional relationship
  - No inherent state information
  - Simply denotes connectivity
- **Use case**: Graph databases, knowledge graphs, semantic networks

### 3. Link as a Transition

A link represents a transition or transformation between a Source state and a Target state.

- **Characteristics**:
  - Implies a process or operation
  - Source is the pre-transition state
  - Target is the post-transition state
- **Use case**: Workflow systems, process modeling, finite state automata

### 4. Link as a Point

#### a) Full Point
- All references are self-references
- **Source**: Points to itself
- **Target**: Points to itself
- **Use case**: Representing singletons, markers, or special constants

#### b) Partial Point
- Not all references are self-references, but at least one is
- **Example**: Source points to itself, Target points to another link
- **Use case**: Reflexive relationships, circular dependencies

### 5. Link as a Pair in Sequence

A link is viewed as a pair that is itself a subsequence made of two other subsequences, connecting a left subsequence and a right subsequence.

- **Characteristics**:
  - Recursive structure
  - Can represent binary trees
  - Left subsequence: Source
  - Right subsequence: Target
- **Use case**: Representing sequences, lists, or trees using binary links

## Reference/Usage Interpretations

The Links Platform stores information not only about links themselves but also about referrers (usages) of these links. There are multiple ways to interpret these relationships:

### 1. Target Usage as Parent

When one link uses another as a Target, it can be interpreted as one of the Parents.

- **Rationale**: The target represents what the link is "derived from" or "pointing towards"
- **Use case**: Inheritance hierarchies, taxonomies

### 2. Source Usage as Child

When one link uses another as a Source, it can be interpreted as one of the Children.

- **Rationale**: The source represents what "comes from" or "originates from" the link
- **Use case**: Tree structures, organizational hierarchies

### 3. All Usages as References

All usages are just usages (references) and have nothing in common with interpretation variants. All usages are simply children without additional semantic meaning.

- **Rationale**: Neutral interpretation focusing on the fact of usage rather than implied hierarchy
- **Use case**: General-purpose data storage, avoiding semantic assumptions

## Applicability

These interpretations apply to:

### Binary Links (Doublets)
- Structure: (Source, Target)
- Also called: Pairs, Doublets
- Examples in codebase: `Platform.Data.Doublets`

### Triple Links (Triplets)
- Structure: (Source, Linker, Target)
- Also called: Triplets, Hyper-edges
- The Linker provides additional context or type information for the relationship
- Examples in codebase: `Platform.Data.Triplets`

## Pure Link Concept

A link can be considered a "pure link" only when:
1. It connects two other links (not itself)
2. It has no usages at all

However, even this definition allows for multiple interpretations depending on context.

## Towards a Universal Interpretation System

**Open Question**: Is it possible to create a single interpretation system that can still be used universally?

The answer may lie in:
1. **Context-dependent interpretation**: Different interpretations for different domains
2. **Layered semantics**: Base layer provides pure structural connections, upper layers add semantic interpretations
3. **Interpretation markers**: Using special links or metadata to indicate which interpretation applies
4. **Multiple simultaneous interpretations**: Allowing the same link structure to be viewed through different interpretive lenses

## Recommendations for Implementation

When working with Links Platform:

1. **Be explicit about interpretation**: Document which interpretation you're using in your context
2. **Consider context**: Choose the interpretation that best fits your use case
3. **Allow flexibility**: Design systems that can accommodate multiple interpretations when needed
4. **Use type markers**: Consider using special "type" links to indicate interpretation intent
5. **Maintain consistency**: Within a single subsystem or module, try to maintain a consistent interpretation

## Related Concepts

- **Point-Pair Contradiction**: See issue #96 for discussion of contradictions that can arise
- **Links Theory**: See `doc/articles/links-theory.md` (Russian) for theoretical foundations
- **Sequences**: The concept of representing sequences using binary links connects to interpretation #5

## Examples from Codebase

### Legacy Implementation (Triple Links)
The early implementation in `28.03.2010-04.11.2010/Net/Net/Link.cs` demonstrates triple links with:
- `Source`: First reference
- `Linker`: Middle reference (type/relationship indicator)
- `Target`: Second reference

### Modern Implementation (Doublets)
Current implementation focuses on doublets (binary links) for efficiency while maintaining expressiveness.

## Conclusion

The Links Platform intentionally allows for multiple interpretations of link structures. This flexibility enables the same fundamental data structure to serve various purposes across different domains. The key is to be explicit about which interpretation you're using and to remain consistent within your specific context.

As the platform evolves, new interpretations may emerge, and existing ones may be refined based on practical experience and community feedback.
