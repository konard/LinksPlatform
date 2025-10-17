# Filter Collection

## Description
Creates a new collection containing only elements that satisfy a specified condition or predicate. This is one of the most common operations in functional programming and data processing.

## Examples

### Cross-platform - C# - .NET 6.0+
**Platform:** Windows, Linux, macOS
**Language:** C# 10.0+
**Framework/Library:** .NET 6.0+ (LINQ)
**Additional Dependencies:** None

```csharp
using System;
using System.Linq;
using System.Collections.Generic;

// Filter even numbers
var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
var evenNumbers = numbers.Where(n => n % 2 == 0).ToList();
// Result: [2, 4, 6, 8, 10]

// Filter strings by length
var words = new List<string> { "cat", "elephant", "dog", "butterfly" };
var longWords = words.Where(w => w.Length > 5).ToList();
// Result: ["elephant", "butterfly"]

// Filter with index
var filteredWithIndex = numbers
    .Where((value, index) => index % 2 == 0)
    .ToList();
// Result: [1, 3, 5, 7, 9] (elements at even indices)

// Filter objects by property
public record Person(string Name, int Age);
var people = new List<Person>
{
    new("Alice", 30),
    new("Bob", 25),
    new("Charlie", 35)
};
var adults = people.Where(p => p.Age >= 30).ToList();
// Result: [Person("Alice", 30), Person("Charlie", 35)]
```

**Notes:**
- `Where()` uses deferred execution (lazy evaluation)
- Call `ToList()` or `ToArray()` to materialize the result
- Can chain multiple `Where()` calls or use complex predicates
- Performance: O(n) where n is the number of elements
- For arrays, consider using `Array.FindAll()` as an alternative

**References:**
- [Enumerable.Where - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/api/system.linq.enumerable.where)
- [LINQ Query Expressions](https://docs.microsoft.com/en-us/dotnet/csharp/linq/)

---

### Cross-platform - Python - Standard Library
**Platform:** Windows, Linux, macOS
**Language:** Python 3.6+
**Framework/Library:** Standard Library
**Additional Dependencies:** None

```python
# Using list comprehension (most Pythonic)
numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
even_numbers = [n for n in numbers if n % 2 == 0]
# Result: [2, 4, 6, 8, 10]

# Using filter() function
even_numbers_filter = list(filter(lambda n: n % 2 == 0, numbers))
# Result: [2, 4, 6, 8, 10]

# Filter strings
words = ["cat", "elephant", "dog", "butterfly"]
long_words = [w for w in words if len(w) > 5]
# Result: ["elephant", "butterfly"]

# Filter with enumerate (access to index)
filtered_by_index = [val for idx, val in enumerate(numbers) if idx % 2 == 0]
# Result: [1, 3, 5, 7, 9]

# Filter dictionaries
people = [
    {"name": "Alice", "age": 30},
    {"name": "Bob", "age": 25},
    {"name": "Charlie", "age": 35}
]
adults = [p for p in people if p["age"] >= 30]
# Result: [{"name": "Alice", "age": 30}, {"name": "Charlie", "age": 35}]

# Filter with multiple conditions
complex_filter = [n for n in numbers if n % 2 == 0 and n > 5]
# Result: [6, 8, 10]
```

**Notes:**
- List comprehensions are generally preferred over `filter()` for readability
- `filter()` returns an iterator; wrap with `list()` to get a list
- Both approaches create new collections; original is unchanged
- List comprehensions can include complex expressions and multiple conditions
- Performance: O(n) where n is the number of elements

**References:**
- [List Comprehensions - Python Documentation](https://docs.python.org/3/tutorial/datastructures.html#list-comprehensions)
- [filter() - Python Documentation](https://docs.python.org/3/library/functions.html#filter)

---

### Web Browser - JavaScript - ES6+
**Platform:** Web Browser (All modern browsers)
**Language:** JavaScript ES6+
**Framework/Library:** None (Vanilla JavaScript)
**Additional Dependencies:** None

```javascript
// Using Array.filter()
const numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
const evenNumbers = numbers.filter(n => n % 2 === 0);
// Result: [2, 4, 6, 8, 10]

// Filter strings
const words = ["cat", "elephant", "dog", "butterfly"];
const longWords = words.filter(w => w.length > 5);
// Result: ["elephant", "butterfly"]

// Filter with index and array parameters
const filteredByIndex = numbers.filter((value, index, array) => {
    return index % 2 === 0;
});
// Result: [1, 3, 5, 7, 9]

// Filter objects
const people = [
    { name: "Alice", age: 30 },
    { name: "Bob", age: 25 },
    { name: "Charlie", age: 35 }
];
const adults = people.filter(p => p.age >= 30);
// Result: [{ name: "Alice", age: 30 }, { name: "Charlie", age: 35 }]

// Filter with multiple conditions
const complexFilter = numbers.filter(n => n % 2 === 0 && n > 5);
// Result: [6, 8, 10]

// Remove null/undefined values
const sparse = [1, null, 2, undefined, 3, 0, 4];
const cleaned = sparse.filter(x => x != null);  // Removes null and undefined
// Result: [1, 2, 3, 0, 4]

// Remove falsy values
const truthyOnly = sparse.filter(Boolean);
// Result: [1, 2, 3, 4]
```

**Notes:**
- `filter()` creates a new array; original array is not modified
- Callback receives three arguments: element, index, array
- Returns empty array if no elements match
- Performance: O(n) where n is the number of elements
- Use `Boolean` as predicate to remove all falsy values

**References:**
- [Array.prototype.filter() - MDN](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/filter)

---

### Cross-platform - Java - JDK 8+
**Platform:** Windows, Linux, macOS
**Language:** Java 8+
**Framework/Library:** Java Streams API
**Additional Dependencies:** None

```java
import java.util.*;
import java.util.stream.*;

// Filter even numbers
List<Integer> numbers = Arrays.asList(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
List<Integer> evenNumbers = numbers.stream()
    .filter(n -> n % 2 == 0)
    .collect(Collectors.toList());
// Result: [2, 4, 6, 8, 10]

// Filter strings
List<String> words = Arrays.asList("cat", "elephant", "dog", "butterfly");
List<String> longWords = words.stream()
    .filter(w -> w.length() > 5)
    .collect(Collectors.toList());
// Result: ["elephant", "butterfly"]

// Filter objects
class Person {
    String name;
    int age;
    Person(String name, int age) {
        this.name = name;
        this.age = age;
    }
}

List<Person> people = Arrays.asList(
    new Person("Alice", 30),
    new Person("Bob", 25),
    new Person("Charlie", 35)
);
List<Person> adults = people.stream()
    .filter(p -> p.age >= 30)
    .collect(Collectors.toList());

// Multiple filters (can chain or combine)
List<Integer> complexFilter = numbers.stream()
    .filter(n -> n % 2 == 0)
    .filter(n -> n > 5)
    .collect(Collectors.toList());
// Result: [6, 8, 10]

// Remove nulls
List<Integer> withNulls = Arrays.asList(1, null, 2, null, 3);
List<Integer> noNulls = withNulls.stream()
    .filter(Objects::nonNull)
    .collect(Collectors.toList());
// Result: [1, 2, 3]
```

**Notes:**
- Streams API uses lazy evaluation; call a terminal operation to execute
- `collect(Collectors.toList())` materializes the stream into a list
- Can chain multiple `filter()` operations
- For parallel processing, use `parallelStream()` instead of `stream()`
- Performance: O(n) where n is the number of elements

**References:**
- [Stream.filter - Java Documentation](https://docs.oracle.com/javase/8/docs/api/java/util/stream/Stream.html#filter-java.util.function.Predicate-)
- [Collectors - Java Documentation](https://docs.oracle.com/javase/8/docs/api/java/util/stream/Collectors.html)

---

### Cross-platform - Go - Standard Library
**Platform:** Windows, Linux, macOS
**Language:** Go 1.16+
**Framework/Library:** Standard Library
**Additional Dependencies:** None

```go
package main

import "fmt"

// Manual filtering (Go doesn't have built-in filter)
func filterInts(numbers []int, predicate func(int) bool) []int {
    var result []int
    for _, num := range numbers {
        if predicate(num) {
            result = append(result, num)
        }
    }
    return result
}

func main() {
    numbers := []int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}

    // Filter even numbers
    evenNumbers := filterInts(numbers, func(n int) bool {
        return n%2 == 0
    })
    fmt.Println(evenNumbers)  // Output: [2 4 6 8 10]

    // Filter strings
    words := []string{"cat", "elephant", "dog", "butterfly"}
    var longWords []string
    for _, word := range words {
        if len(word) > 5 {
            longWords = append(longWords, word)
        }
    }
    fmt.Println(longWords)  // Output: [elephant butterfly]

    // Filter structs
    type Person struct {
        Name string
        Age  int
    }

    people := []Person{
        {"Alice", 30},
        {"Bob", 25},
        {"Charlie", 35},
    }

    var adults []Person
    for _, person := range people {
        if person.Age >= 30 {
            adults = append(adults, person)
        }
    }
    fmt.Println(adults)  // Output: [{Alice 30} {Charlie 35}]
}
```

**Notes:**
- Go doesn't have built-in filter function; must implement manually
- Generic filter function requires type parameters (Go 1.18+) or reflection
- Direct iteration with conditional append is idiomatic Go
- Pre-allocate slice capacity if final size is known for better performance
- Performance: O(n) where n is the number of elements

**References:**
- [Slices - Go Documentation](https://go.dev/blog/slices-intro)
- [Go Generics (1.18+)](https://go.dev/doc/tutorial/generics)

---

### Cross-platform - Rust - Standard Library
**Platform:** Windows, Linux, macOS
**Language:** Rust 1.50+
**Framework/Library:** Standard Library
**Additional Dependencies:** None

```rust
fn main() {
    // Filter even numbers
    let numbers = vec![1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    let even_numbers: Vec<i32> = numbers.iter()
        .filter(|&&n| n % 2 == 0)
        .copied()
        .collect();
    println!("{:?}", even_numbers);  // Output: [2, 4, 6, 8, 10]

    // Filter strings
    let words = vec!["cat", "elephant", "dog", "butterfly"];
    let long_words: Vec<&str> = words.iter()
        .filter(|&&w| w.len() > 5)
        .copied()
        .collect();
    println!("{:?}", long_words);  // Output: ["elephant", "butterfly"]

    // Filter with owned values
    let filtered: Vec<i32> = numbers.into_iter()
        .filter(|&n| n % 2 == 0)
        .collect();

    // Filter structs
    #[derive(Debug)]
    struct Person {
        name: String,
        age: u32,
    }

    let people = vec![
        Person { name: "Alice".to_string(), age: 30 },
        Person { name: "Bob".to_string(), age: 25 },
        Person { name: "Charlie".to_string(), age: 35 },
    ];

    let adults: Vec<&Person> = people.iter()
        .filter(|p| p.age >= 30)
        .collect();
    println!("{:?}", adults);

    // Filter with multiple conditions
    let complex: Vec<i32> = numbers.iter()
        .filter(|&&n| n % 2 == 0 && n > 5)
        .copied()
        .collect();
    println!("{:?}", complex);  // Output: [6, 8, 10]

    // Remove None values from Option
    let with_nones = vec![Some(1), None, Some(2), None, Some(3)];
    let no_nones: Vec<i32> = with_nones.into_iter()
        .filter_map(|x| x)
        .collect();
    println!("{:?}", no_nones);  // Output: [1, 2, 3]
}
```

**Notes:**
- `filter()` is an iterator adapter; returns an iterator
- Call `collect()` to materialize into a collection
- Use `copied()` or `cloned()` when filtering references
- `into_iter()` consumes the collection; `iter()` borrows it
- `filter_map()` combines filtering and mapping
- Performance: O(n) where n is the number of elements

**References:**
- [Iterator::filter - Rust Documentation](https://doc.rust-lang.org/std/iter/trait.Iterator.html#method.filter)
- [Iterator - Rust Documentation](https://doc.rust-lang.org/std/iter/trait.Iterator.html)
