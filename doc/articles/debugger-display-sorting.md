# How to Make Properties Sorted by Meaning in Visual Studio Debugger

## Introduction

When debugging complex objects in Visual Studio, the default alphabetical ordering of properties in the debugger watch window often makes it difficult to understand the object's structure and relationships. By using special .NET attributes, you can control how your objects appear in the debugger, organizing properties by their semantic meaning rather than alphabetical order.

This article demonstrates how to use `DebuggerDisplay` and `DebuggerBrowsable` attributes to create a more intuitive debugging experience.

## The Problem

By default, Visual Studio debugger displays object properties in alphabetical order. For domain objects with meaningful relationships, this can obscure the logical structure. For example, in a Link object that represents connections between entities, you might want to see:
1. The core relationship properties (Source, Linker, Target) first
2. Collections of related objects grouped together
3. Internal count properties hidden to reduce clutter

## The Solution: Using Debugger Attributes

.NET provides several attributes in the `System.Diagnostics` namespace that control debugger visualization:

- **`DebuggerDisplay`**: Customizes how an object or property is displayed in debugger variable windows
- **`DebuggerBrowsable`**: Controls whether and how a member is displayed in debugger variable windows

## Practical Implementation

### Step 1: Create Proxy Properties

Create private properties with meaningful single-letter names (or any naming convention) that serve as proxies for your actual properties. These proxy properties will appear in alphabetical order in the debugger, but you control that order through naming.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

public partial class Link
{
    [DebuggerDisplay(null, Name = "Source")]
    private Link я_A { get { return this.Source; } set { this.Source = value; } }

    [DebuggerDisplay(null, Name = "Linker")]
    private Link я_B { get { return this.Linker; } set { this.Linker = value; } }

    [DebuggerDisplay(null, Name = "Target")]
    private Link я_C { get { return this.Target; } set { this.Target = value; } }
}
```

### Step 2: Display Collections with Count Information

For collection properties, you can show the count in the debugger display, making it easier to understand the object's state at a glance:

```csharp
[DebuggerDisplay("Count = {я_DC}", Name = "ReferersBySource")]
private List<Link> я_D { get { return this.ReferersBySource.ToList(); } }

[DebuggerBrowsable(DebuggerBrowsableState.Never)]
private int я_DC { get { return this.ReferersBySource.Count(); } }
```

### Step 3: Hide Helper Properties

Use `DebuggerBrowsable(DebuggerBrowsableState.Never)` to hide properties that are only used for display purposes and shouldn't clutter the debugger view:

```csharp
[DebuggerBrowsable(DebuggerBrowsableState.Never)]
private int я_DC { get { return this.ReferersBySource.Count(); } }
```

## Complete Example

Here's a complete implementation for a Link class:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Net
{
    public partial class Link
    {
        // Core properties displayed first
        [DebuggerDisplay(null, Name = "Source")]
        private Link я_A { get { return this.Source; } set { this.Source = value; } }

        [DebuggerDisplay(null, Name = "Linker")]
        private Link я_B { get { return this.Linker; } set { this.Linker = value; } }

        [DebuggerDisplay(null, Name = "Target")]
        private Link я_C { get { return this.Target; } set { this.Target = value; } }

        // Collections with count display
        [DebuggerDisplay("Count = {я_DC}", Name = "ReferersBySource")]
        private List<Link> я_D { get { return this.ReferersBySource.ToList(); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int я_DC { get { return this.ReferersBySource.Count(); } }

        [DebuggerDisplay("Count = {я_EC}", Name = "ReferersByLinker")]
        private List<Link> я_E { get { return this.ReferersByLinker.ToList(); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int я_EC { get { return this.ReferersByLinker.Count(); } }

        [DebuggerDisplay("Count = {я_FC}", Name = "ReferersByTarget")]
        private List<Link> я_F { get { return this.ReferersByTarget.ToList(); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int я_FC { get { return this.ReferersByTarget.Count(); } }
    }
}
```

## How It Works

1. **Alphabetical Ordering**: The debugger displays properties alphabetically. By naming proxy properties `я_A`, `я_B`, `я_C`, etc., you control the display order.

2. **Custom Names**: The `Name` parameter in `DebuggerDisplay` shows a friendly name in the debugger while keeping the actual property name simple for ordering.

3. **Custom Display Text**: For collections, `DebuggerDisplay("Count = {я_DC}")` shows the count inline, providing quick insight without expanding the collection.

4. **Hidden Properties**: `DebuggerBrowsable(DebuggerBrowsableState.Never)` hides helper properties (like count getters) from the debugger view.

## Benefits

- **Semantic Grouping**: Properties are grouped by meaning rather than alphabetical order
- **Improved Readability**: Important properties appear first, collections show counts
- **Reduced Clutter**: Helper properties are hidden from view
- **Better Debugging Experience**: Developers can quickly understand object state and relationships

## Considerations

- This technique adds proxy properties to your class, slightly increasing memory footprint
- The proxy properties should be in a separate partial class file (e.g., `ClassName.Debug.cs`) to keep debugging code separate from business logic
- Choose a consistent naming convention for proxy properties across your codebase
- This approach works in Visual Studio and other debuggers that respect .NET debugger attributes

## Conclusion

By leveraging `DebuggerDisplay` and `DebuggerBrowsable` attributes with strategically named proxy properties, you can transform the debugging experience from alphabetical chaos to semantically organized clarity. This is especially valuable for complex domain models where understanding object relationships is crucial for effective debugging.

## References

- [Original implementation example](https://github.com/Konard/LinksPlatform/blob/9d896de75963fb49d8b65e5774676f7f6fd47056/28.03.2010-04.11.2010/Net/Net/Link.Debug.cs)
- [Microsoft Docs: DebuggerDisplayAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.debuggerdisplayattribute)
- [Microsoft Docs: DebuggerBrowsableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.debuggerbrowsableattribute)
