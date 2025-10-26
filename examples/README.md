# Memory Page Access Visualization Example

This example demonstrates how to visualize memory page access patterns when working with the Links Platform database.

## Purpose

Understanding how memory is accessed is crucial for optimizing the database's memory structure. This tool helps identify:

- **Memory hotspots**: Pages that are accessed frequently
- **Access patterns**: Sequential vs. random access
- **Memory efficiency**: How well the data structure utilizes memory pages
- **Optimization opportunities**: Areas where restructuring could improve cache locality

## How It Works

The `MemoryPageAccessVisualizer` tracks every access to memory pages during database operations:

1. **Page Tracking**: Records which memory pages (typically 4KB on Linux) are accessed
2. **Operation Tagging**: Tags each access with the operation type (create, read, search, etc.)
3. **Statistics Collection**: Gathers frequency, timing, and pattern data
4. **Visualization**: Generates heatmaps and detailed reports

## Running the Example

### Basic Demonstration

Run with sample data to see the visualizer in action:

```bash
dotnet run
```

This will:
- Simulate different access patterns (sequential, random, hotspot)
- Display a text-based heatmap
- Show top accessed pages
- Export results to `memory-access-report.csv`

### With Real Database

Analyze memory access on an actual Links database:

```bash
dotnet run database.links 50000 output.csv
```

Parameters:
- `database.links`: Path to the Links database file
- `50000`: Number of operations to perform
- `output.csv`: Output file for detailed statistics

## Understanding the Output

### Console Summary

The tool prints:
- Total unique pages accessed
- Total number of accesses
- Average accesses per page
- Top 10 most accessed pages
- Process memory information (on Linux)

### Heatmap

A text-based visualization where:
- `█` = High access frequency
- `▒` `░` = Medium/low access
- ` ` (space) = No access

The horizontal axis represents memory address space, and vertical axis represents access intensity.

### CSV Export

Detailed data including:
- Page address and number
- Access count and frequency
- First and last access timestamps
- Operation types that accessed each page

## Analyzing Results

### Good Patterns

- **Sequential clusters**: Indicates good cache locality
- **Few hotspots**: Workload is well-distributed
- **High page reuse**: Memory is efficiently utilized

### Problem Patterns

- **Scattered random access**: Poor cache performance
- **Extreme hotspots**: Potential bottlenecks
- **Low page reuse**: Memory is underutilized

## Optimization Strategies

Based on visualization results:

1. **Reorganize data structures** to group frequently accessed data together
2. **Adjust database layout** to align with page boundaries
3. **Implement prefetching** for predictable access patterns
4. **Consider compression** for rarely accessed pages
5. **Optimize index structures** to reduce page spread

## Technical Details

- Works on Linux (uses `/proc` filesystem for memory info)
- Page size determined dynamically (usually 4KB)
- Thread-safe tracking for concurrent operations
- Minimal overhead on database operations

## Example Output

```
=== Memory Page Access Summary ===
Page Size: 4096 bytes
Total Unique Pages Accessed: 245
Total Accesses: 50000
Average Accesses per Page: 204.08
Max Accesses (single page): 1523
Min Accesses (single page): 12

Top 10 Most Accessed Pages:
  Page 0x0000000010032000: 1523 accesses (3.05%) - Operations: random-read, search
  Page 0x0000000010015000: 987 accesses (1.97%) - Operations: sequential-read
  ...

Memory Page Access Heatmap (Page Size: 4096 bytes)
Total Pages: 245, Total Accesses: 50000
Address Range: 0x10000000 - 0x10100000

████████████████████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
████████████████████████████████████████▒▒▒▒░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
...
```

## Further Reading

- [LinksPlatform Documentation](https://github.com/Konard/LinksPlatform)
- [Memory Access Patterns and Performance](https://en.wikipedia.org/wiki/Locality_of_reference)
- [Database Memory Optimization](https://en.wikipedia.org/wiki/Database_tuning)
