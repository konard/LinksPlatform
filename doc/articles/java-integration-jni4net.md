# Java Integration using jni4net

## Table of Contents
* [Introduction](#introduction)
* [What is jni4net](#what-is-jni4net)
* [Prerequisites](#prerequisites)
* [Setup](#setup)
* [Basic Usage](#basic-usage)
* [Integration Approaches](#integration-approaches)
* [Examples](#examples)
* [Troubleshooting](#troubleshooting)
* [Resources](#resources)

## Introduction

This guide explains how to integrate Java applications with LinksPlatform using [jni4net](https://github.com/jni4net/jni4net), a fast object-oriented bridge between the Java Virtual Machine (JVM) and the .NET Common Language Runtime (CLR).

jni4net enables bidirectional calling between Java and .NET, allowing Java applications to use LinksPlatform's powerful associative data storage and processing capabilities.

## What is jni4net

jni4net is a software library that provides:
- **Bidirectional Interoperability**: Call .NET code from Java and vice versa
- **Object-Oriented Bridge**: Seamless object mapping between JVM and CLR
- **Intraprocess Communication**: Fast, in-process method invocations without serialization overhead
- **Type Safety**: Maintains type information across language boundaries

## Prerequisites

### System Requirements
- **Operating System**: Windows (Linux/Mono currently not supported by jni4net)
- **.NET Framework**: SDK 3.5 and 4.0 or higher
- **Java**: JDK 1.5 or higher (JDK 8+ recommended)
- **LinksPlatform**: Any LinksPlatform package (e.g., Platform.Data.Doublets)

### Development Tools
- IDE: Visual Studio, Rider, or any Java IDE (IntelliJ IDEA, Eclipse)
- Build tools: MSBuild, Maven or Gradle (optional)

## Setup

### Step 1: Install jni4net

1. Download the latest jni4net binary package from [SourceForge](https://sourceforge.net/projects/jni4net/)
2. Extract the package to a directory (e.g., `C:\jni4net`)
3. Add jni4net binaries to your project's classpath

### Step 2: Install LinksPlatform Packages

Install the required LinksPlatform NuGet packages in your .NET project:

```bash
dotnet add package Platform.Data.Doublets
dotnet add package Platform.Data
```

### Step 3: Generate Java Proxies

Use jni4net's proxygen tool to generate Java proxy classes for LinksPlatform .NET assemblies:

```bash
proxygen.exe YourLinksPlatformAssembly.dll -wd output_directory
```

This generates Java interfaces and JNI bridge code that allows Java to call LinksPlatform methods.

## Basic Usage

### Initialize jni4net Bridge

In your Java application, initialize the jni4net bridge:

```java
import net.sf.jni4net.Bridge;
import system.io.File;

public class LinksPlatformIntegration {
    static {
        try {
            // Initialize the bridge
            Bridge.init();

            // Load LinksPlatform assemblies
            Bridge.LoadAndRegisterAssemblyFrom(
                new File("Platform.Data.Doublets.dll")
            );
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
```

### Call LinksPlatform from Java

Once initialized, you can instantiate and use LinksPlatform classes:

```java
import platform.data.doublets.*;

public class DoubletsExample {
    public void createLinks() {
        // Create a memory-based doublets storage
        var links = new UnitedMemoryLinks<Long>();

        // Create a link
        long linkId = links.Create();

        // Create a link with specific source and target
        long link = links.Create(1L, 2L);

        // Search for links
        var query = links.Constants().Any();
        links.Each(result -> {
            System.out.println("Link: " + result);
            return true;
        }, query, query, query);
    }
}
```

## Integration Approaches

### Approach 1: Wrapper Library

Create a dedicated .NET wrapper library that exposes a simplified API for Java consumers:

1. Create a new .NET library project
2. Add LinksPlatform dependencies
3. Create wrapper classes with Java-friendly interfaces
4. Use jni4net proxygen to generate Java bindings
5. Package as a JAR file

**Benefits:**
- Clean separation of concerns
- Simplified Java API
- Better error handling and logging
- Version management

### Approach 2: Direct Integration

Directly use LinksPlatform assemblies from Java:

1. Reference LinksPlatform NuGet packages
2. Generate proxies for all needed assemblies
3. Use generated Java classes directly

**Benefits:**
- No additional wrapper code
- Direct access to all LinksPlatform features
- Faster development cycle

### Approach 3: Hybrid Approach

Combine both approaches:

1. Use direct integration for core functionality
2. Create wrappers for complex operations
3. Add Java-specific utilities and helpers

## Examples

### Example 1: Creating and Querying Links

```java
import platform.data.doublets.*;
import platform.data.*;

public class BasicLinksExample {
    public static void main(String[] args) {
        try {
            Bridge.init();
            Bridge.LoadAndRegisterAssemblyFrom(
                new File("Platform.Data.Doublets.dll")
            );

            // Create memory-based storage
            var links = new UnitedMemoryLinks<Long>();

            // Create links
            long link1 = links.Create();
            long link2 = links.Create();
            long connection = links.Create(link1, link2);

            System.out.println("Created link connecting " +
                link1 + " and " + link2 + ": " + connection);

            // Query links
            long[] result = new long[3];
            links.Each(linkArray -> {
                System.out.println("Found link: [" +
                    linkArray[0] + ", " +
                    linkArray[1] + ", " +
                    linkArray[2] + "]");
                return true; // Continue iteration
            }, links.Constants().Any(),
               links.Constants().Any(),
               links.Constants().Any());

        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
```

### Example 2: Persistent Storage

```java
import platform.data.doublets.*;
import java.io.File;

public class PersistentStorageExample {
    public static void main(String[] args) {
        try {
            Bridge.init();
            Bridge.LoadAndRegisterAssemblyFrom(
                new File("Platform.Data.Doublets.dll")
            );

            // Create file-based storage
            String dbPath = "links.db";
            var links = new UnitedMemoryLinks<Long>(dbPath);

            // Create persistent links
            long persistentLink = links.Create(100L, 200L);
            System.out.println("Created persistent link: " + persistentLink);

            // Links will be saved to file automatically

        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
```

### Example 3: Using Sequences

```java
import platform.data.doublets.*;
import platform.data.doublets.sequences.*;

public class SequencesExample {
    public static void main(String[] args) {
        try {
            Bridge.init();
            Bridge.LoadAndRegisterAssemblyFrom(
                new File("Platform.Data.Doublets.dll")
            );

            var links = new UnitedMemoryLinks<Long>();

            // Create a sequence of links
            long[] elements = {1L, 2L, 3L, 4L, 5L};
            var sequences = new Sequences(links);
            long sequenceLink = sequences.Create(elements);

            System.out.println("Created sequence: " + sequenceLink);

        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
```

## Troubleshooting

### Common Issues

#### Issue: `UnsatisfiedLinkError` or JNI library not found

**Solution:**
- Ensure jni4net native libraries are in your Java library path
- Add `-Djava.library.path=/path/to/jni4net` to JVM arguments
- Verify that native DLLs are in the same directory as your JAR

#### Issue: `TypeLoadException` when loading LinksPlatform assemblies

**Solution:**
- Ensure all LinksPlatform dependencies are present
- Load assemblies in the correct order (dependencies first)
- Check that .NET Framework version matches

#### Issue: Performance problems or high memory usage

**Solution:**
- Use `IDisposable` pattern properly (call dispose on links storage)
- Avoid excessive JNI calls in tight loops
- Consider batching operations
- Use memory-mapped files for large datasets

#### Issue: Platform compatibility (Linux/macOS)

**Note:** jni4net currently does not support Linux/Mono platforms. Alternative approaches:
- Use Platform.Data's native implementation if available
- Use REST API or gRPC for cross-platform integration
- Consider using .NET Core's native interop on Linux

### Debugging Tips

1. **Enable jni4net verbose logging:**
   ```java
   Bridge.setVerbose(true);
   ```

2. **Check assembly loading:**
   ```java
   Assembly asm = Bridge.LoadAndRegisterAssemblyFrom(new File("YourAssembly.dll"));
   System.out.println("Loaded: " + asm.toString());
   ```

3. **Use try-catch blocks extensively:**
   JNI errors can be cryptic; wrap all bridge calls in try-catch for better error messages.

## Resources

### Official Documentation
- [jni4net Official Website](http://jni4net.github.io)
- [LinksPlatform Documentation](https://linksplatform.github.io)
- [Platform.Data.Doublets API](https://linksplatform.github.io/Data.Doublets)

### Tutorials and Guides
- [How calling from Java to .NET works](http://zamboch.blogspot.cz/2009/11/how-calling-from-java-to-net-works-in.html)
- [How calling from .NET to Java works](http://zamboch.blogspot.cz/2009/10/how-calling-from-net-to-java-works.html)

### Sample Projects
Study jni4net sample projects:
- `samples/helloWorldFromCLR`
- `samples/helloWorldFromJVM`

### Support and Community
- [jni4net Email Group](https://groups.google.com/forum/?hl=en#!forum/jni4net)
- [jni4net Troubleshooter](http://jni4net.com/troubleshoot.html)
- [LinksPlatform Discord](https://discord.gg/eEXJyjWv5e)

## Licensing Notes

- **jni4net Runtime**: MIT License
- **jni4net Proxygen and Tools**: GPLv3 License
- **LinksPlatform**: Check individual package licenses (typically MIT/Unlicense)

Ensure compliance with all relevant licenses when distributing applications that use jni4net with LinksPlatform.

## Next Steps

1. Explore [Platform.Data.Doublets documentation](https://linksplatform.github.io/Data.Doublets) for advanced features
2. Review [Links Theory](links-theory.md) to understand the conceptual foundation
3. Join the [LinksPlatform Discord](https://discord.gg/eEXJyjWv5e) for support and discussions
4. Contribute examples and improvements to this documentation

---

*For questions or contributions to this documentation, please open an issue or pull request on the [LinksPlatform GitHub repository](https://github.com/konard/LinksPlatform).*
