# Doublets Consistency Checker & Recovery Tool

This tool implements consistency checking and recovery functionality for Platform.Data.Doublets databases, as requested in [issue #39](https://github.com/konard/LinksPlatform/issues/39).

## Features

### Consistency Check
- **Link Address Validity**: Verifies that each link's address matches its stored index
- **Link References**: Validates that source and target references are valid
- **Header Consistency**: Checks that link counts in the header match actual link counts
- **Dangling References**: Detects references to non-existent links

### Recovery Operations

#### Full Recovery (Slow but Thorough)
Implements the full recovery approach suggested by konard:
1. Validate file structure
2. Collect valid links
3. Verify link structure
4. Validate counters

This approach is recommended for severe database corruption.

#### Partial Recovery (Fast)
Implements the fast recovery approach:
1. Clean incomplete links
2. Validate index structures
3. Transaction log replay (placeholder for future implementation)

This approach is suitable for minor inconsistencies.

## Usage

### As a Library

```csharp
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Examples;

// Open your database
using (var links = new UnitedMemoryLinks<ulong>("db.links"))
{
    var checker = new DoubletsConsistencyChecker<ulong>(links);

    // Check consistency
    bool isConsistent = checker.CheckConsistency();
    checker.DisplayResults();

    // Perform recovery if needed
    if (!isConsistent)
    {
        // Try partial recovery first (fast)
        checker.PerformPartialRecovery();

        // Or use full recovery for severe issues
        // checker.PerformFullRecovery(createBackup: true);
    }
}
```

### As a Command-Line Tool

```bash
# Check consistency
DoubletsConsistencyChecker db.links check

# Perform partial recovery
DoubletsConsistencyChecker db.links partial-recovery

# Perform full recovery
DoubletsConsistencyChecker db.links full-recovery

# Perform full recovery without backup prompt
DoubletsConsistencyChecker db.links full-recovery --no-backup
```

## Files

- **DoubletsConsistencyChecker.cs**: Core consistency checking and recovery logic
- **DoubletsConsistencyCheckerCLI.cs**: Command-line interface
- **consistency-checker-example.cs**: Usage examples

## Implementation Notes

### Transaction Log Support
The current implementation includes a placeholder for transaction log replay. Future enhancements could include:
- Recording all operations to a transaction log
- Replaying uncommitted operations during recovery
- Using write-ahead logging for better crash recovery

### Index Rebuilding
Full index rebuilding would require deeper integration with UnitedMemoryLinks internals. The current implementation validates index consistency but doesn't perform full index reconstruction.

### Backup Recommendations
Before performing recovery operations, especially full recovery:
1. Stop all applications using the database
2. Create a backup copy of the database file
3. Test recovery on the backup first if possible

## Testing

See `experiments/consistency-checker-example.cs` for a complete working example that:
1. Creates a test database
2. Checks its consistency
3. Demonstrates recovery operations

## Future Enhancements

1. **Transaction Log**: Implement persistent transaction logging for better recovery
2. **Index Rebuilding**: Deep integration for full index reconstruction
3. **Concurrent Access**: Handle consistency checking on active databases
4. **Repair Options**: More granular control over what to repair
5. **Progress Reporting**: Detailed progress for long-running operations
6. **Backup Integration**: Automatic backup before recovery operations

## Related Issues

- [Issue #39](https://github.com/konard/LinksPlatform/issues/39): Consistency (Check / Recover)

## References

- [Platform.Data.Doublets](https://github.com/linksplatform/Data.Doublets)
- [Examples.Doublets.CRUD](https://github.com/linksplatform/Examples.Doublets.CRUD)
