# Number Representation Experiments

This directory contains experiments demonstrating different ways to represent integer numbers using the Unified Sequences Interface in the LinksPlatform ecosystem.

## Issue Reference

These experiments address [Issue #106](https://github.com/konard/LinksPlatform/issues/106): Numbers

## Implementations

### 1. Powers of 2 Representation (Binary)

**Converters:**
- `IntegerToPowersOf2SequenceConverter` - Converts an integer to a sequence of powers of 2
- `PowersOf2SequenceToIntegerConverter` - Converts a sequence of powers of 2 back to an integer

**Concept:**
Any integer can be represented as a sum of powers of 2 (its binary representation). For example:
- `13 = 2^3 + 2^2 + 2^0 = 8 + 4 + 1`
- Sequence representation: `[3, 2, 0]`
- Binary: `1101`

**Experiment:** `PowersOf2NumbersExperiment.cs`

### 2. Decimal Digits Representation

**Converters:**
- `IntegerToDecimalDigitsSequenceConverter` - Converts an integer to a sequence of decimal digits
- `DecimalDigitsSequenceToIntegerConverter` - Converts a sequence of decimal digits back to an integer

**Concept:**
Any integer can be represented as a sequence of decimal digits (base-10 representation). For example:
- `12345 = [1, 2, 3, 4, 5]`
- Supports negative numbers with a special marker
- Zero is represented as `[0]`

**Experiment:** `DecimalDigitsNumbersExperiment.cs`

## How It Works

Both implementations use the **Unified Sequences Interface** from LinksPlatform:

1. **Links Storage**: Uses doublets (pairs of links) to store data
2. **Sequences**: Numbers are converted to sequences of links
3. **Markers**: Special marker links identify the type of number representation
4. **Bidirectional**: Each implementation supports both encoding and decoding

## Architecture

```
Integer Number
    ↓ (encode)
Sequence of Links (via IConverter)
    ↓ (store)
Links Storage (ILinks<TLink>)
    ↓ (retrieve)
Sequence of Links (via ISequenceWalker)
    ↓ (decode)
Integer Number
```

## File Locations

**Converter Implementations:**
- `Platform/Platform.Examples/IntegerToPowersOf2SequenceConverter.cs`
- `Platform/Platform.Examples/PowersOf2SequenceToIntegerConverter.cs`
- `Platform/Platform.Examples/IntegerToDecimalDigitsSequenceConverter.cs`
- `Platform/Platform.Examples/DecimalDigitsSequenceToIntegerConverter.cs`

**Experiments:**
- `experiments/PowersOf2NumbersExperiment.cs`
- `experiments/DecimalDigitsNumbersExperiment.cs`

## Running the Experiments

The experiments are standalone demonstration code that shows how to:
1. Create an in-memory links storage
2. Initialize the converters
3. Convert numbers to link sequences
4. Convert link sequences back to numbers
5. Verify the round-trip conversion

## Related Work

These implementations build upon existing number representations in the LinksPlatform ecosystem:
- **Raw Numbers**: Direct binary representation using link addresses
- **Unary Numbers**: Numbers represented as chains of links
- **Rational Numbers**: Fractions represented as numerator/denominator pairs
- **BigInteger Support**: Arbitrary precision integers using byte sequences

## Future Work

Potential extensions:
- Hexadecimal representation
- Scientific notation representation
- Floating-point numbers as sequences
- Optimized compression for common number patterns
- Integration with Unicode sequences for textual numbers
