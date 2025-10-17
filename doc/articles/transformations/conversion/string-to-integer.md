# Convert String to Integer

## Description
Parses a string representation of a number and converts it to an integer data type. This is essential for processing user input, reading configuration files, and data interchange.

## Examples

### Cross-platform - C# - .NET 6.0+
**Platform:** Windows, Linux, macOS
**Language:** C# 10.0+
**Framework/Library:** .NET 6.0+
**Additional Dependencies:** None

```csharp
// Safe parsing with TryParse (recommended)
string input = "42";
if (int.TryParse(input, out int result))
{
    Console.WriteLine($"Parsed: {result}");
    // Output: Parsed: 42
}
else
{
    Console.WriteLine("Invalid number");
}

// Direct parsing (throws exception on failure)
try
{
    int number = int.Parse("123");
    // Result: 123
}
catch (FormatException)
{
    // Handle invalid format
}
catch (OverflowException)
{
    // Handle number too large/small
}

// With culture-specific parsing
using System.Globalization;
int num = int.Parse("1,234", NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
// Result: 1234
```

**Notes:**
- `TryParse` is preferred as it doesn't throw exceptions
- `Parse` throws `FormatException` for invalid strings and `OverflowException` for out-of-range values
- Default parsing doesn't accept thousand separators or decimal points
- Use `NumberStyles` to control parsing behavior
- Range: -2,147,483,648 to 2,147,483,647

**References:**
- [Int32.TryParse - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/api/system.int32.tryparse)
- [Int32.Parse - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/api/system.int32.parse)

---

### Cross-platform - Python - Standard Library
**Platform:** Windows, Linux, macOS
**Language:** Python 3.6+
**Framework/Library:** Standard Library
**Additional Dependencies:** None

```python
# Basic conversion
input_str = "42"
number = int(input_str)
# Result: 42

# With error handling
try:
    number = int("123")
    print(number)  # 123
except ValueError:
    print("Invalid number")

# Different bases
binary = int("1010", 2)      # Result: 10
hexadecimal = int("FF", 16)  # Result: 255
octal = int("77", 8)         # Result: 63

# Stripping whitespace (automatic)
number = int("  42  ")  # Result: 42

# Negative numbers
negative = int("-100")  # Result: -100
```

**Notes:**
- `int()` automatically strips leading/trailing whitespace
- Raises `ValueError` for invalid strings
- Supports conversion from different bases (2-36)
- Does not accept thousand separators or decimal points
- Python integers have unlimited precision (no overflow)

**References:**
- [int() - Python Documentation](https://docs.python.org/3/library/functions.html#int)
- [Built-in Types - Python Documentation](https://docs.python.org/3/library/stdtypes.html#numeric-types-int-float-complex)

---

### Web Browser - JavaScript - ES5+
**Platform:** Web Browser (All modern browsers)
**Language:** JavaScript ES5+
**Framework/Library:** None (Vanilla JavaScript)
**Additional Dependencies:** None

```javascript
// Using parseInt (recommended for integers)
const str = "42";
const number = parseInt(str, 10);  // Always specify radix!
// Result: 42

// Handles leading zeros and whitespace
const num1 = parseInt("  42  ", 10);  // Result: 42
const num2 = parseInt("042", 10);     // Result: 42

// Stops at first non-numeric character
const partial = parseInt("42px", 10);  // Result: 42

// Using Number constructor (stricter)
const strict1 = Number("42");      // Result: 42
const strict2 = Number("42px");    // Result: NaN
const strict3 = Number("  42  ");  // Result: 42

// Using unary plus operator (shorthand)
const quick = +"42";  // Result: 42

// Validation
if (isNaN(parseInt(str, 10))) {
    console.log("Invalid number");
}
```

**Notes:**
- Always specify radix (base) in `parseInt()` to avoid unexpected behavior
- `parseInt()` stops parsing at first non-digit character
- `Number()` is stricter and returns NaN for any non-numeric string
- Check for `NaN` using `isNaN()` or `Number.isNaN()`
- Range: -(2^53 - 1) to 2^53 - 1 for safe integers

**References:**
- [parseInt() - MDN](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/parseInt)
- [Number() - MDN](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number)

---

### Cross-platform - Java - JDK 8+
**Platform:** Windows, Linux, macOS
**Language:** Java 8+
**Framework/Library:** Java Standard Library
**Additional Dependencies:** None

```java
// Using Integer.parseInt (throws exception)
String input = "42";
int number = Integer.parseInt(input);
// Result: 42

// With error handling
try {
    int num = Integer.parseInt("123");
    System.out.println(num);  // 123
} catch (NumberFormatException e) {
    System.out.println("Invalid number format");
}

// Using Integer.valueOf (returns Integer object)
Integer numObject = Integer.valueOf("42");
int primitive = numObject;  // Auto-unboxing
// Result: 42

// Different radixes
int binary = Integer.parseInt("1010", 2);      // Result: 10
int hex = Integer.parseInt("FF", 16);          // Result: 255

// Trimming whitespace first
int trimmed = Integer.parseInt("  42  ".trim());
// Result: 42
```

**Notes:**
- `parseInt()` throws `NumberFormatException` for invalid input
- Does not automatically trim whitespace (use `trim()` first)
- `valueOf()` returns cached Integer objects for values -128 to 127
- Range: -2,147,483,648 to 2,147,483,647
- For larger numbers, use `Long.parseLong()`

**References:**
- [Integer.parseInt - Java Documentation](https://docs.oracle.com/javase/8/docs/api/java/lang/Integer.html#parseInt-java.lang.String-)
- [Integer.valueOf - Java Documentation](https://docs.oracle.com/javase/8/docs/api/java/lang/Integer.html#valueOf-java.lang.String-)

---

### Cross-platform - Go - Standard Library
**Platform:** Windows, Linux, macOS
**Language:** Go 1.16+
**Framework/Library:** Standard Library
**Additional Dependencies:** None

```go
package main

import (
    "fmt"
    "strconv"
)

func main() {
    // Using Atoi (ASCII to integer)
    input := "42"
    number, err := strconv.Atoi(input)
    if err != nil {
        fmt.Println("Error:", err)
        return
    }
    fmt.Println(number)  // Output: 42

    // Using ParseInt for more control
    num, err := strconv.ParseInt("123", 10, 64)
    if err != nil {
        fmt.Println("Error:", err)
        return
    }
    fmt.Println(num)  // Output: 123

    // Different bases
    binary, _ := strconv.ParseInt("1010", 2, 64)
    fmt.Println(binary)  // Output: 10

    hex, _ := strconv.ParseInt("FF", 16, 64)
    fmt.Println(hex)  // Output: 255
}
```

**Notes:**
- `Atoi` is shorthand for `ParseInt(s, 10, 0)` and returns `int` type
- `ParseInt` allows specifying base and bit size
- Always check the error return value
- Bit size parameter: 0, 8, 16, 32, or 64
- Returns `*strconv.NumError` on failure

**References:**
- [strconv.Atoi - Go Documentation](https://pkg.go.dev/strconv#Atoi)
- [strconv.ParseInt - Go Documentation](https://pkg.go.dev/strconv#ParseInt)

---

### Cross-platform - Rust - Standard Library
**Platform:** Windows, Linux, macOS
**Language:** Rust 1.50+
**Framework/Library:** Standard Library
**Additional Dependencies:** None

```rust
fn main() {
    // Using parse with turbofish syntax
    let input = "42";
    let number: i32 = input.parse().unwrap();
    println!("{}", number);  // Output: 42

    // With error handling
    match input.parse::<i32>() {
        Ok(num) => println!("Parsed: {}", num),
        Err(e) => println!("Error: {}", e),
    }

    // Using from_str_radix for different bases
    use std::num::ParseIntError;

    let binary = i32::from_str_radix("1010", 2).unwrap();
    println!("{}", binary);  // Output: 10

    let hex = i32::from_str_radix("FF", 16).unwrap();
    println!("{}", hex);  // Output: 255

    // Automatic whitespace trimming
    let trimmed: i32 = "  42  ".trim().parse().unwrap();
    println!("{}", trimmed);  // Output: 42
}
```

**Notes:**
- `parse()` is a trait method from `FromStr`
- Returns `Result<i32, ParseIntError>`
- Use `unwrap()` for quick prototyping, proper error handling in production
- Whitespace must be trimmed manually
- `from_str_radix` for non-decimal bases
- Range for i32: -2,147,483,648 to 2,147,483,647

**References:**
- [str::parse - Rust Documentation](https://doc.rust-lang.org/std/primitive.str.html#method.parse)
- [i32::from_str_radix - Rust Documentation](https://doc.rust-lang.org/std/primitive.i32.html#method.from_str_radix)

---

### Cross-platform - PHP - Standard Library
**Platform:** Windows, Linux, macOS
**Language:** PHP 7.4+
**Framework/Library:** Standard Library
**Additional Dependencies:** None

```php
<?php
// Using intval (forgiving)
$input = "42";
$number = intval($input);
echo $number;  // Output: 42

// Type casting (similar to intval)
$num = (int)"123";
echo $num;  // Output: 123

// Stops at first non-numeric character
$partial = intval("42px");  // Result: 42

// Different bases with intval
$binary = intval("1010", 2);   // Result: 10
$hex = intval("FF", 16);       // Result: 255

// Validation before conversion
$str = "42";
if (is_numeric($str)) {
    $validated = (int)$str;
    echo $validated;  // Output: 42
}

// Using filter_var for strict validation
$filtered = filter_var("42", FILTER_VALIDATE_INT);
if ($filtered !== false) {
    echo $filtered;  // Output: 42
}
?>
```

**Notes:**
- `intval()` returns 0 for invalid input (no error)
- Type casting `(int)` is equivalent to `intval()`
- Both stop parsing at first non-numeric character
- Use `is_numeric()` or `filter_var()` for validation
- `filter_var()` with `FILTER_VALIDATE_INT` returns false for invalid input
- Range: -2,147,483,648 to 2,147,483,647 (32-bit), larger on 64-bit systems

**References:**
- [intval - PHP Documentation](https://www.php.net/manual/en/function.intval.php)
- [filter_var - PHP Documentation](https://www.php.net/manual/en/function.filter-var.php)
