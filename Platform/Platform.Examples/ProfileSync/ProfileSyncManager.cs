using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Platform.Examples.ProfileSync
{
    /// <summary>
    /// Main manager for syncing and backing up account data across platforms
    /// </summary>
    public class ProfileSyncManager
    {
        private readonly IAccountStorage _storage;
        private readonly Dictionary<string, IAccountProvider> _providers;

        public ProfileSyncManager(IAccountStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _providers = new Dictionary<string, IAccountProvider>();
        }

        /// <summary>
        /// Registers a new account provider
        /// </summary>
        /// <param name="provider">Account provider</param>
        public void RegisterProvider(IAccountProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            var key = GetProviderKey(provider.PlatformName, provider.AccountId);
            _providers[key] = provider;
        }

        /// <summary>
        /// Unregisters an account provider
        /// </summary>
        /// <param name="platform">Platform name</param>
        /// <param name="accountId">Account identifier</param>
        public void UnregisterProvider(string platform, string accountId)
        {
            var key = GetProviderKey(platform, accountId);
            _providers.Remove(key);
        }

        /// <summary>
        /// Downloads and backs up data from a specific account
        /// </summary>
        /// <param name="platform">Platform name</param>
        /// <param name="accountId">Account identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task BackupAccountAsync(string platform, string accountId, CancellationToken cancellationToken = default)
        {
            var provider = GetProvider(platform, accountId);
            var data = await provider.DownloadDataAsync(cancellationToken);
            await _storage.SaveAsync(data, cancellationToken);
        }

        /// <summary>
        /// Backs up all registered accounts
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task BackupAllAccountsAsync(CancellationToken cancellationToken = default)
        {
            var tasks = _providers.Values.Select(async provider =>
            {
                try
                {
                    var data = await provider.DownloadDataAsync(cancellationToken);
                    await _storage.SaveAsync(data, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to backup {provider.PlatformName}/{provider.AccountId}: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Restores account data to a platform
        /// </summary>
        /// <param name="platform">Platform name</param>
        /// <param name="accountId">Account identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task RestoreAccountAsync(string platform, string accountId, CancellationToken cancellationToken = default)
        {
            var provider = GetProvider(platform, accountId);
            var data = await _storage.LoadAsync(platform, accountId, cancellationToken);

            if (data == null)
                throw new InvalidOperationException($"No backup found for {platform}/{accountId}");

            await provider.UploadDataAsync(data, cancellationToken);
        }

        /// <summary>
        /// Synchronizes account data (bidirectional sync)
        /// </summary>
        /// <param name="platform">Platform name</param>
        /// <param name="accountId">Account identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task SyncAccountAsync(string platform, string accountId, CancellationToken cancellationToken = default)
        {
            var provider = GetProvider(platform, accountId);
            var localData = await _storage.LoadAsync(platform, accountId, cancellationToken);

            var syncedData = await provider.SyncDataAsync(localData, cancellationToken);
            await _storage.SaveAsync(syncedData, cancellationToken);
        }

        /// <summary>
        /// Synchronizes all registered accounts
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task SyncAllAccountsAsync(CancellationToken cancellationToken = default)
        {
            var tasks = _providers.Values.Select(async provider =>
            {
                try
                {
                    var localData = await _storage.LoadAsync(provider.PlatformName, provider.AccountId, cancellationToken);
                    var syncedData = await provider.SyncDataAsync(localData, cancellationToken);
                    await _storage.SaveAsync(syncedData, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to sync {provider.PlatformName}/{provider.AccountId}: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Migrates data from one platform to another
        /// </summary>
        /// <param name="sourcePlatform">Source platform name</param>
        /// <param name="sourceAccountId">Source account identifier</param>
        /// <param name="targetPlatform">Target platform name</param>
        /// <param name="targetAccountId">Target account identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task MigrateAccountAsync(
            string sourcePlatform,
            string sourceAccountId,
            string targetPlatform,
            string targetAccountId,
            CancellationToken cancellationToken = default)
        {
            var sourceData = await _storage.LoadAsync(sourcePlatform, sourceAccountId, cancellationToken);

            if (sourceData == null)
            {
                var sourceProvider = GetProvider(sourcePlatform, sourceAccountId);
                sourceData = await sourceProvider.DownloadDataAsync(cancellationToken);
            }

            var targetProvider = GetProvider(targetPlatform, targetAccountId);

            // Adapt data for target platform
            var targetData = new AccountData
            {
                Platform = targetPlatform,
                AccountId = targetAccountId,
                Profile = sourceData.Profile,
                Messages = sourceData.Messages,
                Contacts = sourceData.Contacts,
                Media = sourceData.Media,
                Metadata = sourceData.Metadata,
                LastSyncTime = DateTime.UtcNow
            };

            await targetProvider.UploadDataAsync(targetData, cancellationToken);
            await _storage.SaveAsync(targetData, cancellationToken);
        }

        /// <summary>
        /// Lists all stored accounts
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of account identifiers</returns>
        public async Task<List<(string Platform, string AccountId)>> ListAccountsAsync(CancellationToken cancellationToken = default)
        {
            return await _storage.ListAccountsAsync(cancellationToken);
        }

        /// <summary>
        /// Exports account data to a file
        /// </summary>
        /// <param name="platform">Platform name</param>
        /// <param name="accountId">Account identifier</param>
        /// <param name="exportPath">Path to export file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task ExportAccountAsync(string platform, string accountId, string exportPath, CancellationToken cancellationToken = default)
        {
            await _storage.ExportAsync(platform, accountId, exportPath, cancellationToken);
        }

        /// <summary>
        /// Imports account data from a file
        /// </summary>
        /// <param name="importPath">Path to import file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Imported account data</returns>
        public async Task<AccountData> ImportAccountAsync(string importPath, CancellationToken cancellationToken = default)
        {
            var data = await _storage.ImportAsync(importPath, cancellationToken);
            await _storage.SaveAsync(data, cancellationToken);
            return data;
        }

        private IAccountProvider GetProvider(string platform, string accountId)
        {
            var key = GetProviderKey(platform, accountId);
            if (!_providers.TryGetValue(key, out var provider))
                throw new InvalidOperationException($"No provider registered for {platform}/{accountId}");

            return provider;
        }

        private static string GetProviderKey(string platform, string accountId)
        {
            return $"{platform}:{accountId}";
        }
    }
}
