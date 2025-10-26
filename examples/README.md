# Links and SQL Synchronization Examples

This directory contains examples demonstrating bidirectional synchronization between Links storage and SQL tables.

## Overview

The SQL synchronization feature allows you to:
- Map Links substructures to SQL tables
- Export Links data to SQL format
- Import SQL data to Links storage
- Synchronize changes bidirectionally between Links and SQL

## Components

### SqlTableMapper<TLink>
Maps a substructure of Links to a SQL table structure. Each link's source and target become columns in the table.

**Key Features:**
- Creates SQL table structures from Links
- Generates CREATE TABLE statements
- Generates INSERT/UPDATE/DELETE statements
- Manages substructure to table mapping

### SqlExporter<TLink>
Exports Links structures to SQL format (SQLite compatible).

**Key Features:**
- Export all links or specific substructures
- Generate SQL files or SQL strings
- Transaction support for batch operations

### SqlImporter<TLink>
Imports Links structures from SQL DataTable.

**Key Features:**
- Create or update links from SQL data
- Merge strategies for handling conflicts
- Detailed import results with error tracking

### SqlSynchronizer<TLink>
Synchronizes changes between Links storage and SQL tables bidirectionally.

**Key Features:**
- Tracks changes in both structures
- Detects insertions, updates, and deletions
- Applies changes bidirectionally
- Snapshot-based change detection

## Example Usage

See `SqlSyncExample.cs` for a complete working example that demonstrates:

1. Creating sample links
2. Exporting links to SQL
3. Creating table mappers for substructures
4. Setting up synchronization
5. Detecting and applying changes
6. Importing from SQL DataTable

## Running the Example

```bash
# Compile and run the example
cd examples
dotnet run SqlSyncExample.cs
```

## Use Cases

### Use Case 1: Export Links to SQL Database
```csharp
using var links = new UInt64Links(memory);
var exporter = new SqlExporter<ulong>(links, "MyLinks");
exporter.ExportToFile("links.sql");
```

### Use Case 2: Synchronize Substructure with SQL Table
```csharp
var synchronizer = new SqlSynchronizer<ulong>(links);
var substructureLinks = GetMySubstructure();
synchronizer.RegisterTable("MyTable", substructureLinks);

// Detect changes
var changes = synchronizer.DetectLinksChanges();

// Apply to SQL
synchronizer.ApplyChangesToSql("MyTable", sqlDataTable);
```

### Use Case 3: Import from SQL to Links
```csharp
var importer = new SqlImporter<ulong>(links);
var result = importer.Import(sqlDataTable);

Console.WriteLine($"Created: {result.CreatedLinks.Count}");
Console.WriteLine($"Updated: {result.UpdatedLinks.Count}");
```

## Architecture

The synchronization architecture follows these principles:

1. **Substructure Mapping**: Each SQL table represents a specific substructure of links
2. **Bidirectional Sync**: Changes can flow from Links to SQL or SQL to Links
3. **Change Detection**: Snapshot-based tracking identifies what changed
4. **Atomic Operations**: Transactions ensure data consistency

## Related Issues

- Issue #600: Links and SQL synchronization
- Issue #599: Each table as a virtual links structure
- Issue #607: Sync everything

## License

See the main repository LICENSE file for details.
