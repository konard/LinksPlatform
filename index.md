# Links Platform ([русская версия](index.ru.md))
Holistic system for storage and transformation of information (in development) based on associative model of data.

## Prerequisites
* Linux, macOS or Windows operating system.
* [.NET Core](https://www.microsoft.com/net) SDK with version 2.2 or later.
* [MonoDevelop](https://www.monodevelop.com/), [Visual Studio](https://visualstudio.microsoft.com) or any other [IDE](https://en.wikipedia.org/wiki/Integrated_development_environment) or just a [text editor](https://en.wikipedia.org/wiki/Text_editor).

## Links Platform's NuGet packages

### Main packages

#### [Platform.Data](https://linksplatform.github.io/Data)
Common interfaces and classes for both [Doublets](https://linksplatform.github.io/Data.Doublets) and [Triplets](https://linksplatform.github.io/Data.Triplets).

#### [Platform.Data.Doublets](https://linksplatform.github.io/Data.Doublets)
An implementation of Doublets.

#### [Platform.Data.Triplets](https://linksplatform.github.io/Data.Triplets)
A C# adapter of Triplets.

#### [Platform.Data.Triplets.Kernel](https://linksplatform.github.io/Data.Triplets.Kernel)
A native Triplets implementation.

### Auxiliary packages

#### [Platform.Data.Memory](https://linksplatform.github.io/Memory)
Platform.Data.Memory class library contains classes for memory management simplification. There you can find multiple implementations of [IMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.IMemory.html) interface.

The data can be accessed using [the raw pointer](https://linksplatform.github.io/Memory/api/Platform.Memory.IDirectMemory.html) or [by element's index](https://linksplatform.github.io/Memory/api/Platform.Memory.IArrayMemory-1.html) and can be stored in volatile memory:
* [HeapResizableDirect](https://linksplatform.github.io/Memory/api/Platform.Memory.HeapResizableDirectMemory.html),
* [ArrayMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.ArrayMemory-1.html)

or in non-volatile memory:
* [FileMappedResizableDirectMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.FileMappedResizableDirectMemory.html),
* [TemporaryFileMappedResizableDirectMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.TemporaryFileMappedResizableDirectMemory.html),
* [FileArrayMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.FileArrayMemory-1.html).

#### [Platform.Data.Communication](https://linksplatform.github.io/Communication)
Platform.Data.Communication class library contains classes for communication simplification supporting different protocols.

##### Gexf
XML-mapping classes for [Graph Exchange XML Format](https://gephi.org/gexf/format/).

##### Udp
`UdpSender` and `UdpReceiver` classes to simplify implementation of different roles of `UdpClient`.

##### Xml
A `Serializer` class to help with XML serialization and deserialization.

#### [Platform.Collections.Methods](https://linksplatform.github.io/Collections.Methods)
Platform.Collections.Methods class library contains classes with storage/state agnostic implementation of lists and trees.

#### [Platform.IO](https://linksplatform.github.io/IO)
Platform.IO class library provides I/O related functionality and utilities for file operations and I/O management.

#### [Platform.Unsafe](https://linksplatform.github.io/Unsafe)
Platform.Unsafe class library provides low-level unsafe programming utilities for direct memory manipulation and performance-critical operations.

#### [Platform.Numbers](https://linksplatform.github.io/Numbers)
Platform.Numbers class library provides numerical utility functions and type conversion capabilities for working with numbers.

#### [Platform.Converters](https://linksplatform.github.io/Converters)
Platform.Converters class library provides functionality for converting data between different formats and types.

#### [Platform.Scopes](https://linksplatform.github.io/Scopes)
Platform.Scopes class library provides utilities for managing scope-based object lifetimes and dependency injection contexts.

#### [Platform.Singletons](https://linksplatform.github.io/Singletons)
Platform.Singletons class library provides utilities for managing singleton objects and ensuring single instance patterns across the application.

#### [Platform.Reflection](https://linksplatform.github.io/Reflection)
Platform.Reflection class library provides enhanced reflection capabilities for runtime type inspection and manipulation.

#### [Platform.Threading](https://linksplatform.github.io/Threading)
Platform.Threading class library provides threading-related functionality and utilities for concurrent programming.

#### [Platform.Collections](https://linksplatform.github.io/Collections)
Platform.Collections class library provides collection implementations and utilities for managing data structures.

#### [Platform.Diagnostics](https://linksplatform.github.io/Diagnostics)
Platform.Diagnostics class library provides diagnostics and debugging utilities for monitoring and troubleshooting applications.

#### [Platform.Incrementers](https://linksplatform.github.io/Incrementers)
Platform.Incrementers class library provides incrementer utilities for generating sequential values and counters.

#### [Platform.Setters](https://linksplatform.github.io/Setters)
Platform.Setters class library provides setter utilities for modifying and assigning values to objects and properties.

#### [Platform.Comparers](https://linksplatform.github.io/Comparers)
Platform.Comparers class library provides comparison utilities and custom comparers for ordering and equality operations.

#### [Platform.Random](https://linksplatform.github.io/Random)
Platform.Random class library provides random number generation utilities and helpers for generating random values.

#### [Platform.Timestamps](https://linksplatform.github.io/Timestamps)
Platform.Timestamps class library provides timestamp handling utilities for working with time-based data and operations.

#### [Platform.Ranges](https://linksplatform.github.io/Ranges)
Platform.Collections.Methods class library contains `Range` struct with Minimum and Maximum fields.

#### [Platform.Disposables](https://linksplatform.github.io/Disposables)
Platform.Collections.Methods class library contains classes and interfaces that help to make objects disposable in a fast, short, easy and safe way.

##### DisposableBase
`Platform.Disposables.DisposableBase` abstract class tries to dispose the object at both on instance destruction and `OnProcessExit` whatever comes first even if `Dispose` method was not called anywhere by user.

##### Yet another IDisposable
The `Platform.Disposables.IDisposable` interface extends the `System.IDisposable` with `IsDisposed` property and `Destruct` method.

#### [Platform.Exceptions](https://linksplatform.github.io/Exceptions)
Platform.Exceptions class library provides exception handling utilities and custom exception types for error management.

#### [Platform.Interfaces](https://linksplatform.github.io/Interfaces)
Platform.Collections.Methods class library contains common interfaces that did not fit in any major category.
