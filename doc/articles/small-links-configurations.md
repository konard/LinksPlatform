# Small Links Data Store Configurations

## Overview

This document describes how Links Platform can support various bit-width configurations for link addresses, allowing the platform to run on different target platforms and use cases with optimal memory efficiency.

## Bit Configuration Support

The Links Platform is designed with generic type parameters (`TLinkAddress`) that allow it to work with different numeric types representing link addresses. This document explores configurations from 1-bit up to 64-bit addresses.

### 64-bit Configuration (Default)

**Type**: `ulong` (UInt64)
**Address Range**: 0 to 18,446,744,073,709,551,615
**Maximum Links**: ~18 quintillion links
**Memory per Link**: 16 bytes (2 × 64-bit addresses: Source and Target)

**Use Cases**:
- Large-scale knowledge bases
- Enterprise data management
- Long-running production systems
- Systems requiring massive scalability

**Example**:
```csharp
using var links = new UnitedMemoryLinks<ulong>("db.links");
```

### 32-bit Configuration

**Type**: `uint` (UInt32)
**Address Range**: 0 to 4,294,967,295
**Maximum Links**: ~4.3 billion links
**Memory per Link**: 8 bytes (2 × 32-bit addresses)

**Use Cases**:
- Medium-sized applications
- Embedded systems with memory constraints
- Mobile applications
- Resource-constrained environments

**Example**:
```csharp
using var links = new UnitedMemoryLinks<uint>("db.links");
```

### 16-bit Configuration

**Type**: `ushort` (UInt16)
**Address Range**: 0 to 65,535
**Maximum Links**: 65,535 links
**Memory per Link**: 4 bytes (2 × 16-bit addresses)

**Use Cases**:
- Small knowledge bases
- IoT devices
- Microcontroller applications
- Proof-of-concept systems
- Educational demonstrations

**Example**:
```csharp
using var links = new UnitedMemoryLinks<ushort>("db.links");
```

### 8-bit Configuration

**Type**: `byte` (UInt8)
**Address Range**: 0 to 255
**Maximum Links**: 255 links
**Memory per Link**: 2 bytes (2 × 8-bit addresses)

**Use Cases**:
- Extremely constrained environments
- Minimal state machines
- Tiny knowledge representations
- Ultra-low-power devices
- Testing and validation

**Example**:
```csharp
using var links = new UnitedMemoryLinks<byte>("db.links");
```

## Sub-byte Configurations

Sub-byte configurations (4-bit, 2-bit, 1-bit) require special handling as they cannot be directly represented by standard C# types. These configurations are primarily theoretical but have interesting properties.

### 4-bit Configuration

**Type**: Custom packed structure
**Address Range**: 0 to 15
**Maximum Links**: 15 links
**Memory per Link**: 1 byte (2 × 4-bit addresses packed)

**Properties**:
- Each link can reference up to 16 addresses (0-15)
- Two link addresses can be packed into a single byte
- Requires custom bit manipulation for access

**Possible States**:
With 4 bits per address and 2 addresses per link, there are 2^8 = 256 possible link states.

**Use Cases**:
- Minimal state representations
- Highly constrained embedded systems
- Research into minimal associative structures
- Theoretical studies of information storage limits

### 2-bit Configuration

**Type**: Custom packed structure
**Address Range**: 0 to 3
**Maximum Links**: 3 links
**Memory per Link**: 4 bits (2 × 2-bit addresses)

**Properties**:
- Each link can reference up to 4 addresses (0-3)
- Four link addresses can be packed into a single byte
- Represents minimal graph structures

**Possible States**:
With 2 bits per address and 2 addresses per link, there are 2^4 = 16 possible link states.

**Possible Link Structures**:
```
Links: 0, 1, 2, 3

Possible doublets (Source → Target):
- 0 → 0 (point)
- 0 → 1, 0 → 2, 0 → 3
- 1 → 0, 1 → 1 (point)
- 1 → 2, 1 → 3
- 2 → 0, 2 → 1, 2 → 2 (point)
- 2 → 3
- 3 → 0, 3 → 1, 3 → 2, 3 → 3 (point)
```

**Use Cases**:
- Minimal finite state machines
- Educational demonstrations of graph theory
- Theoretical research
- Ultra-minimal knowledge representations

### 1-bit Configuration

**Type**: Custom packed structure
**Address Range**: 0 to 1
**Maximum Links**: 1 link
**Memory per Link**: 2 bits (2 × 1-bit addresses)

**Properties**:
- Only 2 possible addresses: 0 and 1
- Only 1 link can meaningfully exist
- The link can only reference itself: 0 → 0 or 1 → 1

**Possible States**:
With 1 bit per address and 2 addresses per link, there are 2^2 = 4 possible link states:
1. `0 → 0` (point at address 0)
2. `0 → 1` (link from 0 to 1)
3. `1 → 0` (link from 1 to 0)
4. `1 → 1` (point at address 1)

However, since we can only have 1 link in the database, the meaningful question is: "Does the link exist or not?" This requires only 1 bit of information.

**Use Cases**:
- Theoretical minimum for associative storage
- Binary state representation
- Existence/non-existence tracking
- Fundamental research into information theory

## Implementation Considerations

### Native C# Types (8-bit and above)

For configurations using standard C# numeric types (`byte`, `ushort`, `uint`, `ulong`), the implementation is straightforward:

```csharp
public class UnitedMemoryLinks<TLinkAddress> : ILinks<TLinkAddress>
    where TLinkAddress : struct, IComparable<TLinkAddress>, IEquatable<TLinkAddress>
{
    // Implementation uses TLinkAddress directly
}
```

**Supported Types**:
- `byte` (8-bit)
- `ushort` (16-bit)
- `uint` (32-bit)
- `ulong` (64-bit)

### Sub-byte Configurations (1-bit to 4-bit)

Sub-byte configurations face several implementation challenges:

1. **No Native Type Support**: C# does not have native 1-bit, 2-bit, or 4-bit integer types.

2. **Pointer Constraints**: C# does not support native pointers to managed structures that depend on custom generic types representing sub-byte values.

3. **Bit Packing Required**: Multiple addresses must be packed into bytes, requiring custom serialization logic.

**Possible Implementation Approaches**:

#### Option 1: Custom Struct with Bit Manipulation
```csharp
public struct FourBitAddress
{
    private byte _value;

    public FourBitAddress(byte value)
    {
        if (value > 15)
            throw new ArgumentOutOfRangeException(nameof(value));
        _value = value;
    }

    public byte Value => _value;
}
```

#### Option 2: Compile-Time Configuration
- Generate specialized implementations for each bit width
- Use code generation or T4 templates
- Compile separate assemblies for each configuration

#### Option 3: Metaprogramming Languages
As noted in the issue comments, languages with metaprogramming support (like Nemerle) could handle this more elegantly through compile-time code generation.

#### Option 4: Native C++ Templates
C++ templates could provide zero-overhead abstractions for sub-byte configurations:

```cpp
template<unsigned int BitWidth>
class PackedAddress {
    // Compile-time bit packing logic
};

template<typename TAddress>
class Links {
    // Link storage implementation
};
```

### Memory Layout Considerations

#### Standard Configurations (8-bit to 64-bit)

```
Link Structure (Doublet):
[Source Address][Target Address]

8-bit:  [1 byte][1 byte] = 2 bytes per link
16-bit: [2 bytes][2 bytes] = 4 bytes per link
32-bit: [4 bytes][4 bytes] = 8 bytes per link
64-bit: [8 bytes][8 bytes] = 16 bytes per link
```

#### Packed Configurations (Sub-byte)

```
4-bit Configuration:
Link: [Source:4bit][Target:4bit] = 1 byte per link
Byte: [Link0][Link1] or [Addr0][Addr1][Addr2][Addr3]

2-bit Configuration:
Link: [Source:2bit][Target:2bit] = 4 bits per link
Byte: [Link0][Link1][Link2][Link3]

1-bit Configuration:
Link: [Source:1bit][Target:1bit] = 2 bits per link
Byte: [Link0][Link1][Link2][Link3] + 4 spare bits
```

## Theoretical Analysis

### State Space Analysis

For a doublet (2-address link) system:

| Bit Width | Addresses | States per Link | Max Links | Total State Space |
|-----------|-----------|-----------------|-----------|-------------------|
| 1-bit     | 2         | 4               | 1         | 2^2 = 4          |
| 2-bit     | 4         | 16              | 3         | 2^4 = 16         |
| 4-bit     | 16        | 256             | 15        | 2^8 = 256        |
| 8-bit     | 256       | 65,536          | 255       | 2^16             |
| 16-bit    | 65,536    | 4,294,967,296   | 65,535    | 2^32             |
| 32-bit    | 4.3B      | 1.8×10^19       | 4.3B      | 2^64             |
| 64-bit    | 1.8×10^19 | 3.4×10^38       | 1.8×10^19 | 2^128            |

### Memory Efficiency Comparison

For a database with N links:

| Configuration | Bytes per Link | Memory for 1K Links | Memory for 1M Links |
|---------------|----------------|---------------------|---------------------|
| 64-bit        | 16             | 16 KB               | 16 MB               |
| 32-bit        | 8              | 8 KB                | 8 MB                |
| 16-bit        | 4              | 4 KB                | 4 MB                |
| 8-bit         | 2              | 2 KB                | 2 MB                |
| 4-bit         | 1              | 1 KB                | 1 MB                |
| 2-bit         | 0.5            | 512 bytes           | 512 KB              |
| 1-bit         | 0.25           | 256 bytes           | 256 KB              |

Note: For sub-byte configurations, actual memory usage would be higher due to byte alignment and overhead.

## Platform-Specific Recommendations

### Desktop/Server Applications
- **Recommended**: 64-bit (`ulong`)
- **Alternative**: 32-bit (`uint`) for memory-constrained scenarios

### Mobile Applications
- **Recommended**: 32-bit (`uint`)
- **Alternative**: 16-bit (`ushort`) for lightweight apps

### Embedded Systems
- **Recommended**: 16-bit (`ushort`) or 8-bit (`byte`)
- **Alternative**: Custom 4-bit implementation for extremely constrained devices

### IoT Devices
- **Recommended**: 8-bit (`byte`)
- **Alternative**: Custom sub-byte implementations for minimal state tracking

### Research/Education
- **Recommended**: All configurations
- Explore theoretical limits and behavior of minimal associative structures

## Variable-Length Addresses

The issue comments also mention variable-length data (similar to VARCHAR in databases). This would allow dynamic address sizing:

**Potential Approaches**:
1. **Prefix Length Encoding**: First byte indicates address length
2. **Variable-Length Encoding (VLQ)**: Use 7 bits per byte for value, 1 bit for continuation
3. **Hybrid**: Small addresses use 8-bit, large addresses use 64-bit with type tag

**Benefits**:
- Optimal memory usage across different database sizes
- Automatic scaling as database grows
- Backward compatibility

**Challenges**:
- More complex implementation
- Performance overhead for length decoding
- Harder to predict memory usage

## Implementation Roadmap

### Phase 1: Documentation (Current)
- ✓ Document all bit-width configurations
- ✓ Analyze theoretical properties
- ✓ Provide implementation guidelines

### Phase 2: Native Type Support
- Ensure `byte`, `ushort`, `uint`, `ulong` all work correctly
- Add tests for each configuration
- Document performance characteristics

### Phase 3: Sub-byte Research
- Prototype 4-bit, 2-bit, and 1-bit implementations
- Explore custom value types
- Investigate C++/CLI or native C++ for packed storage

### Phase 4: Variable-Length Addresses
- Design variable-length encoding scheme
- Implement prototype
- Benchmark performance vs fixed-width

## Conclusion

The Links Platform's generic type design already supports standard bit-width configurations (8-bit through 64-bit) through native C# types. Sub-byte configurations (1-bit, 2-bit, 4-bit) are theoretically interesting and practically useful for extremely constrained environments but require custom implementation approaches due to C# language limitations.

The choice of configuration depends on:
- Maximum expected database size
- Available memory
- Target platform constraints
- Performance requirements
- Energy/power constraints

For most applications, 32-bit or 64-bit configurations provide the best balance of capacity, performance, and simplicity. Smaller configurations shine in specialized scenarios where memory is at an absolute premium.
