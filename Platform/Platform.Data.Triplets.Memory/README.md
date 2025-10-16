# Platform.Data.Triplets.Memory

A modern C# implementation of triplet (triple/tuple) links for associative data storage.

## Overview

This library provides an in-memory implementation of triplet links, where each link consists of three components:
- **Source**: The starting point of the relationship
- **Linker**: The type or nature of the relationship
- **Target**: The ending point of the relationship

Each link can reference other links, forming a graph structure that enables powerful associative data representations.

## Features

- **Triplet Structure**: Each link has Source, Linker, and Target properties
- **Automatic Referrer Tracking**: Links automatically track what other links reference them
- **Multiple Factory Methods**: Convenient creation patterns for common link structures
- **Deduplication**: Automatically reuses existing links with the same components
- **Recursive Deletion**: Deleting a link cascades to all dependent links
- **Extension Methods**: Rich set of query and analysis methods
- **Modern C#**: Uses nullable reference types, modern patterns, and best practices

## Basic Usage

```csharp
using Platform.Data.Triplets.Memory;

// Create basic links
var source = new Link();
var linker = new Link();
var target = new Link();

// Create a relationship
var link = Link.Create(source, linker, target);

// Query referers
foreach (var referer in target.ReferersByTarget)
{
    Console.WriteLine($"Link referencing target: {referer}");
}

// Delete a link and all its dependents
link.Delete();
```

## Factory Methods

The library provides several factory methods for common link patterns:

- `Create(source, linker, target)` - Creates or retrieves a link with the specified components
- `CreateLinkLinkingItself()` - Creates a link that references itself in all three positions
- `CreateOutcomingSelfLinker(target)` - Creates a link where source and linker are the link itself
- `CreateCycleSelfLink(linker)` - Creates a link where source and target are the link itself
- `CreateSelfLinker(source, target)` - Creates a link where linker is the link itself
- And more...

## Extension Methods

Extension methods provide convenient querying capabilities:

- `GetAllReferers()` - Returns all links that reference this link
- `CountReferers()` - Counts all referers
- `HasReferers()` - Checks if the link has any referers
- `IsSelfReference()` - Checks if the link references itself
- `IsCompleteSelfLoop()` - Checks if the link references itself in all positions
- `FindReferer(source, linker, target)` - Finds a specific referer
- `FindReferers(source, linker, target)` - Finds all matching referers

## Design Principles

This implementation is a modern reimplementation of the original 2010 Links Platform concept with:

1. **Clean separation of concerns** - Core logic, factory methods, and extensions are separated
2. **Type safety** - Uses C# 8.0+ nullable reference types
3. **Performance** - Efficient linked-list-based referrer tracking
4. **Simplicity** - Clear, maintainable code with comprehensive documentation

## Relationship to Platform.Data.Triplets

This library (`Platform.Data.Triplets.Memory`) is an in-memory implementation that demonstrates
the core concepts of triplet links. The `Platform.Data.Triplets` NuGet package provides a more
complete implementation with persistent storage capabilities.

## License

This project is part of the Links Platform and follows the same license as the parent repository.
