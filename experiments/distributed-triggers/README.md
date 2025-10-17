# Distributed Triggers Execution Process

This directory contains the implementation of a distributed triggers execution system for LinksPlatform, implementing the design described in [Issue #653](https://github.com/konard/LinksPlatform/issues/653).

## Overview

Triggers are transformations on the transaction log. This implementation allows multiple workers to reconstruct the full links store (database) and handle different triggers in parallel, enabling infinite parallelization of trigger execution.

## Architecture

The system implements a **map-reduce** pattern:

1. **Transaction Log Broadcasting**: Events are broadcast to all workers
2. **Snapshot Creation**: After each event, a snapshot of the data is distributed to all workers
3. **Parallel Execution (Map)**: Each worker handles a subset of triggers based on partition keys
4. **Result Merging (Reduce)**: Results from all workers are merged back to the transaction log

## Core Components

### 1. IDistributedTrigger<TLinkAddress>
Interface for implementing distributed triggers. Each trigger has:
- **TriggerId**: Unique identifier
- **PartitionKey**: Determines which worker handles this trigger
- **Execute()**: Performs the trigger logic
- **ShouldExecute()**: Determines if trigger should run for an event

### 2. TransactionLogBroadcaster<TLinkAddress>
Broadcasts transaction events to all subscribed workers in parallel.

### 3. SnapshotManager<TLinkAddress>
Manages snapshots of the links store state. Creates new snapshots after each transaction event by applying operations to the current state.

### 4. TriggerWorker<TLinkAddress>
Represents a worker that processes a subset of triggers. Workers:
- Subscribe to transaction events
- Execute their registered triggers
- Send results to the result collector

### 5. ResultsMerger<TLinkAddress>
Implements the reduce phase:
- Collects results from all workers
- Resolves conflicts when multiple workers produce operations for the same link
- Writes merged results back to the transaction log

### 6. DistributedTriggersCoordinator<TLinkAddress>
Orchestrates the entire workflow:
- Manages workers
- Handles trigger registration/unregistration
- Coordinates transaction event processing
- Maintains system statistics

## Key Features

- **Infinite Parallelization**: Triggers can be distributed across unlimited workers
- **Partition-Based Distribution**: Triggers with the same partition key are handled by the same worker
- **Snapshot Consistency**: All workers receive consistent snapshots
- **Conflict Resolution**: Automatic merging of results with configurable conflict resolution
- **Map-Reduce Pattern**: Clean separation between map (trigger execution) and reduce (result merging) phases

## Data Flow

```
Transaction Event
    ↓
Snapshot Creation
    ↓
Broadcast to Workers (parallel)
    ↓
Workers Execute Triggers (parallel - MAP phase)
    ↓
Collect Results
    ↓
Merge Results (REDUCE phase)
    ↓
Write to Transaction Log
```

## Usage Example

See `/examples/distributed-triggers/DistributedTriggersExample.cs` for a complete working example.

```csharp
// Initialize coordinator with 4 workers
var coordinator = new DistributedTriggersCoordinator<ulong>(4, transactionLogWriter);

// Register triggers (automatically distributed to workers by partition key)
coordinator.RegisterTrigger(new SimpleLoggingTrigger<ulong>(partitionKey: 0));
coordinator.RegisterTrigger(new LinkCounterTrigger<ulong>(partitionKey: 1));

// Process transaction events
await coordinator.ProcessTransactionEventAsync(transactionEvent);
```

## Extending the System

### Creating Custom Triggers

Implement `IDistributedTrigger<TLinkAddress>`:

```csharp
public class MyCustomTrigger : IDistributedTrigger<ulong>
{
    public Guid TriggerId => _triggerId;
    public int PartitionKey => _partitionKey;

    public IEnumerable<TransactionOperation<ulong>> Execute(
        TransactionEvent<ulong> @event,
        IReadOnlyDictionary<ulong, Link<ulong>> snapshot)
    {
        // Your trigger logic here
        // Return new operations to be written to transaction log
    }

    public bool ShouldExecute(TransactionEvent<ulong> @event)
    {
        // Return true if this trigger should process this event
    }
}
```

### Custom Conflict Resolution

Extend `ResultsMerger<TLinkAddress>` and override `ResolveConflicts()` to implement custom merge strategies.

## Performance Considerations

- Workers execute triggers in parallel, limited only by available compute resources
- Partition keys should be distributed evenly for optimal load balancing
- Snapshot creation is optimized with copy-on-write semantics
- Broadcasting uses async/await for efficient I/O

## Future Enhancements

- Distributed workers across multiple machines
- Persistent transaction log implementation
- Historical snapshot storage for time-travel queries
- Priority-based trigger execution
- Dynamic worker scaling based on load

## References

- Issue: https://github.com/konard/LinksPlatform/issues/653
- Map-Reduce: https://en.wikipedia.org/wiki/MapReduce
- Platform.Data.Doublets: https://linksplatform.github.io/Data.Doublets
