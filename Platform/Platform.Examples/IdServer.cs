using System;
using System.Collections.Concurrent;
using System.Threading;
using Platform.Communication.Protocol.Udp;

namespace Platform.Examples
{
    /// <summary>
    /// IdServer distributes unique ID ranges to servers in a cluster.
    /// This enables a shared global address space across multiple servers.
    /// </summary>
    public class IdServer
    {
        private readonly UdpSender _sender;
        private readonly ulong _blockSize;
        private ulong _nextAvailableId;
        private readonly ConcurrentDictionary<string, IdBlock> _allocatedBlocks;
        private readonly object _allocationLock = new object();

        public IdServer(UdpSender sender, ulong startId = 1, ulong blockSize = 1000)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _blockSize = blockSize;
            _nextAvailableId = startId;
            _allocatedBlocks = new ConcurrentDictionary<string, IdBlock>();
        }

        /// <summary>
        /// Allocates a new block of IDs to a requesting server.
        /// </summary>
        /// <param name="serverId">Unique identifier for the requesting server</param>
        /// <returns>Allocated ID block</returns>
        public IdBlock AllocateBlock(string serverId)
        {
            if (string.IsNullOrWhiteSpace(serverId))
            {
                throw new ArgumentException("Server ID cannot be null or empty", nameof(serverId));
            }

            lock (_allocationLock)
            {
                var startId = _nextAvailableId;
                var endId = _nextAvailableId + _blockSize - 1;
                _nextAvailableId += _blockSize;

                var block = new IdBlock(startId, endId, serverId);
                _allocatedBlocks[serverId] = block;

                return block;
            }
        }

        /// <summary>
        /// Handles incoming ID allocation requests from servers.
        /// </summary>
        /// <param name="message">Request message containing server ID</param>
        public void HandleRequest(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            message = message.Trim();

            if (message.StartsWith("REQUEST_ID_BLOCK:"))
            {
                var serverId = message.Substring("REQUEST_ID_BLOCK:".Length).Trim();

                try
                {
                    var block = AllocateBlock(serverId);
                    var response = $"ID_BLOCK_ALLOCATED:{serverId}:{block.StartId}:{block.EndId}";
                    _sender.Send(response);
                    Console.WriteLine($"Allocated block [{block.StartId}-{block.EndId}] to server '{serverId}'");
                }
                catch (Exception ex)
                {
                    var errorResponse = $"ID_BLOCK_ERROR:{serverId}:{ex.Message}";
                    _sender.Send(errorResponse);
                    Console.WriteLine($"Error allocating block to server '{serverId}': {ex.Message}");
                }
            }
            else if (message == "STATUS")
            {
                SendStatus();
            }
        }

        /// <summary>
        /// Sends current status information about allocated blocks.
        /// </summary>
        public void SendStatus()
        {
            var status = $"IdServer Status - Next Available ID: {_nextAvailableId}, Block Size: {_blockSize}, Allocated Blocks: {_allocatedBlocks.Count}";
            _sender.Send(status);
            Console.WriteLine(status);

            foreach (var kvp in _allocatedBlocks)
            {
                var blockInfo = $"  Server '{kvp.Key}': [{kvp.Value.StartId}-{kvp.Value.EndId}]";
                _sender.Send(blockInfo);
                Console.WriteLine(blockInfo);
            }
        }

        /// <summary>
        /// Gets information about a specific allocated block.
        /// </summary>
        public IdBlock GetBlockInfo(string serverId)
        {
            if (_allocatedBlocks.TryGetValue(serverId, out var block))
            {
                return block;
            }
            return null;
        }
    }

    /// <summary>
    /// Represents a block of IDs allocated to a server.
    /// </summary>
    public class IdBlock
    {
        public ulong StartId { get; }
        public ulong EndId { get; }
        public string ServerId { get; }
        public DateTime AllocatedAt { get; }

        public IdBlock(ulong startId, ulong endId, string serverId)
        {
            if (endId < startId)
            {
                throw new ArgumentException("End ID must be greater than or equal to start ID");
            }

            StartId = startId;
            EndId = endId;
            ServerId = serverId ?? throw new ArgumentNullException(nameof(serverId));
            AllocatedAt = DateTime.UtcNow;
        }

        public ulong Count => EndId - StartId + 1;

        public override string ToString()
        {
            return $"IdBlock[{StartId}-{EndId}] for '{ServerId}' ({Count} IDs)";
        }
    }
}
