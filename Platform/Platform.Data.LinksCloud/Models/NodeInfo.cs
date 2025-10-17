using System;

namespace Platform.Data.LinksCloud.Models
{
    /// <summary>
    /// Represents information about a node in the decentralized network.
    /// </summary>
    public class NodeInfo
    {
        /// <summary>
        /// Unique identifier for the node.
        /// </summary>
        public Guid NodeId { get; set; }

        /// <summary>
        /// Display name of the node.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Network address of the node.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Port number for P2P communication.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Timestamp when the node joined the network.
        /// </summary>
        public DateTime JoinedAt { get; set; }

        /// <summary>
        /// Last time the node was seen active.
        /// </summary>
        public DateTime LastSeenAt { get; set; }

        /// <summary>
        /// Indicates whether the node is currently online.
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// Node's computational capacity (arbitrary units).
        /// </summary>
        public int ComputeCapacity { get; set; }

        /// <summary>
        /// Node's storage capacity in bytes.
        /// </summary>
        public long StorageCapacity { get; set; }

        /// <summary>
        /// Current storage usage in bytes.
        /// </summary>
        public long StorageUsed { get; set; }

        public NodeInfo()
        {
            NodeId = Guid.NewGuid();
            JoinedAt = DateTime.UtcNow;
            LastSeenAt = DateTime.UtcNow;
            IsOnline = true;
        }
    }
}
