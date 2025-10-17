using System;
using System.Threading.Tasks;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.LinksCloud.Compute;
using Platform.Data.LinksCloud.Models;
using Platform.Data.LinksCloud.Network;
using Platform.Data.LinksCloud.Storage;

namespace Platform.Data.LinksCloud.Core
{
    /// <summary>
    /// Main LinksCloud node that integrates all components:
    /// - P2P networking
    /// - Distributed knowledge storage
    /// - Computational grid
    /// </summary>
    public class LinksCloudNode : IDisposable
    {
        private readonly ILinks<ulong, LinksConstants<ulong>> _links;
        private readonly IP2PNetworkManager _networkManager;
        private readonly IDistributedKnowledgeStore _knowledgeStore;
        private readonly ITaskScheduler _taskScheduler;

        public NodeInfo LocalNode => _networkManager.LocalNode;
        public IP2PNetworkManager NetworkManager => _networkManager;
        public IDistributedKnowledgeStore KnowledgeStore => _knowledgeStore;
        public ITaskScheduler TaskScheduler => _taskScheduler;

        public LinksCloudNode(
            ILinks<ulong, LinksConstants<ulong>> links,
            string nodeName,
            int port,
            int computeCapacity = 100,
            long storageCapacity = 1073741824)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _networkManager = new P2PNetworkManager(nodeName, port, computeCapacity, storageCapacity);
            _knowledgeStore = new DistributedKnowledgeStore(_links, _networkManager);
            _taskScheduler = new Compute.TaskScheduler(_networkManager);

            SetupEventHandlers();
        }

        /// <summary>
        /// Starts the LinksCloud node.
        /// </summary>
        public async Task StartAsync()
        {
            await _networkManager.StartAsync();
            await _taskScheduler.StartWorkerAsync();

            // Initial sync with network
            await _knowledgeStore.SyncWithNetworkAsync();
        }

        /// <summary>
        /// Stops the LinksCloud node.
        /// </summary>
        public async Task StopAsync()
        {
            await _taskScheduler.StopWorkerAsync();
            await _networkManager.StopAsync();
        }

        /// <summary>
        /// Connects to a peer node.
        /// </summary>
        public async Task<bool> ConnectToPeerAsync(string address, int port)
        {
            return await _networkManager.ConnectToPeerAsync(address, port);
        }

        /// <summary>
        /// Creates a new knowledge entry.
        /// </summary>
        public async Task<ulong> CreateKnowledgeEntryAsync(string title, string content, params string[] tags)
        {
            return await _knowledgeStore.CreateEntryAsync(title, content, new System.Collections.Generic.List<string>(tags));
        }

        /// <summary>
        /// Searches for knowledge entries.
        /// </summary>
        public async Task<System.Collections.Generic.List<KnowledgeEntry>> SearchKnowledgeAsync(string query)
        {
            return await _knowledgeStore.SearchEntriesAsync(query);
        }

        /// <summary>
        /// Submits a computational task to the grid.
        /// </summary>
        public async Task<Guid> SubmitComputeTaskAsync(ComputeTask task)
        {
            return await _taskScheduler.SubmitTaskAsync(task);
        }

        /// <summary>
        /// Gets the status of a computational task.
        /// </summary>
        public async Task<ComputeTask?> GetTaskStatusAsync(Guid taskId)
        {
            return await _taskScheduler.GetTaskStatusAsync(taskId);
        }

        private void SetupEventHandlers()
        {
            _networkManager.PeerConnected += (sender, peer) =>
            {
                Console.WriteLine($"[LinksCloud] Peer connected: {peer.Name} ({peer.NodeId})");
            };

            _networkManager.PeerDisconnected += (sender, peer) =>
            {
                Console.WriteLine($"[LinksCloud] Peer disconnected: {peer.Name} ({peer.NodeId})");
            };

            _knowledgeStore.EntryCreated += (sender, entry) =>
            {
                Console.WriteLine($"[LinksCloud] Knowledge entry created: {entry.Title}");
            };

            _knowledgeStore.EntryUpdated += (sender, entry) =>
            {
                Console.WriteLine($"[LinksCloud] Knowledge entry updated: {entry.Title}");
            };

            _taskScheduler.TaskCompleted += (sender, task) =>
            {
                Console.WriteLine($"[LinksCloud] Task completed: {task.Name}");
            };

            _taskScheduler.TaskFailed += (sender, task) =>
            {
                Console.WriteLine($"[LinksCloud] Task failed: {task.Name} - {task.ErrorMessage}");
            };
        }

        public void Dispose()
        {
            StopAsync().Wait();
            (_networkManager as IDisposable)?.Dispose();
        }
    }
}
