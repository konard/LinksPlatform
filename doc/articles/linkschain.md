# LinksChain: Blockchain in Associative Storage

## Overview

LinksChain is an implementation of a blockchain-like chain structure that can be stored inside associative storage (doublets). This solution addresses [Issue #647](https://github.com/konard/LinksPlatform/issues/647) by providing a way to transform a range of links into a links chain that can be functionally equivalent to a blockchain.

## Concept

Based on the diagram from the original issue:

![LinksChain Concept](https://user-images.githubusercontent.com/1431904/167183903-7a777f74-df3d-46c3-b00b-7f9a2a998d2e.jpeg)

The LinksChain implementation allows you to create chains of links where:
- Each chain element references data stored in the associative memory
- Elements are connected via "Previous" and "Next" relationships
- The chain can be traversed in both directions
- External data can reference any link in the chain
- The entire structure is stored using only doublets (pairs of links)

## Key Features

1. **Blockchain-like Structure**: Each element in the chain references the previous element, creating an immutable sequence
2. **Bidirectional Traversal**: Navigate forward and backward through the chain
3. **Validation**: Built-in methods to verify chain integrity
4. **Associative Storage**: The entire chain is stored using doublets, allowing efficient storage and retrieval
5. **Flexible Data References**: Each chain element can reference any data stored in the links database

## Implementation Details

### Core Components

The `LinksChain<TLink>` class provides the following functionality:

- **Markers**: Three special links mark the chain structure:
  - `ChainMarker`: Identifies elements as part of a chain
  - `PrevMarker`: Marks previous relationships
  - `NextMarker`: Marks next relationships

- **Chain Operations**:
  - `CreateChain(data)`: Creates a new chain with a single element
  - `Append(chainHead, data)`: Appends a new element to the chain
  - `GetPrevious(element)`: Gets the previous element
  - `GetNext(element)`: Gets the next element
  - `GetData(element)`: Retrieves the data stored in a chain element

- **Navigation**:
  - `GetFirstElement(anyElement)`: Finds the first element from any point in the chain
  - `GetLastElement(anyElement)`: Finds the last element from any point in the chain
  - `GetAllElements(chainHead)`: Iterates through all elements in order

- **Validation**:
  - `ValidateChain(chainHead)`: Verifies chain integrity (no cycles, consistent relationships)
  - `GetChainLength(anyElement)`: Returns the number of elements in the chain

### Storage Structure

Each chain element is stored as follows:

```
ChainElement: ChainMarker -> Data

NextRelationship: NextMarker -> (ElementA -> ElementB)
PrevRelationship: PrevMarker -> (ElementB -> ElementA)
```

This structure ensures that:
1. Each chain element is explicitly marked as part of a chain
2. Prev/Next relationships are stored as separate links
3. The entire structure can be reconstructed by traversing the links

## Usage Examples

### Basic Chain Creation

```csharp
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Examples;

// Initialize links storage
using (var memoryAdapter = new UInt64UnitedMemoryLinks("db.links", 8 * 1024 * 1024))
using (var links = new UInt64Links(memoryAdapter))
{
    var chain = new LinksChain<ulong>(links);

    // Create data
    var data1 = links.Create();
    var data2 = links.Create();
    var data3 = links.Create();

    // Create chain
    var chainHead = chain.CreateChain(data1);
    chain.Append(chainHead, data2);
    chain.Append(chainHead, data3);

    // Traverse chain
    foreach (var element in chain.GetAllElements(chainHead))
    {
        var data = chain.GetData(element);
        Console.WriteLine($"Element: {element}, Data: {data}");
    }
}
```

### Blockchain Example

```csharp
// Create blockchain-like structure
var chain = new LinksChain<ulong>(links);

// Genesis block
var genesisData = links.Create();
var block1 = chain.CreateChain(genesisData);

// Block 2 references block 1
var block2Data = links.Update(links.Create(), genesisData, block1);
var block2 = chain.Append(block1, block2Data);

// Block 3 references block 2
var block3Data = links.Update(links.Create(), block2Data, block2);
var block3 = chain.Append(block2, block3Data);

// Validate blockchain
bool isValid = chain.ValidateChain(block1);
```

### Creating Chain from Sequence

```csharp
// Create sequence of data
var dataSequence = new ulong[10];
for (int i = 0; i < dataSequence.Length; i++)
{
    dataSequence[i] = links.Create();
}

// Create chain from sequence
var chainHead = chain.CreateChainFromSequence(dataSequence);

// Get chain length
int length = chain.GetChainLength(chainHead); // Returns 10
```

## Running the Examples

The solution includes several example programs:

### LinksChainCLI

Run the command-line interface to see LinksChain in action:

```bash
dotnet run --project Platform.Examples -- LinksChainCLI
```

This will:
1. Create a new links database
2. Run basic chain operations
3. Demonstrate sequence creation
4. Show blockchain-like immutable chains
5. Display all links in the database

### LinksChainTest

Run the test in the Sandbox:

```csharp
using Platform.Sandbox;

LinksChainTest.Test();
```

This runs comprehensive tests including:
- Basic chain operations
- Chain navigation (forward and backward)
- Chain validation
- Sequence creation

## Benefits

1. **Immutability**: Once created, chain relationships are permanent
2. **Verifiability**: Chain integrity can be validated at any time
3. **Efficiency**: Uses the efficient doublets storage model
4. **Flexibility**: Can store any data in the chain elements
5. **Bidirectional**: Can traverse from any point in either direction

## Use Cases

- **Audit Trails**: Track changes to data over time
- **Version Control**: Store versioned data with references to previous versions
- **Event Sourcing**: Record events in a verifiable sequence
- **Linked Data**: Create chains of related information
- **Blockchain Applications**: Build blockchain-like structures in associative memory

## Related Work

- [Platform.Data.Doublets](https://github.com/linksplatform/Data.Doublets) - The underlying doublets storage
- [Platform.Data.Doublets.Sequences](https://github.com/linksplatform/Data.Doublets.Sequences) - Sequence abstractions
- [Links Theory (Russian)](../doc/articles/links-theory.md) - Theoretical foundation

## Future Enhancements

Potential improvements for LinksChain:

1. **Hash Verification**: Add cryptographic hashes for data integrity
2. **Merkle Trees**: Implement Merkle tree structures for efficient verification
3. **Branching**: Support chain branching and merging
4. **Compression**: Optimize storage using sequence compression
5. **Query Language**: Add a query interface for chain traversal

## Conclusion

LinksChain demonstrates how blockchain-like structures can be elegantly implemented using associative storage. By leveraging doublets, we achieve an efficient, flexible, and verifiable chain structure suitable for various applications.
