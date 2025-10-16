# Issue #170: Test Interfaces, Not Implementations

## Problem

Some tests in the LinksPlatform ecosystem (specifically in `linksplatform/Data.Doublets`) are testing concrete implementations directly instead of testing through interfaces. This creates tight coupling and makes tests less flexible.

## Examples

### Before (Testing Implementation)
```csharp
[Fact]
public static void DeleteAllUsages()
{
    var mem = new HeapResizableDirectMemory();
    var links = new UnitedMemoryLinks<uint>(mem);  // ❌ Direct instantiation

    var root = links.CreatePoint();
    // ... test code
}
```

### After (Testing Interface)
```csharp
[Fact]
public static void DeleteAllUsages()
{
    Using<uint>(links =>  // ✅ Testing through ILinks<T> interface
    {
        var root = links.CreatePoint();
        // ... test code
    });
}

private static void Using<TLinkAddress>(Action<ILinks<TLinkAddress>> action)
    where TLinkAddress : IUnsignedNumber<TLinkAddress>, /* ... */
{
    var mem = new HeapResizableDirectMemory();
    var links = new UnitedMemoryLinks<TLinkAddress>(mem);
    action(links);  // Pass as interface
}
```

## Benefits

1. **Tests the Contract**: Tests verify the interface contract, not implementation details
2. **Implementation Agnostic**: Easy to test multiple implementations (UnitedMemoryLinks, SplitMemoryLinks, Ffi.Links)
3. **Better Maintainability**: Changes to implementation don't break tests if interface is stable
4. **Follows Best Practices**: Aligns with patterns in `GenericLinksTests.cs` and `SplitMemoryGenericLinksTests.cs`

## Files to Refactor

In `linksplatform/Data.Doublets` repository:

1. ✅ **ILinksBasicTests.cs** - Refactored to use `Using<T>` helper pattern
2. ⚠️ **ResizableDirectMemoryLinksTests.cs** - Partially follows pattern (uses extension methods on ILinks), but could be further improved

## Pattern to Follow

The recommended pattern is already used in:
- `GenericLinksTests.cs`
- `SplitMemoryGenericLinksTests.cs`

Key elements:
1. Create a `Using<TLinkAddress>(Action<ILinks<TLinkAddress>> action)` helper method
2. Test methods call the helper with test logic as lambda
3. Helper creates concrete instance but passes it as interface to test action

## See Also

- Issue #140: Make a use of LinksTestScope in tests
- Issue #141: Implement SequencesTestScope
