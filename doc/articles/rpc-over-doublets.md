# Remote Procedure Calls over Doublets Storage

## Abstract

This document describes a universal approach for implementing Remote Procedure Calls (RPC) and class method invocations across different programming languages using Doublets storage as a synchronization and communication medium. The solution enables any programming language to call functions/methods in any other language through a standardized Links-based protocol.

## Introduction

### Problem Statement

Modern software systems often require components written in different programming languages to communicate. Traditional solutions include:
- Language-specific FFI (Foreign Function Interface)
- gRPC/Protocol Buffers
- REST APIs
- Message queues

Each has limitations:
- **FFI**: Language-specific, complex memory management
- **gRPC/REST**: Network overhead, serialization complexity
- **Message Queues**: Require separate infrastructure

### Proposed Solution

Use Doublets storage as a universal inter-language communication medium. Doublets naturally represent relationships and can encode:
- Method signatures
- Arguments
- Return values
- Call states

## Core Concepts

### 1. Doublets as a Universal Data Model

Doublets store consists of links with two references (Source → Target). This simple structure can represent:
- **Types**: `TypeLink → TypeNameLink`
- **Methods**: `MethodNameLink → MethodSignatureLink`
- **Arguments**: Sequences of argument links
- **Results**: `CallLink → ResultLink`

### 2. Method Call Representation

A method call in Doublets consists of:

```
CallLink:
  Source: MethodDefinitionLink
  Target: ArgumentsSequenceLink

MethodDefinitionLink:
  Source: TargetObjectLink (or null for static)
  Target: MethodNameLink

ArgumentsSequenceLink:
  Sequence of ArgumentLinks using balanced variant
```

### 3. Synchronization States

Each call has a state represented as a link:
- **Pending**: Call created, awaiting execution
- **Processing**: Executor picked up the call
- **Completed**: Result available
- **Failed**: Error occurred

## Architecture

### Component Overview

```
┌─────────────────┐         ┌─────────────────┐
│   Caller        │         │   Executor       │
│  (Any Language) │         │  (Any Language)  │
└────────┬────────┘         └────────┬─────────┘
         │                           │
         │  1. Create Call Link      │
         ├──────────────────────────►│
         │                           │
         │  2. Wait for Result       │
         │  3. Poll or Subscribe     │
         │◄──────────────────────────┤
         │                           │
         │  4. Read Result           │
         │◄──────────────────────────┤
         │                           │
┌────────▼───────────────────────────▼─────────┐
│         Doublets Storage (Shared)            │
│  - Call Definitions                          │
│  - Method Registry                           │
│  - State Tracking                            │
│  - Results                                   │
└──────────────────────────────────────────────┘
```

### Flow Diagram

1. **Registration Phase**
   - Executor registers available methods
   - Creates MethodDefinition links

2. **Invocation Phase**
   - Caller creates CallLink with method + arguments
   - Sets state to Pending

3. **Execution Phase**
   - Executor monitors for Pending calls
   - Updates state to Processing
   - Executes method
   - Stores result
   - Updates state to Completed/Failed

4. **Return Phase**
   - Caller retrieves result
   - Optionally cleans up call links

## Protocol Specification

### Link Structure

#### Type System Links

```
// Primitive types
IntegerTypeLink: [TypeMarkerLink → "Integer"]
StringTypeLink: [TypeMarkerLink → "String"]
BooleanTypeLink: [TypeMarkerLink → "Boolean"]
FloatTypeLink: [TypeMarkerLink → "Float"]

// Complex types
ArrayTypeLink: [TypeMarkerLink → ElementTypeLink]
ObjectTypeLink: [TypeMarkerLink → ClassDefinitionLink]
```

#### Method Registration

```
MethodRegistrationLink:
  Source: MethodNameSequence (e.g., "CalculateSum")
  Target: MethodSignatureLink

MethodSignatureLink:
  Source: ParameterTypesSequence
  Target: ReturnTypeLink

// Example: int CalculateSum(int a, int b)
CalculateSum → [
  [IntegerType, IntegerType] → IntegerType
]
```

#### Method Call

```
CallLink:
  Source: CallMetadataLink
  Target: CallStateLink

CallMetadataLink:
  Source: MethodRegistrationLink
  Target: ArgumentsSequence

CallStateLink:
  Source: StateMarkerLink (Pending/Processing/Completed/Failed)
  Target: ResultLink (null if not completed)
```

### State Transitions

```
Pending → Processing → Completed
                    ↓
                  Failed
```

Each state transition creates a new link, preserving history.

## Implementation Guide

### For Language Binding Authors

To implement RPC support in a language:

1. **Connection Layer**
   ```
   - Connect to Doublets storage (file or network)
   - Initialize LinksConstants
   ```

2. **Type Mapping**
   ```
   - Map language types to Doublets TypeLinks
   - Implement serialization/deserialization
   ```

3. **Caller API**
   ```
   - Method: CreateCall(methodName, args[])
   - Method: WaitForResult(callLink, timeout)
   - Method: GetResult(callLink)
   ```

4. **Executor API**
   ```
   - Method: RegisterMethod(name, signature, handler)
   - Method: ProcessPendingCalls()
   - Method: StoreResult(callLink, result)
   ```

### Example: C# Caller

```csharp
// Initialize
var links = new UnitedMemoryLinks<ulong>("rpc.links");
var rpc = new DoubletsRPC(links);

// Call remote method
var call = rpc.CreateCall("CalculateSum", 10, 20);
var result = rpc.WaitForResult<int>(call, timeout: 5000);
Console.WriteLine($"Result: {result}"); // 30
```

### Example: Python Executor

```python
# Initialize
links = DoubletsLinks("rpc.links")
rpc = DoubletsRPC(links)

# Register method
@rpc.register("CalculateSum")
def calculate_sum(a: int, b: int) -> int:
    return a + b

# Process calls
rpc.process_pending_calls()
```

## Serialization Format

### Primitive Types

- **Integer**: Direct link to value (small ints) or sequence of digits
- **String**: Unicode sequence using existing UnicodeMap
- **Boolean**: True/False marker links
- **Float**: IEEE 754 representation as byte sequence

### Complex Types

- **Arrays**: Sequence of element links
- **Objects**: Map of key-value pairs as link sequences
- **Null**: Special NullMarkerLink

## Advantages

1. **Language Agnostic**: Any language with Doublets bindings can participate
2. **No Network Required**: Can work with file-based or memory-mapped storage
3. **Type Safe**: Types preserved in the storage
4. **Inspectable**: All calls visible in the storage for debugging
5. **Persistent**: Call history maintained
6. **Extensible**: Easy to add new types and methods

## Performance Considerations

### Optimization Strategies

1. **Call Pooling**: Reuse call links
2. **State Indexing**: Index by state for fast pending call lookup
3. **Result Caching**: Cache frequently called methods
4. **Batch Processing**: Process multiple calls together
5. **Memory Mapping**: Use memory-mapped files for low latency

### Benchmarks (Estimated)

- Local call overhead: ~10-100 microseconds
- Network call overhead: ~1-10 milliseconds (depends on sync method)
- Throughput: ~10,000-100,000 calls/second (optimized)

## Use Cases

1. **Microservices**: Language-agnostic service communication
2. **Plugin Systems**: Load plugins in different languages
3. **Scientific Computing**: Call Python from C++ and vice versa
4. **Testing**: Test implementations across languages
5. **Legacy Integration**: Bridge old and new systems

## Security Considerations

1. **Access Control**: Implement caller identification
2. **Method Whitelisting**: Restrict callable methods
3. **Input Validation**: Validate arguments before execution
4. **Resource Limits**: Prevent resource exhaustion
5. **Audit Logging**: Track all calls for security analysis

## Future Extensions

1. **Async/Await Support**: Non-blocking calls
2. **Streaming**: Large data transfer support
3. **Events**: Publish-subscribe pattern
4. **Transactions**: Multi-call transactions
5. **Distributed**: Multiple storage nodes

## Comparison with Existing Solutions

| Feature | Doublets RPC | gRPC | REST | FFI |
|---------|--------------|------|------|-----|
| Language Support | Universal* | Many | Universal | Specific |
| Network Overhead | Optional | Required | Required | None |
| Type Safety | Strong | Strong | Weak | Strong |
| Setup Complexity | Low | Medium | Low | High |
| Performance | High | Medium | Low | Highest |
| Debugging | Easy | Medium | Easy | Hard |
| Persistence | Built-in | None | None | None |

*Requires language binding for Doublets

## Implementation Roadmap

### Phase 1: Foundation (Current)
- [ ] Specification document
- [ ] Basic C# implementation
- [ ] Examples

### Phase 2: Core Features
- [ ] Full type system
- [ ] Error handling
- [ ] Timeout support
- [ ] C++ implementation

### Phase 3: Advanced Features
- [ ] Async support
- [ ] Python implementation
- [ ] JavaScript implementation
- [ ] Performance optimization

### Phase 4: Production Ready
- [ ] Security features
- [ ] Comprehensive tests
- [ ] Documentation
- [ ] Benchmarks

## Conclusion

Doublets storage provides a unique foundation for universal RPC. By leveraging its associative model, we can create a language-agnostic communication protocol that is simple, efficient, and powerful. This opens new possibilities for polyglot systems and simplifies cross-language integration.

## References

1. [Associative Model of Data](https://en.wikipedia.org/wiki/Associative_model_of_data)
2. [Links Theory](links-theory.md)
3. [Doublets Storage Documentation](https://github.com/linksplatform/Data.Doublets)
4. [gRPC Documentation](https://grpc.io/)
5. [Foreign Function Interface (FFI)](https://en.wikipedia.org/wiki/Foreign_function_interface)

## Appendix A: Complete Example

See `Platform.Examples.DoubletsRPC` for a working implementation demonstrating:
- Method registration in C#
- Remote calls from C#
- Type serialization
- State management
- Error handling

## Appendix B: Link Notation

Throughout this document, we use the notation:
- `[Source → Target]`: A doublet link
- `Link1 → Link2`: Link1's target is Link2
- `Sequence[a, b, c]`: A balanced sequence of links

## Authors

- LinksPlatform Contributors
- AI Assistant (Initial Design)

## License

This document and associated code are licensed under MIT License.
