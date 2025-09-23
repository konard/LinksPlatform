# Event Sequence System

This project implements a comprehensive **Sequence of Events** system as requested in [Issue #684](https://github.com/konard/LinksPlatform/issues/684). The system provides a single, centralized database for collecting all events from background, parallel, remote, and other processes with precise timestamp ordering using UUIDv7.

## Overview

The Event Sequence System enables **full automation of debugging** for complex systems by:

1. **Centralized Event Collection**: Single database collecting events from all processes
2. **Precise Ordering**: Events sorted by timestamp and UUIDv7 for perfect sequence preservation
3. **Command Wrapping**: Automatic timestamping of all terminal commands and console statements
4. **Process Tracking**: Events tagged with process IDs for multi-process debugging
5. **Query Interface**: Rich querying capabilities for event analysis

## Key Components

### 1. EventStore<TLinkAddress>
Core event storage system built on LinksPlatform's doublets architecture:
- Stores events as sequences of links
- UUIDv7-based unique identifiers with embedded timestamps
- Thread-safe operations
- Query methods for filtering by type, process, time range

### 2. EventCommandWrapper<TLinkAddress>
Command execution wrapper that automatically logs:
- Command start/completion events
- Standard output and error streams
- Exit codes and execution duration
- Exceptions and failures

### 3. GlobalEventLogger
Application-wide event logging with:
- Console output interception
- Fallback file logging
- Global singleton access
- Process identification

### 4. EventAwareMasterServerCLI
Enhanced version of the original MasterServer with:
- Integrated event logging
- Interactive event viewing (`events` command)
- Event statistics (`stats` command)
- All operations automatically logged

## UUIDv7 Implementation

The system uses UUIDv7 for event identification, which provides:
- **Sortable by creation time**: First 48 bits contain Unix timestamp
- **Globally unique**: Remaining bits ensure uniqueness
- **Monotonic ordering**: Perfect for sequence preservation
- **Database-friendly**: Natural clustering and indexing

## Usage Examples

### Basic Event Logging
```csharp
using var eventStore = new EventStore<ulong>(links, sequences, unicodeMap);

// Log events from different processes
eventStore.StoreEvent("Application.Start", "App started", "MainProcess");
eventStore.StoreEvent("Database.Connect", "Connected to DB", "DBProcess");
eventStore.StoreEvent("User.Login", "User authenticated", "AuthProcess");
```

### Command Execution with Automatic Logging
```csharp
using var commandWrapper = new EventCommandWrapper<ulong>(eventStore, "MyProcess");

// Execute commands with automatic event logging
var result = await commandWrapper.ExecuteCommandAsync("ls", "-la");
commandWrapper.LogInfo("Directory listing completed");
```

### Global Event Logging
```csharp
// Global logger automatically intercepts console output
GlobalEventLogger.LogInfo("Application started");
Console.WriteLine("This output is automatically logged");
GlobalEventLogger.LogError("Something went wrong");
```

### Event Querying
```csharp
// Query events by various criteria
var recentEvents = eventStore.GetEventsByTimeRange(
    DateTimeOffset.UtcNow.AddHours(-1),
    DateTimeOffset.UtcNow);

var processEvents = eventStore.GetEventsByProcess("WorkerProcess1");
var errorEvents = eventStore.GetEventsByType("Error");

// All events in chronological order
var allEvents = eventStore.GetEvents().OrderBy(e => e.Timestamp);
```

## Running the System

### Demo Mode
```bash
dotnet run demo
```
Runs comprehensive demonstration showing:
- Basic event logging
- Command execution tracking
- Parallel process simulation
- Event querying capabilities

### Event-Aware Server Mode
```bash
dotnet run
```
Starts the enhanced MasterServer with event logging. Interactive commands:
- `events` - Show recent events
- `stats` - Display event statistics
- Any other command operates normally with automatic logging

## Architecture Benefits

1. **Debugging Automation**: Complete event sequences enable automated debugging
2. **System Monitoring**: Real-time visibility into all system activities
3. **Performance Analysis**: Precise timing data for optimization
4. **Audit Trail**: Complete history of all operations
5. **Multi-Process Coordination**: Clear view of parallel process interactions
6. **Root Cause Analysis**: Chronological event sequences simplify troubleshooting

## Integration with LinksPlatform

The system seamlessly integrates with existing LinksPlatform infrastructure:
- Uses doublets for event storage
- Leverages Unicode mapping for text data
- Employs sequences for structured event data
- Maintains consistency with existing patterns

## Future Enhancements

- **Network Event Distribution**: Send events to remote databases
- **Event Visualization**: Real-time dashboards and timeline views
- **Alert System**: Automated alerting on specific event patterns
- **Event Replay**: Ability to replay system state from event sequences
- **Compression**: Advanced compression for high-volume scenarios

This implementation provides the foundation for fully automated debugging of complex systems by preserving the complete sequence of events across all processes, exactly as requested in the original issue.