using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Platform.Examples.ProfileSync
{
    /// <summary>
    /// JSON-based implementation of account storage
    /// </summary>
    public class JsonAccountStorage : IAccountStorage
    {
        private readonly string _basePath;
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };

        public JsonAccountStorage(string basePath)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            Directory.CreateDirectory(_basePath);
        }

        public Task SaveAsync(AccountData data, CancellationToken cancellationToken = default)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var accountPath = GetAccountPath(data.Platform, data.AccountId);
            Directory.CreateDirectory(accountPath);

            var dataPath = Path.Combine(accountPath, "data.json");
            var json = JsonConvert.SerializeObject(data, JsonSettings);
            File.WriteAllText(dataPath, json);
            return Task.CompletedTask;
        }

        public Task<AccountData> LoadAsync(string platform, string accountId, CancellationToken cancellationToken = default)
        {
            var accountPath = GetAccountPath(platform, accountId);
            var dataPath = Path.Combine(accountPath, "data.json");

            if (!File.Exists(dataPath))
                return Task.FromResult<AccountData>(null);

            var json = File.ReadAllText(dataPath);
            return Task.FromResult(JsonConvert.DeserializeObject<AccountData>(json));
        }

        public Task DeleteAsync(string platform, string accountId, CancellationToken cancellationToken = default)
        {
            var accountPath = GetAccountPath(platform, accountId);

            if (Directory.Exists(accountPath))
            {
                Directory.Delete(accountPath, recursive: true);
            }

            return Task.CompletedTask;
        }

        public Task<List<(string Platform, string AccountId)>> ListAccountsAsync(CancellationToken cancellationToken = default)
        {
            var accounts = new List<(string Platform, string AccountId)>();

            if (!Directory.Exists(_basePath))
                return Task.FromResult(accounts);

            foreach (var platformDir in Directory.GetDirectories(_basePath))
            {
                var platform = Path.GetFileName(platformDir);
                foreach (var accountDir in Directory.GetDirectories(platformDir))
                {
                    var accountId = Path.GetFileName(accountDir);
                    accounts.Add((platform, accountId));
                }
            }

            return Task.FromResult(accounts);
        }

        public async Task ExportAsync(string platform, string accountId, string exportPath, CancellationToken cancellationToken = default)
        {
            var data = await LoadAsync(platform, accountId, cancellationToken);

            if (data == null)
                throw new InvalidOperationException($"No data found for {platform}/{accountId}");

            var json = JsonConvert.SerializeObject(data, JsonSettings);
            File.WriteAllText(exportPath, json);
        }

        public Task<AccountData> ImportAsync(string importPath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(importPath))
                throw new FileNotFoundException($"Import file not found: {importPath}");

            var json = File.ReadAllText(importPath);
            return Task.FromResult(JsonConvert.DeserializeObject<AccountData>(json));
        }

        private string GetAccountPath(string platform, string accountId)
        {
            // Sanitize platform and accountId to create safe directory names
            var safePlatform = SanitizePathComponent(platform);
            var safeAccountId = SanitizePathComponent(accountId);
            return Path.Combine(_basePath, safePlatform, safeAccountId);
        }

        private static string SanitizePathComponent(string component)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", component.Split(invalid));
        }
    }
}
