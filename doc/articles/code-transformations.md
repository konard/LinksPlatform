# Code Transformation Snippets Database

A community-editable database of code transformation examples across different platforms, languages, frameworks, and environments.

## Table of Contents
* [Introduction](#introduction)
* [How to Use](#how-to-use)
* [How to Contribute](#how-to-contribute)
* [Transformation Categories](#transformation-categories)

## Introduction

This database provides examples of common data transformations across different programming environments. Each transformation includes:
- The specific transformation being performed
- Platform and version information
- Language and version
- Relevant frameworks, libraries, or tools
- Complete, working code examples

## How to Use

Browse the transformation categories below to find examples for your specific use case. Each transformation is organized by:
1. **Operation Type** (e.g., string manipulation, data conversion)
2. **Platform** (e.g., Windows, Linux, macOS, Web)
3. **Language** (e.g., C#, Python, JavaScript)
4. **Environment** (specific frameworks, libraries, or runtime versions)

## How to Contribute

To add a new transformation example:

1. Choose the appropriate category or create a new one
2. Follow the template structure provided in each category
3. Include complete, tested code examples
4. Specify all relevant version information
5. Add references to official documentation where applicable
6. Submit a pull request with your additions

See [Contributing Guidelines](code-transformations-contributing.md) for detailed instructions.

## Transformation Categories

### String Transformations
- [Convert String to Lowercase](transformations/string/to-lowercase.md)
- [Convert String to Uppercase](transformations/string/to-uppercase.md)
- [Trim Whitespace](transformations/string/trim.md)
- [String Concatenation](transformations/string/concatenation.md)
- [String Splitting](transformations/string/split.md)

### Data Type Conversions
- [String to Integer](transformations/conversion/string-to-integer.md)
- [Integer to String](transformations/conversion/integer-to-string.md)
- [String to Date/Time](transformations/conversion/string-to-datetime.md)
- [Array to List](transformations/conversion/array-to-list.md)

### Collection Operations
- [Filter Collection](transformations/collections/filter.md)
- [Map/Transform Collection](transformations/collections/map.md)
- [Reduce/Aggregate Collection](transformations/collections/reduce.md)
- [Sort Collection](transformations/collections/sort.md)

### File Operations
- [Read File](transformations/file/read.md)
- [Write File](transformations/file/write.md)
- [Copy File](transformations/file/copy.md)
- [Delete File](transformations/file/delete.md)

### Encoding/Decoding
- [Base64 Encode](transformations/encoding/base64-encode.md)
- [Base64 Decode](transformations/encoding/base64-decode.md)
- [URL Encode](transformations/encoding/url-encode.md)
- [JSON Serialize](transformations/encoding/json-serialize.md)
- [JSON Deserialize](transformations/encoding/json-deserialize.md)

## Template for New Transformations

When creating a new transformation page, use this structure:

```markdown
# [Transformation Name]

## Description
Brief description of what this transformation does.

## Examples

### [Platform] - [Language] - [Framework/Library]
**Platform:** [OS name and version]
**Language:** [Language name and version]
**Framework/Library:** [Framework/library name and version]
**Additional Dependencies:** [Any additional tools or packages]

```[language]
// Code example here
```

**Notes:**
- Any important notes about this implementation
- Performance considerations
- Edge cases to be aware of

**References:**
- [Official documentation link]
```
