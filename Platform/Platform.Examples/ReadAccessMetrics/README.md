# Read Access Frequency Metrics

This implementation provides a solution for tracking read access frequency metrics for links in the LinksPlatform.

## Overview

The solution implements both variants proposed in issue #589:

### Variant 1: Incremental Average
- Tracks `AverageReadAccessTime` and `LastReadAccessTime`
- Updates `AverageReadAccessTime` incrementally based on the time between consecutive reads
- More accurately reflects actual read frequency patterns

### Variant 2: Formula-Based Average
- Tracks `CreationDateTime` and `TotalReadAccessCount`
- Calculates average using formula: `(Now - CreationDateTime) / TotalReadAccessCount`
- Simpler calculation but represents average time per read since creation

## Architecture

### Components

1. **IReadAccessMetrics&lt;TLinkAddress&gt;** - Interface defining metric access methods
2. **LinkReadAccessMetrics** - Data class storing metrics for a single link
3. **ReadAccessTrackingLinks&lt;TLinkAddress&gt;** - Decorator implementing ILinks with read tracking
4. **ReadAccessMetricsExample** - Example demonstrating usage

### Design Pattern

The implementation uses the **Decorator Pattern** to wrap any `ILinks<TLinkAddress>` instance, adding read access tracking without modifying the original implementation.

## Usage

```csharp
// Create a links storage instance
using var links = new UnitedMemoryLinks<uint>(memory);

// Wrap with read access tracking
var trackingLinks = new ReadAccessTrackingLinks<uint>(links);

// Create and access links as normal
var link = trackingLinks.Create();
trackingLinks.Each(new[] { link, links.Constants.Any, links.Constants.Any }, handler);

// Access metrics
var avgTime = trackingLinks.GetAverageReadAccessTime(link);
var lastAccess = trackingLinks.GetLastReadAccessTime(link);
var totalReads = trackingLinks.GetTotalReadAccessCount(link);

// Variant 2 calculation
var avgTimeV2 = trackingLinks.GetAverageReadAccessTimeVariant2(link);
```

## Implementation Details

### Read Tracking

Read accesses are tracked in two scenarios:
1. When `Count()` is called with a restriction
2. When `Each()` is called (both for the restriction and each visited link)

### Metrics Storage

Metrics are stored in a `ConcurrentDictionary` for thread-safe access. Each link has its own `LinkReadAccessMetrics` instance containing:
- `CreationDateTime` - When the link was created
- `LastReadAccessTime` - Timestamp of the last read
- `AverageReadAccessTime` - Incrementally calculated average (Variant 1)
- `TotalReadAccessCount` - Total number of reads

### Variant 1 Calculation

The average is updated incrementally using the formula:
```
newAverage = (currentAverage × readCount + timeSinceLastRead) / (readCount + 1)
```

This provides a weighted average that accurately reflects read frequency patterns.

### Variant 2 Calculation

The average is calculated on-demand using:
```
average = (Now - CreationDateTime) / TotalReadAccessCount
```

This provides the average time elapsed per read access since the link was created.

## Performance Considerations

- Tracking can be disabled by passing `trackMetrics: false` to the constructor
- Metrics are stored in memory; for large-scale deployments, consider persistence
- Thread-safe implementation using `ConcurrentDictionary`

## Future Enhancements

Potential improvements:
- Persistent metrics storage
- Configurable metric retention policies
- Additional metrics (e.g., read frequency histograms)
- Export capabilities for analysis
