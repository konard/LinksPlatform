# Doublets RPC - Reference Implementation

This directory contains reference implementations and examples demonstrating Remote Procedure Calls (RPC) over Doublets storage.

## Overview

The Doublets RPC system enables cross-language method invocation using Doublets storage as a communication medium. This approach allows any programming language with Doublets bindings to call methods implemented in any other supported language.

## Architecture

```
┌─────────────┐
│   Caller    │
│  (C#/etc)   │
└──────┬──────┘
       │
       │ Creates CallLink
       ↓
┌─────────────────────┐
│  Doublets Storage   │
│  (Shared .links)    │
└─────────────────────┘
       ↑
       │ Monitors & Executes
       │
┌──────┴──────┐
│  Executor   │
│  (Py/etc)   │
└─────────────┘
```

## Files

- `DoubletsRPC.cs` - C# implementation (caller + executor)
- `doublets_rpc.py` - Python implementation (executor)
- `example_csharp_caller.cs` - Example: C# calling Python
- `example_python_executor.py` - Example: Python executing C# calls
- `README.md` - This file

## Concepts

### Link Structure

All RPC operations use Doublets links with the following structure:

#### Method Registration
```
MethodLink:
  Source: MethodNameSequence
  Target: SignatureLink
```

#### Method Call
```
CallLink:
  Source: MethodLink
  Target: ArgumentsSequence
```

#### Call State
```
StateLink:
  Source: CallLink
  Target: StateMarker (Pending/Processing/Completed/Failed)
```

#### Result
```
ResultLink:
  Source: CallLink
  Target: ResultValue
```

## Example Usage

### 1. Python Executor (Server)

```python
# Register a method that C# can call
from doublets_rpc import DoubletsRPC

rpc = DoubletsRPC("shared.links")

@rpc.register("CalculateFactorial")
def factorial(n: int) -> int:
    if n <= 1:
        return 1
    return n * factorial(n - 1)

# Process pending calls
while True:
    rpc.process_pending_calls()
    time.sleep(0.1)
```

### 2. C# Caller (Client)

```csharp
// Call Python method from C#
using var links = new UnitedMemoryLinks<ulong>("shared.links");
var rpc = new DoubletsRPC(links);

var result = rpc.CallMethod<int>("CalculateFactorial", 5);
Console.WriteLine($"5! = {result}"); // Output: 5! = 120
```

## Implementation Status

### Phase 1: Conceptual Design ✅
- [x] Architecture design
- [x] Protocol specification
- [x] Documentation

### Phase 2: Basic Implementation (In Progress)
- [ ] C# RPC library
- [ ] Python RPC library
- [ ] Basic type marshalling
- [ ] State management

### Phase 3: Advanced Features
- [ ] Complex type support (objects, arrays)
- [ ] Async/await support
- [ ] Error handling and exceptions
- [ ] Timeout management

### Phase 4: Additional Languages
- [ ] C++ implementation
- [ ] JavaScript implementation
- [ ] Rust implementation

## Type Mapping

### C# ↔ Python

| C# Type | Python Type | Doublets Representation |
|---------|-------------|------------------------|
| int     | int         | IntegerSequence        |
| string  | str         | UnicodeSequence        |
| double  | float       | Float64Sequence        |
| bool    | bool        | BooleanMarker          |
| null    | None        | NullMarker             |
| List<T> | list        | ArraySequence          |
| object  | dict        | ObjectMap              |

## Protocol Details

### State Machine

```
┌──────────┐
│  PENDING │ Initial state when call is created
└─────┬────┘
      │
      ↓
┌────────────┐
│ PROCESSING │ Executor picked up the call
└─────┬──────┘
      │
      ├─→ ┌───────────┐
      │   │ COMPLETED │ Success with result
      │   └───────────┘
      │
      └─→ ┌────────┐
          │ FAILED │ Error occurred
          └────────┘
```

### Link Markers

Special marker links are created during initialization:

- `TypeMarker` - Identifies type definitions
- `MethodMarker` - Identifies method registrations
- `CallMarker` - Identifies method calls
- `StateMarker` - Identifies state links
- `PendingMarker` - Pending state
- `ProcessingMarker` - Processing state
- `CompletedMarker` - Completed state
- `FailedMarker` - Failed state
- `NullMarker` - Represents null/None values

## Performance Considerations

### Local File

- Latency: ~10-100 microseconds per call
- Best for: Single-machine polyglot systems
- Use case: Plugin systems, mixed-language applications

### Network-Mapped File

- Latency: ~1-10 milliseconds per call
- Best for: Distributed systems on same network
- Use case: Microservices, distributed computing

### Optimization Tips

1. **Batch calls** - Process multiple calls together
2. **Reuse links** - Don't create new method links each time
3. **Index states** - Use state indices for fast lookups
4. **Cache results** - Cache frequently called methods
5. **Memory-map** - Use memory-mapped files for speed

## Testing

Run the example:

```bash
# Terminal 1: Start Python executor
cd experiments/rpc-doublets
python example_python_executor.py

# Terminal 2: Run C# caller
dotnet run --project example_csharp_caller.csproj
```

## Security Notes

⚠️ **Warning**: This is a reference implementation for demonstration purposes. Production use requires:

1. **Authentication** - Verify caller identity
2. **Authorization** - Check method access permissions
3. **Validation** - Sanitize and validate all inputs
4. **Rate Limiting** - Prevent abuse
5. **Audit Logging** - Track all method calls
6. **Encryption** - Protect sensitive data in links

## Contributing

To add support for a new language:

1. Implement `DoubletsRPC` class with:
   - `RegisterMethod(name, handler)` - Register callable methods
   - `CallMethod(name, ...args)` - Invoke remote methods
   - `ProcessPendingCalls()` - Execute pending calls
2. Implement type marshalling for common types
3. Add examples demonstrating bidirectional calls
4. Document language-specific considerations

## References

- [RPC over Doublets Design Doc](../../doc/articles/rpc-over-doublets.md)
- [Links Theory](../../doc/articles/links-theory.md)
- [Doublets Storage](https://github.com/linksplatform/Data.Doublets)

## License

MIT License - See repository root for full license text.
