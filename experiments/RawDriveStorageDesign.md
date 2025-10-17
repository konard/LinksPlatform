# Raw Drive Storage Design for Links Platform

## Overview

This document outlines the design for a "whole drive" fixed-size links storage allocation system that operates without relying on file systems or memory-mapped files. This approach is intended to facilitate compatibility with minimalist operating systems like [BareMetal OS](https://github.com/ReturnInfinity/BareMetal-OS) and to provide the lowest possible overhead for links storage.

## Motivation

Current implementations of LinksPlatform's doublets storage (e.g., `Platform.Data.Doublets`) rely on:
- File system APIs for creating and managing storage files
- Memory-mapped files for efficient random access
- Operating system abstractions that may not be available in bare-metal environments

The goal is to enable direct block device access, bypassing the file system layer entirely, which would:
1. Reduce dependencies on OS features
2. Minimize overhead from file system metadata and caching
3. Enable deployment on minimal/exokernel systems like BareMetal OS
4. Provide deterministic storage layout and access patterns

## Design Principles

### 1. Fixed-Size Allocation
- Pre-allocate the entire storage space at initialization
- No dynamic resizing - size is determined at creation time
- Predictable memory layout for all link records

### 2. Direct Block Access
- Use raw device I/O instead of file system operations
- Direct sector/block addressing
- No file system metadata overhead

### 3. Platform Abstraction
- Define platform-independent interfaces for raw storage access
- Provide platform-specific implementations (Linux block devices, Windows physical drives, BareMetal OS, etc.)

## Architecture

### Storage Layout

```
+------------------+
| Header Block     | <- Fixed size header with metadata
+------------------+
| Link Records     | <- Fixed-size array of doublet structures
| ...              |
| ...              |
+------------------+
```

#### Header Structure
```
Offset | Size | Field
-------|------|------------------
0x0000 | 8    | Magic number (identifier)
0x0008 | 8    | Version number
0x0010 | 8    | Total capacity (max links)
0x0018 | 8    | Current count (used links)
0x0020 | 8    | Link record size (bytes)
0x0028 | 8    | Reserved for future use
0x0030 | 8    | Reserved for future use
0x0038 | 8    | Header checksum
```

#### Link Record Structure
For a basic doublet (2 references):
```
Offset | Size | Field
-------|------|------------------
0x0000 | 8    | Source link address
0x0008 | 8    | Target link address
```

### Interface Design

```csharp
/// <summary>
/// Represents a raw block device that can be used for direct storage access.
/// </summary>
public interface IRawBlockDevice : IDisposable
{
    /// <summary>
    /// Gets the total size of the device in bytes.
    /// </summary>
    long SizeInBytes { get; }

    /// <summary>
    /// Gets the block size (sector size) in bytes.
    /// </summary>
    int BlockSize { get; }

    /// <summary>
    /// Reads data from the device at the specified offset.
    /// </summary>
    /// <param name="offset">Byte offset from the start of the device.</param>
    /// <param name="buffer">Buffer to read data into.</param>
    /// <param name="count">Number of bytes to read.</param>
    /// <returns>Number of bytes actually read.</returns>
    int Read(long offset, byte[] buffer, int count);

    /// <summary>
    /// Writes data to the device at the specified offset.
    /// </summary>
    /// <param name="offset">Byte offset from the start of the device.</param>
    /// <param name="buffer">Buffer containing data to write.</param>
    /// <param name="count">Number of bytes to write.</param>
    void Write(long offset, byte[] buffer, int count);

    /// <summary>
    /// Flushes any cached writes to the physical device.
    /// </summary>
    void Flush();
}

/// <summary>
/// Provides direct access to a raw storage device for links storage.
/// </summary>
public interface IRawLinksStorage<TLinkAddress> : IDisposable
{
    /// <summary>
    /// Gets the maximum number of links that can be stored.
    /// </summary>
    TLinkAddress Capacity { get; }

    /// <summary>
    /// Gets the current number of links stored.
    /// </summary>
    TLinkAddress Count { get; }

    /// <summary>
    /// Reads a link at the specified address.
    /// </summary>
    void ReadLink(TLinkAddress address, out TLinkAddress source, out TLinkAddress target);

    /// <summary>
    /// Writes a link at the specified address.
    /// </summary>
    void WriteLink(TLinkAddress address, TLinkAddress source, TLinkAddress target);

    /// <summary>
    /// Ensures all pending writes are persisted to the device.
    /// </summary>
    void Sync();
}
```

### Platform-Specific Implementations

#### Linux Implementation
- Use `/dev/sdX` or `/dev/nvmeXnY` device paths
- Open with `O_DIRECT` flag to bypass page cache
- Use `pread`/`pwrite` for positioned I/O
- Require appropriate permissions (typically root or specific user groups)

#### Windows Implementation
- Use `\\.\PhysicalDriveX` paths
- Open with `FILE_FLAG_NO_BUFFERING` and `FILE_FLAG_WRITE_THROUGH`
- Align all I/O to sector boundaries
- Require administrator privileges

#### BareMetal OS Implementation
- Direct AHCI/NVMe/IDE driver calls
- No OS abstraction layer needed
- Direct DMA to/from link structures
- Memory addresses map directly to link addresses

## Implementation Considerations

### 1. Alignment Requirements
- All reads/writes must be aligned to device block boundaries
- Link record sizes should be multiples of common block sizes (512, 4096 bytes)
- Use padding if necessary to maintain alignment

### 2. Caching Strategy
- No OS page cache when using direct I/O
- Implement application-level caching if needed
- Consider write-back vs write-through tradeoffs

### 3. Error Handling
- Device I/O errors must be handled gracefully
- Consider journaling or transaction logs for consistency
- Implement checksum validation for critical structures

### 4. Concurrency
- Single-writer principle recommended for raw devices
- Multiple readers possible with careful synchronization
- Consider lock-free data structures where applicable

### 5. Migration Path
- Provide utilities to migrate from file-based to raw storage
- Support export/import for backup and portability
- Maintain compatibility with existing APIs where possible

## Performance Characteristics

### Advantages
- No file system overhead
- Predictable I/O latency
- Direct control over caching and buffering
- Optimal for sequential and random access patterns

### Tradeoffs
- Loss of file system features (permissions, metadata, etc.)
- Requires elevated privileges
- More complex initialization and management
- Limited portability without abstraction layer

## Security Considerations

- Raw device access requires elevated privileges
- No file system permissions for access control
- Consider encryption at the application layer
- Audit and logging must be implemented separately

## Testing Strategy

1. **Unit Tests**: Test individual components with mock block devices
2. **Integration Tests**: Test with file-backed virtual block devices (loop devices on Linux)
3. **Performance Tests**: Benchmark against file-based implementations
4. **Compatibility Tests**: Verify cross-platform implementations produce identical results

## Future Enhancements

1. **Redundancy**: RAID-like configurations across multiple raw devices
2. **Compression**: Block-level compression for space efficiency
3. **Encryption**: Transparent encryption layer
4. **Snapshots**: Copy-on-write snapshots for backup and versioning
5. **Network Block Devices**: Support for iSCSI, NBD, etc.

## References

- [BareMetal OS](https://github.com/ReturnInfinity/BareMetal-OS) - Exokernel-based OS with direct hardware access
- [Platform.Data.Doublets](https://github.com/linksplatform/Data.Doublets) - Current file-based implementation
- Linux Direct I/O documentation
- Windows Direct Disk Access documentation
