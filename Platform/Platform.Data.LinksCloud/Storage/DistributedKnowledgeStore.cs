using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.LinksCloud.Models;
using Platform.Data.LinksCloud.Network;

namespace Platform.Data.LinksCloud.Storage
{
    /// <summary>
    /// Distributed knowledge storage using Links as the underlying data structure.
    /// Provides wiki-like functionality with version control and distributed synchronization.
    /// </summary>
    public class DistributedKnowledgeStore : IDistributedKnowledgeStore
    {
        private readonly ILinks<ulong, LinksConstants<ulong>> _links;
        private readonly IP2PNetworkManager _networkManager;
        private readonly Dictionary<ulong, KnowledgeEntry> _localCache;
        private readonly object _cacheLock = new();
        private ulong _nextEntryId = 1000;

        public event EventHandler<KnowledgeEntry>? EntryCreated;
        public event EventHandler<KnowledgeEntry>? EntryUpdated;
        public event EventHandler<ulong>? EntryDeleted;

        public DistributedKnowledgeStore(ILinks<ulong, LinksConstants<ulong>> links, IP2PNetworkManager networkManager)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            _localCache = new Dictionary<ulong, KnowledgeEntry>();

            // Subscribe to network events for synchronization
            _networkManager.MessageReceived += OnNetworkMessageReceived;
        }

        public async Task<ulong> CreateEntryAsync(string title, string content, List<string> tags)
        {
            var entry = new KnowledgeEntry
            {
                EntryId = _nextEntryId++,
                Title = title,
                Content = content,
                Tags = tags ?? new List<string>(),
                AuthorNodeId = _networkManager.LocalNode.NodeId,
                ContentHash = ComputeHash(content)
            };

            lock (_cacheLock)
            {
                _localCache[entry.EntryId] = entry;
            }

            // Store in Links database
            await StoreEntryInLinksAsync(entry);

            // Broadcast to network
            await _networkManager.BroadcastMessageAsync("KNOWLEDGE_CREATE", entry);

            EntryCreated?.Invoke(this, entry);

            return entry.EntryId;
        }

        public async Task<bool> UpdateEntryAsync(ulong entryId, string content, Guid editorNodeId)
        {
            KnowledgeEntry? entry;
            lock (_cacheLock)
            {
                if (!_localCache.TryGetValue(entryId, out entry))
                {
                    return false;
                }
            }

            entry.Content = content;
            entry.ModifiedAt = DateTime.UtcNow;
            entry.Version++;
            entry.ContentHash = ComputeHash(content);

            lock (_cacheLock)
            {
                _localCache[entryId] = entry;
            }

            // Update in Links database
            await StoreEntryInLinksAsync(entry);

            // Broadcast update to network
            await _networkManager.BroadcastMessageAsync("KNOWLEDGE_UPDATE", entry);

            EntryUpdated?.Invoke(this, entry);

            return true;
        }

        public Task<KnowledgeEntry?> GetEntryAsync(ulong entryId)
        {
            lock (_cacheLock)
            {
                return Task.FromResult(_localCache.TryGetValue(entryId, out var entry) ? entry : null);
            }
        }

        public Task<List<KnowledgeEntry>> SearchEntriesAsync(string query)
        {
            lock (_cacheLock)
            {
                var results = _localCache.Values
                    .Where(e => e.Title.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               e.Content.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
                return Task.FromResult(results);
            }
        }

        public Task<List<KnowledgeEntry>> GetEntriesByTagAsync(string tag)
        {
            lock (_cacheLock)
            {
                var results = _localCache.Values
                    .Where(e => e.Tags.Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                return Task.FromResult(results);
            }
        }

        public async Task<bool> DeleteEntryAsync(ulong entryId)
        {
            lock (_cacheLock)
            {
                if (!_localCache.Remove(entryId))
                {
                    return false;
                }
            }

            // Broadcast deletion to network
            await _networkManager.BroadcastMessageAsync("KNOWLEDGE_DELETE", entryId);

            EntryDeleted?.Invoke(this, entryId);

            return true;
        }

        public Task<List<KnowledgeEntry>> GetAllEntriesAsync()
        {
            lock (_cacheLock)
            {
                return Task.FromResult(_localCache.Values.ToList());
            }
        }

        public async Task SyncWithNetworkAsync()
        {
            // Request knowledge entries from all peers
            await _networkManager.BroadcastMessageAsync("KNOWLEDGE_SYNC_REQUEST", _networkManager.LocalNode.NodeId);
        }

        private void OnNetworkMessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            switch (e.MessageType)
            {
                case "KNOWLEDGE_CREATE":
                    if (e.Data is KnowledgeEntry createEntry)
                    {
                        lock (_cacheLock)
                        {
                            if (!_localCache.ContainsKey(createEntry.EntryId))
                            {
                                _localCache[createEntry.EntryId] = createEntry;
                                EntryCreated?.Invoke(this, createEntry);
                            }
                        }
                    }
                    break;

                case "KNOWLEDGE_UPDATE":
                    if (e.Data is KnowledgeEntry updateEntry)
                    {
                        lock (_cacheLock)
                        {
                            if (_localCache.TryGetValue(updateEntry.EntryId, out var existingEntry))
                            {
                                // Version-based conflict resolution: keep the higher version
                                if (updateEntry.Version > existingEntry.Version)
                                {
                                    _localCache[updateEntry.EntryId] = updateEntry;
                                    EntryUpdated?.Invoke(this, updateEntry);
                                }
                            }
                        }
                    }
                    break;

                case "KNOWLEDGE_DELETE":
                    if (e.Data is ulong deleteEntryId)
                    {
                        lock (_cacheLock)
                        {
                            if (_localCache.Remove(deleteEntryId))
                            {
                                EntryDeleted?.Invoke(this, deleteEntryId);
                            }
                        }
                    }
                    break;

                case "KNOWLEDGE_SYNC_REQUEST":
                    // Respond with all our knowledge entries
                    Task.Run(async () =>
                    {
                        List<KnowledgeEntry> entries;
                        lock (_cacheLock)
                        {
                            entries = _localCache.Values.ToList();
                        }
                        await _networkManager.SendMessageAsync(e.SenderNodeId, "KNOWLEDGE_SYNC_RESPONSE", entries);
                    });
                    break;

                case "KNOWLEDGE_SYNC_RESPONSE":
                    if (e.Data is List<KnowledgeEntry> syncEntries)
                    {
                        lock (_cacheLock)
                        {
                            foreach (var entry in syncEntries)
                            {
                                if (!_localCache.TryGetValue(entry.EntryId, out var existingEntry) ||
                                    entry.Version > existingEntry.Version)
                                {
                                    _localCache[entry.EntryId] = entry;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private async Task StoreEntryInLinksAsync(KnowledgeEntry entry)
        {
            // Store knowledge entry as links
            // This is a simplified implementation - in production, you'd use Platform.Data.Doublets.Sequences
            await Task.CompletedTask;
        }

        private static string ComputeHash(string content)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
            return Convert.ToBase64String(hashBytes);
        }
    }
}
