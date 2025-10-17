# LinksCloud Examples

LinksCloud is a decentralized computational network (grid) with an integrated code and knowledge encyclopedia that anyone can edit.

## Overview

LinksCloud combines three core components:

1. **P2P Network** - Decentralized peer-to-peer networking allowing nodes to discover and communicate with each other
2. **Distributed Knowledge Store** - Wiki-like encyclopedia where knowledge can be created, edited, and shared across the network
3. **Computational Grid** - Distributed task execution system for running computations across available nodes

## Quick Start

### Running a Single Node

```bash
cd Platform/Platform.Data.LinksCloud.Node
dotnet run
```

This will start a LinksCloud node on the default port (9000) with a default name.

### Running Multiple Nodes

Start the first node:
```bash
dotnet run "Node1" 9000
```

Start a second node on a different port:
```bash
dotnet run "Node2" 9001
```

Connect the second node to the first:
```
> connect localhost 9000
```

## Example Usage

### Knowledge Encyclopedia

Create a knowledge entry:
```
> create "LinksPlatform" "A platform for working with associative data using doublets"
```

Search for knowledge:
```
> search "LinksPlatform"
```

List all knowledge entries:
```
> list
```

### Computational Grid

Submit a computational task:
```
> task "MyComputation" "analysis"
```

Check task status:
```
> tasks
```

### Network Management

Show node status:
```
> status
```

List connected peers:
```
> peers
```

Connect to a peer:
```
> connect <address> <port>
```

## Architecture

### P2P Networking (`Platform.Data.LinksCloud.Network`)

- **P2PNetworkManager**: Manages peer connections and message passing
- Uses UDP for lightweight communication
- Supports peer discovery and automatic synchronization

### Distributed Knowledge Storage (`Platform.Data.LinksCloud.Storage`)

- **DistributedKnowledgeStore**: Stores and syncs knowledge entries across nodes
- Version-based conflict resolution
- Content hashing for integrity verification
- Built on top of Links associative storage

### Computational Grid (`Platform.Data.LinksCloud.Compute`)

- **TaskScheduler**: Distributes tasks across available nodes
- Load balancing based on node capacity
- Task status tracking and failure handling
- Priority-based task scheduling

## Integration with LinksPlatform

LinksCloud is built on top of the LinksPlatform's core doublets storage system. Knowledge entries and computational tasks are stored as links, providing:

- Efficient associative storage
- Fast querying and relationship traversal
- Persistence and scalability

## Use Cases

1. **Decentralized Wikipedia**: Create a collaborative knowledge base that's distributed across nodes
2. **Distributed Computing**: Run computational workloads across multiple machines
3. **Code Encyclopedia**: Share and search code snippets and documentation
4. **Research Collaboration**: Share research data and computational resources
5. **Decentralized GitHub**: Version control and code hosting without central servers

## Related Projects

- [CorrelationCenterConcept](https://github.com/AKMAxelerator/CorrelationCenterConcept) - Resource-oriented system mapping
- [Diaspora](https://github.com/diaspora/diaspora) - Decentralized social network
- [IPFS](https://ipfs.io) - Distributed file system

## Future Enhancements

- [ ] IPFS integration for content-addressed storage
- [ ] Blockchain-based consensus for knowledge validation
- [ ] DHT for improved peer discovery
- [ ] WebRTC for browser-based nodes
- [ ] Encrypted communication between peers
- [ ] Reputation system for nodes and contributors
- [ ] Advanced search with semantic queries
- [ ] Plugin system for custom task executors
