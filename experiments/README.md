# Experiments

This directory contains experimental scripts and test files for verifying the data importers functionality.

## Available Importers

### JSON Importer
- Imports JSON files into Links storage
- Preserves hierarchical structure
- Usage: See `JsonImporterCLI.cs`

### FileSystem Importer
- Imports directory and file structures
- Can optionally include file contents
- Usage: See `FileSystemImporterCLI.cs`

### XML Importer (Existing)
- Imports XML files into Links storage
- Usage: See `XmlImporterCLI.cs`

## Testing

Sample test data is available in the `examples/` directory.
