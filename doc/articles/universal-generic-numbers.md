# Universal Implementations for Generic Numbers: Integer and MathHelpers Classes

## Introduction

Before the introduction of generic math in C# 11 and .NET 7, working with generic numeric types in C# was a significant challenge. The LinksPlatform project developed innovative solutions to this problem through the `Integer` and `MathHelpers` classes, which were part of the `Platform.Helpers.Numbers` namespace. These classes provided universal implementations for working with different numeric types in a type-safe, generic manner.

This article documents the design and implementation of these classes, which represent an important milestone in the evolution of generic programming in C# and serve as a historical reference for techniques used before C#'s native generic math support.

## Historical Context

### The Problem

Prior to C# 11, there was no way to write generic code that performed mathematical operations on numeric types. For example, you couldn't write a method like:

```csharp
public T Add<T>(T a, T b) => a + b; // This didn't work before C# 11
```

The compiler had no way to know that type `T` supported the `+` operator, even if `T` was constrained to be a value type or struct.

### The LinksPlatform Solution

The LinksPlatform project created two key classes to solve this problem:

1. **`Integer<T>` and `Integer`**: Universal wrapper types that could convert between any numeric type
2. **`MathHelpers<T>` and `MathHelpers`**: Helper classes providing generic mathematical operations

These classes were originally located in `Platform.Helpers.Numbers` namespace in the Konard/LinksPlatform repository and were partially migrated to the `Platform.Numbers` NuGet package in July 2019.

## The Integer Class

### Purpose

The `Integer` struct serves as a universal numeric type that can seamlessly convert to and from any .NET numeric type. It internally stores a `ulong` value and provides implicit conversions for all primitive numeric types.

### Implementation

```csharp
public struct Integer
{
    public readonly ulong Value;

    public Integer(ulong value) => Value = value;

    // Implicit conversions FROM all numeric types TO Integer
    public static implicit operator Integer(ulong integer) => new Integer(integer);
    public static implicit operator Integer(long integer) => To.UInt64(integer);
    public static implicit operator Integer(uint integer) => new Integer(integer);
    public static implicit operator Integer(int integer) => To.UInt64(integer);
    public static implicit operator Integer(ushort integer) => new Integer(integer);
    public static implicit operator Integer(short integer) => To.UInt64(integer);
    public static implicit operator Integer(byte integer) => new Integer(integer);
    public static implicit operator Integer(sbyte integer) => To.UInt64(integer);
    public static implicit operator Integer(bool integer) => To.UInt64(integer);

    // Implicit conversions FROM Integer TO all numeric types
    public static implicit operator ulong(Integer integer) => integer.Value;
    public static implicit operator long(Integer integer) => To.Int64(integer.Value);
    public static implicit operator uint(Integer integer) => To.UInt32(integer.Value);
    public static implicit operator int(Integer integer) => To.Int32(integer.Value);
    public static implicit operator ushort(Integer integer) => To.UInt16(integer.Value);
    public static implicit operator short(Integer integer) => To.Int16(integer.Value);
    public static implicit operator byte(Integer integer) => To.Byte(integer.Value);
    public static implicit operator sbyte(Integer integer) => To.SByte(integer.Value);
    public static implicit operator bool(Integer integer) => To.Boolean(integer.Value);

    public override string ToString() => Value.ToString();
}
```

### Key Design Decisions

1. **Internal ulong Storage**: Using `ulong` as the internal representation allows storing any unsigned integer value up to 64 bits.

2. **Implicit Conversions**: All conversions are implicit, making the type transparent to use in code. You can pass any numeric type where an `Integer` is expected, and vice versa.

3. **Checked Conversions**: The `To` class from `Platform.Converters` handles the actual conversions with appropriate overflow checking.

4. **Boolean Support**: Even `bool` values can be converted, treating `false` as 0 and `true` as 1.

### Usage Example

```csharp
Integer value1 = 42;        // int → Integer
Integer value2 = 100L;      // long → Integer
Integer value3 = (byte)5;   // byte → Integer

int result1 = value1;       // Integer → int
long result2 = value2;      // Integer → long
byte result3 = value3;      // Integer → byte
```

## The Generic Integer<T> Struct

### Purpose

While `Integer` provides a universal numeric type, `Integer<T>` provides a generic wrapper that maintains type information while still allowing universal conversions.

### Implementation

```csharp
public struct Integer<T>
{
    private static readonly Func<ulong, Integer<T>> Create;

    public static readonly T Zero;
    public static readonly T One;
    public static readonly T Two;

    public readonly T Value;

    static Integer()
    {
        // Dynamic code generation using Sigil
        DelegateHelpers.Compile(out Create, emiter =>
        {
            if (!CachedTypeInfo<T>.CanBeNumeric && typeof(T) != typeof(Integer))
                throw new NotSupportedException();

            emiter.LoadArgument(0);

            if (typeof(T) != typeof(ulong) && typeof(T) != typeof(Integer))
                emiter.Call(typeof(To).GetTypeInfo().GetMethod(typeof(T).Name, Types<ulong>.Array));

            if (CachedTypeInfo<T>.IsNullable)
                emiter.NewObject(typeof(T), CachedTypeInfo<T>.UnderlyingType);

            if (typeof(T) == typeof(Integer))
                emiter.NewObject(typeof(Integer), typeof(ulong));

            emiter.NewObject(typeof(Integer<T>), typeof(T));

            emiter.Return();
        });

        try
        {
            Zero = default;
            One = ArithmeticHelpers.Increment(Zero);
            Two = ArithmeticHelpers.Increment(One);
        }
        catch (Exception exception)
        {
            Global.OnIgnoredException(exception);
        }
    }

    public Integer(T value) => Value = value;

    // Implicit conversions between Integer<T>, Integer, ulong, and T
    public static implicit operator Integer(Integer<T> integer) => /* ... */;
    public static implicit operator ulong(Integer<T> integer) => /* ... */;
    public static implicit operator T(Integer<T> integer) => integer.Value;
    public static implicit operator Integer<T>(T integer) => new Integer<T>(integer);
    public static implicit operator Integer<T>(ulong integer) => Create(integer);
    // ... more conversions for all numeric types

    public override string ToString() => Value.ToString();
}
```

### Key Techniques

1. **IL Code Generation**: The static constructor uses `DelegateHelpers.Compile` with Sigil to generate optimized IL code for conversions at runtime.

2. **Type Checking**: The implementation validates that `T` is a numeric type using `CachedTypeInfo<T>.CanBeNumeric`.

3. **Static Constants**: `Zero`, `One`, and `Two` are pre-computed for the generic type `T`.

4. **Null-safe**: Handles nullable numeric types correctly.

## The MathHelpers Class

### Purpose

`MathHelpers` provides cached mathematical constants and generic mathematical operations that work across all numeric types.

### Implementation

```csharp
public class MathHelpers
{
    // Cached factorial values from OEIS A000142
    private static readonly ulong[] Factorials =
    {
        1, 1, 2, 6, 24, 120, 720, 5040, 40320, 362880, 3628800, 39916800,
        479001600, 6227020800, 87178291200, 1307674368000, 20922789888000,
        355687428096000, 6402373705728000, 121645100408832000, 2432902008176640000
    };

    // Cached Catalan numbers from OEIS A000108
    private static readonly ulong[] Catalans =
    {
        1, 1, 2, 5, 14, 42, 132, 429, 1430, 4862, 16796, 58786, 208012,
        742900, 2674440, 9694845, 35357670, 129644790, 477638700, 1767263190,
        6564120420, 24466267020, 91482563640, 343059613650, 1289904147324, 4861946401452,
        18367353072152, 69533550916004, 263747951750360, 1002242216651368, 3814986502092304
    };

    public static double Factorial(double n)
    {
        if (n <= 1) return 1;
        if (n < Factorials.Length) return Factorials[(int)n];
        return n * Factorial(n - 1);
    }

    public static double Catalan(double n)
    {
        if (n <= 1) return 1;
        if (n < Catalans.Length) return Catalans[(int)n];
        return Factorial(2 * n) / (Factorial(n + 1) * Factorial(n));
    }

    public static bool IsPowerOfTwo(ulong x) => (x & x - 1) == 0;

    // Generic operations delegating to MathHelpers<T>
    public static T Abs<T>(T x) => MathHelpers<T>.Abs(x);
    public static T Negate<T>(T x) => MathHelpers<T>.Negate(x);
}
```

### Key Features

1. **Performance Optimization**: Pre-computed factorial and Catalan number arrays for instant lookup.

2. **OEIS Integration**: Values sourced from the Online Encyclopedia of Integer Sequences (OEIS), ensuring mathematical correctness.

3. **Bit Manipulation**: The `IsPowerOfTwo` method uses a clever bit trick: `(x & x - 1) == 0` works because powers of two have exactly one bit set.

4. **Generic Delegation**: Generic methods delegate to the specialized `MathHelpers<T>` class.

## The Generic MathHelpers<T> Class

### Purpose

`MathHelpers<T>` provides compile-time-generated operations for specific numeric types using IL emission.

### Implementation

```csharp
public static class MathHelpers<T>
{
    public static readonly Func<T, T> Abs;
    public static readonly Func<T, T> Negate;

    static MathHelpers()
    {
        // Compile Abs function
        DelegateHelpers.Compile(out Abs, emiter =>
        {
            if (!CachedTypeInfo<T>.IsNumeric)
                throw new NotSupportedException();

            emiter.LoadArgument(0);

            if (CachedTypeInfo<T>.IsSigned)
                emiter.Call(typeof(Math).GetTypeInfo().GetMethod("Abs", new[] { typeof(T) }));

            emiter.Return();
        });

        // Compile Negate function
        DelegateHelpers.Compile(out Negate, emiter =>
        {
            if (!CachedTypeInfo<T>.IsNumeric || !CachedTypeInfo<T>.IsSigned)
                throw new NotSupportedException();

            emiter.LoadArgument(0);
            emiter.Negate();
            emiter.Return();
        });
    }
}
```

### Key Techniques

1. **Static Compilation**: The static constructor runs once per `T`, generating optimized delegates.

2. **IL Emission with Sigil**: Uses the Sigil library to emit IL code for mathematical operations.

3. **Type-Specific Logic**: Different code paths for signed vs. unsigned types.

4. **Zero-Overhead Abstraction**: After compilation, the delegates are as fast as hand-written type-specific code.

## Technical Implementation Details

### Platform.Reflection.Sigil

The implementations heavily rely on the Sigil library for IL emission. Sigil provides a type-safe, fluent API for generating IL code:

```csharp
DelegateHelpers.Compile(out Abs, emiter =>
{
    emiter.LoadArgument(0);  // Load first argument onto stack
    emiter.Call(method);      // Call a method
    emiter.Return();          // Return from function
});
```

This approach allows creating optimized delegates at runtime without reflection overhead during actual usage.

### CachedTypeInfo<T>

The `CachedTypeInfo<T>` class provides cached type information:

- `IsNumeric`: Whether T is a numeric type
- `IsSigned`: Whether T is a signed numeric type
- `IsNullable`: Whether T is a nullable type
- `UnderlyingType`: The underlying type for nullables
- `CanBeNumeric`: Whether T can represent numeric values

## Comparison with Modern C# Generic Math

With C# 11 and .NET 7, Microsoft introduced native generic math support through static abstract interface members:

```csharp
// Modern C# 11 approach
public T Add<T>(T a, T b) where T : INumber<T>
    => T.CreateChecked(a + b);
```

### Advantages of the LinksPlatform Approach

1. **Backwards Compatibility**: Works with .NET Framework and older .NET Core versions
2. **Universal Conversions**: `Integer` provides seamless conversions between all numeric types
3. **Cached Constants**: Pre-computed mathematical sequences for performance

### Advantages of Modern Generic Math

1. **Native Compiler Support**: No IL emission required
2. **Operator Overloading**: Direct use of operators like `+`, `-`, `*`, `/`
3. **Better Type Safety**: Compile-time checking instead of runtime validation
4. **Simpler Code**: No need for complex IL emission code

## Migration to Platform.Numbers

When the LinksPlatform project was modularized in July 2019, `Platform.Helpers.Numbers` was split:

### Files Migrated to Platform.Numbers Package
- `Bit.cs` and `Bit<T>.cs`: Bitwise operations
- `MathHelpers.cs` → renamed to `Math.cs`: Mathematical helpers

### Files NOT Migrated
- `Integer.cs` and `Integer<T>.cs`: Universal integer conversions
- `MathHelpers<T>.cs`: Generic math operations using IL emission
- `ArithmeticHelpers.cs`, `ArithmeticHelpers<T>.cs`: Generic arithmetic
- `BitwiseHelpers.cs`, `BitwiseHelpers<T>.cs`: Generic bitwise operations
- Various extension classes

The migrated `Math.cs` class was later updated (in 2023) to use C# 11's generic math features instead of IL emission, making it compatible with modern .NET while maintaining the same API.

## Current State

As of 2025, the `Platform.Numbers` package uses modern generic math with constraints like:

```csharp
public static TLinkAddress Factorial<TLinkAddress>(TLinkAddress n)
    where TLinkAddress : IUnsignedNumber<TLinkAddress>,
                         IComparisonOperators<TLinkAddress, TLinkAddress, bool>
{
    // Implementation using modern generic math
}
```

This represents the evolution from the original IL emission-based approach to native C# generic math support.

## Conclusion

The `Integer` and `MathHelpers` classes from LinksPlatform represent an important chapter in C# generic programming history. They demonstrate:

1. **Innovation in Constraint**: Creative solutions to language limitations
2. **Performance Optimization**: Using IL emission for zero-overhead abstractions
3. **Practical Design**: Solving real-world problems in generic data structures

While modern C# has superseded these techniques with native generic math support, the principles and design patterns remain valuable for understanding:

- How to work with generic numeric types
- IL emission and runtime code generation
- Performance optimization in generic code
- Universal type conversion patterns

These classes served as a critical foundation for the LinksPlatform's associative data storage systems, where efficient generic numeric operations were essential for managing links and addresses in a type-safe manner.

## References

- [Original Platform.Helpers.Numbers code (commit 19902d5)](https://github.com/Konard/LinksPlatform/tree/19902d5c6221b5c93a5e06849de28bb97edac5f8/Platform/Platform.Helpers/Numbers)
- [Platform.Numbers GitHub Repository](https://github.com/linksplatform/Numbers)
- [Platform.Numbers NuGet Package](https://www.nuget.org/packages/Platform.Numbers)
- [OEIS A000142 - Factorials](https://oeis.org/A000142)
- [OEIS A000108 - Catalan Numbers](https://oeis.org/A000108)
- [C# 11 Generic Math Documentation](https://learn.microsoft.com/en-us/dotnet/standard/generics/math)
- [Sigil - IL Emission Library](https://github.com/kevin-montrose/Sigil)

## See Also

- [Links Theory](links-theory.md) - Understanding the associative data model
- [Platform.Converters](https://github.com/linksplatform/Converters) - Type conversion utilities
- [Platform.Reflection](https://github.com/linksplatform/Reflection) - Reflection helpers including Sigil support
