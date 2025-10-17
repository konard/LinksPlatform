# Infinite Dimensional Model

Implementation for issue #524: Model with infinite number of special dimensions.

## Concept

Instead of storing all infinite dimensions, we define values on them using a default value that applies to all dimensions, and only track changes/overrides.

As described in the issue:

- **Zero**: `0, 0, 0, ...` (infinite zeros)
- **One**: `1, 1, 1, ...` (infinite ones)

We do not need to store all infinite dimensions - we just define values on them. Calculations are needed only on changes to those values.

## Implementation

The `InfiniteDimensionalModel<T>` class provides:

1. **Default Value**: A single value that applies to all infinite dimensions
2. **Override Tracking**: Only dimensions that differ from the default are stored
3. **Efficient Operations**: Operations only process dimensions with overrides
4. **Memory Efficiency**: O(k) space where k = number of overrides, not O(∞)

### Key Features

- Get/Set values at any dimension index (long range: -2^63 to 2^63-1)
- Automatic override removal when setting to default value
- Element-wise operations between models
- Transformation operations on overridden dimensions
- Cloning and iteration support

## Files

- `InfiniteDimensionalModel.cs` - Core implementation
- `InfiniteDimensionalModelTests.cs` - Comprehensive unit tests
- `InfiniteDimensionalModelExample.cs` - Usage examples

## Usage Examples

### Basic Usage

```csharp
// Zero model: 0, 0, 0, ...
var zero = new InfiniteDimensionalModel<int>(0);
Console.WriteLine(zero.GetValue(0));        // 0
Console.WriteLine(zero.GetValue(1000000));  // 0

// One model: 1, 1, 1, ...
var one = new InfiniteDimensionalModel<int>(1);
Console.WriteLine(one.GetValue(0));         // 1
Console.WriteLine(one.GetValue(1000000));   // 1
```

### Sparse Modifications

```csharp
var model = new InfiniteDimensionalModel<double>(0.0);

// Only modify specific dimensions
model.SetValue(10, 3.14);
model.SetValue(100, 2.71);

Console.WriteLine(model.GetValue(10));   // 3.14
Console.WriteLine(model.GetValue(50));   // 0.0 (default)
Console.WriteLine(model.OverrideCount);  // 2 (only 2 stored)
```

### Vector Operations

```csharp
var v1 = new InfiniteDimensionalModel<int>(0);
v1.SetValue(0, 5);
v1.SetValue(1, 10);

var v2 = new InfiniteDimensionalModel<int>(0);
v2.SetValue(1, 3);
v2.SetValue(2, 7);

// Element-wise addition
var sum = v1.OperateWith(v2, (a, b) => a + b, 0);
// sum[0] = 5, sum[1] = 13, sum[2] = 7, sum[3...∞] = 0
```

## Running Tests

To compile and run tests:

```bash
# Compile the test file with xunit
dotnet test

# Or compile manually
csc /reference:xunit.core.dll InfiniteDimensionalModel.cs InfiniteDimensionalModelTests.cs

# Run the example
dotnet run InfiniteDimensionalModelExample.cs
```

## Design Philosophy

This implementation demonstrates the key insight from issue #524:

> We do not need to store all infinite dimensions we just define values on them. And calculations needed only on changes to that values.

The model achieves:
- **Space Efficiency**: O(k) where k = number of non-default values
- **Time Efficiency**: Operations only computed on override dimensions
- **Conceptual Simplicity**: Infinite dimensions represented by a single default value
- **Practical Utility**: Sparse data structures, default backgrounds, zero-initialized arrays

## Related Concepts

- Sparse vectors/matrices in numerical computing
- Default values in programming languages
- Lazy evaluation and infinite data structures
- Functional programming infinite lists
