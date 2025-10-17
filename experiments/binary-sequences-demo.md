# Binary Sequences File Format Demo

This document demonstrates the usage of the Binary Sequences file format implementation.

## Overview

The binary sequences file format provides a compact way to backup or transmit sequences data. The format contains two main sections:

1. **Links Section**: Stores duplicate/reusable subsequences
2. **Sequences Section**: Stores unique uncompressable sequences

## File Format Structure

```
[File Header]
- Magic bytes: "BSEQ" (4 bytes)
- Version: 1 (2 bytes)

[Links Section]
- Section header: "LINK" (4 bytes)
- Count: variable-length encoded number of reusable links
- Links: pairs of (source, target) as variable-length encoded values

[Sequences Section]
- Section header: "SEQS" (4 bytes)
- Count: variable-length encoded number of unique sequences
- Sequences: pairs of (source, target) as variable-length encoded values
```

## Variable-Length Encoding

The format uses variable-length encoding for integers to save space:
- Each byte contains 7 bits of data
- The high bit (0x80) is the continuation flag
- If continuation flag is set, more bytes follow
- Numbers are encoded in little-endian 7-bit chunks

## Usage Examples

### Export to Binary Sequences Format

```csharp
using var links = new UnitedMemoryLinks<ulong>("data.links");
var synchronizedLinks = new SynchronizedLinks<ulong>(links);

var exporter = new BinarySequencesExporter();
exporter.Export(synchronizedLinks, "output.bseq");
```

### Import from Binary Sequences Format

```csharp
using var links = new UnitedMemoryLinks<ulong>("data.links");
var synchronizedLinks = new SynchronizedLinks<ulong>(links);

var importer = new BinarySequencesImporter();
importer.Import(synchronizedLinks, "input.bseq");
```

### Command Line Usage

Export:
```bash
BinarySequencesExporterCLI data.links output.bseq
```

Import:
```bash
BinarySequencesImporterCLI input.bseq data.links
```

## Key Features

1. **Compact Storage**: Variable-length encoding minimizes file size
2. **Deduplication**: Reusable subsequences are stored once in the links section
3. **Efficiency**: References to reusable links use high-bit flag (0x8000000000000000) to distinguish from direct values
4. **Simplicity**: Two-section structure is easy to parse and extend

## Related Issues

- [#320 Binary Sequences file format](https://github.com/konard/LinksPlatform/issues/320)
- [#29 Links data structure as a binary data protocol](https://github.com/konard/LinksPlatform/issues/29)
- [#194 Universal Links String file format](https://github.com/konard/LinksPlatform/issues/194)
