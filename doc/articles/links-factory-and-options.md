# LinksFactory and LinksOptions Design

## Overview

`LinksFactory` and `LinksOptions` are components designed to simplify the creation and configuration of Links storage instances with appropriate decorator combinations. They provide a fluent API for selecting storage backends, memory management strategies, and behavioral decorators.

## Historical Context

These classes were previously part of the Platform.Data.Core.Doublets namespace but were removed during refactoring. The core functionality has since moved to the [linksplatform/Data.Doublets](https://github.com/linksplatform/Data.Doublets) repository.

See historical implementation: [LinksFactory.cs (da1fb25)](https://github.com/Konard/LinksPlatform/blob/da1fb252c11d32ae8a937db7bd4b9ddbc4031a56/Platform/Platform.Data.Core/Doublets/LinksFactory.cs#L10)

## Purpose

The primary purpose of `LinksFactory` is to:

1. **Simplify instantiation** - Reduce boilerplate code when creating Links storage instances
2. **Configure decorators** - Enable easy selection of decorator combinations based on use case
3. **Provide defaults** - Offer sensible default configurations for common scenarios
4. **Type safety** - Ensure decorator combinations are valid at compile time

## Architecture

### LinksOptions<TLinkAddress>

`LinksOptions` encapsulates configuration parameters for creating a Links storage instance.

**Key Properties:**

```csharp
public interface ILinksOptions<TLinkAddress>
{
    // Storage Backend
    IResizableDirectMemory? Memory { get; set; }
    string? FilePath { get; set; }
    long InitialSize { get; set; }

    // Decorator Flags
    bool UseUniquenessValidation { get; set; }
    bool UseUniquenessResolution { get; set; }
    bool UseCascadeUsagesResolution { get; set; }
    bool UseCascadeUniquenessAndUsagesResolution { get; set; }
    bool UseNonExistentDependenciesCreation { get; set; }
    bool UseLogging { get; set; }
    bool UseNoExceptions { get; set; }

    // Constants Configuration
    LinksConstants<TLinkAddress>? Constants { get; set; }

    // Advanced Options
    bool UseItselfConstantResolver { get; set; }
    bool UseNullConstantResolver { get; set; }
    bool UseInnerReferenceValidator { get; set; }
}
```

### LinksFactory<TLinkAddress>

`LinksFactory` uses `ILinksOptions<TLinkAddress>` to construct the appropriate decorator chain.

**Decorator Composition Order (from innermost to outermost):**

1. **Memory Layer** - `UnitedMemoryLinks<TLinkAddress>` or `SplitMemoryLinks<TLinkAddress>`
2. **Reference Resolution** - `LinksItselfConstantToSelfReferenceResolver` or `LinksNullConstantToSelfReferenceResolver`
3. **Validation Layer** - `LinksInnerReferenceExistenceValidator`, `LinksUniquenessValidator`, `LinksUsagesValidator`
4. **Resolution Layer** - `LinksUniquenessResolver`, `LinksCascadeUsagesResolver`, `LinksCascadeUniquenessAndUsagesResolver`
5. **Dependency Creation** - `LinksNonExistentDependenciesCreator`
6. **Error Handling** - `NoExceptionsDecorator`
7. **Observability** - `LoggingDecorator`

## Available Decorators

### Validation Decorators

| Decorator | Purpose | When to Use |
|-----------|---------|-------------|
| `LinksUniquenessValidator` | Prevents creation of duplicate links | Strict data integrity requirements |
| `LinksUsagesValidator` | Prevents deletion of links that are referenced | Prevent dangling references |
| `LinksInnerReferenceExistenceValidator` | Validates that source/target links exist | Ensure referential integrity |

### Resolution Decorators

| Decorator | Purpose | When to Use |
|-----------|---------|-------------|
| `LinksUniquenessResolver` | Returns existing link instead of creating duplicate | Auto-deduplication |
| `LinksCascadeUsagesResolver` | Automatically deletes dependent links | Cascade delete behavior |
| `LinksCascadeUniquenessAndUsagesResolver` | Combines uniqueness and cascade resolution | Complex resolution scenarios |

### Utility Decorators

| Decorator | Purpose | When to Use |
|-----------|---------|-------------|
| `LinksNonExistentDependenciesCreator` | Auto-creates missing referenced links | Simplified API, auto-vivification |
| `NoExceptionsDecorator` | Converts exceptions to error codes | Performance-critical paths |
| `LoggingDecorator` | Logs all operations | Debugging and audit trails |

## Usage Examples

### Example 1: Simple In-Memory Storage

```csharp
var options = new LinksOptions<ulong>
{
    Memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep),
    UseUniquenessResolution = true
};

var factory = new LinksFactory<ulong>(options);
ILinks<ulong> links = factory.Create();
```

### Example 2: File-Backed Storage with Full Validation

```csharp
var options = new LinksOptions<ulong>
{
    FilePath = "links.db",
    InitialSize = 1024 * 1024 * 100, // 100 MB
    UseUniquenessValidation = true,
    UseInnerReferenceValidator = true,
    UseLogging = true
};

var factory = new LinksFactory<ulong>(options);
using ILinks<ulong> links = factory.Create();
```

### Example 3: High-Performance Configuration

```csharp
var options = new LinksOptions<ulong>
{
    Memory = new HeapResizableDirectMemory(),
    UseNoExceptions = true,
    UseUniquenessResolution = true,
    UseNonExistentDependenciesCreation = true
};

var factory = new LinksFactory<ulong>(options);
ILinks<ulong> links = factory.Create();
```

### Example 4: Development/Debug Configuration

```csharp
var options = new LinksOptions<ulong>
{
    FilePath = "debug-links.db",
    UseUniquenessValidation = true,
    UseCascadeUsagesResolution = true,
    UseInnerReferenceValidator = true,
    UseLogging = true
};

var factory = new LinksFactory<ulong>(options);
using ILinks<ulong> links = factory.Create();
```

## Implementation Considerations

### Decorator Order Matters

The order in which decorators are applied is critical:

```
User Code
    ↓
LoggingDecorator (observe all operations)
    ↓
NoExceptionsDecorator (handle errors gracefully)
    ↓
LinksNonExistentDependenciesCreator (auto-create missing refs)
    ↓
LinksCascadeUniquenessAndUsagesResolver (resolve complex scenarios)
    ↓
LinksUniquenessResolver (deduplicate)
    ↓
LinksUsagesValidator (check for usages before delete)
    ↓
LinksUniquenessValidator (check for duplicates)
    ↓
LinksInnerReferenceExistenceValidator (validate references exist)
    ↓
LinksItselfConstantToSelfReferenceResolver (handle self-references)
    ↓
UnitedMemoryLinks (actual storage)
```

### Performance Trade-offs

| Configuration | Performance | Safety | Use Case |
|--------------|-------------|--------|----------|
| Minimal (no decorators) | Fastest | Lowest | Performance benchmarks, trusted code |
| Validators only | Fast | Medium | Testing, development |
| Resolvers + Validators | Medium | High | Production systems |
| All decorators + Logging | Slowest | Highest | Debugging, audit systems |

### Memory Management

```csharp
public class LinksFactory<TLinkAddress> : IFactory<ILinks<TLinkAddress>>, IDisposable
{
    private IResizableDirectMemory? _ownedMemory;

    public ILinks<TLinkAddress> Create()
    {
        IResizableDirectMemory memory;

        if (_options.Memory != null)
        {
            memory = _options.Memory;
        }
        else if (!string.IsNullOrEmpty(_options.FilePath))
        {
            _ownedMemory = new FileMappedResizableDirectMemory(_options.FilePath, _options.InitialSize);
            memory = _ownedMemory;
        }
        else
        {
            _ownedMemory = new HeapResizableDirectMemory(_options.InitialSize);
            memory = _ownedMemory;
        }

        ILinks<TLinkAddress> links = new UnitedMemoryLinks<TLinkAddress>(memory);

        // Apply decorators based on options...

        return links;
    }

    public void Dispose()
    {
        _ownedMemory?.Dispose();
    }
}
```

## Preset Configurations

For common scenarios, preset configurations should be provided:

```csharp
public static class LinksPresets
{
    public static LinksOptions<TLinkAddress> Production<TLinkAddress>() => new()
    {
        UseUniquenessResolution = true,
        UseCascadeUsagesResolution = true,
        UseInnerReferenceValidator = true,
        UseNoExceptions = true
    };

    public static LinksOptions<TLinkAddress> Development<TLinkAddress>() => new()
    {
        UseUniquenessValidation = true,
        UseCascadeUsagesResolution = true,
        UseInnerReferenceValidator = true,
        UseLogging = true
    };

    public static LinksOptions<TLinkAddress> HighPerformance<TLinkAddress>() => new()
    {
        UseNoExceptions = true,
        UseUniquenessResolution = true
    };

    public static LinksOptions<TLinkAddress> StrictValidation<TLinkAddress>() => new()
    {
        UseUniquenessValidation = true,
        UseInnerReferenceValidator = true,
        UseLogging = true
    };
}
```

## Testing Strategy

### Unit Tests

Each decorator combination should be tested:

```csharp
[Fact]
public void FactoryCreatesUniquenessValidator_WhenOptionSet()
{
    var options = new LinksOptions<ulong>
    {
        Memory = new HeapResizableDirectMemory(),
        UseUniquenessValidation = true
    };

    var factory = new LinksFactory<ulong>(options);
    var links = factory.Create();

    var link1 = links.Create(1, 2);
    Assert.Throws<LinkAlreadyExistsException>(() => links.Create(1, 2));
}

[Fact]
public void FactoryCreatesUniquenessResolver_WhenOptionSet()
{
    var options = new LinksOptions<ulong>
    {
        Memory = new HeapResizableDirectMemory(),
        UseUniquenessResolution = true
    };

    var factory = new LinksFactory<ulong>(options);
    var links = factory.Create();

    var link1 = links.Create(1, 2);
    var link2 = links.Create(1, 2);
    Assert.Equal(link1, link2);
}
```

### Integration Tests

Test common decorator combinations:

```csharp
[Theory]
[InlineData(true, true, false)]
[InlineData(true, false, true)]
[InlineData(false, true, true)]
public void FactorySupportsDecoratorCombinations(bool uniqueness, bool cascade, bool logging)
{
    var options = new LinksOptions<ulong>
    {
        Memory = new HeapResizableDirectMemory(),
        UseUniquenessResolution = uniqueness,
        UseCascadeUsagesResolution = cascade,
        UseLogging = logging
    };

    var factory = new LinksFactory<ulong>(options);
    var links = factory.Create();

    // Perform operations and verify behavior
    Assert.NotNull(links);
}
```

## Migration Path

For codebases using direct instantiation:

**Before:**
```csharp
var memory = new HeapResizableDirectMemory();
var links = new UnitedMemoryLinks<ulong>(memory);
var validatedLinks = new LinksUniquenessValidator<ulong>(links);
var resolvedLinks = new LinksUniquenessResolver<ulong>(validatedLinks);
```

**After:**
```csharp
var options = new LinksOptions<ulong>
{
    UseUniquenessValidation = true,
    UseUniquenessResolution = true
};
var links = new LinksFactory<ulong>(options).Create();
```

## Future Enhancements

1. **Fluent API** - `LinksOptionsBuilder` for more intuitive configuration
2. **Profiles** - Named configuration profiles (e.g., "Production", "Development")
3. **Dependency Injection** - Integration with DI containers
4. **Dynamic Reconfiguration** - Change decorator stack at runtime
5. **Performance Monitoring** - Built-in telemetry and metrics
6. **Configuration Files** - Load options from JSON/YAML

## References

- [Historical LinksFactory Implementation](https://github.com/Konard/LinksPlatform/blob/da1fb252c11d32ae8a937db7bd4b9ddbc4031a56/Platform/Platform.Data.Core/Doublets/LinksFactory.cs#L10)
- [Data.Doublets Repository](https://github.com/linksplatform/Data.Doublets)
- [Platform.Data.Doublets Decorators](https://github.com/linksplatform/Data.Doublets/tree/main/csharp/Platform.Data.Doublets/Decorators)
- [Issue #430](https://github.com/konard/LinksPlatform/issues/430)

## Recommended Implementation Repository

This design should be implemented in the [linksplatform/Data.Doublets](https://github.com/linksplatform/Data.Doublets) repository, as it contains:
- All decorator implementations
- Memory management infrastructure
- Existing unit test framework
- Active maintenance and CI/CD

## Conclusion

`LinksFactory` and `LinksOptions` provide a powerful, flexible way to configure Links storage instances. By abstracting decorator composition, they make it easier for developers to create correctly configured storage instances without deep knowledge of the decorator pattern or the specific order in which decorators must be applied.

The factory pattern also enables future optimizations, such as decorator caching, lazy initialization, and dynamic reconfiguration, without changing the public API.
