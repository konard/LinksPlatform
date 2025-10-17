# Complex Identity

## Table of Contents
* [Introduction](#introduction)
* [Simple vs Complex Identity](#simple-vs-complex-identity)
* [Sync Identities](#sync-identities)
* [Async Identities](#async-identities)
* [Partially Synchronized Identities](#partially-synchronized-identities)
* [Comparison and Use Cases](#comparison-and-use-cases)

## Introduction

In distributed systems, proper identity management is crucial for uniquely identifying objects across multiple nodes. While simple identities like integer identifiers work well for single-node systems, distributed environments require more sophisticated approaches to ensure uniqueness and avoid conflicts.

This document describes various identity strategies for distributed systems, categorized by their synchronization requirements.

## Simple vs Complex Identity

**Simple identity** typically consists of a single value, such as an integer identifier. For example:
- Sequential integers: 1, 2, 3, ...
- Single GUID or UUID

**Complex identity** consists of multiple values combined to form a unique identifier. The complexity arises from the need to:
- Avoid conflicts in distributed environments
- Minimize coordination overhead
- Support offline operation
- Scale horizontally

In distributed systems, the term "node" can refer to either a server or a client, depending on the context and architecture.

## Sync Identities

Sync identities require a server or coordination service to handle the generation of new identities. This ensures global uniqueness but introduces a synchronization point in the system.

### Characteristics
- Require centralized coordination for identity generation
- Guarantee global uniqueness
- May become a bottleneck in highly distributed systems
- Suitable when strong consistency is required

### Examples

#### 1. Single Integer Identity
The simplest form of sync identity. A central authority (identity server) maintains a sequence and issues sequential numbers.

**Example:** PostgreSQL's `sequence` can be used to generate sync identities for tables.

```
Node request -> Identity Server -> Returns: 1
Node request -> Identity Server -> Returns: 2
Node request -> Identity Server -> Returns: 3
```

**Pros:**
- Simple to implement
- Minimal storage overhead
- Sequential ordering

**Cons:**
- Single point of failure
- Network dependency for every new identity
- Scalability bottleneck

#### 2. Single Timestamp Identity
Uses the timestamp from a central time authority to identify objects.

```
Object created at 2025-10-17T10:30:45.123Z
Identity: 1697537445123
```

**Pros:**
- Provides temporal ordering
- Relatively simple

**Cons:**
- Requires synchronized clocks
- Collisions possible if multiple objects created in same millisecond
- Clock skew can cause issues

#### 3. Timestamp + Ordinal Number
Combines a timestamp with an ordinal counter for objects created at the same moment.

```
(timestamp, ordinal)
(1697537445123, 0)
(1697537445123, 1)
(1697537445123, 2)
(1697537445200, 0)
```

**Pros:**
- Handles multiple objects at same timestamp
- Temporal ordering maintained
- Deterministic ordering within same timestamp

**Cons:**
- Requires central coordination
- More storage than single values

#### 4. Timestamp + Random Number
Combines timestamp with a random number for uniqueness.

```
(timestamp, random)
(1697537445123, 847562)
(1697537445123, 293847)
(1697537445124, 563829)
```

**Pros:**
- Reduced coordination overhead
- Temporal component useful for time-based queries
- Low collision probability

**Cons:**
- Non-zero collision probability
- Requires good random number generator
- Slightly larger storage

## Async Identities

Async identities can be generated independently on different nodes without requiring coordination at generation time. This enables offline operation and eliminates synchronization bottlenecks.

### Characteristics
- No coordination required for identity generation
- Enable offline operation
- May require larger identity space to avoid collisions
- Trade coordination for collision probability

### Examples

#### 1. Large Random Number
Uses a sufficiently large random number space to make collisions statistically improbable.

```
Identity: 7364827364827364827364
Identity: 2837465283746528374652
Identity: 9283746529837465298374
```

**Recommendations:**
- Use hardware random number generators when available
- Minimum 128-bit random numbers (UUID/GUID level)
- Cryptographically secure random generation preferred

**Pros:**
- No coordination required
- Simple implementation
- Works offline

**Cons:**
- Larger storage requirements
- Non-zero collision probability
- No inherent ordering

#### 2. Node Number + Object Number
Combines a unique node identifier with a node-local object counter.

```
(node_id, object_number)
Node 1: (1, 0), (1, 1), (1, 2)
Node 2: (2, 0), (2, 1), (2, 2)
Node 3: (3, 0), (3, 1), (3, 2)
```

**Pros:**
- Guaranteed uniqueness if node IDs are unique
- Efficient use of space
- Node-local counter is simple

**Cons:**
- Requires unique node ID assignment
- May reveal topology information

#### 3. Node Number + Timestamp + Ordinal Number
Combines node ID, timestamp, and ordinal for objects created at the same node and time.

```
(node_id, timestamp, ordinal)
(1, 1697537445123, 0)
(1, 1697537445123, 1)
(2, 1697537445123, 0)
```

**Pros:**
- Guaranteed uniqueness
- Temporal ordering
- Can identify creation node and time

**Cons:**
- Larger storage requirement
- Requires synchronized clocks (loosely)
- More complex structure

#### 4. Node Number + Timestamp + Random Number
Combines node ID, timestamp, and random number.

```
(node_id, timestamp, random)
(1, 1697537445123, 847562)
(1, 1697537445123, 293847)
(2, 1697537445124, 563829)
```

**Pros:**
- High uniqueness guarantee
- Temporal component
- Reduced coordination

**Cons:**
- Largest storage requirement
- Most complex structure

## Partially Synchronized Identities

Partially synchronized identities represent a middle ground: instead of synchronizing every object identity, only node identities are synchronized. This reduces coordination frequency while maintaining uniqueness guarantees.

### Characteristics
- Node identity generation requires coordination
- Object identity generation is local to each node
- Reduced coordination overhead compared to full sync
- Node identity generation is infrequent compared to object creation

### Strategy

A central identity server generates unique node IDs. Nodes can then independently generate object identities without further coordination.

```
Startup:
Node A -> Identity Server -> Receives Node ID: 1
Node B -> Identity Server -> Receives Node ID: 2

Runtime (no coordination):
Node A creates objects: (1, 0), (1, 1), (1, 2)
Node B creates objects: (2, 0), (2, 1), (2, 2)
```

### Examples

#### 1. Node Number + Object Number
Same structure as async variant, but with coordinated node ID assignment.

```
(node_id, object_number)
```

**Coordination:** Only at node registration/startup

#### 2. Node Number + Random Object Number
Node ID is coordinated, but objects use random numbers locally.

```
(node_id, random)
```

**Coordination:** Only at node registration/startup

**Pros:**
- Reduced coordination compared to full sync
- Lower collision probability than pure random
- Works well for dynamic node pools

#### 3. Node Number + Timestamp + Ordinal Number
Node ID is coordinated, objects use local timestamp and counter.

```
(node_id, timestamp, ordinal)
```

**Coordination:** Only at node registration/startup

**Pros:**
- Temporal ordering
- Guaranteed uniqueness
- Minimal coordination

#### 4. Node Number + Timestamp + Random Number
Node ID is coordinated, objects use local timestamp and random number.

```
(node_id, timestamp, random)
```

**Coordination:** Only at node registration/startup

**Pros:**
- Strong uniqueness guarantees
- Temporal information preserved
- Flexible for various workloads

## Comparison and Use Cases

### When to Use Sync Identities

- **Use cases:**
  - Strong consistency requirements
  - Sequential ordering is important
  - Small to medium scale systems
  - Financial transactions
  - Audit logs requiring strict ordering

- **Trade-offs:**
  - ✅ Guaranteed uniqueness
  - ✅ Sequential ordering
  - ✅ Compact representation
  - ❌ Coordination bottleneck
  - ❌ Single point of failure
  - ❌ Network dependency

### When to Use Async Identities

- **Use cases:**
  - High scalability requirements
  - Offline-first applications
  - Geographically distributed systems
  - IoT devices with intermittent connectivity
  - Mobile applications

- **Trade-offs:**
  - ✅ No coordination overhead
  - ✅ Offline operation
  - ✅ High scalability
  - ❌ Larger storage requirements
  - ❌ Statistical collision probability
  - ❌ No inherent ordering

### When to Use Partially Synchronized Identities

- **Use cases:**
  - Microservices architectures
  - Multi-tenant systems
  - Container orchestration platforms
  - Load-balanced application clusters
  - Systems with dynamic node pools

- **Trade-offs:**
  - ✅ Reduced coordination frequency
  - ✅ Good balance of guarantees and scalability
  - ✅ Node-level attribution
  - ⚠️ Requires node management
  - ⚠️ Initial coordination needed

### Storage Considerations

| Identity Type | Typical Size | Example |
|--------------|--------------|---------|
| Single Integer | 8 bytes | `uint64` |
| Single Timestamp | 8 bytes | `int64` (milliseconds) |
| Timestamp + Ordinal | 12 bytes | `(int64, uint32)` |
| Timestamp + Random | 16 bytes | `(int64, uint64)` |
| Large Random | 16 bytes | `UUID/GUID` |
| Node + Object | 12 bytes | `(uint32, uint64)` |
| Node + Time + Ordinal | 16 bytes | `(uint32, int64, uint32)` |
| Node + Time + Random | 20 bytes | `(uint32, int64, uint64)` |

### Performance Considerations

**Throughput:**
- Sync: Limited by coordination service capacity
- Async: Limited only by local node capacity
- Partial: Limited by local node capacity (after initialization)

**Latency:**
- Sync: Network RTT + coordination service processing
- Async: Local generation only (microseconds)
- Partial: Local generation only (after initialization)

**Collision Handling:**
- Sync: No collisions possible
- Async: Must handle collisions (retry/regenerate)
- Partial: No collisions if node IDs are unique

## Conclusion

Complex identities are essential for building scalable distributed systems. The choice between sync, async, and partially synchronized approaches depends on your specific requirements:

- **Sync identities** provide the strongest guarantees but limit scalability
- **Async identities** offer maximum scalability but require larger storage and collision handling
- **Partially synchronized identities** provide a practical middle ground for many distributed applications

When designing your identity strategy, consider:
1. Scale requirements (objects per second, number of nodes)
2. Consistency requirements (strong vs eventual)
3. Network characteristics (latency, reliability, offline operation)
4. Storage constraints
5. Ordering requirements
6. Security and privacy implications

The identity strategy is a fundamental architectural decision that should be made early in system design, as changing it later can be extremely costly.
