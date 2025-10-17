using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Unicode;
using Platform.Data.Doublets.Sequences;

namespace Platform.Examples
{
    /// <summary>
    /// Demonstrates how Links Platform can be used as a metadata database for Storj-like distributed storage systems.
    /// This example shows how to track file chunks, storage nodes, chunk distribution, and node availability.
    /// </summary>
    public class DistributedStorageMetadata
    {
        private readonly ILinks<ulong> _links;
        private readonly Sequences _sequences;

        // Marker links for different entity types
        private readonly ulong _fileMarker;
        private readonly ulong _chunkMarker;
        private readonly ulong _nodeMarker;
        private readonly ulong _locationMarker;
        private readonly ulong _hashMarker;
        private readonly ulong _sizeMarker;
        private readonly ulong _statusMarker;
        private readonly ulong _replicationMarker;
        private readonly ulong _erasureCodeMarker;
        private readonly ulong _uploadDateMarker;

        public DistributedStorageMetadata(ILinks<ulong> links, Sequences sequences)
        {
            _links = links;
            _sequences = sequences;

            // Initialize markers
            _fileMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("File"));
            _chunkMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("Chunk"));
            _nodeMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("Node"));
            _locationMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("Location"));
            _hashMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("Hash"));
            _sizeMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("Size"));
            _statusMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("Status"));
            _replicationMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("Replication"));
            _erasureCodeMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("ErasureCode"));
            _uploadDateMarker = _sequences.Create(UnicodeMap.FromStringToLinkArray("UploadDate"));
        }

        /// <summary>
        /// Creates a storage node with address and capacity information.
        /// </summary>
        public ulong CreateNode(string address, long capacity, string status = "online")
        {
            var addressLink = _sequences.Create(UnicodeMap.FromStringToLinkArray(address));
            var capacityLink = _sequences.Create(UnicodeMap.FromStringToLinkArray(capacity.ToString()));
            var statusLink = _sequences.Create(UnicodeMap.FromStringToLinkArray(status));

            // Create node with address
            var nodeAddress = _links.Create();
            _links.Update(nodeAddress, _locationMarker, addressLink);

            // Add capacity
            var nodeCapacity = _links.Create();
            _links.Update(nodeCapacity, _sizeMarker, capacityLink);

            // Add status
            var nodeStatus = _links.Create();
            _links.Update(nodeStatus, _statusMarker, statusLink);

            // Combine metadata
            var metadata1 = _links.Create();
            _links.Update(metadata1, nodeAddress, nodeCapacity);

            var metadata2 = _links.Create();
            _links.Update(metadata2, metadata1, nodeStatus);

            // Create node
            var nodeLink = _links.Create();
            _links.Update(nodeLink, _nodeMarker, metadata2);

            return nodeLink;
        }

        /// <summary>
        /// Creates a file record with metadata.
        /// </summary>
        public ulong CreateFile(string fileName, string hash, long size, DateTime uploadDate)
        {
            var nameLink = _sequences.Create(UnicodeMap.FromStringToLinkArray(fileName));
            var hashLink = _sequences.Create(UnicodeMap.FromStringToLinkArray(hash));
            var sizeLink = _sequences.Create(UnicodeMap.FromStringToLinkArray(size.ToString()));
            var dateLink = _sequences.Create(UnicodeMap.FromStringToLinkArray(uploadDate.ToString("o")));

            // Create file metadata relations
            var fileHash = _links.Create();
            _links.Update(fileHash, _hashMarker, hashLink);

            var fileSize = _links.Create();
            _links.Update(fileSize, _sizeMarker, sizeLink);

            var fileDate = _links.Create();
            _links.Update(fileDate, _uploadDateMarker, dateLink);

            // Combine metadata
            var metadata1 = _links.Create();
            _links.Update(metadata1, fileHash, fileSize);

            var metadata2 = _links.Create();
            _links.Update(metadata2, metadata1, fileDate);

            var fileNameRelation = _links.Create();
            _links.Update(fileNameRelation, nameLink, metadata2);

            // Create file
            var fileLink = _links.Create();
            _links.Update(fileLink, _fileMarker, fileNameRelation);

            return fileLink;
        }

        /// <summary>
        /// Creates a chunk of a file with hash and erasure coding information.
        /// </summary>
        public ulong CreateChunk(ulong fileLink, int chunkIndex, string chunkHash, int erasureCodeSegment)
        {
            var indexLink = _sequences.Create(UnicodeMap.FromStringToLinkArray(chunkIndex.ToString()));
            var hashLink = _sequences.Create(UnicodeMap.FromStringToLinkArray(chunkHash));
            var erasureLink = _sequences.Create(UnicodeMap.FromStringToLinkArray(erasureCodeSegment.ToString()));

            // Create chunk metadata
            var chunkHashRelation = _links.Create();
            _links.Update(chunkHashRelation, _hashMarker, hashLink);

            var chunkErasure = _links.Create();
            _links.Update(chunkErasure, _erasureCodeMarker, erasureLink);

            var chunkMetadata = _links.Create();
            _links.Update(chunkMetadata, chunkHashRelation, chunkErasure);

            var chunkIndexRelation = _links.Create();
            _links.Update(chunkIndexRelation, indexLink, chunkMetadata);

            // Create chunk
            var chunkLink = _links.Create();
            _links.Update(chunkLink, _chunkMarker, chunkIndexRelation);

            // Link chunk to file
            var fileChunkRelation = _links.Create();
            _links.Update(fileChunkRelation, fileLink, chunkLink);

            return chunkLink;
        }

        /// <summary>
        /// Records that a chunk is stored on a specific node (replication).
        /// </summary>
        public ulong StoreChunkOnNode(ulong chunkLink, ulong nodeLink)
        {
            var replicationRelation = _links.Create();
            _links.Update(replicationRelation, chunkLink, nodeLink);

            var markedReplication = _links.Create();
            _links.Update(markedReplication, _replicationMarker, replicationRelation);

            return markedReplication;
        }

        /// <summary>
        /// Gets all chunks for a specific file.
        /// </summary>
        public List<ulong> GetFileChunks(ulong fileLink)
        {
            var chunks = new List<ulong>();

            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                if (_links.GetSource(linkIndex) == fileLink)
                {
                    var target = _links.GetTarget(linkIndex);
                    if (_links.GetSource(target) == _chunkMarker)
                    {
                        chunks.Add(target);
                    }
                }
                return _links.Constants.Continue;
            });

            return chunks;
        }

        /// <summary>
        /// Gets all nodes that store a specific chunk.
        /// </summary>
        public List<ulong> GetChunkLocations(ulong chunkLink)
        {
            var nodes = new List<ulong>();

            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                if (_links.GetSource(linkIndex) == _replicationMarker)
                {
                    var replicationRelation = _links.GetTarget(linkIndex);
                    if (_links.GetSource(replicationRelation) == chunkLink)
                    {
                        nodes.Add(_links.GetTarget(replicationRelation));
                    }
                }
                return _links.Constants.Continue;
            });

            return nodes;
        }

        /// <summary>
        /// Gets all chunks stored on a specific node.
        /// </summary>
        public List<ulong> GetNodeChunks(ulong nodeLink)
        {
            var chunks = new List<ulong>();

            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                if (_links.GetSource(linkIndex) == _replicationMarker)
                {
                    var replicationRelation = _links.GetTarget(linkIndex);
                    if (_links.GetTarget(replicationRelation) == nodeLink)
                    {
                        chunks.Add(_links.GetSource(replicationRelation));
                    }
                }
                return _links.Constants.Continue;
            });

            return chunks;
        }

        /// <summary>
        /// Calculates the replication factor for a chunk.
        /// </summary>
        public int GetReplicationFactor(ulong chunkLink)
        {
            return GetChunkLocations(chunkLink).Count;
        }

        /// <summary>
        /// Finds all files stored across the network.
        /// </summary>
        public List<ulong> GetAllFiles()
        {
            var files = new List<ulong>();

            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                if (_links.GetSource(linkIndex) == _fileMarker)
                {
                    files.Add(linkIndex);
                }
                return _links.Constants.Continue;
            });

            return files;
        }

        /// <summary>
        /// Finds all storage nodes in the network.
        /// </summary>
        public List<ulong> GetAllNodes()
        {
            var nodes = new List<ulong>();

            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                if (_links.GetSource(linkIndex) == _nodeMarker)
                {
                    nodes.Add(linkIndex);
                }
                return _links.Constants.Continue;
            });

            return nodes;
        }

        /// <summary>
        /// Updates the status of a node (e.g., online, offline, maintenance).
        /// </summary>
        public void UpdateNodeStatus(ulong nodeLink, string newStatus)
        {
            var statusLink = _sequences.Create(UnicodeMap.FromStringToLinkArray(newStatus));

            // Find the node's metadata structure and update status
            // This is simplified - in production you'd traverse the metadata structure
            var statusRelation = _links.Create();
            _links.Update(statusRelation, _statusMarker, statusLink);

            // Link the new status to the node
            var nodeStatusUpdate = _links.Create();
            _links.Update(nodeStatusUpdate, nodeLink, statusRelation);
        }

        /// <summary>
        /// Prints statistics about the distributed storage network.
        /// </summary>
        public void PrintStatistics()
        {
            Console.WriteLine("Distributed Storage Network Statistics:");
            Console.WriteLine($"Total links: {_links.Count()}");

            var fileCount = GetAllFiles().Count;
            var nodeCount = GetAllNodes().Count;

            Console.WriteLine($"Total files: {fileCount}");
            Console.WriteLine($"Total storage nodes: {nodeCount}");

            // Calculate total chunks
            var chunkCount = 0;
            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                if (_links.GetSource(linkIndex) == _chunkMarker)
                {
                    chunkCount++;
                }
                return _links.Constants.Continue;
            });
            Console.WriteLine($"Total chunks: {chunkCount}");

            // Calculate total replications
            var replicationCount = 0;
            _links.Each(link =>
            {
                var linkIndex = link[_links.Constants.IndexPart];
                if (_links.GetSource(linkIndex) == _replicationMarker)
                {
                    replicationCount++;
                }
                return _links.Constants.Continue;
            });
            Console.WriteLine($"Total chunk replications: {replicationCount}");

            if (chunkCount > 0)
            {
                Console.WriteLine($"Average replication factor: {(double)replicationCount / chunkCount:F2}");
            }
        }
    }
}
