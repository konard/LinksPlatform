using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Platform.Examples.ProfileSync
{
    /// <summary>
    /// Base implementation for account providers with common functionality
    /// </summary>
    public abstract class BaseAccountProvider : IAccountProvider
    {
        public abstract string PlatformName { get; }
        public abstract string AccountId { get; }

        public abstract Task<AccountData> DownloadDataAsync(CancellationToken cancellationToken = default);
        public abstract Task UploadDataAsync(AccountData data, CancellationToken cancellationToken = default);

        public virtual async Task<AccountData> SyncDataAsync(AccountData localData, CancellationToken cancellationToken = default)
        {
            // Default implementation: download remote data and merge with local
            var remoteData = await DownloadDataAsync(cancellationToken);

            if (localData == null)
                return remoteData;

            // Merge logic: keep most recent data
            var mergedData = MergeAccountData(localData, remoteData);

            // Upload merged data back to platform
            await UploadDataAsync(mergedData, cancellationToken);

            return mergedData;
        }

        /// <summary>
        /// Merges local and remote account data
        /// </summary>
        /// <param name="local">Local account data</param>
        /// <param name="remote">Remote account data</param>
        /// <returns>Merged account data</returns>
        protected virtual AccountData MergeAccountData(AccountData local, AccountData remote)
        {
            var merged = new AccountData
            {
                Platform = PlatformName,
                AccountId = AccountId,
                LastSyncTime = DateTime.UtcNow
            };

            // Merge profile: use most recent
            merged.Profile = local.LastSyncTime > remote.LastSyncTime ? local.Profile : remote.Profile;

            // Merge messages: combine and deduplicate
            merged.Messages = MergeMessages(local.Messages, remote.Messages);

            // Merge contacts: combine and deduplicate
            merged.Contacts = MergeContacts(local.Contacts, remote.Contacts);

            // Merge media: combine and deduplicate
            merged.Media = MergeMedia(local.Media, remote.Media);

            // Merge metadata
            merged.Metadata = new Dictionary<string, object>(local.Metadata);
            foreach (var kvp in remote.Metadata)
            {
                merged.Metadata[kvp.Key] = kvp.Value;
            }

            return merged;
        }

        protected virtual List<Message> MergeMessages(List<Message> local, List<Message> remote)
        {
            var merged = new List<Message>(local);
            var existingIds = new HashSet<string>(local.Select(m => m.Id));

            foreach (var message in remote)
            {
                if (!existingIds.Contains(message.Id))
                {
                    merged.Add(message);
                }
            }

            return merged.OrderBy(m => m.Timestamp).ToList();
        }

        protected virtual List<Contact> MergeContacts(List<Contact> local, List<Contact> remote)
        {
            var merged = new Dictionary<string, Contact>();

            foreach (var contact in local)
            {
                merged[contact.Id] = contact;
            }

            foreach (var contact in remote)
            {
                if (!merged.ContainsKey(contact.Id))
                {
                    merged[contact.Id] = contact;
                }
            }

            return merged.Values.ToList();
        }

        protected virtual List<MediaItem> MergeMedia(List<MediaItem> local, List<MediaItem> remote)
        {
            var merged = new List<MediaItem>(local);
            var existingIds = new HashSet<string>(local.Select(m => m.Id));

            foreach (var media in remote)
            {
                if (!existingIds.Contains(media.Id))
                {
                    merged.Add(media);
                }
            }

            return merged.OrderBy(m => m.Timestamp).ToList();
        }
    }
}
