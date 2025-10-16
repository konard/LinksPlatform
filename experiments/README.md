# Managed Memory Implementation for Links Platform

This directory contains experimental implementations for issue #199: "Try Array/List memory, that uses only managed memory".

## Problem Statement

The current Links Platform implementation uses unmanaged memory (via `Marshal.AllocHGlobal`) which may not work in restricted .NET environments such as:
- Certain cloud platforms with security restrictions
- Sandboxed environments
- WebAssembly (Blazor)
- Some mobile platforms with strict memory policies

## Solution

Implement memory adapters that use **only managed memory** by:
1. Allocating regular .NET arrays (managed memory)
2. Pinning them using `GCHandle.Alloc(array, GCHandleType.Pinned)`
3. Exposing the pinned array through the `IDirectMemory` interface

## Implementations

### 1. PinnedArrayMemory<TElement>

A simple, non-resizable pinned array memory implementation.

**Features:**
- Uses only managed memory (standard .NET arrays)
- Pins the array for the lifetime of the object
- Implements both `IDirectMemory` and `IArrayMemory<TElement>`
- Suitable for fixed-size memory allocations

**Usage:**
```csharp
using var memory = new PinnedArrayMemory<byte>(1024 * 1024); // 1 MB
// Use with Links as you would use HeapResizableDirectMemory
```

### 2. PinnedResizableArrayMemory<TElement>

A resizable pinned array memory implementation.

**Features:**
- Supports dynamic resizing (by allocating a new array and copying)
- Implements `IResizableDirectMemory` and `IArrayMemory<TElement>`
- Compatible with `UInt64UnitedMemoryLinks` and other resizable memory consumers
- Less efficient than unmanaged reallocation, but works in restricted environments

**Usage:**
```csharp
using var memory = new PinnedResizableArrayMemory<byte>(4 * 1024 * 1024); // 4 MB initial
// Memory will automatically resize if needed
```

## How It Works

### Array Pinning with GCHandle

When you create a `GCHandle` with `GCHandleType.Pinned`:
1. The garbage collector is instructed not to move the array in memory
2. The array's address becomes stable and can be used as a pointer
3. This pointer remains valid until the GCHandle is freed
4. The array is still managed memory - no unmanaged allocation occurs

### Key Differences from Unmanaged Memory

| Aspect | Unmanaged (HeapResizableDirectMemory) | Managed (PinnedArrayMemory) |
|--------|--------------------------------------|----------------------------|
| Memory Source | OS heap via Marshal.AllocHGlobal | .NET managed heap |
| Resizing | Marshal.ReAllocHGlobal (efficient) | Allocate new array + copy (slower) |
| GC Interaction | None | Pinned (prevents movement) |
| Restrictions | May not work in sandboxed envs | Works in all .NET environments |
| Performance | Slightly faster for large resizes | Slightly slower, but negligible for most uses |

## Testing

The `PinnedArrayMemoryExample.cs` demonstrates how to use these implementations with the Links platform:

```csharp
using var memory = new PinnedArrayMemory<byte>(totalElements * sizeof(ulong));
using var memoryManager = new UInt64UnitedMemoryLinks(memory);
using var links = new UInt64Links(memoryManager);

// Create and use links normally
var point1 = links.CreatePoint();
var point2 = links.CreatePoint();
var link = links.CreateAndUpdate(point1, point2);
```

## Trade-offs

### Advantages ✓
- Works in ANY .NET environment (no platform restrictions)
- No unmanaged code or unsafe allocations
- Still provides pointer access when needed
- Fully compatible with existing Links APIs

### Disadvantages ✗
- Resizing is slower (requires array copy instead of realloc)
- Pinned objects can fragment the managed heap if many are allocated
- Slight overhead from GC tracking the pinned object

## Recommendations

1. **For restricted environments**: Use `PinnedArrayMemory` or `PinnedResizableArrayMemory`
2. **For maximum performance**: Use `HeapResizableDirectMemory` (unmanaged) when available
3. **For most use cases**: Either approach works fine; choose based on deployment environment

## Next Steps

To integrate this into the Platform.Memory library:
1. Submit a PR to [linksplatform/Memory](https://github.com/linksplatform/Memory)
2. Add unit tests for both implementations
3. Update documentation to mention managed memory option
4. Consider adding a factory method to auto-select based on environment capabilities

## References

- Issue: https://github.com/konard/LinksPlatform/issues/199
- Platform.Memory: https://github.com/linksplatform/Memory
- GCHandle documentation: https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.gchandle
