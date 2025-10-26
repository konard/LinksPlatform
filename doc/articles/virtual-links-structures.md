# Virtual Links Structures

## Overview

Virtual Links Structures are a design pattern in LinksPlatform that allows traditional table data to be accessed through a links-like interface without being physically stored in the links space. This concept addresses the need for efficient storage of specialized data types while maintaining a uniform access pattern.

## Problem Statement

When working with links-based storage systems, there are scenarios where storing certain data types (like strings, binary blobs, or large numbers) directly as links would be inefficient. For example:

- A string "Hello World" could theoretically be represented as a sequence of character links
- Each character would need its own link ID
- The structure connecting these characters would require additional links
- This creates significant overhead for simple string storage

## Solution: Virtual Links Structures

Instead of physically storing such data as links, we can create **virtual links structures** that:

1. Store data in an optimized format (e.g., strings in a dictionary/table)
2. Present a links-like interface for uniform access
3. Are traversable like real links
4. Do not occupy space in the physical links database

## Key Concepts

### 1. Physical vs Virtual Storage

- **Physical Links**: Stored in the actual links database (e.g., relationships between entities)
- **Virtual Links**: Stored in specialized tables but accessible through a links interface

### 2. Traversability

Virtual links structures implement the same traversal patterns as real links:
- `GetSource(link)` - Returns the source of a virtual link
- `GetTarget(link)` - Returns the target of a virtual link
- `Contains(link)` - Checks if a link belongs to the virtual structure

### 3. ID Space Partitioning

To avoid conflicts between real and virtual links, the ID space is partitioned:
- Real links: IDs 1 to N
- Virtual structure 1: IDs N+1 to M
- Virtual structure 2: IDs M+1 to K
- And so on...

## Implementation

### Interface

```csharp
public interface IVirtualLinksStructure<TLink>
{
    TLink GetSource(TLink link);
    TLink GetTarget(TLink link);
    bool Contains(TLink link);
    long Count { get; }
}
```

### Example: Virtual String Table

The `VirtualStringTable<TLink>` class demonstrates this concept:

```csharp
var stringTable = new VirtualStringTable<long>(
    baseId: 1_000_000L,
    increment: x => x + 1,
    greaterThanOrEqual: (a, b) => a >= b,
    lessThan: (a, b) => a < b,
    subtract: (a, b) => a - b,
    add: (a, n) => a + n
);

// Add strings (stored in a table, not as links)
var link1 = stringTable.Add("Hello");
var link2 = stringTable.Add("World");

// Access like links
var source = stringTable.GetSource(link1);  // Virtual source
var target = stringTable.GetTarget(link1);  // Virtual target
var exists = stringTable.Contains(link1);   // true
```

## Benefits

1. **Efficient Storage**: Data is stored in a format optimal for its type
2. **Uniform Access**: Same interface for real and virtual links
3. **Traversability**: Virtual structures can be navigated like real links
4. **Type Specialization**: Different virtual structures for different data types
5. **Reduced Overhead**: No need to decompose complex data into individual links

## Use Cases

### 1. String Storage

Store strings efficiently while allowing them to be referenced as links:
- Labels and names
- Text content
- Metadata

### 2. Binary Data

Store binary blobs without converting each byte to a link:
- Images
- Files
- Serialized objects

### 3. Numeric Data

Store large numbers or numeric sequences:
- Timestamps
- Measurements
- Counters

### 4. External Data

Reference data stored in external systems:
- Database records
- API responses
- File system entries

## Example Scenario

Consider a system tracking people and their attributes:

```
Real Links Space (IDs 1-999,999):
- Person entity relationships
- Organizational structure
- Connections between people

Virtual String Table (IDs 1,000,000+):
- Names: "John Doe", "Jane Smith"
- Emails: "john@example.com"
- Labels: "Name", "Email", "Phone"

Virtual Number Table (IDs 2,000,000+):
- Ages: 25, 30, 45
- Phone numbers
- ZIP codes
```

A person entity might have:
- Link ID: 42 (real link)
- Name: 1,000,001 (virtual link to "John Doe")
- Email: 1,000,002 (virtual link to "john@example.com")
- Age: 2,000,001 (virtual link to number 30)

## Integration with Existing Systems

Virtual links structures can be integrated with existing `ILinks<TLink>` implementations:

1. **Unified Query Interface**: A wrapper that can query both real and virtual links
2. **Transparent Access**: Client code doesn't need to know if a link is real or virtual
3. **Efficient Traversal**: The system automatically handles virtual link resolution

## Future Enhancements

Potential extensions to this concept:

1. **Virtual Number Tables**: For efficient numeric data storage
2. **Virtual Binary Tables**: For blob/file storage
3. **Virtual Time Series**: For temporal data
4. **Virtual Indexes**: For computed/derived data
5. **Lazy Virtual Structures**: Generate links on-the-fly from external sources

## Conclusion

Virtual Links Structures provide a powerful abstraction that combines the flexibility of links-based systems with the efficiency of specialized storage. By allowing data to be stored optimally while maintaining a uniform interface, they enable LinksPlatform to handle diverse data types efficiently.

## Related Issues

- [#599](https://github.com/konard/LinksPlatform/issues/599) - Each table as a virtual links structure

## See Also

- [IVirtualLinksStructure.cs](../../Platform/Platform.Examples/IVirtualLinksStructure.cs)
- [VirtualStringTable.cs](../../Platform/Platform.Examples/VirtualStringTable.cs)
- [VirtualLinksStructureDemo.cs](../../experiments/VirtualLinksStructureDemo.cs)
- [VirtualLinksIntegrationExample.cs](../../examples/VirtualLinksIntegrationExample.cs)
