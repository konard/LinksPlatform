# Platform.Data.EntityFramework

EntityFramework Core database provider for LinksPlatform.

## Overview

This package provides an Entity Framework Core database provider that enables using LinksPlatform's associative data storage (Links/Doublets) as the backend for EF Core applications.

## Installation

```bash
dotnet add package Platform.Data.EntityFramework
```

## Basic Usage

```csharp
using Microsoft.EntityFrameworkCore;
using Platform.Data.EntityFramework.Extensions;

public class MyContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLinks("MyLinksDatabase");
    }

    public DbSet<MyEntity> MyEntities { get; set; }
}
```

## Features

- **EF Core Integration**: Use familiar EF Core APIs with Links storage
- **LINQ Support**: Query your data using LINQ expressions
- **Multiple Target Frameworks**: Supports .NET Standard 2.0, 2.1, and .NET 6.0

## Current Status

This is an initial implementation of the EF Core provider for Links. The following features are currently supported:

- Basic provider infrastructure
- DbContext configuration
- Provider registration and service setup

### Planned Features

- Full CRUD operations mapping to ILinks interface
- Advanced LINQ query translation
- Change tracking integration
- Entity relationships and navigation properties
- Migrations support

## Architecture

The provider follows the standard EF Core provider architecture:

- **Infrastructure**: Core provider configuration and options
- **Storage**: Database abstraction layer
- **Query**: LINQ query translation components
- **Extensions**: Helper methods for DbContext configuration

## Contributing

See the main [LinksPlatform repository](https://github.com/Konard/LinksPlatform) for contribution guidelines.

## License

This project is licensed under the same license as LinksPlatform - see the [LICENSE](https://github.com/Konard/LinksPlatform/blob/master/LICENSE) file for details.

## Related Issues

- [Issue #195: EntityFramework Database Provider for Links](https://github.com/Konard/LinksPlatform/issues/195)
- [Issue #165: LINQ to Links](https://github.com/Konard/LinksPlatform/issues/165)

## References

- [Writing an EF Core Database Provider](https://learn.microsoft.com/en-us/ef/core/providers/writing-a-provider)
- [Platform.Data.Doublets](https://github.com/linksplatform/Data.Doublets)
