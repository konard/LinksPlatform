# Testing Disk Space Exhaustion (Issue #17)

This document describes how to test the situation when no more memory is available on disk, as requested in [Issue #17](https://github.com/Konard/LinksPlatform/issues/17).

## Overview

The LinksPlatform uses file-mapped memory storage through the `Platform.Data.Doublets` package. When disk space is exhausted during link creation, the system should handle it gracefully.

## Two Possible Strategies

As discussed in the issue, there are two main strategies for handling disk exhaustion:

### Strategy 1: Stop Creation, Allow Update/Delete
- When disk space is full, **creation** operations fail with appropriate exceptions
- **Update** and **Delete** operations continue to work normally
- After deleting links and freeing space, creation should work again

### Strategy 2: Auto-Delete Least Used Links (LRU-like)
- Automatically delete the least-used link when disk is full
- Create the new link in the freed space
- Makes it appear that memory is always available
- Database may "forget" some things (similar to human memory under constraints)

## Testing Approach

### Test Environment Setup

Use a small storage device (e.g., small USB flash drive) or create a small file-mapped storage:

```csharp
const string testFile = "limited-storage-test.links";
const long smallStorageSize = 1024 * 1024; // 1 MB for quick testing

using (var memoryManager = new UInt64UnitedMemoryLinks(testFile, smallStorageSize))
{
    // Test code here
}
```

### Test Steps for Strategy 1

1. **Initialize Storage**: Create a links storage with limited size
2. **Fill Storage**: Create links in a loop until disk space exception occurs
3. **Verify Exception**: Catch and verify the exception is disk-space related:
   - `IOException`
   - `OutOfMemoryException`
   - Exception message contains "out of space", "disk full", etc.
4. **Test Delete**: Verify that DELETE operations still work after disk is full
5. **Test Update**: Verify that UPDATE operations still work after disk is full
6. **Test Create After Delete**: After deleting a link, verify that CREATE works again

### Expected Exceptions

When disk space is exhausted, expect one of these exceptions:
- `System.IO.IOException`
- `System.OutOfMemoryException`
- Exceptions with messages containing:
  - "out of space"
  - "disk full"
  - "not enough space"

The exact exception depends on the underlying Platform.Memory implementation.

## Implementation Location

The disk space handling logic is in the Platform.Memory package:
- `FileMappedResizableDirectMemory` class
- Specifically in the `OnReservedCapacityChanged` method
- When `FileHelpers.SetSize()` tries to expand the file

## Related Files

- **Platform.Memory**: File-mapped memory implementation
  - Repository: https://github.com/linksplatform/Memory
  - Key file: `FileMappedResizableDirectMemory.cs`

- **Platform.Data.Doublets**: Links storage implementation
  - Repository: https://github.com/linksplatform/Data.Doublets
  - Key file: `UnitedMemoryLinks.cs`

## Manual Testing

To manually test with an actual small USB drive:

1. Insert a small USB flash drive (e.g., 64MB or 128MB)
2. Format it with a filesystem
3. Run the links creation test pointing to a file on the USB drive
4. Monitor the behavior when the drive fills up
5. Verify UPDATE and DELETE still work
6. Delete some links and verify CREATE works again

## Automated Testing Considerations

For automated testing:
- Use small in-memory or file-based storage with strict size limits
- Mock the file system to simulate disk full conditions
- Test both exception handling and recovery paths
- Verify data integrity after recovery

## Related Issues

- Issue #213: "The 'whole drive' fixed sized links storage allocation (no file system)"
  - Related to optimizing storage allocation
  - May affect disk space handling strategy

## References

- Original Issue: https://github.com/Konard/LinksPlatform/issues/17
- ACID Milestone: This issue is part of ensuring ACID properties
- Discussion on handling strategies in issue comments
