# Coding Conventions

This document describes the coding conventions used across LinksPlatform projects.

## Table of Contents
* [C++ Conventions](#c-conventions)
  * [Class Member Declaration Order](#class-member-declaration-order)
  * [Pointer and Reference Declaration Style](#pointer-and-reference-declaration-style)

## C++ Conventions

### Class Member Declaration Order

Use a conventional order of members when declaring a class to improve readability.

Reference: [C++ Core Guidelines NL.16](https://isocpp.github.io/CppCoreGuidelines/CppCoreGuidelines#nl16-use-a-conventional-class-member-declaration-order)

**Order of member types:**
1. Types: classes, enums, and aliases (`using`)
2. Constructors, assignments, destructor
3. Functions
4. Data

**Access specifier order:**

Use `public` before `protected` before `private` order.

**Example:**

```cpp
class X {
public:
    // types
    using value_type = int;
    enum class Status { Active, Inactive };

    // constructors, assignments, destructor
    X();
    X(const X&);
    X& operator=(const X&);
    ~X();

    // functions
    void DoSomething();
    int GetValue() const;

    // data
    int publicValue;

protected:
    // protected interface for derived classes
    void ProtectedHelper();
    int protectedValue;

private:
    // implementation details
    void PrivateImplementation();
    int privateValue;
};
```

### Pointer and Reference Declaration Style

Place the asterisk and ampersand close to the **variable name**, not the type.

**Correct:**

```cpp
Link *source;
Link *target;
int &reference;
```

**Incorrect:**

```cpp
Link* source;
Link* target;
int& reference;
```

**Rationale:**

This style emphasizes that the pointer/reference nature applies to the specific variable being declared, which is particularly important when declaring multiple variables in C-style declarations.

**Note:** This convention differs from the C++ Core Guidelines NL.18 recommendation, but is the preferred style for LinksPlatform projects.

**Example:**

```cpp
// Multiple pointer declarations
Link *first, *second, *third;

// Single pointer declaration
Link *node;

// Reference declaration
void Process(Link &item);

// Const pointer
const Link *constPointer;

// Pointer to const
Link * const pointerToConst;
```
