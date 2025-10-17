using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Platform.Data.LinksCloud.Models;

namespace Platform.Data.LinksCloud.Network
{
    /// <summary>
    /// Interface for P2P network management operations.
    /// </summary>
    public interface IP2PNetworkManager
    {
        /// <summary>
        /// Gets the current node's information.
        /// </summary>
        NodeInfo LocalNode { get; }

        /// <summary>
        /// Gets list of known peer nodes.
        /// </summary>
        IReadOnlyList<NodeInfo> Peers { get; }

        /// <summary>
        /// Initializes and starts the P2P network.
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Stops the P2P network.
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// Connects to a specific peer node.
        /// </summary>
        Task<bool> ConnectToPeerAsync(string address, int port);

        /// <summary>
        /// Disconnects from a specific peer.
        /// </summary>
        Task DisconnectPeerAsync(Guid nodeId);

        /// <summary>
        /// Broadcasts a message to all connected peers.
        /// </summary>
        Task BroadcastMessageAsync(string messageType, object data);

        /// <summary>
        /// Sends a message to a specific peer.
        /// </summary>
        Task SendMessageAsync(Guid targetNodeId, string messageType, object data);

        /// <summary>
        /// Event triggered when a new peer connects.
        /// </summary>
        event EventHandler<NodeInfo>? PeerConnected;

        /// <summary>
        /// Event triggered when a peer disconnects.
        /// </summary>
        event EventHandler<NodeInfo>? PeerDisconnected;

        /// <summary>
        /// Event triggered when a message is received.
        /// </summary>
        event EventHandler<MessageReceivedEventArgs>? MessageReceived;
    }

    /// <summary>
    /// Event args for message received events.
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        public Guid SenderNodeId { get; set; }
        public string MessageType { get; set; } = string.Empty;
        public object Data { get; set; } = new();
    }
}
