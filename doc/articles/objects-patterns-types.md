# Objects, Patterns, and Types in LinksPlatform

## Introduction

This document addresses fundamental questions about the conceptual model of LinksPlatform's associative data system:
- What is an object or type?
- Is it a concrete pattern (sequence) or general pattern?
- Can patterns apply to sets?

## Definitions

### Object (Объект)

In the context of LinksPlatform's associative model, an **object** is a concrete instance - a specific arrangement of links that represents a particular piece of data. An object is:

- **Concrete**: A specific link or sequence of links with definite values
- **Instance-based**: Represents actual data stored in the associative memory
- **Identifiable**: Has a unique address/identifier within the link space
- **Static at a given moment**: Represents a fixed state at a point in time

**Example**: The sequence `[з][е][л][ё][н][а][я]` (green) stored as specific links is an **object** - a concrete instance of a word.

### Pattern (Паттерн)

A **pattern** is a general description or template that can match multiple objects. A pattern is:

- **Abstract**: Describes a structure or relationship, not specific data
- **Template-based**: Defines criteria or structure that objects may conform to
- **Matching function**: Can be applied to test whether an object conforms to it
- **Dynamic**: Represents a rule or trigger that can be applied repeatedly

**Example**: "A sequence starting with 'з' and ending with 'я'" is a **pattern** that matches multiple objects including `[з][е][л][ё][н][а][я]`.

According to contributor lubyagin's insight: **pattern = trigger**. This means patterns are active elements that:
- Match against data structures
- Trigger transformations or actions when matched
- Can be represented as sequences of conditions in a programming language

### Type (Тип)

A **type** is a classification or category that groups objects by shared characteristics. A type is:

- **Categorical**: Defines a class or set of objects with common properties
- **Schema-like**: Can be used to enforce structure (in typed systems)
- **Hierarchical**: Can participate in inheritance or containment relationships
- **Semantic**: Provides meaning and interpretation to raw link structures

**Example**: "Color adjective" is a **type** that categorizes words like "green", "red", "blue" as belonging to the same conceptual category.

## Relationships Between Concepts

### Object vs Pattern

```
Pattern (Abstract/General)
    ↓ matches
Object (Concrete/Specific)
```

- **Pattern is to Object** as **Class is to Instance** (in OOP terms)
- **Pattern is to Object** as **Type is to Value** (in functional programming)
- An **object** can match zero, one, or multiple patterns
- A **pattern** can match zero, one, or many objects

### Concrete Pattern vs General Pattern

The question "Is it concrete pattern (sequence) or general pattern?" reveals two interpretations of "pattern":

#### Concrete Pattern (Конкретный паттерн)
- A specific sequence stored as links
- Example: The exact sequence `[а][б][в]` as stored data
- This is actually more of an **object** than a pattern

#### General Pattern (Общий паттерн)
- An abstract matching rule or template
- Example: "Any sequence of three Cyrillic letters"
- This is a true **pattern** in the abstract sense

**Resolution**: When we say "pattern" in LinksPlatform context, we typically mean **general pattern** - the abstract matching/trigger concept. A "concrete pattern" is better called an **object** or **instance**.

### Types and Sets

Based on issue #100 (Sets) and issue #78 (Objects/Type Systems):

A **type** can be represented as a **set** of objects that satisfy certain criteria:

```
Type "Color" = Set { [red], [green], [blue], ... }
```

**Can patterns apply to sets?** **Yes:**

1. **Set Membership Pattern**: Match objects that belong to a set
   - Pattern: "Is this object in the Color set?"
   - Matches: Any object that is a member of the Color set

2. **Set Structure Pattern**: Match sets by their structure
   - Pattern: "Does this set contain exactly 3 elements?"
   - Matches: Any set with cardinality = 3

3. **Set Relationship Pattern**: Match relationships between sets
   - Pattern: "Is set A a subset of set B?"
   - Matches: When A ⊆ B

## Implementation in LinksPlatform

### Static Type System (from issue #78)
- Types defined explicitly through schema links
- Objects must conform to their type definition
- Similar to statically-typed languages (C#, Java)

### Dynamic Type System (from issue #78)
- Types determined by structure and usage
- Objects can change type through transformation
- Similar to dynamically-typed languages (JavaScript, Python)

### Pattern Matching (from issue #110)

Patterns in LinksPlatform can match:
- By Id/Index of one or multiple links
- By graph structure of link
- By tree structure of link
- By sequence structure of link
- By surroundings (parents set, children set)

## Examples in Associative Model

### Example 1: Sequence as Object

```
Object: [H][e][l][l][o]
- Concrete sequence
- Specific links: Link₁→H, Link₂→(Link₁→e), Link₃→(Link₂→l), ...
- This is an object (instance)
```

### Example 2: Pattern for Greetings

```
Pattern: "Sequence starting with greeting word"
- General rule
- Matches: [Hello], [Hi], [Hey], [Привет], ...
- This is a pattern (template/trigger)
```

### Example 3: Type as Set

```
Type "Greeting" = { [Hello], [Hi], [Hey], [Привет], ... }
- Collection of objects
- Can be represented as links: (IsGreeting [Hello]), (IsGreeting [Hi]), ...
- Pattern can check: "Is X in Greeting set?"
```

## Theoretical Foundations

### From Links Theory Document

The links-theory.md document establishes that:

1. **Sequences** are built from doublet links (pairs)
2. **Objects** are concrete arrangements of these pairs
3. **Patterns** emerge as ways to describe and match these arrangements
4. **Types** can be interpreted as sets of links with shared semantics

### Connection to Graph Theory

- **Object**: A specific subgraph instance
- **Pattern**: A graph matching rule or isomorphism condition
- **Type**: A class of isomorphic subgraphs
- **Set**: A collection of graph elements (nodes/edges/subgraphs)

## Conclusion

To answer the original question:

**"What is object or type?"**
- **Object**: A concrete, specific instance of links/data
- **Type**: An abstract category/set grouping objects by shared properties

**"Is it concrete pattern (sequence) or general pattern?"**
- If concrete: It's an **object** (specific sequence)
- If general: It's a **pattern** (matching rule/trigger)
- True "patterns" are general/abstract

**"Can patterns apply to sets?"**
- **Yes**: Patterns can match set membership, set structure, and set relationships
- Sets can be used to define types
- Patterns can check if objects belong to type-sets

## References

- Issue #111: Object or pattern? (this document)
- Issue #78: Objects (static type system & dynamic type system)
- Issue #100: Sets
- Issue #110: Patterns
- doc/articles/links-theory.md: Draft Theory of Links

## Future Work

- Implement pattern matching for set operations
- Define formal grammar for pattern expressions
- Create examples showing object/pattern/type distinctions in code
- Develop trigger system for pattern-based transformations
