using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Platform.Examples.ProfileSync;

namespace Platform.Examples
{
    /// <summary>
    /// Example demonstrating how to use the Profile Sync system
    /// </summary>
    public class ProfileSyncExample
    {
        /// <summary>
        /// Mock provider for demonstration purposes
        /// </summary>
        private class MockAccountProvider : BaseAccountProvider
        {
            public override string PlatformName { get; }
            public override string AccountId { get; }

            public MockAccountProvider(string platformName, string accountId)
            {
                PlatformName = platformName;
                AccountId = accountId;
            }

            public override Task<AccountData> DownloadDataAsync(System.Threading.CancellationToken cancellationToken = default)
            {
                // Simulate downloading data from platform
                var data = new AccountData
                {
                    Platform = PlatformName,
                    AccountId = AccountId,
                    Profile = new ProfileInfo
                    {
                        Username = AccountId,
                        DisplayName = $"User {AccountId}",
                        Bio = "Example user profile",
                        Email = $"{AccountId}@example.com"
                    },
                    Messages = new List<Message>
                    {
                        new Message
                        {
                            Id = "msg1",
                            SenderId = AccountId,
                            ReceiverId = "friend1",
                            Content = "Hello, world!",
                            Timestamp = DateTime.UtcNow.AddDays(-1),
                            Type = MessageType.Text
                        }
                    },
                    Contacts = new List<Contact>
                    {
                        new Contact
                        {
                            Id = "contact1",
                            Name = "Friend 1",
                            Username = "friend1"
                        }
                    },
                    LastSyncTime = DateTime.UtcNow
                };

                return Task.FromResult(data);
            }

            public override Task UploadDataAsync(AccountData data, System.Threading.CancellationToken cancellationToken = default)
            {
                // Simulate uploading data to platform
                Console.WriteLine($"Uploading {data.Messages.Count} messages to {PlatformName}...");
                return Task.CompletedTask;
            }
        }

        public static async Task RunExampleAsync()
        {
            Console.WriteLine("=== Profile Sync System Example ===\n");

            // Initialize storage and manager
            var storage = new JsonAccountStorage("./example-sync-data");
            var manager = new ProfileSyncManager(storage);

            // Register mock providers
            var whatsappProvider = new MockAccountProvider("WhatsApp", "user123");
            var telegramProvider = new MockAccountProvider("Telegram", "user456");

            manager.RegisterProvider(whatsappProvider);
            manager.RegisterProvider(telegramProvider);

            Console.WriteLine("1. Backing up WhatsApp account...");
            await manager.BackupAccountAsync("WhatsApp", "user123");
            Console.WriteLine("   ✓ Backup completed\n");

            Console.WriteLine("2. Backing up Telegram account...");
            await manager.BackupAccountAsync("Telegram", "user456");
            Console.WriteLine("   ✓ Backup completed\n");

            Console.WriteLine("3. Listing all stored accounts...");
            var accounts = await manager.ListAccountsAsync();
            foreach (var (platform, accountId) in accounts)
            {
                Console.WriteLine($"   - {platform}/{accountId}");
            }
            Console.WriteLine();

            Console.WriteLine("4. Syncing WhatsApp account...");
            await manager.SyncAccountAsync("WhatsApp", "user123");
            Console.WriteLine("   ✓ Sync completed\n");

            Console.WriteLine("5. Migrating from WhatsApp to Telegram...");
            await manager.MigrateAccountAsync("WhatsApp", "user123", "Telegram", "user456");
            Console.WriteLine("   ✓ Migration completed\n");

            Console.WriteLine("6. Exporting WhatsApp account...");
            await manager.ExportAccountAsync("WhatsApp", "user123", "./whatsapp-export.json");
            Console.WriteLine("   ✓ Export completed\n");

            Console.WriteLine("7. Importing account from file...");
            var importedData = await manager.ImportAccountAsync("./whatsapp-export.json");
            Console.WriteLine($"   ✓ Import completed: {importedData.Platform}/{importedData.AccountId}\n");

            Console.WriteLine("=== Example completed successfully ===");
        }

        public static void Main(string[] args)
        {
            RunExampleAsync().Wait();
        }
    }
}
