# Contributing to Code Transformations Database

Thank you for your interest in contributing to the Code Transformations Database! This guide will help you add new transformation examples or improve existing ones.

## Table of Contents
* [Getting Started](#getting-started)
* [Adding a New Transformation](#adding-a-new-transformation)
* [Adding an Example to Existing Transformation](#adding-an-example-to-existing-transformation)
* [Quality Guidelines](#quality-guidelines)
* [Submission Process](#submission-process)

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally
3. **Create a new branch** for your contribution
   ```bash
   git checkout -b add-transformation-[name]
   ```

## Adding a New Transformation

### 1. Choose the Right Category

Transformations are organized into categories:
- `string/` - String manipulation operations
- `conversion/` - Data type conversions
- `collections/` - Collection/array operations
- `file/` - File system operations
- `encoding/` - Encoding and serialization

If your transformation doesn't fit existing categories, propose a new category in your pull request.

### 2. Create the Transformation File

Create a new markdown file in the appropriate category:
```
doc/articles/transformations/[category]/[transformation-name].md
```

### 3. Use the Template

```markdown
# [Transformation Name]

## Description
A clear, concise description of what this transformation does and when it's useful.

## Examples

### [Platform] - [Language] - [Framework/Library]
**Platform:** [Operating System and version, e.g., "Windows 10", "Ubuntu 20.04", "macOS 12", "Web Browser"]
**Language:** [Programming language and version, e.g., "Python 3.9", "C# 10.0", "JavaScript ES2021"]
**Framework/Library:** [Framework or library name and version, e.g., ".NET 6.0", "Node.js 16.x", "Django 4.0"]
**Additional Dependencies:** [Any other required tools, packages, or extensions]

[language]
// Your complete, working code example
// Include comments to explain non-obvious parts
// Make sure the code is ready to copy and paste


**Notes:**
- Any important implementation details
- Performance characteristics
- Common pitfalls or edge cases
- When to use this approach vs alternatives

**References:**
- [Official Documentation](URL)
- [Related StackOverflow Discussion](URL) (if applicable)
```

## Adding an Example to Existing Transformation

If a transformation page already exists but lacks examples for your platform/language combination:

1. Open the existing transformation file
2. Add a new section following the template above
3. Ensure your example provides unique value (different platform, language, or approach)

## Quality Guidelines

### Code Examples Must:
- ✅ Be complete and self-contained
- ✅ Be tested and verified to work
- ✅ Include error handling where appropriate
- ✅ Follow the language's standard conventions and best practices
- ✅ Include comments for complex or non-obvious code
- ✅ Specify exact version numbers for all dependencies

### Code Examples Should:
- 📝 Be concise while remaining clear
- 📝 Demonstrate idiomatic usage for the language
- 📝 Include multiple approaches if they differ significantly
- 📝 Note performance implications when relevant
- 📝 Reference official documentation

### Avoid:
- ❌ Incomplete code snippets that won't run without modification
- ❌ Deprecated or outdated approaches without noting them as such
- ❌ Code with security vulnerabilities
- ❌ Unnecessary complexity or over-engineering
- ❌ Vague version specifications (e.g., "Python 3.x" instead of "Python 3.9+")

## Submission Process

### 1. Test Your Code
Before submitting, verify:
- The code runs without errors
- All dependencies are clearly listed
- Version numbers are accurate

### 2. Commit Your Changes
```bash
git add doc/articles/transformations/[category]/[file].md
git commit -m "Add [transformation name] for [platform/language]"
```

### 3. Push to Your Fork
```bash
git push origin add-transformation-[name]
```

### 4. Create a Pull Request
1. Go to the original repository on GitHub
2. Click "New Pull Request"
3. Select your fork and branch
4. Fill in the PR template with:
   - What transformation you're adding
   - What platforms/languages are covered
   - Any special notes about your examples

### 5. Respond to Feedback
Maintainers may suggest improvements or ask questions. Be responsive and collaborative!

## Example Contribution

Here's a complete example of adding a new transformation:

**File:** `doc/articles/transformations/string/to-lowercase.md`

```markdown
# Convert String to Lowercase

## Description
Converts all characters in a string to their lowercase equivalents. This is commonly used for case-insensitive comparisons, normalization, and data processing.

## Examples

### Cross-platform - C# - .NET 6.0
**Platform:** Windows, Linux, macOS
**Language:** C# 10.0
**Framework/Library:** .NET 6.0
**Additional Dependencies:** None

csharp
string original = "Hello WORLD";
string lowercase = original.ToLower();
// Result: "hello world"

// Culture-aware (recommended for international text)
using System.Globalization;
string lowercaseInvariant = original.ToLower(CultureInfo.InvariantCulture);


**Notes:**
- `ToLower()` uses the current culture by default
- Use `ToLower(CultureInfo.InvariantCulture)` for culture-independent operations
- For Turkish locale, 'I' lowercases to 'ı' (dotless i), not 'i'

**References:**
- [String.ToLower Method - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/api/system.string.tolower)

### Cross-platform - Python - Standard Library
**Platform:** Windows, Linux, macOS
**Language:** Python 3.9+
**Framework/Library:** Standard Library
**Additional Dependencies:** None

python
original = "Hello WORLD"
lowercase = original.lower()
# Result: "hello world"


**Notes:**
- Python's `lower()` handles Unicode characters correctly
- Works with all Python string types (str)
- For case-insensitive comparisons, consider `casefold()` which is more aggressive

**References:**
- [str.lower() - Python Documentation](https://docs.python.org/3/library/stdtypes.html#str.lower)
```

## Questions?

If you have questions about contributing, please:
- Open an issue with the "question" label
- Reach out on our community channels
- Review existing transformations for examples

Thank you for helping make this database comprehensive and useful for developers worldwide!
