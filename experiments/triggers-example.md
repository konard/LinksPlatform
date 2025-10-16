# Triggers Engine Example

This example demonstrates the Triggers Engine implementation for issue #10.

## Overview

The Triggers Engine allows executing code written in links, inspired by Markov algorithms.

## Basic Concepts

### 1. Trigger Pattern Matching

Triggers match link triplets (Source, Linker, Target) and execute actions:

```csharp
var storage = new MockLinkStorage();
var engine = new TriggerEngine(storage);

// Create a trigger that matches specific pattern
var trigger = new MarkovTrigger(
    name: "greeting-trigger",
    patternSource: "User",
    patternLinker: "says",
    patternTarget: "Hello",
    operation: TriggerOperation.Create,
    action: context => {
        // Respond with a greeting link
        context.Storage.CreateLink("Bot", "says", "Hi!");
    }
);

engine.RegisterTrigger(trigger);
```

### 2. Wildcard Patterns

Use wildcards to match any link:

```csharp
var trigger = new MarkovTrigger(
    name: "log-all-creates",
    patternSource: PatternMatcher.Wildcard,  // Match any source
    patternLinker: PatternMatcher.Wildcard,  // Match any linker
    patternTarget: PatternMatcher.Wildcard,  // Match any target
    operation: TriggerOperation.Create,
    action: context => {
        Console.WriteLine($"Created: {context.Source} -> {context.Linker} -> {context.Target}");
    }
);
```

### 3. Markov Algorithm Style Execution

Execute triggers iteratively until no more matches (like Markov algorithms):

```csharp
// Rule 1: Transform A to B
var rule1 = new MarkovTrigger(
    "A-to-B",
    "A", PatternMatcher.Wildcard, PatternMatcher.Wildcard,
    TriggerOperation.Any,
    context => { context.Source = "B"; }
);

// Rule 2: Transform B to C (terminal rule)
var rule2 = new MarkovTrigger(
    "B-to-C",
    "B", PatternMatcher.Wildcard, PatternMatcher.Wildcard,
    TriggerOperation.Any,
    context => { context.Source = "C"; },
    isTerminal: true  // Stop after this rule
);

engine.RegisterTrigger(rule1);
engine.RegisterTrigger(rule2);

// Execute: A -> B -> C
var result = engine.ExecuteMarkovStyle("A", "X", "Y", TriggerOperation.Any);
// result.Source will be "C"
```

### 4. Priority and Terminal Rules

Control execution order with priorities and terminal rules:

```csharp
// High priority trigger (executes first)
var highPriority = new MarkovTrigger(
    "validate",
    PatternMatcher.Wildcard, "delete", PatternMatcher.Wildcard,
    TriggerOperation.Delete,
    context => {
        // Validation logic
        if (ShouldPreventDeletion(context.Source))
        {
            context.Cancel = true;  // Cancel the operation
        }
    },
    priority: 100  // Higher priority
);

// Terminal trigger (stops execution after match)
var terminalTrigger = new MarkovTrigger(
    "security-check",
    PatternMatcher.Wildcard, "admin", PatternMatcher.Wildcard,
    TriggerOperation.Any,
    context => {
        // Security check
        if (!IsAuthorized(context))
        {
            context.Cancel = true;
        }
    },
    isTerminal: true  // No further triggers execute after this
);
```

## Complete Example: Simple Computation System

Here's how to implement a simple computational system using triggers:

```csharp
var storage = new MockLinkStorage();
var engine = new TriggerEngine(storage);

// Define computation rules
// Rule: (add, 0, X) -> X
var addZero = new MarkovTrigger(
    "add-zero",
    "add", "0", PatternMatcher.Wildcard,
    TriggerOperation.Any,
    context => {
        // Transform (add, 0, X) to just X
        context.Source = context.Target;
        context.Linker = null;
        context.Target = null;
    }
);

// Rule: (add, 1, X) -> (successor, X)
var addOne = new MarkovTrigger(
    "add-one",
    "add", "1", PatternMatcher.Wildcard,
    TriggerOperation.Any,
    context => {
        context.Source = "successor";
        context.Linker = context.Target;
        context.Target = null;
    }
);

engine.RegisterTrigger(addZero);
engine.RegisterTrigger(addOne);

// Execute computation
var result = engine.ExecuteMarkovStyle("add", "0", "5", TriggerOperation.Any);
// Result: 5

var result2 = engine.ExecuteMarkovStyle("add", "1", "5", TriggerOperation.Any);
// Result: (successor, 5) = 6
```

## Integration with Links Platform

To integrate with actual Platform.Data.Triplets:

```csharp
// Implement ILinkStorage adapter for Platform.Data.Triplets
public class TripletsLinkStorage : ILinkStorage
{
    private readonly ILinks<ulong> _links;

    public TripletsLinkStorage(ILinks<ulong> links)
    {
        _links = links;
    }

    public object CreateLink(object source, object linker, object target)
    {
        return _links.Create((ulong)source, (ulong)linker, (ulong)target);
    }

    // Implement other methods...
}

// Use with trigger engine
var links = new Links(...);
var storage = new TripletsLinkStorage(links);
var engine = new TriggerEngine(storage);
```

## Use Cases

1. **Validation Rules**: Prevent invalid link operations
2. **Computed Links**: Automatically create derived links
3. **State Machines**: Implement state transitions
4. **Event Handling**: React to link changes
5. **Turing-Complete Computations**: Implement algorithms within the link database

## References

- Issue #10: https://github.com/konard/LinksPlatform/issues/10
- Markov Algorithms: https://en.wikipedia.org/wiki/Markov_algorithm
- Video Reference: http://www.youtube.com/watch?v=gJQTFhkhwPA
