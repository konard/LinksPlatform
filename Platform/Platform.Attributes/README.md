# Platform.Attributes

LinksPlatform's Platform.Attributes Class Library

## Description

This library provides custom attributes for the LinksPlatform ecosystem.

## Features

### ExperimentalAttribute

Marks APIs as experimental and subject to change. This attribute can be applied to various code elements to indicate that they are not yet stable and may be modified or removed in future versions.

**Usage:**

```csharp
using Platform.Attributes;

[Experimental("LP0001")]
public class MyExperimentalClass
{
    [Experimental("LP0002", UrlFormat = "https://github.com/konard/LinksPlatform/issues/{0}")]
    public void ExperimentalMethod()
    {
        // Experimental implementation
    }
}
```

**Note:** This is a custom implementation compatible with older .NET versions. For .NET 8+ and C# 12+, consider using the built-in `System.Diagnostics.CodeAnalysis.ExperimentalAttribute` which provides compiler warnings.

## Installation

```bash
dotnet add package Platform.Attributes
```

## Links

- [GitHub Repository](https://github.com/Konard/LinksPlatform)
- [Issue #635](https://github.com/konard/LinksPlatform/issues/635)
