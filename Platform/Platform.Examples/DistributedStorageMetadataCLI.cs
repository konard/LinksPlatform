using System;
using Platform.IO;
using Platform.Memory;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Unicode;
using Platform.Data.Doublets.Sequences;
using Platform.Data.Doublets.Decorators;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for demonstrating DistributedStorageMetadata capabilities.
    /// Shows how Links Platform can replace traditional databases in Storj-like distributed storage systems.
    /// </summary>
    public class DistributedStorageMetadataCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            try
            {
                Console.WriteLine("=== Storj-like Distributed Storage Metadata Example using Links Platform ===");
                Console.WriteLine();
                Console.WriteLine("This example demonstrates how Links Platform can be used as a metadata database");
                Console.WriteLine("for Storj-like distributed storage systems, tracking:");
                Console.WriteLine("  - Storage nodes with addresses and status");
                Console.WriteLine("  - Files with metadata (hash, size, upload date)");
                Console.WriteLine("  - File chunks with erasure coding");
                Console.WriteLine("  - Chunk replication across nodes");
                Console.WriteLine("  - Node availability and chunk distribution");
                Console.WriteLine();

                // Initialize storage
                using (var memory = new HeapResizableDirectMemory())
                using (var links = new UnitedMemoryLinks<ulong>(memory))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    var unicodeMap = new UnicodeMap(syncLinks);
                    unicodeMap.Init();
                    var sequences = new Sequences(syncLinks, new SequencesOptions<ulong> { UseSequenceMarker = true, UseCompression = false });
                    var storage = new DistributedStorageMetadata(syncLinks, sequences);

                    // Create storage nodes (simulating a distributed network)
                    Console.WriteLine("Creating storage nodes in the network...");
                    var node1 = storage.CreateNode("192.168.1.10:7777", 1024L * 1024 * 1024 * 100, "online"); // 100GB
                    var node2 = storage.CreateNode("192.168.1.11:7777", 1024L * 1024 * 1024 * 200, "online"); // 200GB
                    var node3 = storage.CreateNode("192.168.1.12:7777", 1024L * 1024 * 1024 * 150, "online"); // 150GB
                    var node4 = storage.CreateNode("192.168.1.13:7777", 1024L * 1024 * 1024 * 80, "online");  // 80GB
                    var node5 = storage.CreateNode("192.168.1.14:7777", 1024L * 1024 * 1024 * 120, "online"); // 120GB

                    Console.WriteLine($"  - Created node 1: 192.168.1.10:7777 (100GB) - ID: {node1}");
                    Console.WriteLine($"  - Created node 2: 192.168.1.11:7777 (200GB) - ID: {node2}");
                    Console.WriteLine($"  - Created node 3: 192.168.1.12:7777 (150GB) - ID: {node3}");
                    Console.WriteLine($"  - Created node 4: 192.168.1.13:7777 (80GB) - ID: {node4}");
                    Console.WriteLine($"  - Created node 5: 192.168.1.14:7777 (120GB) - ID: {node5}");
                    Console.WriteLine();

                    // Create files
                    Console.WriteLine("Uploading files to the network...");
                    var file1 = storage.CreateFile(
                        "document.pdf",
                        "a1b2c3d4e5f6",
                        1024L * 1024 * 50, // 50MB
                        DateTime.Now.AddDays(-7)
                    );
                    var file2 = storage.CreateFile(
                        "video.mp4",
                        "f6e5d4c3b2a1",
                        1024L * 1024 * 500, // 500MB
                        DateTime.Now.AddDays(-3)
                    );

                    Console.WriteLine($"  - Created file: document.pdf (50MB) - ID: {file1}");
                    Console.WriteLine($"  - Created file: video.mp4 (500MB) - ID: {file2}");
                    Console.WriteLine();

                    // Create chunks for file1 (document.pdf) - split into 5 chunks
                    Console.WriteLine("Splitting files into chunks with erasure coding...");
                    var file1Chunks = new ulong[5];
                    for (int i = 0; i < 5; i++)
                    {
                        file1Chunks[i] = storage.CreateChunk(
                            file1,
                            i,
                            $"chunk1_{i}_hash",
                            i % 3 // Erasure code segment
                        );
                    }
                    Console.WriteLine($"  - document.pdf split into 5 chunks");

                    // Create chunks for file2 (video.mp4) - split into 10 chunks
                    var file2Chunks = new ulong[10];
                    for (int i = 0; i < 10; i++)
                    {
                        file2Chunks[i] = storage.CreateChunk(
                            file2,
                            i,
                            $"chunk2_{i}_hash",
                            i % 3 // Erasure code segment
                        );
                    }
                    Console.WriteLine($"  - video.mp4 split into 10 chunks");
                    Console.WriteLine();

                    // Distribute chunks across nodes (with replication for redundancy)
                    Console.WriteLine("Distributing chunks across nodes with replication...");

                    // Replicate file1 chunks across 3 nodes each (for redundancy)
                    storage.StoreChunkOnNode(file1Chunks[0], node1);
                    storage.StoreChunkOnNode(file1Chunks[0], node2);
                    storage.StoreChunkOnNode(file1Chunks[0], node3);

                    storage.StoreChunkOnNode(file1Chunks[1], node2);
                    storage.StoreChunkOnNode(file1Chunks[1], node3);
                    storage.StoreChunkOnNode(file1Chunks[1], node4);

                    storage.StoreChunkOnNode(file1Chunks[2], node1);
                    storage.StoreChunkOnNode(file1Chunks[2], node4);
                    storage.StoreChunkOnNode(file1Chunks[2], node5);

                    storage.StoreChunkOnNode(file1Chunks[3], node2);
                    storage.StoreChunkOnNode(file1Chunks[3], node4);
                    storage.StoreChunkOnNode(file1Chunks[3], node5);

                    storage.StoreChunkOnNode(file1Chunks[4], node1);
                    storage.StoreChunkOnNode(file1Chunks[4], node3);
                    storage.StoreChunkOnNode(file1Chunks[4], node5);

                    Console.WriteLine("  - document.pdf chunks replicated 3x each");

                    // Replicate file2 chunks across 2-3 nodes each
                    for (int i = 0; i < 10; i++)
                    {
                        // Each chunk stored on 2-3 different nodes
                        storage.StoreChunkOnNode(file2Chunks[i], i % 5 == 0 ? node1 : i % 5 == 1 ? node2 : i % 5 == 2 ? node3 : i % 5 == 3 ? node4 : node5);
                        storage.StoreChunkOnNode(file2Chunks[i], (i + 1) % 5 == 0 ? node1 : (i + 1) % 5 == 1 ? node2 : (i + 1) % 5 == 2 ? node3 : (i + 1) % 5 == 3 ? node4 : node5);
                        if (i % 2 == 0)
                        {
                            storage.StoreChunkOnNode(file2Chunks[i], (i + 2) % 5 == 0 ? node1 : (i + 2) % 5 == 1 ? node2 : (i + 2) % 5 == 2 ? node3 : (i + 2) % 5 == 3 ? node4 : node5);
                        }
                    }

                    Console.WriteLine("  - video.mp4 chunks replicated 2-3x each");
                    Console.WriteLine();

                    // Demonstrate queries
                    Console.WriteLine("=== Query Examples ===");
                    Console.WriteLine();

                    Console.WriteLine("1. Finding all chunks for a file:");
                    var chunks = storage.GetFileChunks(file1);
                    Console.WriteLine($"   document.pdf has {chunks.Count} chunks");
                    Console.WriteLine();

                    Console.WriteLine("2. Finding where a specific chunk is stored:");
                    var locations = storage.GetChunkLocations(file1Chunks[0]);
                    Console.WriteLine($"   Chunk 0 of document.pdf is stored on {locations.Count} nodes");
                    Console.WriteLine();

                    Console.WriteLine("3. Checking replication factor:");
                    var replicationFactor = storage.GetReplicationFactor(file1Chunks[0]);
                    Console.WriteLine($"   Chunk 0 replication factor: {replicationFactor}x");
                    Console.WriteLine();

                    Console.WriteLine("4. Finding all chunks on a node:");
                    var nodeChunks = storage.GetNodeChunks(node1);
                    Console.WriteLine($"   Node 1 stores {nodeChunks.Count} chunks");
                    Console.WriteLine();

                    Console.WriteLine("5. Simulating node failure:");
                    storage.UpdateNodeStatus(node3, "offline");
                    Console.WriteLine("   Node 3 marked as offline");
                    Console.WriteLine("   (In a real system, chunks would be re-replicated to other nodes)");
                    Console.WriteLine();

                    // Print statistics
                    Console.WriteLine("=== Network Statistics ===");
                    storage.PrintStatistics();
                    Console.WriteLine();

                    Console.WriteLine("=== Benefits of Links Platform for Distributed Storage Metadata ===");
                    Console.WriteLine("  ✓ Natural graph model: Nodes, chunks, and files are native entities");
                    Console.WriteLine("  ✓ Efficient traversal: Find chunk locations without complex queries");
                    Console.WriteLine("  ✓ Flexible schema: Add new metadata without schema migrations");
                    Console.WriteLine("  ✓ Fast updates: Node status changes are simple link updates");
                    Console.WriteLine("  ✓ Relationship tracking: Direct links between chunks and nodes");
                    Console.WriteLine("  ✓ Replication tracking: Easy to calculate and verify redundancy");
                    Console.WriteLine("  ✓ Erasure coding support: Store coding segments as link attributes");
                    Console.WriteLine();

                    Console.WriteLine("Example completed successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
