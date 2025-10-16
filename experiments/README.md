# Experiments

This folder contains experimental code and examples for testing LinksPlatform features.

## Tag File System Example

The `tag-filesystem-example.cs` demonstrates the Tag File System implementation that solves issue #176.

### How to run

```bash
# Compile and run (example - adjust paths as needed)
dotnet run --project tag-filesystem-example.cs
```

### What it demonstrates

- Creating files in a flat collection
- Adding multiple tags to files
- Creating tag inheritance hierarchies (implementing path-like structures)
- Searching for files by single tag
- Searching for files by multiple tags (AND operation)
- Listing tags for a file
- Navigating tag hierarchies (parent/child relationships)

### Key Concepts

1. **Flat File Collection**: All files are stored in one collection, not in hierarchical directories
2. **Multiple Tags per File**: Each file can have unlimited tags
3. **Tag Inheritance**: Tags can inherit from other tags, allowing path-like structures (e.g., personal <- vacation <- summer)
4. **Efficient Search**: Find files by single or multiple tags using associative memory
