# Auto-Complete Algorithm Based on Stored Sequences

## Overview

This implementation provides an auto-complete algorithm for the LinksPlatform that works with sequences stored in the doublets (associative memory) structure. The algorithm can find sequences that start with a given prefix and suggest possible completions.

## Architecture

### Core Components

1. **SequenceAutoCompleter<TLink>** - The main class implementing the autocomplete logic
2. **SequenceAutoCompleterCLI** - Command-line interface for interactive demonstrations
3. **Test/Experiment Scripts** - Located in `experiments/` directory for validation

## How It Works

### Sequence Representation

In LinksPlatform, sequences are represented as tree structures built from doublets (pairs). Each doublet has:
- **Source**: First element or sub-sequence
- **Target**: Second element or sub-sequence

For example, the sequence "hello" can be represented as:
```
h -> (e -> (l -> (l -> o)))
```

### Algorithm Steps

#### 1. Finding Completions

```
FindCompletions(prefix):
  1. For each link in storage:
     a. Extract the flat sequence from the link
     b. Check if the sequence starts with the prefix
     c. If yes, add to results
  2. Return all matching sequences
```

#### 2. Finding Next Elements

```
FindNextElements(prefix):
  1. For each link in storage:
     a. Extract the flat sequence
     b. If sequence starts with prefix AND has more elements:
        - Add the element at position [prefix.length] to results
  2. Return unique next elements
```

#### 3. Sequence Extraction

The algorithm traverses the doublet tree structure recursively:

```
ExtractSequence(linkIndex):
  1. Get the link at linkIndex
  2. If link is a "point" (self-referencing), it's a leaf element
  3. Otherwise, recursively traverse source and target
  4. Collect all leaf elements in order
```

## Usage Examples

### Programmatic Usage

```csharp
using var links = new UnitedMemoryLinks<ulong>();
var autoCompleter = new SequenceAutoCompleter<ulong>(links);

// Store some sequences
// ... (create and store sequences)

// Find completions
var prefix = new List<ulong> { h, e };  // "he"
var completions = autoCompleter.FindCompletions(prefix);

// Find next possible elements
var nextElements = autoCompleter.FindNextElements(prefix);
```

### CLI Usage

```bash
# Run the CLI
dotnet run SequenceAutoCompleterCLI

# Commands:
> add hello
> add help
> add world
> complete he    # Finds: hello, help
> next hel       # Suggests: l, p
> list           # Shows stored sequences
```

## Performance Considerations

### Current Implementation
- **Time Complexity**: O(n * m) where n = number of links, m = average sequence length
- **Space Complexity**: O(k) where k = number of matching sequences

### Optimization Opportunities

1. **Indexing**: Add prefix tree (trie) index for faster lookups
2. **Caching**: Cache frequently accessed sequences
3. **Early Termination**: Stop extraction once prefix mismatch is found
4. **Parallel Processing**: Use parallel search for large datasets

## Integration with Links Theory

This implementation aligns with the Links Theory concepts described in `doc/articles/links-theory.md`:

1. **Associative Memory**: Uses doublets as fundamental storage unit
2. **Sequence Compression**: Leverages shared sub-sequences to save space
3. **Tree Structures**: Represents sequences as balanced or ladder structures
4. **Universal Data Model**: Works with any data that can be represented as sequences

## Testing

Test scripts are available in `experiments/autocomplete_test.cs`:

```bash
cd experiments
csc autocomplete_test.cs
./autocomplete_test
```

## Future Enhancements

1. **Fuzzy Matching**: Support approximate prefix matching
2. **Ranking**: Score completions by frequency or relevance
3. **Unicode Support**: Better integration with UnicodeMap for text sequences
4. **Persistence**: Save/load autocomplete index for faster startup
5. **Streaming**: Support incremental sequence building
6. **Context-Aware**: Consider surrounding context for better suggestions

## References

- Links Theory: `doc/articles/links-theory.md`
- Sequence Handling: `Platform.Examples/CSVSequencesExporter.cs`
- Doublets Documentation: Platform.Data.Doublets package
