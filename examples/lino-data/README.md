# Lino Import/Export Examples

This directory contains example Lino (Links Notation) files for testing the importer and exporter.

## What is Lino?

Lino (Links Notation) is a lightweight, human-readable format for representing links and references. It's designed to be:
- **Natural**: Most text can be parsed as links notation
- **Flexible**: Supports doublets, triplets, and N-tuples
- **Universal**: Can represent complex hierarchical data structures

## Syntax Examples

### Doublets (2-tuple)
```lino
papa loves mama
son loves mama
```

### Named Links
```lino
lovesMama (loves mama)
papa lovesMama
```

### Triplets (3-tuple)
```lino
papa has car
mama has house
```

### N-tuple Links
```lino
family (papa mama son daughter)
relationship (papa loves mama)
```

### References
```lino
father: papa
mother: mama
```

## Using the Importer

The Lino importer reads .lino files and imports them into Links platform storage:

```bash
# Using the CLI
dotnet run --project Platform.Examples LinoImporterCLI [links-file] [lino-file]

# Example
dotnet run --project Platform.Examples LinoImporterCLI data.links examples/lino-data/sample.lino
```

## Using the Exporter

The Lino exporter exports Links platform storage to .lino format:

```bash
# Using the CLI
dotnet run --project Platform.Examples LinoExporterCLI [links-file] [output-lino-file] [unicode-mapped] [convert-to-text]

# Example
dotnet run --project Platform.Examples LinoExporterCLI data.links output.lino true true
```

## Sample Files

- `sample.lino`: Comprehensive example demonstrating various Lino syntax patterns
- `debug.lino`: Simple file for quick debugging and testing

## Format Specification

### Comments
Lines starting with `#` are treated as comments:
```lino
# This is a comment
```

### Simple Links
Space-separated values create doublets:
```lino
source target
```

### Parenthetical Notation
Parentheses group multiple values:
```lino
identifier (value1 value2 value3)
(value1 value2)
```

### References
Colon syntax creates references to existing links:
```lino
alias: existingIdentifier
```

### Quoted Strings
Use quotes for values with spaces or special characters:
```lino
person (name "John Doe" age 30)
```

## For Debugging

The issue #590 mentions this format is "Good for debug". Lino's human-readable format makes it excellent for:
- Debugging link structures
- Visualizing relationships
- Quick data prototyping
- Manual data inspection
- Educational purposes

## Related Links

- [Platform.Protocols.Lino on NuGet](https://www.nuget.org/packages/Platform.Protocols.Lino)
- [Protocols.Lino GitHub Repository](https://github.com/linksplatform/Protocols.Lino)
- [Links Platform Documentation](https://linksplatform.github.io/)
