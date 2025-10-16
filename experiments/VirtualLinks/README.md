# Virtual Links

## Overview

Virtual Links is a conceptual framework for accessing computed or metadata information about links in the Links Platform without physically storing that information in the database.

This implementation addresses [Issue #85](https://github.com/konard/LinksPlatform/issues/85).

## Concept

Virtual links provide a method to access data about links from the Links Platform itself. They are useful for:

1. **Triggers** - Accessing link metadata when events occur
2. **Computed Properties** - Retrieving information that's calculated on-demand rather than stored
3. **Reserved Address Ranges** - Using special address ranges that automatically trigger computation

## Features

Virtual links can be used to access:

1. **Index/ID** - The address of a link (computed, not stored separately)
2. **Link Parts** - Source, target, linker (nth part of link)
3. **Reference Counts** - Number of direct references to a link
4. **Nth Reference** - Access to specific references or children
5. **Weight** - Link weight (can be computed from reference patterns)
6. **Timestamps** - Creation/update time (if tracking is enabled)

## Architecture

### Core Interfaces

- **`IVirtualLink<TLink>`** - Base interface for all virtual links
- **`VirtualLinkBase<TLink>`** - Abstract base class providing common functionality
- **`VirtualLinkType`** - Enum defining available virtual link types

### Implementations

- **`IndexVirtualLink<TLink>`** - Returns the link's index/ID
- **`LinkPartVirtualLink<TLink>`** - Accesses link parts (source, target, linker)
- **`ReferenceCountVirtualLink<TLink>`** - Counts references to a link
- **`VirtualLinkFactory<TLink>`** - Factory for creating virtual links
- **`VirtualLinkAddressSpace<TLink>`** - Manages virtual address ranges

## Usage Examples

### Basic Usage

```csharp
using Platform.Data.VirtualLinks;

// Create links storage
using var links = new UnitedMemoryLinks<ulong>(memory);
var factory = new VirtualLinkFactory<ulong>(links);

// Create a link
var link = links.CreateAndUpdate(source, target);

// Access virtual properties
var indexVirtualLink = factory.CreateIndexLink(link);
Console.WriteLine($"Link ID: {indexVirtualLink.GetValue()}");

var sourceVirtualLink = factory.CreateSourceLink(link);
Console.WriteLine($"Link Source: {sourceVirtualLink.GetValue()}");

var refCountVirtualLink = factory.CreateReferenceCountLink(link);
Console.WriteLine($"Reference Count: {refCountVirtualLink.GetValue()}");
```

### Virtual Address Space

```csharp
// Create virtual address space
var virtualSpace = new VirtualLinkAddressSpace<ulong>(
    factory,
    virtualBase: 1_000_000_000UL,
    virtualLinksPerRealLink: 16
);

// Get virtual address for a property
var sourceVirtualAddr = virtualSpace.GetVirtualAddress(link, VirtualLinkType.Source);

// Resolve virtual address
var virtualLink = virtualSpace.CreateFromAddress(sourceVirtualAddr);
Console.WriteLine($"Value: {virtualLink.GetValue()}");
```

### Use with Triggers

Virtual links are particularly useful for trigger implementations (see [Issue #10](https://github.com/konard/LinksPlatform/issues/10)):

```csharp
// In a trigger handler
void OnLinkCreated(ulong link)
{
    var factory = new VirtualLinkFactory<ulong>(links);

    // Access metadata without storing it
    var index = factory.CreateIndexLink(link).GetValue();
    var source = factory.CreateSourceLink(link).GetValue();
    var target = factory.CreateTargetLink(link).GetValue();

    // Log or process metadata
    Console.WriteLine($"Link {index} created: {source} -> {target}");
}
```

## Virtual Address Ranges

The concept of reserved addresses allows virtual links to be "addressable." For any link, a number of reserved addresses are allocated that automatically trigger computation when accessed.

For example, if a link has address `N`, virtual links might occupy addresses:
```
VirtualBase + N * VirtualLinksPerRealLink + offset
```

Where `offset` determines the type of virtual link:
- offset 0: Index
- offset 1: Source
- offset 2: Target
- offset 3: Linker
- offset 4: Reference Count
- etc.

This approach is similar to Unicode ranges, where certain ranges represent different character sets or symbols.

## Relationship to Other Issues

### Issue #10: Triggers
Virtual links provide a clean way to access link metadata within trigger handlers without modifying the core storage structure.

### Issue #57: Walkers
Virtual links can be used by walkers to access computed information while traversing the link structure.

## Implementation Notes

This is an **experimental implementation** demonstrating the virtual links concept. It provides:

- ✅ Core abstractions and interfaces
- ✅ Basic virtual link types (Index, Source, Target, Reference Count)
- ✅ Virtual address space management
- ✅ Working examples

Future enhancements could include:
- Additional virtual link types (Weight, Timestamps, Nth Reference)
- Integration with actual trigger system
- Performance optimizations
- Caching mechanisms for frequently accessed virtual properties

## Running Examples

To run the examples:

```csharp
using Platform.Data.VirtualLinks;

VirtualLinksExample.RunAllExamples();
```

Or run individual examples:
```csharp
VirtualLinksExample.BasicExample();
VirtualLinksExample.AddressSpaceExample();
VirtualLinksExample.TriggerExample();
```

## Design Decisions

1. **Computed vs Stored**: Virtual links compute values on-demand rather than storing them, reducing storage overhead
2. **Type Safety**: Strong typing through generics ensures type safety across different link address types
3. **Extensibility**: Factory pattern allows easy addition of new virtual link types
4. **Address Space**: Reserved address ranges enable virtual links to be treated like regular links in some contexts

## Related Standards

The virtual address range concept can be extended to real-world standards (as mentioned in issue comments):
- IATA codes for airports
- Standard identifiers for hotels, landmarks, etc.
- Any publicly standardized reference system

This allows Links Platform to integrate with external datasets while maintaining internal consistency.

## License

This implementation follows the LinksPlatform license.
