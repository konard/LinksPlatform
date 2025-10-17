# Convert String to Lowercase

## Description
Converts all characters in a string to their lowercase equivalents. This is commonly used for case-insensitive comparisons, data normalization, user input processing, and standardization of text data.

## Examples

### Cross-platform - C# - .NET 6.0+
**Platform:** Windows, Linux, macOS
**Language:** C# 10.0+
**Framework/Library:** .NET 6.0+
**Additional Dependencies:** None

```csharp
string original = "Hello WORLD";
string lowercase = original.ToLower();
// Result: "hello world"

// Culture-aware (recommended for international text)
using System.Globalization;
string lowercaseInvariant = original.ToLower(CultureInfo.InvariantCulture);
// Result: "hello world"

// For case-insensitive comparisons, consider using comparison methods instead:
bool isEqual = original.Equals("hello world", StringComparison.OrdinalIgnoreCase);
```

**Notes:**
- `ToLower()` uses the current culture by default, which may produce unexpected results with certain characters (e.g., Turkish 'I')
- Use `ToLower(CultureInfo.InvariantCulture)` for culture-independent operations and consistent results
- In Turkish locale, 'I' lowercases to 'ı' (dotless i), not 'i'
- For case-insensitive operations, consider using `StringComparison.OrdinalIgnoreCase` instead of converting to lowercase
- Performance: O(n) where n is the string length

**References:**
- [String.ToLower Method - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/api/system.string.tolower)
- [Best Practices for Using Strings in .NET](https://docs.microsoft.com/en-us/dotnet/standard/base-types/best-practices-strings)

---

### Cross-platform - Python - Standard Library
**Platform:** Windows, Linux, macOS
**Language:** Python 3.6+
**Framework/Library:** Standard Library
**Additional Dependencies:** None

```python
original = "Hello WORLD"
lowercase = original.lower()
# Result: "hello world"

# For more aggressive case folding (better for comparisons)
casefolded = original.casefold()
# Result: "hello world"

# Example where casefold() differs from lower():
german = "Straße"  # German street sign
print(german.lower())     # "straße"
print(german.casefold())  # "strasse" (ß -> ss)
```

**Notes:**
- `lower()` converts to lowercase based on Unicode standard
- `casefold()` is more aggressive and recommended for case-insensitive comparisons
- `casefold()` handles special characters like German ß (sharp s) better
- Both methods return a new string; original is unchanged
- Performance: O(n) where n is the string length

**References:**
- [str.lower() - Python Documentation](https://docs.python.org/3/library/stdtypes.html#str.lower)
- [str.casefold() - Python Documentation](https://docs.python.org/3/library/stdtypes.html#str.casefold)

---

### Web Browser - JavaScript - ES5+
**Platform:** Web Browser (Chrome 90+, Firefox 88+, Safari 14+, Edge 90+)
**Language:** JavaScript ES5+
**Framework/Library:** None (Vanilla JavaScript)
**Additional Dependencies:** None

```javascript
const original = "Hello WORLD";
const lowercase = original.toLowerCase();
// Result: "hello world"

// For locale-specific lowercasing (e.g., Turkish)
const turkishLowercase = original.toLocaleLowerCase('tr-TR');

// Example: Turkish 'I' handling
const turkishI = "TITLE";
console.log(turkishI.toLowerCase());              // "title"
console.log(turkishI.toLocaleLowerCase('tr-TR')); // "tıtle" (dotless i)
```

**Notes:**
- `toLowerCase()` uses Unicode default case mapping
- `toLocaleLowerCase()` respects locale-specific rules
- Both methods return a new string without modifying the original
- For most use cases, `toLowerCase()` is sufficient
- Consider `toLocaleLowerCase()` when working with user-facing text in specific locales

**References:**
- [String.prototype.toLowerCase() - MDN](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/toLowerCase)
- [String.prototype.toLocaleLowerCase() - MDN](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/toLocaleLowerCase)

---

### Cross-platform - Java - JDK 8+
**Platform:** Windows, Linux, macOS
**Language:** Java 8+
**Framework/Library:** Java Standard Library
**Additional Dependencies:** None

```java
String original = "Hello WORLD";
String lowercase = original.toLowerCase();
// Result: "hello world"

// Locale-specific conversion
import java.util.Locale;
String lowercaseInvariant = original.toLowerCase(Locale.ROOT);
// Result: "hello world"

// Turkish locale example
String lowercaseTurkish = original.toLowerCase(new Locale("tr", "TR"));
```

**Notes:**
- `toLowerCase()` without arguments uses the default locale
- Always use `toLowerCase(Locale.ROOT)` or `toLowerCase(Locale.ENGLISH)` for locale-independent operations
- Turkish locale has special case conversion rules for 'I' and 'i'
- Returns a new String object; original is immutable
- For case-insensitive comparison, use `equalsIgnoreCase()` instead

**References:**
- [String.toLowerCase() - Java Documentation](https://docs.oracle.com/javase/8/docs/api/java/lang/String.html#toLowerCase--)
- [Locale - Java Documentation](https://docs.oracle.com/javase/8/docs/api/java/util/Locale.html)

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
    "strings"
)

func main() {
    original := "Hello WORLD"
    lowercase := strings.ToLower(original)
    fmt.Println(lowercase)
    // Output: hello world

    // Works correctly with Unicode
    unicode := "ΑΛΦΑ"  // Greek letters
    fmt.Println(strings.ToLower(unicode))
    // Output: αλφα
}
```

**Notes:**
- `strings.ToLower()` handles Unicode correctly
- Returns a new string; original is unchanged
- Uses Unicode case mapping rules
- For case-insensitive comparison, use `strings.EqualFold()`
- Performance: O(n) where n is the string length

**References:**
- [strings.ToLower - Go Documentation](https://pkg.go.dev/strings#ToLower)
- [strings Package - Go Documentation](https://pkg.go.dev/strings)

---

### Cross-platform - Ruby - Standard Library
**Platform:** Windows, Linux, macOS
**Language:** Ruby 2.7+
**Framework/Library:** Standard Library
**Additional Dependencies:** None

```ruby
original = "Hello WORLD"
lowercase = original.downcase
# Result: "hello world"

# Mutating version (modifies original)
str = "Hello WORLD"
str.downcase!
puts str  # "hello world"

# Unicode support
unicode = "ΑΛΦΑ"  # Greek letters
puts unicode.downcase  # "αλφα"
```

**Notes:**
- `downcase` returns a new string with lowercase letters
- `downcase!` modifies the string in-place
- Fully supports Unicode and multibyte characters
- For ASCII-only conversion (faster), use `swapcase` or custom implementation
- For case-insensitive comparison, use `casecmp` or `casecmp?`

**References:**
- [String#downcase - Ruby Documentation](https://ruby-doc.org/core-3.0.0/String.html#method-i-downcase)

---

### Linux/macOS - Bash - Shell Utilities
**Platform:** Linux, macOS, Unix-like systems
**Language:** Bash 4.0+
**Framework/Library:** GNU Coreutils
**Additional Dependencies:** None

```bash
# Using parameter expansion (Bash 4.0+)
original="Hello WORLD"
lowercase="${original,,}"
echo "$lowercase"
# Output: hello world

# Using tr command (more portable)
lowercase=$(echo "$original" | tr '[:upper:]' '[:lower:]')
echo "$lowercase"
# Output: hello world

# Using awk
lowercase=$(echo "$original" | awk '{print tolower($0)}')
echo "$lowercase"
# Output: hello world
```

**Notes:**
- `${var,,}` is a Bash 4.0+ feature and may not work in older shells
- `tr` command is POSIX-compliant and works across most Unix-like systems
- `awk` solution is also widely portable
- For maximum portability, use `tr` command
- Unicode support depends on locale settings (LC_CTYPE)

**References:**
- [Bash Parameter Expansion](https://www.gnu.org/software/bash/manual/html_node/Shell-Parameter-Expansion.html)
- [tr Command Manual](https://man7.org/linux/man-pages/man1/tr.1.html)

---

### Cross-platform - Rust - Standard Library
**Platform:** Windows, Linux, macOS
**Language:** Rust 1.50+
**Framework/Library:** Standard Library
**Additional Dependencies:** None

```rust
fn main() {
    let original = "Hello WORLD";
    let lowercase = original.to_lowercase();
    println!("{}", lowercase);
    // Output: hello world

    // Works with Unicode
    let unicode = "ΑΛΦΑ";  // Greek letters
    println!("{}", unicode.to_lowercase());
    // Output: αλφα

    // Note: to_lowercase() returns a String, not &str
    let owned: String = original.to_lowercase();
}
```

**Notes:**
- `to_lowercase()` returns a new `String` (heap-allocated)
- Correctly handles Unicode case mapping
- Some characters may expand when lowercased (e.g., 'İ' becomes multiple bytes)
- For ASCII-only strings, consider `to_ascii_lowercase()` for better performance
- For case-insensitive comparison, use `eq_ignore_ascii_case()` or convert both strings

**References:**
- [str::to_lowercase - Rust Documentation](https://doc.rust-lang.org/std/primitive.str.html#method.to_lowercase)
- [String::to_lowercase - Rust Documentation](https://doc.rust-lang.org/std/string/struct.String.html#method.to_lowercase)

---

### Cross-platform - PHP - Standard Library
**Platform:** Windows, Linux, macOS
**Language:** PHP 7.4+
**Framework/Library:** Standard Library
**Additional Dependencies:** None

```php
<?php
$original = "Hello WORLD";
$lowercase = strtolower($original);
echo $lowercase;
// Output: hello world

// Multibyte-safe version (for Unicode)
$unicode = "ΑΛΦΑ";  // Greek letters
$mb_lowercase = mb_strtolower($unicode, 'UTF-8');
echo $mb_lowercase;
// Output: αλφα

// Locale-aware conversion
setlocale(LC_CTYPE, 'tr_TR.UTF-8');
$locale_lowercase = strtolower("TITLE");
?>
```

**Notes:**
- `strtolower()` works well for ASCII characters
- Use `mb_strtolower()` for multibyte/Unicode strings to avoid data corruption
- Always specify encoding ('UTF-8') with multibyte functions
- `strtolower()` is locale-aware based on `LC_CTYPE` setting
- For case-insensitive comparison, use `strcasecmp()` or `mb_strtolower()` comparison

**References:**
- [strtolower - PHP Documentation](https://www.php.net/manual/en/function.strtolower.php)
- [mb_strtolower - PHP Documentation](https://www.php.net/manual/en/function.mb-strtolower.php)
