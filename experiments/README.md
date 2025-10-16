# Evented IO Style API for Links - Implementation Experiments

This directory contains experimental implementation of an **Evented IO style API** for Links CRUD operations, addressing issue #19.

## Overview

The implementation provides:
- **Event-driven architecture** with native triggers for CRUD operations
- **Operation queue** for batching and ordering operations
- **Transaction support** with consistency and conflict checking
- **Before/After event hooks** for all CRUD operations
- **Validation and conflict detection** before commit

## Files

### `EventedLinksAPI.cs`
Core implementation containing:
- `ILinksEvented<TLink>` - Interface defining the evented API
- `LinksEvented<TLink>` - Concrete implementation
- `LinkOperation<TLink>` - Represents a single operation/transition
- `LinkTransaction<TLink>` - Represents a transaction containing multiple operations
- `LinkEventArgs<TLink>` - Event arguments for CRUD operations
- `OperationType` - Enum for CRUD operation types

### `EventedLinksExample.cs`
Comprehensive examples demonstrating:
- Basic CRUD operations with event handlers
- Transaction usage with conflict detection
- Custom validation triggers and business rules
- Advanced patterns (logging, caching, consistency enforcement)

### `EventedLinksTests.cs`
Unit tests covering:
- Operation queuing for all CRUD operations
- Event firing (Before/After) for each operation type
- Operation validation
- Conflict detection
- Transaction management (Begin, Commit, Rollback)
- Consistency checking

## Key Features

### 1. Operation Queue
Operations are queued before execution, allowing for:
- Batching multiple operations
- Conflict detection before execution
- Consistency validation
- Transaction grouping

```csharp
var links = new LinksEvented<long>();
var linkId = links.CreateAsync(1, 2);  // Queued, not executed
links.ProcessQueue();                   // Execute all queued operations
```

### 2. Native Triggers (Event Hooks)
All CRUD operations fire Before/After events:

```csharp
links.BeforeCreate += (sender, args) => {
    // Validate, log, or cancel operation
    if (SomeCondition) args.Cancel = true;
};

links.AfterCreate += (sender, args) => {
    // Update cache, notify observers, etc.
};
```

Supported events:
- `BeforeCreate` / `AfterCreate`
- `BeforeRead` / `AfterRead`
- `BeforeUpdate` / `AfterUpdate`
- `BeforeDelete` / `AfterDelete`
- `OnConflict` - Fired when operation conflicts detected
- `OnValidationError` - Fired when validation fails

### 3. Transactions
Group multiple operations with ACID-like properties:

```csharp
var transaction = links.BeginTransaction();
transaction.Operations.Add(createOp);
transaction.Operations.Add(updateOp);

// Checks consistency before executing
links.CommitTransaction(transaction.TransactionId);

// Or rollback
links.RollbackTransaction(transaction.TransactionId);
```

### 4. Conflict Detection
Automatically detects conflicting operations:
- Update after Delete
- Delete after Delete
- Operations on same link

```csharp
links.DeleteAsync(100);
links.UpdateAsync(100, 1, 2);  // Conflict detected!
// OnConflict event fires, operation can be cancelled
```

### 5. Consistency Checking
Validates operation sequences before commit:
- Cannot update/delete non-existent links
- Cannot create duplicate links (if enforced)
- Operations must be in valid order

```csharp
var ops = new[] {
    deleteOp,
    updateOp  // Same link - inconsistent!
};
bool isConsistent = links.CheckConsistency(ops, out var error);
```

## Usage Examples

### Basic CRUD
```csharp
var links = new LinksEvented<long>();

// Setup triggers
links.AfterCreate += (s, e) => Console.WriteLine($"Created: {e.Operation.LinkId}");

// Queue operations
var id = links.CreateAsync(source: 1, target: 2);
links.ReadAsync(id);
links.UpdateAsync(id, newSource: 3, newTarget: 4);
links.DeleteAsync(id);

// Execute
links.ProcessQueue();
```

### Transaction with Validation
```csharp
var transaction = links.BeginTransaction();

// Add operations
var op1 = new LinkOperation<long> {
    Type = OperationType.Create,
    Source = 10, Target = 20, LinkId = 100
};
transaction.Operations.Add(op1);

var op2 = new LinkOperation<long> {
    Type = OperationType.Update,
    LinkId = 100, NewSource = 11, NewTarget = 21
};
transaction.Operations.Add(op2);

// Commit with automatic consistency checking
try {
    links.CommitTransaction(transaction.TransactionId);
} catch (InvalidOperationException ex) {
    Console.WriteLine($"Transaction failed: {ex.Message}");
    links.RollbackTransaction(transaction.TransactionId);
}
```

### Custom Business Rules
```csharp
links.BeforeCreate += (sender, args) => {
    // Enforce: Source must be less than Target
    if (args.Operation.Source >= args.Operation.Target) {
        args.Cancel = true;
        Console.WriteLine("Business rule violation!");
    }
};
```

## Running Examples

To run the examples:
```bash
cd experiments
dotnet run --project EventedLinksExample.cs
```

## Running Tests

To run the unit tests:
```bash
cd experiments
dotnet test EventedLinksTests.cs
```

## Design Decisions

### Why Operation Queue?
- Allows batching for performance
- Enables conflict detection before execution
- Provides atomic transaction support
- Facilitates validation and consistency checking

### Why Generic TLink?
- Matches existing `ILinks<TLink>` pattern in Platform.Data.Doublets
- Supports different link ID types (long, ulong, int, etc.)
- Maintains type safety

### Why Before/After Events?
- Before: validation, cancellation, business rules
- After: logging, caching, notifications, side effects
- Industry standard pattern (EF Core, many ORMs)

### Why Separate Validation and Conflict Checking?
- Validation: checks single operation integrity
- Conflicts: checks against other pending operations
- Different concerns, different handlers

## Future Enhancements

Potential improvements:
1. **Persistence**: Save operation queue to storage
2. **Distributed**: Support for distributed transactions
3. **Performance**: Parallel operation execution
4. **Priority Queue**: Priority-based operation ordering
5. **Undo/Redo**: Operation history and reversal
6. **Integration**: Wrapper for actual ILinks<TLink> implementations
7. **Async/Await**: True async operation processing
8. **Dead Letter Queue**: Failed operation handling

## Integration with Existing Code

This implementation is designed as a **wrapper** around existing `ILinks<TLink>` implementations:

```csharp
// Future integration pattern
public class LinksEventedWrapper<TLink> : ILinksEvented<TLink> {
    private readonly ILinks<TLink> _innerLinks;

    public LinksEventedWrapper(ILinks<TLink> innerLinks) {
        _innerLinks = innerLinks;
    }

    // Wrap actual ILinks operations with eventing
}
```

## References

- Issue: [#19 - Test out Evented IO style for API of Links](https://github.com/konard/LinksPlatform/issues/19)
- Related concepts:
  - CRUD operations (Create, Read, Update, Delete)
  - Event-driven architecture
  - Transaction processing
  - Persistent data structures
  - Operation queues

## License

Same as parent repository (LinksPlatform).
