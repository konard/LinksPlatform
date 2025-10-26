# EditableArray Implementation - Issue #591

## Overview

This implementation provides an immutably editable array with infinite length support. The array achieves immutability through a range-based architecture where each modification creates a new range appended to the end, rather than modifying existing data.

## Features

- **Infinite Length**: The array can grow to any size limited only by available memory
- **Immutable Editing**: Each change creates a new range; original data is preserved
- **Range-Based Architecture**: Efficient storage through segmented ranges
- **Read Optimization**: Latest values are retrieved by reading ranges from end to beginning
- **Array Rebuilding**: Merge multiple ranges into optimized single ranges
- **File Persistence**: Save and load arrays with full metadata preservation
- **BitString Support**: Specialized implementation for bit manipulation

## Architecture

### Core Components

1. **EditableArrayRange<T>**: Represents a single range with:
   - `Offset`: Position in the virtual array (immutable)
   - `Length`: Size of the range (immutable)
   - `Data`: The actual data stored (immutable)
   - `Next`: Reference to the next range (mutable for linking)

2. **EditableArray<T>**: Main array implementation with:
   - Range collection management
   - Read/Write operations
   - Optimization and rebuilding
   - File persistence

3. **EditableBitString**: Specialized implementation for bit manipulation using byte-based storage

## Usage Examples

### Basic Usage

```csharp
var array = new EditableArray<int>(defaultValue: 0);

// Write data
array.Write(0, new[] { 1, 2, 3, 4, 5 });

// Read individual value
int value = array.Read(2); // Returns 3

// Read range
int[] range = array.Read(0, 5); // Returns [1, 2, 3, 4, 5]

// Overlapping write (immutable edit)
array.Write(2, new[] { 10, 20, 30 });

// Latest value is returned
value = array.Read(2); // Returns 10 (from latest write)
```

### Optimization

```csharp
var array = new EditableArray<int>(0);

// Multiple writes create multiple ranges
array.Write(0, new[] { 1, 2, 3 });
array.Write(2, new[] { 20, 30 });
array.Write(10, new[] { 100, 200 });

Console.WriteLine($"Ranges: {array.RangeCount}"); // 3 ranges

// Optimize merges all ranges
var optimized = array.Optimize();
Console.WriteLine($"Ranges: {optimized.RangeCount}"); // 1 range
```

### File Persistence

```csharp
var array = new EditableArray<long>(0);
array.Write(0, new[] { 100L, 200L, 300L });

// Save to file
array.SaveToFile("data.dat");

// Load from file
var loaded = EditableArray<long>.LoadFromFile("data.dat");
```

### BitString Usage

```csharp
var bitString = new EditableBitString(defaultBit: false);

// Set individual bits
bitString.SetBit(0, true);
bitString.SetBit(5, true);

// Get bit
bool bit = bitString.GetBit(0); // true

// Set range of bits
bitString.SetBits(10, new[] { true, false, true, true });

// Get range of bits
bool[] bits = bitString.GetBits(10, 4);
```

## Implementation Details

### Reading Strategy

When reading a value at index `i`, the algorithm:
1. Iterates through ranges from **end to beginning**
2. Returns the first (most recent) range containing index `i`
3. Returns default value if no range contains `i`

This ensures the latest written value is always retrieved, avoiding repeated overwrites.

### File Format

The file format stores:
- Type name for validation
- Number of ranges
- Default value
- For each range:
  - Offset (8 bytes, long)
  - Length (8 bytes, long)
  - Data elements (size depends on type)

### Supported Types for Persistence

- byte, short, ushort, int, uint, long, ulong
- float, double
- bool, char

## Files Created

### Core Implementation
- `Platform/Platform.Sandbox/EditableArrayRange.cs` - Range class
- `Platform/Platform.Sandbox/EditableArray.cs` - Main array implementation
- `Platform/Platform.Sandbox/EditableBitString.cs` - BitString specialization

### Examples and Tests
- `examples/EditableArrayExample.cs` - Usage examples
- `experiments/EditableArrayTests.cs` - Test suite
- `experiments/EditableArrayRunner.cs` - Test/example runner

## Running Tests

```bash
cd Platform
dotnet build Platform.Sandbox/Platform.Sandbox.csproj
```

The build should complete successfully with no errors.

## Performance Characteristics

- **Write**: O(1) - Appends new range to list
- **Read (single)**: O(R) where R is number of ranges (optimized by reading from end)
- **Read (range)**: O(N * R) where N is range length, R is number of ranges
- **Optimize**: O(N) where N is total data size
- **Space**: O(N) where N is total data written (ranges not deduplicated)

## Benefits

1. **Immutability**: Original data never modified, enabling safe concurrent reads
2. **History Preservation**: All writes are preserved in range history
3. **Efficient Updates**: Small updates don't require copying entire array
4. **Flexible Size**: No pre-allocation required, grows as needed
5. **Persistence**: Full state can be saved and restored

## Potential Optimizations

1. **Range Merging**: Automatically merge adjacent ranges during write
2. **Indexed Access**: Build index structure for faster reads (O(log R))
3. **Compression**: Compress ranges with repeated values
4. **Lazy Evaluation**: Delay optimization until read operations
5. **Concurrent Access**: Add thread-safe operations

## License

Copyright Konstantin Diachenko
See LICENSE file for details.
