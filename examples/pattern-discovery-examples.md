# Pattern Discovery Extension - Usage Examples

This document provides examples of how the Pattern Discovery Extension detects and suggests repetitive code patterns.

## Example 1: Repetitive Method Creation

When you create similar methods multiple times:

```csharp
// First method
public void ProcessOrder(Order order)
{
    ValidateOrder(order);
    SaveOrder(order);
}

// Second method (similar pattern)
public void ProcessInvoice(Invoice invoice)
{
    ValidateInvoice(invoice);
    SaveInvoice(invoice);
}
```

**Pattern Detected**: The extension will detect the pattern of creating methods with Validate-Save structure and offer to repeat it for the next entity.

## Example 2: Property Addition Pattern

Adding multiple properties with similar structure:

```csharp
private string _firstName;
public string FirstName
{
    get => _firstName;
    set => _firstName = value;
}

private string _lastName;
public string LastName
{
    get => _lastName;
    set => _lastName = value;
}
```

**Pattern Detected**: The extension identifies the backing field + property pattern and suggests repeating it.

## Example 3: Parameter Validation Pattern

Adding validation checks to multiple methods:

```csharp
public void Method1(string arg1)
{
    if (string.IsNullOrEmpty(arg1))
        throw new ArgumentNullException(nameof(arg1));
    // method body
}

public void Method2(string arg2)
{
    if (string.IsNullOrEmpty(arg2))
        throw new ArgumentNullException(nameof(arg2));
    // method body
}
```

**Pattern Detected**: The extension recognizes the null-check pattern and offers to apply it to other methods.

## Example 4: Interface Implementation

Implementing similar interface members:

```csharp
public int GetCount() => _items.Count;
public bool IsEmpty() => _items.Count == 0;
public void Clear() => _items.Clear();
```

**Pattern Detected**: When implementing interface members with delegation to a backing collection, the extension can suggest continuing the pattern.

## How It Works

1. **AST Analysis**: The extension uses Roslyn to analyze the Abstract Syntax Tree of your code changes
2. **Pattern Detection**: It identifies when similar structural changes repeat
3. **Notification**: When a well-formed pattern is detected (2+ occurrences with 70%+ confidence), you'll see a notification
4. **Repetition**: You can choose to apply the pattern again at your current cursor position

## Configuration

Pattern detection works automatically in the background. The extension:
- Analyzes only C# files
- Maintains a history of the last 100 changes
- Looks for patterns of length 1-10 changes
- Shows suggestions at most once per 30 seconds
- Requires 80% similarity between changes to form a pattern

## Benefits

- **Productivity**: Reduce repetitive typing
- **Consistency**: Maintain consistent code structure
- **Learning**: Become aware of your coding patterns
- **Refactoring Hints**: Repeated patterns may indicate opportunities for abstraction
