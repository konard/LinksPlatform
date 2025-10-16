# Transaction Log Snapshot Feature Proposal

## Issue Reference
This document addresses issue [#46](https://github.com/konard/LinksPlatform/issues/46): "Allow to push Links Data Snapshot to Transaction Log"

## Problem Statement

Currently, the `LoggingDecorator` in `Platform.Data.Doublets` only logs **new** changes to the database (Create, Update, Delete operations). When transaction logging is enabled on an existing database that already contains data, the log only captures future changes.

This creates a problem: if you need to restore the database from the transaction log, you cannot fully reconstruct it because the initial state (all links that existed before logging was enabled) is not in the log.

## Proposed Solution

Add a `PushSnapshot()` method to the `LoggingDecorator` class that writes all existing links to the transaction log as Create operations. This creates a "snapshot" of the database at the moment logging is enabled.

### Implementation

The feature should be implemented in the `linksplatform/Data.Doublets` repository by adding a new method to the `LoggingDecorator<TLinkAddress>` class:

```csharp
/// <summary>
/// Pushes a snapshot of all existing links to the transaction log.
/// This allows restoring the database to its current state from the log.
/// Выгружает снимок всех существующих связей в лог транзакций.
/// Это позволяет восстановить базу данных в её текущем состоянии из лога.
/// </summary>
/// <returns>The number of links written to the snapshot.</returns>
public TLinkAddress PushSnapshot()
{
    var count = TLinkAddress.Zero;

    // Write snapshot header to log
    _logStreamWriter.WriteLine("# Snapshot Start");

    // Iterate through all existing links
    _links.Each(null, link =>
    {
        // Skip the null/void link (index 0)
        if (link[_constants.IndexPart] != _constants.Null)
        {
            // Write each link as a Create operation
            _logStreamWriter.WriteLine($"Create. Before: {_constants.Null}: {_constants.Null}->{_constants.Null}. After: {new Link<TLinkAddress>(values: link)}");
            count++;
        }
        return _constants.Continue;
    });

    // Write snapshot footer to log
    _logStreamWriter.WriteLine("# Snapshot End");
    _logStreamWriter.Flush();

    return count;
}
```

### Usage Example

```csharp
// Create a database and populate it with data
using (var memoryAdapter = new UnitedMemoryLinks<ulong>("db.links"))
{
    var links = memoryAdapter;
    var link1 = links.Create();
    var link2 = links.Create();
    var link3 = links.Create(link1, link2);
}

// Later, enable transaction logging and push snapshot
using (var memoryAdapter = new UnitedMemoryLinks<ulong>("db.links"))
using (var logStream = new FileStream("transaction.log", FileMode.Create))
{
    var loggingDecorator = new LoggingDecorator<ulong>(memoryAdapter, logStream);

    // Push snapshot of all existing links
    var snapshotCount = loggingDecorator.PushSnapshot();
    Console.WriteLine($"Pushed {snapshotCount} links to transaction log");

    // From now on, all changes will be logged automatically
    var newLink = loggingDecorator.Create();
}
```

## Benefits

1. **Full Database Recovery**: The transaction log can be used to fully restore a database, including its initial state
2. **ACID Compliance**: Supports the ACID milestone by enabling complete transaction logging
3. **Flexible Deployment**: Allows enabling transaction logging on existing databases without losing data
4. **Simple API**: Single method call to push the snapshot

## Implementation Plan

1. Add `PushSnapshot()` method to `LoggingDecorator` class in `linksplatform/Data.Doublets`
2. Add unit tests to verify snapshot functionality
3. Update documentation with usage examples
4. Release new version of `Platform.Data.Doublets` package
5. Update `konard/LinksPlatform` repository to use new package version

## Related Files

- `linksplatform/Data.Doublets/csharp/Platform.Data.Doublets/Decorators/LoggingDecorator.cs`
- Current version in use: `Platform.Data.Doublets 0.6.10`
- LoggingDecorator was added in: commit `8a3544d` (2023-01-07)

## References

- Issue: https://github.com/konard/LinksPlatform/issues/46
- Milestone: ACID
- Related Package: Platform.Data.Doublets
- Repository: https://github.com/linksplatform/Data.Doublets
