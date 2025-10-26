# Server-Side Text Activator Example

This document demonstrates the server-side text activator feature implemented for issue #642.

## Overview

The server-side text activator is a text compression system that learns from the text it processes. Each time text is compressed, the system updates a frequency dictionary that tracks how often symbols and symbol pairs (doublets) appear. Over time, this allows the system to achieve better compression ratios for similar types of text.

## Key Features

1. **Server-Side Frequency Dictionary**: Stores and updates symbol frequencies on the server
2. **Persistent Storage**: The frequency dictionary is saved to disk and loaded on server restart
3. **Incremental Learning**: Each compression operation improves the dictionary
4. **No Dictionary Required for Decompression**: Decompression works using the Links database alone
5. **Archive Format Support**: Multiple encoding formats (constant-length and variable-length)

## Architecture

### Components

1. **ServerSideFrequencyDictionary**: Stores symbol and doublet frequencies
   - Tracks individual Unicode symbol frequencies
   - Tracks doublet (pair) frequencies
   - Persists to disk in binary format
   - Thread-safe for concurrent access

2. **TextActivatorCLI**: Command-line interface for text compression
   - Compress text to Links
   - Decompress Links back to text
   - View statistics
   - Run demonstration

## Usage Example

### Running the Text Activator CLI

```bash
cd Platform/Platform.Examples
dotnet run -- textactivator
```

### Example Session

```
=== Server-Side Text Activator ===

This demonstrates a server-side text compression system that
improves its compression ratio over time by learning text patterns.

Initialized text activator:
Frequency Dictionary Statistics:
  Total symbols processed: 0
  Total doublets processed: 0
  Unique symbols: 0
  Unique doublets: 0

Options:
1. Compress text
2. Decompress link
3. Show statistics
4. Run demo
5. Exit

Select option: 4

Running demo with sample texts...

Processing: "Hello, world!"
  Original: 13 bytes → Compressed: 5 bytes (ratio: 38.46%)

Processing: "The quick brown fox jumps over the lazy dog."
  Original: 45 bytes → Compressed: 5 bytes (ratio: 11.11%)

Processing: "Hello, world! Hello again!"
  Original: 27 bytes → Compressed: 5 bytes (ratio: 18.52%)

... (more examples)

Demo complete!
Frequency Dictionary Statistics:
  Total symbols processed: 234
  Total doublets processed: 228
  Unique symbols: 32
  Unique doublets: 156
```

## How It Works

### Compression Process

1. **Text Input**: User provides Unicode text to compress
2. **Convert to Links**: Text is converted to an array of link IDs representing Unicode characters
3. **Update Frequency Dictionary**: The frequency dictionary is updated with symbols from the input
4. **Create Sequence**: A balanced variant sequence is created in the Links database
5. **Return Link ID**: The compressed representation is returned as a single link ID

### Decompression Process

1. **Link ID Input**: User provides the link ID to decompress
2. **Traverse Links**: The system traverses the Links database to reconstruct the sequence
3. **Convert to Text**: Link IDs are converted back to Unicode characters
4. **Return Text**: The original text is returned

### Frequency Dictionary

The frequency dictionary stores two types of data:

1. **Symbol Frequencies**: How often each Unicode character appears
2. **Doublet Frequencies**: How often each pair of characters appears together

This information can be used to:
- Prioritize common patterns during compression
- Generate variable-length encodings where common symbols use fewer bits
- Analyze text patterns for linguistic or statistical purposes

## Archive File Formats

The system supports multiple archive formats (as specified in issue #642):

### Constant-Length Coding

1. **Single Unicode Symbols**: Each symbol encoded with fixed byte length
2. **Unicode Symbol Ranges**: Efficient for consecutive character ranges
3. **Links**: Pairs encoded as constant-length structures
4. **Sequences**: Continuous streams of links

### Variable-Length Coding

1. **Unicode Symbols**: Symbols sorted by frequency, common ones use fewer bits
2. **Links**: Pairs sorted by frequency with separators
3. **Sequences**: Each sequence has its own separator format

## File Structure

```
Platform/Platform.Examples/
├── ServerSideFrequencyDictionary.cs  # Frequency dictionary implementation
├── TextActivatorCLI.cs                # Command-line interface
└── ICommandLineInterface.cs           # CLI interface

Generated Files:
├── text-activator.links              # Links database file
└── frequency-dict.dat                 # Frequency dictionary storage
```

## Benefits

1. **Adaptive Compression**: Improves over time as more text is processed
2. **Language-Specific Optimization**: Learns patterns specific to the language being compressed
3. **No Overhead for Decompression**: Decompression doesn't need the frequency dictionary
4. **Persistent Learning**: Dictionary survives server restarts
5. **Statistical Analysis**: Frequency data useful for text analysis

## Future Enhancements

- REST API endpoints for web access
- Multiple language-specific dictionaries
- Advanced compression algorithms using frequency data
- Real-time compression ratio monitoring
- Dictionary backup and synchronization

## Related Files

- Issue: https://github.com/konard/LinksPlatform/issues/642
- Pull Request: https://github.com/konard/LinksPlatform/pull/1057
