# Binary Links Notation (BLN) - Specification v0.1

## Abstract

Binary Links Notation (BLN) is a compact binary data protocol based on the Links Platform associative data model. It provides an efficient alternative to text-based formats like JSON and XML by representing data as variable-length sequences of link indices with optional compression. This specification defines the encoding, structure, and serialization rules for BLN.

## Table of Contents

1. [Introduction](#introduction)
2. [Design Principles](#design-principles)
3. [Data Model](#data-model)
4. [Encoding Format](#encoding-format)
5. [Variable-Length Encoding](#variable-length-encoding)
6. [Compression Techniques](#compression-techniques)
7. [Binary Structure](#binary-structure)
8. [Examples](#examples)
9. [Comparison with Other Formats](#comparison-with-other-formats)
10. [Implementation Considerations](#implementation-considerations)

## Introduction

The Links Platform is built on an associative data model where all data is represented as links (connections between elements). Binary Links Notation leverages this structure to create a highly efficient binary serialization format that:

- Minimizes storage and transmission overhead
- Supports variable-length encoding for optimal space usage
- Enables compression through shared link references
- Provides a universal representation for any data structure

## Design Principles

### 1. Minimalism
Use the minimum number of bits necessary to represent data, with variable-length encoding adapting to the actual values being stored.

### 2. Universality
Any data structure representable in the Links Platform can be serialized to BLN, making it a universal data exchange format.

### 3. Compression-Friendly
The format naturally supports compression through link reuse and reference sharing, eliminating redundancy at the protocol level.

### 4. Self-Describing
The structure contains enough information to reconstruct the original data without external schema definitions.

## Data Model

### Links as Fundamental Units

In the Links Platform:
- A **link** is a connection between two other links (forming a doublet)
- Links can reference themselves (forming points)
- Links can be composed into sequences, trees, and graphs
- Each link has a unique identifier (index)

### Representation Hierarchy

```
Empty Link (⛶)
    ↓
Point (link referencing itself)
    ↓
Doublet (link with two references)
    ↓
Sequence (ordered collection of links)
    ↓
Complex Structures (trees, graphs, networks)
```

## Encoding Format

### Basic Encoding Scheme

BLN uses a variable-length encoding scheme where:
- **1** represents a link reference
- **0** (or **00**) serves as a separator
- Sequences are encoded as space-separated indices

### Encoding Examples

#### Simple Pair
```
Conceptual:  (1, 1)
BLN Binary:  1 0 1
BLN Compact: 01 00 01
```

#### Doublet Reference
```
Conceptual:  (1, 1)
BLN Binary:  1 1 0
BLN Compact: 01 01 00
```

The second form is more compact when a single doublet/triplet is stored, as the separator comes at the end.

## Variable-Length Encoding

### Integer Encoding

BLN uses variable-length integer encoding to minimize space. Multiple schemes are supported:

#### 1. VLQ (Variable-Length Quantity)
- Similar to UTF-8 and Protocol Buffers
- Uses continuation bit to indicate if more bytes follow
- 7 bits of data per byte, 1 continuation bit

```
Value Range          | Bytes | Format
---------------------|-------|------------------------
0-127                | 1     | 0xxxxxxx
128-16,383           | 2     | 1xxxxxxx 0xxxxxxx
16,384-2,097,151     | 3     | 1xxxxxxx 1xxxxxxx 0xxxxxxx
```

#### 2. Prefix-Based Encoding
- First bits indicate the byte length
- Optimized for small values

```
Prefix | Length | Value Range
-------|--------|-------------
0      | 1 byte | 0-127
10     | 2 bytes| 128-16,511
110    | 3 bytes| 16,512-2,113,663
111    | 4+ bytes| Larger values
```

### Link Index Encoding

Link indices are encoded using the variable-length integer scheme, with special optimizations:

- **Index 0**: Reserved for null/empty link (⛶)
- **Index 1**: Often represents the first actual link
- **Relative encoding**: Option to encode differences between consecutive indices for better compression

## Compression Techniques

### 1. Link Reuse

Since links can reference existing links, repeated structures only need to be stored once:

```
Without compression: [A, B, C] [A, B, C] = 6 links
With compression:    [A, B, C] referenced twice = 3 links + 1 reference
```

### 2. Sequence Deduplication

Identical sequences share the same link structure:

```
Sequence "hello" appears 3 times:
Without compression: 15 character links + 15 structure links = 30 links
With compression:    5 character links + 5 structure links + 3 references = 13 links
```

### 3. Relative Indexing

Sequential indices can be encoded as deltas:

```
Absolute: [100, 101, 102, 103, 104]
Relative: [100, +1, +1, +1, +1]
```

### 4. Unique Sequence Table

All unique sequences can be placed at the end of the structure as a lookup table, with references in the main body:

```
Main Data: [ref1, ref2, ref1, ref3, ref2]
Sequence Table:
  ref1: [1, 2, 3]
  ref2: [4, 5]
  ref3: [6, 7, 8, 9]
```

## Binary Structure

### File Format

A BLN file consists of three main sections:

```
┌─────────────────────────────────────┐
│         HEADER                      │
├─────────────────────────────────────┤
│         LINK TABLE                  │
├─────────────────────────────────────┤
│         SEQUENCE TABLE (optional)   │
└─────────────────────────────────────┘
```

### Header Format

```
Offset | Size  | Field              | Description
-------|-------|--------------------|---------------------------------
0      | 4     | Magic Number       | 0x424C4E00 ("BLN\0")
4      | 2     | Version            | Major.Minor (e.g., 0.1)
6      | 1     | Encoding Type      | VLQ=0, Prefix=1, etc.
7      | 1     | Flags              | Compression options
8      | 4/8   | Link Count         | Number of links in table
12/16  | 4/8   | Root Link Index    | Index of the root link
```

### Link Table Entry

Each link is encoded as:

```
┌──────────────────────────────────────┐
│  Link Index (VLQ)                    │
├──────────────────────────────────────┤
│  Source Link Index (VLQ)             │
├──────────────────────────────────────┤
│  Target Link Index (VLQ)             │
└──────────────────────────────────────┘
```

For a point (self-referencing link):
```
Link Index: N
Source: N
Target: N
```

For a doublet:
```
Link Index: N
Source: M (where M < N)
Target: K (where K < N or K = N)
```

## Examples

### Example 1: Simple Pair

Representing the pair `(1, 1)`:

```
Human Readable: (1, 1)
Link Structure:
  Link 0: ⛶ (empty)
  Link 1: Point (1→1, 1→1)
  Link 2: Doublet (2→1, 2→1)

Binary Encoding:
  Header: [BLN\0][0.1][VLQ][0x00][3 links][2]
  Link Table:
    Link 0: [00] (empty marker)
    Link 1: [01][01][01] (point)
    Link 2: [02][01][01] (doublet referencing link 1 twice)
```

### Example 2: Sequence "ABC"

Representing the character sequence "ABC":

```
Human Readable: "ABC"
Link Structure (balanced tree):
  Link 0: ⛶
  Link 1: 'A' (character value)
  Link 2: 'B'
  Link 3: 'C'
  Link 4: (A, B) = doublet of links 1 and 2
  Link 5: (AB, C) = doublet of links 4 and 3

Binary Encoding:
  Header: [BLN\0][0.1][VLQ][0x00][6 links][5]
  Link Table:
    Link 0: [00]
    Link 1: [01][01][01] + value[0x41]
    Link 2: [02][02][02] + value[0x42]
    Link 3: [03][03][03] + value[0x43]
    Link 4: [04][01][02]
    Link 5: [05][04][03]
```

### Example 3: Nested Structure

JSON equivalent: `{"name": "Alice", "age": 30}`

```
Link Structure:
  Links 1-4: Characters 'n','a','m','e'
  Link 5-6: Pairs forming "name"
  Link 7: Complete "name" sequence
  Links 8-12: Characters 'A','l','i','c','e'
  Links 13-15: Pairs forming "Alice"
  Link 16: Complete "Alice" sequence
  Link 17: Key-value pair ("name", "Alice")
  Links 18-19: Characters 'a','g','e'
  Link 20: "age" sequence
  Link 21: Number 30
  Link 22: Key-value pair ("age", 30)
  Link 23: Object containing both pairs
```

With compression, repeated characters and common patterns are stored only once.

## Comparison with Other Formats

### Size Comparison

For a simple object `{"id": 123, "active": true}`:

| Format | Size (bytes) | Notes |
|--------|--------------|-------|
| JSON   | 32           | Text-based, human-readable |
| XML    | 56           | Verbose tags |
| MessagePack | 18      | Binary, schema-less |
| Protocol Buffers | 8 | Binary, requires schema |
| **BLN**  | **12-15**  | Binary, self-describing, with compression |

### Advantages of BLN

1. **No schema required**: Unlike Protocol Buffers, BLN is self-describing
2. **Natural deduplication**: Link reuse provides automatic compression
3. **Flexible structure**: Can represent any graph or tree structure
4. **Efficient for repetitive data**: Shared sequences are stored once
5. **Universal**: Same format for all data types (numbers, strings, objects, arrays)

### Disadvantages of BLN

1. **Learning curve**: Requires understanding the Links data model
2. **Overhead for simple data**: Small individual values may have higher overhead than specialized formats
3. **Implementation complexity**: Requires a Links Platform runtime or compatible parser

## Implementation Considerations

### Encoding Algorithm

```
1. Traverse the data structure
2. Convert each element to a link
3. Build the link table, reusing existing links where possible
4. Apply compression (optional):
   a. Deduplicate sequences
   b. Use relative indexing
   c. Build sequence lookup table
5. Write header
6. Write link table
7. Write sequence table (if compression enabled)
```

### Decoding Algorithm

```
1. Read and validate header
2. Read link table into memory
3. Read sequence table (if present)
4. Reconstruct the data structure starting from root link:
   a. Resolve link references
   b. Expand sequences
   c. Build final structure
```

### Performance Characteristics

- **Encoding**: O(n) where n is the number of unique links
- **Decoding**: O(n) where n is the number of links
- **Memory**: O(n) for link table storage
- **Compression ratio**: Typically 30-70% of equivalent JSON, better with repetitive data

### Streaming Support

BLN can be adapted for streaming:
- Use incremental link indices
- Emit links as they are created
- Use special markers for stream boundaries
- Support partial parsing for large datasets

## Future Enhancements

### Version 0.2 Planned Features

1. **Delta encoding**: Store only changes between versions
2. **Custom compression dictionaries**: For domain-specific optimization
3. **Type annotations**: Optional type hints for faster parsing
4. **Encryption support**: Built-in encryption header
5. **Checksums**: Integrity validation
6. **Streaming extensions**: Full streaming protocol support

## Appendix A: Reserved Values

| Value | Meaning |
|-------|---------|
| 0x00  | Empty link (⛶) |
| 0x01  | First actual link |
| 0xFF..FF | Reserved for future use |

## Appendix B: Reference Implementation

Reference implementations are available in:
- C# (Platform.Data.Doublets)
- C++ (coming soon)
- JavaScript (coming soon)

## Appendix C: MIME Type

Proposed MIME type: `application/vnd.linksplatform.bln`

File extension: `.bln`

## References

1. Links Platform Documentation: https://linksplatform.github.io/
2. Links Theory: [links-theory.md](./links-theory.md)
3. Platform.Data.Doublets: https://github.com/linksplatform/Data.Doublets
4. Variable-Length Quantity: https://en.wikipedia.org/wiki/Variable-length_quantity

## License

This specification is released under the same license as the Links Platform project.

## Changelog

- **v0.1** (2025-10-16): Initial specification draft
