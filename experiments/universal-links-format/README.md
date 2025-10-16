# Universal Links String Format

## Overview

The Universal Links String Format is a simple, human-readable file format for storing and transferring links of any size. It uses two base symbols to represent link structures:

1. **Link Part Separator**: Space character (` `)
2. **Link Separator**: Newline character (`\n`)

This format can represent:
- Pair links (doublets)
- Triple links (through composition)
- Sequences of any size
- Can be scaled down to Unicode-like single reference sequences

## Format Specification

Each line in the file represents one link with the following structure:

```
source target
```

Where:
- `source` and `target` are numeric identifiers (link addresses or indices)
- Space (` `) separates the source from the target
- Newline (`\n`) separates one link from another

## Examples

### Example 1: Simple Pair Links

```
1 2
2 3
3 1
```

This represents three links:
- Link at index/address 1: source=1, target=2
- Link at index/address 2: source=2, target=3
- Link at index/address 3: source=3, target=1

### Example 2: Building Sequences

Links can reference other links to build complex structures:

```
65 66
67 68
1 2
```

Here:
- Link 1 connects 65→66 (could represent "AB" if 65='A', 66='B')
- Link 2 connects 67→68 (could represent "CD" if 67='C', 68='D')
- Link 3 connects link 1→link 2 (creates sequence "ABCD")

### Example 3: Unicode-like Representation

When characters are mapped to link indices (Unicode mapping):

```
72 101
101 108
108 108
108 111
```

Could represent "Hello" where:
- 72 = 'H'
- 101 = 'e'
- 108 = 'l'
- 111 = 'o'

## Advantages

1. **Simplicity**: Only two special characters needed
2. **Human-readable**: Easy to view and edit with any text editor
3. **Universal**: Works for any link size and structure
4. **Scalable**: From simple pairs to complex nested structures
5. **Compatible**: Similar to Unicode but extended to multi-dimensional structures
6. **Efficient**: Compact representation without heavy markup

## Implementation

The format is implemented in Platform.Examples with:

- `UniversalLinksStringFormatExporter` - Export links to the format
- `UniversalLinksStringFormatImporter` - Import links from the format
- `UniversalLinksStringFormatExporterCLI` - Command-line exporter
- `UniversalLinksStringFormatImporterCLI` - Command-line importer

## Usage

### Exporting Links

```csharp
var exporter = new UniversalLinksStringFormatExporter();
exporter.Export(
    links: syncLinks,
    path: "output.links",
    unicodeMapped: true,
    convertUnicodeLinksToCharacters: false,
    referenceByLines: false,
    cancellationToken: cancellationToken
);
```

### Importing Links

```csharp
var importer = new UniversalLinksStringFormatImporter(
    links: syncLinks,
    useLineNumbersAsIndices: false
);
importer.Import("input.links", cancellationToken);
```

## Comparison with Other Formats

### vs CSV
- **CSV**: Requires escaping for commas and quotes, more complex parsing
- **Universal Links Format**: Only two simple separators, no escaping needed for most cases

### vs JSON/XML
- **JSON/XML**: Heavy markup, harder to read, larger file size
- **Universal Links Format**: Minimal markup, easy to read, compact

### vs Binary Protocols
- **Binary**: Not human-readable, requires special tools
- **Universal Links Format**: Human-readable, editable with any text editor

## Relation to Links Theory

This format aligns with the Links Platform theory where:
- Everything is represented as links (associations)
- Links can reference other links
- Complex structures emerge from simple binary relationships

The format provides a textual representation that mirrors the internal doublet structure, making it ideal for:
- Data interchange
- Debugging and inspection
- Human-readable storage
- Cross-platform compatibility

## Future Extensions

Possible extensions while maintaining backward compatibility:
- Support for link metadata (through special marker links)
- Compression using reference counting
- Unicode character literals for mapped links
- Comments (lines starting with `#`)
