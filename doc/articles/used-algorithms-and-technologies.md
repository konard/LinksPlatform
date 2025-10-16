# List of Used Algorithms, Methods, and Technologies

This document lists the algorithms, methods, and technologies developed by other people that are used in the LinksPlatform project.

## Table of Contents
* [Core Data Structures and Algorithms](#core-data-structures-and-algorithms)
* [.NET Technologies](#net-technologies)
* [Web Technologies](#web-technologies)
* [Communication Protocols and Formats](#communication-protocols-and-formats)
* [Development Tools](#development-tools)
* [Runtime Technologies](#runtime-technologies)

## Core Data Structures and Algorithms

### Binary Search Trees
- **Used in**: `Platform/Platform.Sandbox/TreeStructureExperiments.cs`
- **Purpose**: Tree-based data structure implementations with threaded binary tree variants
- **Key algorithms**: Tree rotation (left/right), binary search, tree traversal
- **Developer**: Various (classic computer science algorithms)

### Graph Algorithms
- **Used in**: Various Doublets and Triplets implementations
- **Purpose**: Graph traversal and manipulation in associative data storage
- **Key concepts**: Directed graphs, node relationships, graph export
- **Developer**: Various (graph theory fundamentals)

### Memory-Mapped Files
- **Used in**: Platform.Memory library
- **Purpose**: Non-volatile storage for links/associations
- **Technology**: System.IO.MemoryMappedFiles
- **Developer**: Microsoft (.NET Framework)

### Compression and String Algorithms
- **Used in**: `Platform/Platform.Sandbox/CompressionExperiments.cs`, `Platform/Platform.Sandbox/AllRepeatingSubstringsInString.cs`
- **Purpose**: Data compression and substring pattern recognition
- **Developer**: Various (information theory and string algorithms)

## .NET Technologies

### .NET Core / .NET Standard
- **Version**: .NET Core 2.2, .NET Standard 2.0
- **Purpose**: Cross-platform runtime and standard library
- **Developer**: Microsoft
- **Website**: https://dotnet.microsoft.com/

### ASP.NET Core
- **Version**: 2.2
- **Used in**: `Platform.Data.WebTerminal`
- **Purpose**: Web application framework for HTTP APIs and web interfaces
- **Key components**:
  - MVC (Model-View-Controller) pattern
  - Razor templating engine
  - Dependency injection
  - HTTP request pipeline
- **Developer**: Microsoft
- **Website**: https://dotnet.microsoft.com/apps/aspnet

### System Libraries
The project extensively uses .NET System libraries:

- **System.Collections.Generic**: Generic collections (lists, dictionaries, etc.)
- **System.Collections.Concurrent**: Thread-safe collections
- **System.Linq**: Language Integrated Query for data manipulation
- **System.Linq.Expressions**: Expression trees for dynamic code generation
- **System.Reflection**: Runtime type inspection and dynamic invocation
- **System.Reflection.Emit**: Dynamic code generation
- **System.Threading**: Multi-threading and synchronization
- **System.Threading.Tasks**: Task-based asynchronous programming
- **System.Text.RegularExpressions**: Regular expression pattern matching
- **System.Runtime.CompilerServices**: Compiler services and unsafe operations
- **System.Runtime.InteropServices**: Platform invocation and marshalling
- **System.Diagnostics**: Debugging and performance monitoring
- **System.Xml**: XML parsing and manipulation

### xUnit
- **Version**: 2.4.1 (runner utility)
- **Used in**: `Platform.Examples`
- **Purpose**: Unit testing framework
- **Developer**: xUnit.net community
- **Website**: https://xunit.net/

## Web Technologies

### JavaScript Libraries

#### jQuery
- **Used in**: `Platform.Data.WebTerminal/wwwroot/lib/jquery`
- **Purpose**: DOM manipulation and AJAX requests
- **Developer**: jQuery Foundation
- **Website**: https://jquery.com/

#### Bootstrap
- **Used in**: `Platform.Data.WebTerminal/wwwroot/lib/bootstrap`
- **Purpose**: Responsive UI framework and components
- **Developer**: Twitter / Bootstrap team
- **Website**: https://getbootstrap.com/

#### jQuery Validation
- **Used in**: `Platform.Data.WebTerminal/wwwroot/lib/jquery-validation`
- **Purpose**: Client-side form validation
- **Developer**: jQuery Validation Plugin team
- **Website**: https://jqueryvalidation.org/

#### jQuery Validation Unobtrusive
- **Used in**: `Platform.Data.WebTerminal/wwwroot/lib/jquery-validation-unobtrusive`
- **Purpose**: Integration of jQuery Validation with ASP.NET MVC
- **Developer**: Microsoft

## Communication Protocols and Formats

### GEXF (Graph Exchange XML Format)
- **Used in**: `Platform.Communication.Protocol.Gexf`, `GEXFExporter.cs`
- **Purpose**: Export graph data for visualization tools like Gephi
- **Format specification**: XML-based graph description format
- **Developer**: Gephi Consortium
- **Website**: https://gephi.org/gexf/format/

### UDP (User Datagram Protocol)
- **Used in**: `Platform.Communication.Protocol.Udp`
- **Purpose**: Network communication for distributed link storage
- **Technology**: System.Net.Sockets
- **Developer**: Internet Engineering Task Force (IETF standard)

### XML (eXtensible Markup Language)
- **Used in**: Multiple locations for data serialization
- **Purpose**: Data exchange and configuration
- **Technology**: System.Xml
- **Developer**: W3C (World Wide Web Consortium)

### HTTP/HTTPS
- **Used in**: Web terminal and communication modules
- **Technology**: System.Net.Http
- **Purpose**: Web service communication
- **Developer**: IETF / W3C

## Development Tools

### DocFX
- **Config file**: `docfx.json`
- **Purpose**: Documentation generation from source code and markdown
- **Developer**: Microsoft
- **Website**: https://dotnet.github.io/docfx/

### Git
- **Used throughout**: Version control system
- **Purpose**: Source code management and collaboration
- **Developer**: Linus Torvalds and the Git community
- **Website**: https://git-scm.com/

### GitHub
- **Used throughout**: Code hosting and collaboration platform
- **Purpose**: Repository hosting, issue tracking, CI/CD
- **Developer**: GitHub Inc. (Microsoft)
- **Website**: https://github.com

### Travis CI
- **Used in**: CI/CD pipeline (referenced in README.md)
- **Purpose**: Continuous integration and testing
- **Developer**: Travis CI
- **Website**: https://travis-ci.org/

## Runtime Technologies

### Common Language Runtime (CLR)
- **Part of**: .NET Framework / .NET Core
- **Purpose**: Managed code execution, garbage collection, JIT compilation
- **Developer**: Microsoft

### Unsafe Code and Pointers
- **Used in**: Multiple projects (AllowUnsafeBlocks=true)
- **Purpose**: Direct memory manipulation for performance-critical operations
- **Technology**: C# unsafe keyword, pointers
- **Developer**: Microsoft (C# language feature)

### File System APIs
- **Used in**: Platform.IO library
- **Technology**: System.IO
- **Purpose**: File and directory operations
- **Developer**: Microsoft

## Platform-Specific Libraries

The following are LinksPlatform's own libraries that build upon external technologies:

### Core Platform Libraries
1. **Platform.Data.Doublets** - Binary associative storage implementation
2. **Platform.Data.Triplets** - Ternary associative storage implementation
3. **Platform.Memory** - Memory management abstractions
4. **Platform.Communication** - Communication protocol implementations
5. **Platform.Counters** - Counting and statistics utilities
6. **Platform.IO** - I/O operations abstractions
7. **Platform.Threading** - Threading utilities
8. **Platform.Collections** - Collection data structures
9. **Platform.Converters** - Type conversion utilities
10. **Platform.Exceptions** - Exception handling utilities
11. **Platform.Disposables** - Resource disposal patterns
12. **Platform.Reflection** - Reflection utilities
13. **Platform.Numbers** - Numeric operations
14. **Platform.Singletons** - Singleton pattern implementations

## Pattern Recognition for Automatic List Generation

As mentioned in issue #137, this list was created through pattern recognition in the source code by analyzing:

1. **NuGet package references** in `.csproj` files
2. **Using statements** in C# source files
3. **Third-party library folders** in wwwroot
4. **Configuration files** (docfx.json, etc.)
5. **Algorithm implementations** in source code
6. **Documentation references** in README and other docs

This methodology can be recreated for other open-source projects by:
- Scanning package manager configuration files (package.json, requirements.txt, pom.xml, etc.)
- Analyzing import/using/include statements in source files
- Examining third-party library directories
- Reviewing build configuration files
- Identifying well-known algorithm implementations in code

## References

- [LinksPlatform GitHub Organization](https://github.com/linksplatform)
- [LinksPlatform Main Repository](https://github.com/konard/LinksPlatform)
- [LinksPlatform Documentation](https://github.com/linksplatform/Documentation)

## License

This documentation is part of the LinksPlatform project and follows the same license.
