# Raw Drive Storage Experiments

This directory contains experimental implementations for issue #213: "The 'whole drive' fixed sized links storage allocation (no file system)".

## Contents

- **RawDriveStorageDesign.md** - Comprehensive design document outlining the architecture, interfaces, and implementation strategy
- **IRawBlockDevice.cs** - Interface for raw block device access
- **IRawLinksStorage.cs** - Interface for links storage on raw devices
- **FileBackedRawBlockDevice.cs** - File-backed implementation of IRawBlockDevice for testing
- **RawLinksStorage.cs** - Implementation of fixed-size links storage on raw devices
- **RawStorageExample.cs** - Examples demonstrating usage
- **RawStorageTests.cs** - Simple test suite

## Purpose

The goal is to enable LinksPlatform doublets storage to operate without file system dependencies, allowing:

1. Deployment on bare-metal/exokernel systems like [BareMetal OS](https://github.com/ReturnInfinity/BareMetal-OS)
2. Direct block device access for minimal overhead
3. Predictable, fixed-size storage allocation

## Building

These files are standalone C# code that can be:

1. Integrated into an existing Platform.Data project
2. Compiled as a separate library
3. Used as reference for implementation in other LinksPlatform repositories

## Running Examples

To run the examples:

```csharp
using Platform.Data.Core.RawStorage.Examples;

RawStorageExample.RunAllExamples();
```

To run tests:

```csharp
using Platform.Data.Core.RawStorage.Tests;

RawStorageTests.RunAllTests();
```

## Production Deployment

For production use with actual raw devices (Linux example):

```csharp
// WARNING: Requires root/elevated privileges and will overwrite the device!
using var device = new LinuxRawBlockDevice("/dev/sdb", readOnly: false);
using var storage = new RawLinksStorage<ulong>(device, capacity: 1000000, initializeNew: true);

// Use storage...
storage.WriteLink(0, 1, 2);
storage.ReadLink(0, out var source, out var target);
storage.Sync();
```

**CAUTION**: Direct device access can destroy data. Always verify device paths and use test devices.

## Next Steps

1. Implement platform-specific raw device drivers (Linux, Windows, BareMetal OS)
2. Add proper error handling and recovery mechanisms
3. Implement transaction support and journaling
4. Performance benchmarking against file-based storage
5. Integration with Platform.Data.Doublets
6. Security hardening (permissions, encryption)

## References

- Issue: https://github.com/konard/LinksPlatform/issues/213
- Pull Request: https://github.com/konard/LinksPlatform/pull/863
- BareMetal OS: https://github.com/ReturnInfinity/BareMetal-OS
- Platform.Data.Doublets: https://github.com/linksplatform/Data.Doublets
