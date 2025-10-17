using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Platform.Data.LinksCloud.Models;
using Platform.Communication.Protocol.Udp;

namespace Platform.Data.LinksCloud.Network
{
    /// <summary>
    /// Manages P2P network connections and communication between nodes.
    /// Uses UDP for lightweight message passing between peers.
    /// </summary>
    public class P2PNetworkManager : IP2PNetworkManager, IDisposable
    {
        private readonly NodeInfo _localNode;
        private readonly List<NodeInfo> _peers;
        private readonly object _peersLock = new();
        private UdpSender? _sender;
        private UdpReceiver? _receiver;
        private bool _isRunning;

        public NodeInfo LocalNode => _localNode;

        public IReadOnlyList<NodeInfo> Peers
        {
            get
            {
                lock (_peersLock)
                {
                    return _peers.ToList();
                }
            }
        }

        public event EventHandler<NodeInfo>? PeerConnected;
        public event EventHandler<NodeInfo>? PeerDisconnected;
        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

        public P2PNetworkManager(string nodeName, int port, int computeCapacity = 100, long storageCapacity = 1073741824)
        {
            _localNode = new NodeInfo
            {
                Name = nodeName,
                Port = port,
                ComputeCapacity = computeCapacity,
                StorageCapacity = storageCapacity,
                StorageUsed = 0
            };
            _peers = new List<NodeInfo>();
        }

        public async Task StartAsync()
        {
            if (_isRunning)
            {
                return;
            }

            _sender = new UdpSender(_localNode.Port + 1);
            _receiver = new UdpReceiver(_localNode.Port, OnMessageReceived);

            _isRunning = true;

            // Announce presence to network
            await BroadcastMessageAsync("NODE_ANNOUNCE", _localNode);
        }

        public async Task StopAsync()
        {
            if (!_isRunning)
            {
                return;
            }

            // Announce departure
            await BroadcastMessageAsync("NODE_LEAVE", _localNode);

            _receiver?.Dispose();
            _sender?.Dispose();

            _isRunning = false;
        }

        public async Task<bool> ConnectToPeerAsync(string address, int port)
        {
            try
            {
                var peerInfo = new NodeInfo
                {
                    Address = address,
                    Port = port,
                    IsOnline = true
                };

                await SendMessageAsync(peerInfo.NodeId, "NODE_CONNECT_REQUEST", _localNode);

                lock (_peersLock)
                {
                    if (!_peers.Any(p => p.Address == address && p.Port == port))
                    {
                        _peers.Add(peerInfo);
                        PeerConnected?.Invoke(this, peerInfo);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Task DisconnectPeerAsync(Guid nodeId)
        {
            lock (_peersLock)
            {
                var peer = _peers.FirstOrDefault(p => p.NodeId == nodeId);
                if (peer != null)
                {
                    _peers.Remove(peer);
                    PeerDisconnected?.Invoke(this, peer);
                }
            }
            return Task.CompletedTask;
        }

        public async Task BroadcastMessageAsync(string messageType, object data)
        {
            if (_sender == null || !_isRunning)
            {
                return;
            }

            var message = SerializeMessage(messageType, data);

            List<NodeInfo> peersCopy;
            lock (_peersLock)
            {
                peersCopy = _peers.ToList();
            }

            foreach (var peer in peersCopy)
            {
                try
                {
                    _sender.Send(message);
                }
                catch
                {
                    // Handle send failure
                }
            }

            await Task.CompletedTask;
        }

        public async Task SendMessageAsync(Guid targetNodeId, string messageType, object data)
        {
            if (_sender == null || !_isRunning)
            {
                return;
            }

            NodeInfo? peer;
            lock (_peersLock)
            {
                peer = _peers.FirstOrDefault(p => p.NodeId == targetNodeId);
            }

            if (peer != null)
            {
                var message = SerializeMessage(messageType, data);
                try
                {
                    _sender.Send(message);
                }
                catch
                {
                    // Handle send failure
                }
            }

            await Task.CompletedTask;
        }

        private void OnMessageReceived(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            try
            {
                var (messageType, data, senderNodeId) = DeserializeMessage(message);
                MessageReceived?.Invoke(this, new MessageReceivedEventArgs
                {
                    SenderNodeId = senderNodeId,
                    MessageType = messageType,
                    Data = data
                });

                HandleSystemMessage(messageType, data, senderNodeId);
            }
            catch
            {
                // Handle deserialization errors
            }
        }

        private void HandleSystemMessage(string messageType, object data, Guid senderNodeId)
        {
            switch (messageType)
            {
                case "NODE_ANNOUNCE":
                    if (data is NodeInfo nodeInfo)
                    {
                        lock (_peersLock)
                        {
                            if (!_peers.Any(p => p.NodeId == nodeInfo.NodeId))
                            {
                                _peers.Add(nodeInfo);
                                PeerConnected?.Invoke(this, nodeInfo);
                            }
                        }
                    }
                    break;

                case "NODE_LEAVE":
                    lock (_peersLock)
                    {
                        var peer = _peers.FirstOrDefault(p => p.NodeId == senderNodeId);
                        if (peer != null)
                        {
                            _peers.Remove(peer);
                            PeerDisconnected?.Invoke(this, peer);
                        }
                    }
                    break;
            }
        }

        private string SerializeMessage(string messageType, object data)
        {
            // Simple serialization - in production, use JSON or MessagePack
            return $"{_localNode.NodeId}|{messageType}|{data}";
        }

        private (string messageType, object data, Guid senderNodeId) DeserializeMessage(string message)
        {
            // Simple deserialization - in production, use JSON or MessagePack
            var parts = message.Split('|');
            if (parts.Length >= 3)
            {
                return (parts[1], parts[2], Guid.Parse(parts[0]));
            }
            return (string.Empty, new object(), Guid.Empty);
        }

        public void Dispose()
        {
            StopAsync().Wait();
        }
    }
}
