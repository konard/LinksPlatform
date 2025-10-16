# Triple Links Java Example

This example demonstrates how to use the Platform.Data.Triplets.Kernel native library from Java using JNA (Java Native Access).

## What are Triple Links?

Triple links are a data structure where each link consists of three components:
- **Source**: The starting point or subject of the link
- **Linker**: The relationship or predicate connecting source and target
- **Target**: The ending point or object of the link

This structure allows building semantic networks and graph databases with rich relationships.

## Prerequisites

1. **Java Development Kit (JDK)** - version 8 or higher
2. **JNA (Java Native Access)** - version 4.2.1 or higher
3. **Platform.Data.Triplets.Kernel native library** - compiled for your platform

### Getting JNA

Download JNA from: https://github.com/java-native-access/jna/releases

Or if using Maven, add to your `pom.xml`:
```xml
<dependency>
    <groupId>net.java.dev.jna</groupId>
    <artifactId>jna</artifactId>
    <version>5.13.0</version>
</dependency>
```

### Getting the Native Library

The Platform.Data.Triplets.Kernel native library can be obtained from:
https://github.com/linksplatform/Data.Triplets.Kernel

#### Building on Linux
```bash
git clone https://github.com/linksplatform/Data.Triplets.Kernel
cd Data.Triplets.Kernel/Platform.Data.Triplets.Kernel
make
```

This will produce `libPlatform_Data_Triplets_Kernel.so`

#### Building on Windows
```bash
# Using Visual Studio or MinGW
# Follow the instructions in the repository README
```

#### Building on macOS
```bash
git clone https://github.com/linksplatform/Data.Triplets.Kernel
cd Data.Triplets.Kernel/Platform.Data.Triplets.Kernel
make
```

This will produce `libPlatform_Data_Triplets_Kernel.dylib`

## Compiling the Example

### Linux/macOS

```bash
# Compile with JNA in current directory
javac -classpath .:jna-5.13.0.jar TripletsLibrary.java TripleLinksExample.java

# Or if JNA is in a different location
javac -classpath .:/path/to/jna-5.13.0.jar TripletsLibrary.java TripleLinksExample.java
```

### Windows

```cmd
javac -classpath .;jna-5.13.0.jar TripletsLibrary.java TripleLinksExample.java
```

## Running the Example

Before running, ensure the native library is in your library path:

### Linux

```bash
# Option 1: Copy library to system path
sudo cp libPlatform_Data_Triplets_Kernel.so /usr/local/lib/

# Option 2: Add current directory to library path
export LD_LIBRARY_PATH=.:$LD_LIBRARY_PATH

# Run the example
java -classpath .:jna-5.13.0.jar TripleLinksExample
```

### macOS

```bash
# Option 1: Copy library to system path
sudo cp libPlatform_Data_Triplets_Kernel.dylib /usr/local/lib/

# Option 2: Add current directory to library path
export DYLD_LIBRARY_PATH=.:$DYLD_LIBRARY_PATH

# Run the example
java -classpath .:jna-5.13.0.jar TripleLinksExample
```

### Windows

```cmd
REM Ensure the DLL is in the same directory or in PATH
java -classpath .;jna-5.13.0.jar TripleLinksExample
```

## What the Example Demonstrates

The example shows:

1. **Opening and closing a database** - Managing persistent storage
2. **Creating simple links** - Basic link creation with self-references
3. **Building a semantic network** - Creating relationships like "isA" and "isNotA"
4. **Updating links** - Modifying existing link properties
5. **Querying referers** - Finding links that reference other links
6. **Database statistics** - Getting total link counts
7. **Walking through links** - Iterating over all links in the database
8. **Searching for links** - Finding links by their components
9. **Cleanup** - Properly deleting links and closing the database

## Expected Output

When you run the example, you should see output similar to:

```
=== Triple Links Example ===

Opening database: examples.db
Database opened successfully.

--- Example 1: Creating a simple link ---
Created link with index: 1
  Source: 0
  Linker: 0
  Target: 0

--- Example 2: Building a semantic network ---
Created 'isA' relationship link: 2
Created 'isNotA' relationship link: 3
Created 'link' concept: 4
Created 'thing' concept: 5

--- Example 3: Updating a link ---
Updating 'isA' link to reference 'link' as target
Updated link index: 2
  Source: 2
  Linker: 2
  Target: 4
The minimal system core is now formed.

[... more output ...]

=== Example completed ===
```

## Understanding the Code

### TripletsLibrary.java

This file defines the JNA interface to the native library. It declares all the C functions available in Platform.Data.Triplets.Kernel:

- Memory management: `OpenLinks`, `CloseLinks`, `AllocateLink`, `FreeLink`
- Link operations: `CreateLink`, `UpdateLink`, `DeleteLink`, `SearchLink`
- Queries: `GetSourceIndex`, `GetLinkerIndex`, `GetTargetIndex`
- Referer queries: Functions to find links that reference a given link
- Database operations: `GetLinksCount`, `WalkThroughAllLinks`

### TripleLinksExample.java

This file demonstrates practical usage of the library:

- It creates a minimal semantic network with "isA" relationships
- Shows how links can reference other links to build complex structures
- Demonstrates querying and traversal operations
- Properly handles resource cleanup

## Common Issues

### Library Not Found

If you get an error like:
```
java.lang.UnsatisfiedLinkError: Unable to load library 'Platform_Data_Triplets_Kernel'
```

Solutions:
1. Ensure the native library is compiled for your platform
2. Check that the library is in your system's library path
3. On Linux/macOS, try setting `LD_LIBRARY_PATH` or `DYLD_LIBRARY_PATH`
4. On Windows, ensure the DLL is in the same directory as your Java files or in PATH

### ClassNotFoundException

If you get:
```
Error: Could not find or load main class TripleLinksExample
```

Ensure you're running from the correct directory and the classpath includes the current directory (`.`).

## Further Reading

- [Links Platform Documentation](https://github.com/linksplatform/Documentation)
- [Data.Triplets Repository](https://github.com/linksplatform/Data.Triplets)
- [Data.Triplets.Kernel Repository](https://github.com/linksplatform/Data.Triplets.Kernel)
- [JNA Documentation](https://github.com/java-native-access/jna)

## License

This example is part of the Links Platform project and is licensed under the same terms.

## Support

For questions and support:
- Stack Overflow tag: `links-platform`
- [Official Discord Server](https://discord.gg/links-platform)
