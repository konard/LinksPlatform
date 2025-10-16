# Meta / Semantic Links

## Overview

Meta / Semantic Links provide a minimal set of terms that can construct exact definitions of everything within the Links Platform. This vocabulary enables links to describe themselves and supports fundamental cognitive operations necessary for reasoning and knowledge representation.

**Related Issue**: [#107 - Meta / Semantic Links](https://github.com/konard/LinksPlatform/issues/107)

## Motivation

To fully understand and work with any system, we need:
1. **Analysis**: The ability to decompose/split/decompose structures into details
2. **Synthesis**: The ability to rebuild/reproduce/combine/recreate as a whole
3. **Dynamics**: Understanding what can change and how it impacts other objects
4. **Self-description**: The ability for the system to describe itself using its own primitives

The Links Platform's associative model, based on doublets (pairs), provides the foundation. Semantic Links add the vocabulary needed to express these operations within the system itself.

## Core Semantic Vocabulary

### Structural Terms

The foundation of every link:
- **Source**: The first component of a link
- **Linker**: The relationship/connection component
- **Target**: The second component of a link

### Fundamental Relations

Basic relationships between entities:
- **Is**: Identity relation (a is b)
- **Has**: Possession/property relation (a has b)
- **PartOf**: Composition relation (a is part of b)
- **RelatesTo**: Generic relation

### Meta-Description Terms

Terms for describing descriptions:
- **Describes**: Meta-level description
- **DefinedAs**: Explicit definition marker
- **MeaningSame**: Synonymy relation
- **MeaningOpposite**: Antonymy relation

### Temporal and Dynamic Terms

For modeling change and causality:
- **Causes**: Causal relation
- **Precedes**: Temporal precedence
- **Follows**: Temporal sequence
- **ChangesTo**: State transformation
- **CanChange**: Mutability marker
- **Dimension**: Dimension of change
- **State**: Current state
- **Transition**: State transition

### Cognitive Operations

The fundamental operations for reasoning:

#### Analysis
Decomposition/splitting a whole into parts. Breaking down complex structures to understand their components.

```csharp
var analysisResults = SemanticLinks.AnalyzeLink(someLink);
// Returns: [source analysis, linker analysis, target analysis]
```

#### Synthesis
Combination/building parts into a whole. Creating complex structures from simple components.

```csharp
var synthesized = SemanticLinks.SynthesizeLink(source, linker, target);
// Creates a new link and documents the synthesis operation
```

#### Induction
Bottom-up reasoning: generalizing from specific instances to general patterns/rules.

```csharp
var pattern = SemanticLinks.InducePattern(instances);
// Extracts common pattern from multiple instances
```

#### Deduction
Top-down reasoning: applying general rules to specific cases.

```csharp
var instance = SemanticLinks.DeduceInstance(pattern, context);
// Applies pattern to generate specific instance
```

#### Prognosis
Prediction/consequences: projecting future states from current state and transformations.

```csharp
var futureState = SemanticLinks.PrognoseFuture(currentState, transformations);
// Predicts resulting state
```

### Logical Terms

For expressing logical relationships:
- **And**: Conjunction
- **Or**: Disjunction
- **Not**: Negation
- **Implies**: Logical implication

### Quantification

For expressing scope:
- **All**: Universal quantifier (∀)
- **Some**: Existential quantifier (∃)
- **None**: Negated existence

### Type and Category Terms

For classification:
- **Type**: Type classification
- **Instance**: Instance of a type
- **Property**: Property/attribute
- **Value**: Property value

### Collection Terms

For working with sets and sequences:
- **Set**: Mathematical set
- **Sequence**: Ordered collection
- **Element**: Collection member
- **Contains**: Containment relation

### Abstract vs Concrete

For distinguishing abstraction levels:
- **Concept**: Abstract idea
- **Concrete**: Concrete instance
- **Abstract**: Abstract instance

### Self-Reference Terms

For meta-level operations:
- **Itself**: Self-reference marker
- **Meta**: Meta-level marker
- **Object**: Object-level marker

## Self-Describing Links

One of the key achievements of semantic links is the ability to create self-describing structures. A link can describe its own structure:

```csharp
var selfDesc = SemanticLinks.CreateSelfDescribingLinkDefinition();
// Creates: Link describes (Link has source AND Link has linker AND Link has target)
```

This enables:
- **Introspection**: The system can examine its own structure
- **Meta-programming**: Operations defined within the system can operate on themselves
- **Consistency**: The same vocabulary describes both data and meta-data

## Dynamic Dimensions

As mentioned in issue #107, reality is dynamic - things change. Semantic links support modeling change:

```csharp
var dimension = SemanticLinks.CreateDynamicDimension(entity, characteristic);
// Marks that entity's characteristic can change, creating a dimension
```

Each changeable characteristic creates its own dimension. State transitions can be explicitly modeled:

```csharp
var transition = SemanticLinks.CreateTransition(fromState, toState, cause);
// Documents how and why states change
```

## Usage Example

```csharp
using Platform.Sandbox;

// Initialize the semantic vocabulary
SemanticLinks.Initialize();

// Create a self-describing link
var selfDesc = SemanticLinks.CreateSelfDescribingLinkDefinition();

// Analyze a link (decomposition)
var myLink = Link.Create(source, linker, target);
var parts = SemanticLinks.AnalyzeLink(myLink);

// Synthesize a new link (composition)
var newLink = SemanticLinks.SynthesizeLink(partA, relation, partB);

// Induce a pattern from examples (bottom-up)
var examples = new[] { example1, example2, example3 };
var pattern = SemanticLinks.InducePattern(examples);

// Deduce instance from pattern (top-down)
var instance = SemanticLinks.DeduceInstance(pattern, context);

// Predict future state (prognosis)
var future = SemanticLinks.PrognoseFuture(currentState, transformations);

// Model dynamic characteristics
var colorDimension = SemanticLinks.CreateDynamicDimension(object, color);
var transition = SemanticLinks.CreateTransition(redState, blueState, paintingAction);
```

## Running the Demo

To see semantic links in action:

1. Uncomment the demo line in `Platform.Sandbox/Program.cs`:
```csharp
SemanticLinks.RunDemo();
```

2. Build and run the sandbox project:
```bash
cd Platform
dotnet build
dotnet run --project Platform.Sandbox
```

The demo will showcase:
- Self-describing link definitions
- Analysis (decomposition)
- Synthesis (composition)
- Induction (pattern extraction)
- Deduction (pattern application)
- Prognosis (prediction)
- Dynamic dimensions
- State transitions

## Integration with Links Theory

This implementation aligns with the concepts described in [links-theory.md](links-theory.md):

- **Doublets Model**: All semantic terms are themselves links (doublets)
- **Sequences**: Semantic operations work with sequences built from doublets
- **Self-Reference**: Links can reference themselves and other links
- **Compression**: The vocabulary is minimal yet sufficient for universal description

## Future Extensions

The minimal semantic vocabulary can be extended for specific domains:

- **Spatial terms**: near, far, inside, outside
- **Numerical terms**: greater, less, equal
- **Linguistic terms**: subject, verb, object, adjective
- **Domain-specific vocabularies**: medical, legal, scientific terms

All extensions build on the core vocabulary, maintaining consistency and enabling translation between domains.

## Implementation Notes

**Location**: `Platform/Platform.Sandbox/SemanticLinks.cs`

The implementation:
- Defines 50+ core semantic terms
- Uses the Links Platform's doublet model
- Provides factory methods for common cognitive operations
- Includes comprehensive documentation and examples
- Demonstrates self-describing capabilities

**Design Principles**:
1. **Minimalism**: Include only essential terms
2. **Composability**: Complex meanings from simple combinations
3. **Self-consistency**: Same primitives for data and meta-data
4. **Extensibility**: Foundation for domain-specific vocabularies

## References

- [Issue #107: Meta / Semantic Links](https://github.com/konard/LinksPlatform/issues/107)
- [Links Theory](links-theory.md)
- [Links Platform Documentation](https://linksplatform.github.io/)
- Implementation: `Platform/Platform.Sandbox/SemanticLinks.cs`

---

*This document describes the minimal semantic vocabulary for the Links Platform, enabling universal description and cognitive operations within an associative data model.*
