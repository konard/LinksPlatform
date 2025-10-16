# Floating Point Numbers in Links Platform

## Overview

This document describes how floating point numbers can be represented in the Links Platform using the associative data structure (doublets/links).

## Motivation

Floating point numbers are essential for:
- Probability calculations for triggers (see [issue #10](https://github.com/Konard/LinksPlatform/issues/10))
- Scientific computations
- Statistical analysis
- Machine learning applications

## Theoretical Foundation

### Standard Floating Point Representation

According to the [IEEE 754 standard](https://en.wikipedia.org/wiki/Floating_point), floating point numbers are typically represented as:

```
value = sign × mantissa × base^exponent
```

For decimal representation:
```
15 / 10^2 = 0.15
```

### Links Platform Representation

In the Links Platform, floating point numbers can be represented as **partially executed operations**. This means that instead of storing the final computed value, we store the operation tree itself.

#### Example: Representing 0.15

The number `0.15` can be represented as:

```
0.15 = 15 / (10 ^ 2)
```

In Links notation, this becomes a structure of links representing:

1. **Integer numbers** (15, 10, 2) - represented using existing integer number encoding
2. **Division operation** (div) - a link representing the division operator
3. **Power operation** (pow) - a link representing the exponentiation operator
4. **Operation structure** - links connecting operands to operators

#### Visual Structure

```
        [div]
       /     \
     [15]    [pow]
            /     \
          [10]    [2]
```

Each box represents a link, and the tree structure is built using the doublet (Source, Target) pattern.

### Key Concepts

#### 1. Never-Ending Recursive Calculation

As noted in [issue #86 comment](https://github.com/Konard/LinksPlatform/issues/86#issuecomment-208862899), floating point representation is actually a **partially executed operation notation**. This means:

- Calculations can be stored in a **partially cached/computed** form
- The last digit of the sequence can be stepped forward on demand
- Calculations are not finite functions - you decide when precision is sufficient
- This enables "semi-accurate" results that can be refined as needed

#### 2. Lazy Evaluation

The Links Platform representation naturally supports lazy evaluation:
- Operations are stored symbolically
- Computation happens only when the actual value is needed
- Precision can be adjusted dynamically based on requirements

#### 3. Symbolic Computation

This approach enables:
- **Exact symbolic representation** - no rounding errors until evaluation
- **Composable operations** - operations can reference other operations
- **Traceable computation** - the entire calculation history is preserved

## Implementation Approach

### Required Components

1. **Integer Numbers** (see [issue #106](https://github.com/Konard/LinksPlatform/issues/106))
   - Powers of 2 representation (for binary)
   - Decimal digit sequences (for decimal)

2. **Operation Links**
   - Division operator link
   - Multiplication operator link
   - Power/Exponentiation operator link
   - Addition and subtraction operators

3. **Operation Structure**
   - Binary operation links: `[operator, left_operand, right_operand]`
   - Unary operation links: `[operator, operand]`

### Encoding Scheme

#### Option 1: Operation Tree (Recommended)

```
FloatingPoint = [DivisionOp, Numerator, Denominator]
Denominator = [PowerOp, Base, Exponent]
```

For `0.15`:
```
fp_0_15 = [div, 15, [pow, 10, 2]]
```

#### Option 2: IEEE 754 Bit Pattern

Alternatively, floating point numbers could be represented by directly encoding their IEEE 754 bit pattern:
```
Float32 = [SignBit, Exponent, Mantissa]
```

However, this loses the symbolic computation advantages.

### Advantages of Operation Tree Representation

1. **Arbitrary Precision**: Can represent numbers with any precision by adjusting the numerator and denominator
2. **Exact Rationals**: Rational numbers (fractions) are represented exactly
3. **Symbolic Simplification**: Operations can be simplified algebraically before evaluation
4. **Traceability**: Complete operation history is maintained
5. **Composability**: Operations can be nested and combined freely

### Disadvantages

1. **Storage Overhead**: Requires multiple links per number
2. **Computation Cost**: Evaluation requires traversing the operation tree
3. **Comparison Complexity**: Comparing two floating point values requires evaluation or symbolic manipulation

## Related Work

- **Integer Numbers**: [Issue #106](https://github.com/Konard/LinksPlatform/issues/106)
- **Triggers with Probabilities**: [Issue #10](https://github.com/Konard/LinksPlatform/issues/10)
- **Platform.Numbers Library**: [linksplatform/Numbers](https://github.com/linksplatform/Numbers)

## Future Directions

1. **Automatic Simplification**: Implement rules for simplifying operation trees
2. **Common Subexpression Elimination**: Reuse common sub-operations
3. **Precision Management**: Develop strategies for determining when to evaluate vs. when to keep symbolic
4. **Performance Optimization**: Cache evaluated results with appropriate invalidation

## References

- [IEEE 754 Floating Point Standard](https://en.wikipedia.org/wiki/Floating_point)
- [Associative Model of Data](https://en.wikipedia.org/wiki/Associative_model_of_data)
- [Links Theory](./links-theory.md)

## Conclusion

Representing floating point numbers as partially executed operations in the Links Platform provides a flexible, symbolic approach that naturally integrates with the associative data model. This representation enables arbitrary precision, exact symbolic computation, and complete operation traceability while maintaining the simplicity and elegance of the doublet-based storage system.
