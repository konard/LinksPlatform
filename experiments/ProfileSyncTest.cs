using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Platform.Examples.ProfileSync;

namespace Platform.Experiments
{
    /// <summary>
    /// Test/experiment script for Profile Sync functionality
    /// </summary>
    public class ProfileSyncTest
    {
        /// <summary>
        /// Mock provider for testing
        /// </summary>
        private class TestAccountProvider : BaseAccountProvider
        {
            private readonly AccountData _mockData;

            public override string PlatformName { get; }
            public override string AccountId { get; }

            public TestAccountProvider(string platformName, string accountId, AccountData mockData = null)
            {
                PlatformName = platformName;
                AccountId = accountId;
                _mockData = mockData ?? CreateMockData(platformName, accountId);
            }

            public override Task<AccountData> DownloadDataAsync(System.Threading.CancellationToken cancellationToken = default)
            {
                Console.WriteLine($"[{PlatformName}] Downloading data for account {AccountId}...");
                return Task.FromResult(_mockData);
            }

            public override Task UploadDataAsync(AccountData data, System.Threading.CancellationToken cancellationToken = default)
            {
                Console.WriteLine($"[{PlatformName}] Uploading data for account {AccountId}...");
                Console.WriteLine($"  - Profile: {data.Profile?.DisplayName}");
                Console.WriteLine($"  - Messages: {data.Messages?.Count ?? 0}");
                Console.WriteLine($"  - Contacts: {data.Contacts?.Count ?? 0}");
                Console.WriteLine($"  - Media: {data.Media?.Count ?? 0}");
                return Task.CompletedTask;
            }

            private static AccountData CreateMockData(string platform, string accountId)
            {
                return new AccountData
                {
                    Platform = platform,
                    AccountId = accountId,
                    Profile = new ProfileInfo
                    {
                        Username = accountId,
                        DisplayName = $"Test User ({platform})",
                        Bio = $"Test account on {platform}",
                        Email = $"{accountId}@{platform.ToLower()}.test"
                    },
                    Messages = new List<Message>
                    {
                        new Message
                        {
                            Id = $"{platform}-msg-1",
                            SenderId = accountId,
                            ReceiverId = "test_recipient",
                            Content = $"Test message from {platform}",
                            Timestamp = DateTime.UtcNow.AddHours(-1),
                            Type = MessageType.Text
                        },
                        new Message
                        {
                            Id = $"{platform}-msg-2",
                            SenderId = accountId,
                            ReceiverId = "test_recipient",
                            Content = $"Another test message from {platform}",
                            Timestamp = DateTime.UtcNow.AddMinutes(-30),
                            Type = MessageType.Text
                        }
                    },
                    Contacts = new List<Contact>
                    {
                        new Contact
                        {
                            Id = "contact-1",
                            Name = "Test Contact 1",
                            Username = "testcontact1"
                        },
                        new Contact
                        {
                            Id = "contact-2",
                            Name = "Test Contact 2",
                            Username = "testcontact2"
                        }
                    },
                    Media = new List<MediaItem>
                    {
                        new MediaItem
                        {
                            Id = $"{platform}-media-1",
                            Url = $"https://{platform.ToLower()}.test/media/1.jpg",
                            Type = MediaType.Image,
                            Timestamp = DateTime.UtcNow.AddDays(-1)
                        }
                    },
                    LastSyncTime = DateTime.UtcNow
                };
            }
        }

        public static async Task RunTestsAsync()
        {
            Console.WriteLine("=== Profile Sync System Tests ===\n");

            var testDir = "./test-sync-data";

            // Clean up test directory
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, recursive: true);
            }

            try
            {
                await TestBasicBackupRestore();
                await TestSynchronization();
                await TestMigration();
                await TestExportImport();
                await TestMultipleAccounts();

                Console.WriteLine("\n=== All tests passed successfully! ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Test failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                // Clean up
                if (Directory.Exists(testDir))
                {
                    Directory.Delete(testDir, recursive: true);
                }
            }
        }

        private static async Task TestBasicBackupRestore()
        {
            Console.WriteLine("Test 1: Basic Backup & Restore");
            Console.WriteLine("─────────────────────────────────");

            var storage = new JsonAccountStorage("./test-sync-data");
            var manager = new ProfileSyncManager(storage);

            var provider = new TestAccountProvider("TestPlatform", "testuser1");
            manager.RegisterProvider(provider);

            // Backup
            await manager.BackupAccountAsync("TestPlatform", "testuser1");
            Console.WriteLine("✓ Backup completed");

            // Verify storage
            var loadedData = await storage.LoadAsync("TestPlatform", "testuser1");
            if (loadedData == null)
                throw new Exception("Failed to load backed up data");

            Console.WriteLine("✓ Data verified in storage");

            // Restore
            await manager.RestoreAccountAsync("TestPlatform", "testuser1");
            Console.WriteLine("✓ Restore completed");

            Console.WriteLine("✅ Test 1 passed\n");
        }

        private static async Task TestSynchronization()
        {
            Console.WriteLine("Test 2: Synchronization");
            Console.WriteLine("─────────────────────────────────");

            var storage = new JsonAccountStorage("./test-sync-data");
            var manager = new ProfileSyncManager(storage);

            var provider = new TestAccountProvider("TestPlatform", "testuser2");
            manager.RegisterProvider(provider);

            // Initial sync
            await manager.SyncAccountAsync("TestPlatform", "testuser2");
            Console.WriteLine("✓ Initial sync completed");

            // Second sync (with existing local data)
            await manager.SyncAccountAsync("TestPlatform", "testuser2");
            Console.WriteLine("✓ Second sync completed");

            Console.WriteLine("✅ Test 2 passed\n");
        }

        private static async Task TestMigration()
        {
            Console.WriteLine("Test 3: Account Migration");
            Console.WriteLine("─────────────────────────────────");

            var storage = new JsonAccountStorage("./test-sync-data");
            var manager = new ProfileSyncManager(storage);

            var sourceProvider = new TestAccountProvider("SourcePlatform", "user123");
            var targetProvider = new TestAccountProvider("TargetPlatform", "user456");

            manager.RegisterProvider(sourceProvider);
            manager.RegisterProvider(targetProvider);

            // Backup source
            await manager.BackupAccountAsync("SourcePlatform", "user123");
            Console.WriteLine("✓ Source backed up");

            // Migrate
            await manager.MigrateAccountAsync(
                "SourcePlatform", "user123",
                "TargetPlatform", "user456"
            );
            Console.WriteLine("✓ Migration completed");

            // Verify target
            var targetData = await storage.LoadAsync("TargetPlatform", "user456");
            if (targetData == null)
                throw new Exception("Migration failed - no target data found");

            Console.WriteLine("✓ Target data verified");
            Console.WriteLine("✅ Test 3 passed\n");
        }

        private static async Task TestExportImport()
        {
            Console.WriteLine("Test 4: Export & Import");
            Console.WriteLine("─────────────────────────────────");

            var storage = new JsonAccountStorage("./test-sync-data");
            var manager = new ProfileSyncManager(storage);

            var provider = new TestAccountProvider("TestPlatform", "testuser3");
            manager.RegisterProvider(provider);

            // Backup
            await manager.BackupAccountAsync("TestPlatform", "testuser3");
            Console.WriteLine("✓ Backup completed");

            // Export
            var exportPath = "./test-export.json";
            await manager.ExportAccountAsync("TestPlatform", "testuser3", exportPath);
            Console.WriteLine("✓ Export completed");

            if (!File.Exists(exportPath))
                throw new Exception("Export file not created");

            // Import
            var importedData = await manager.ImportAccountAsync(exportPath);
            Console.WriteLine("✓ Import completed");

            if (importedData.Platform != "TestPlatform" || importedData.AccountId != "testuser3")
                throw new Exception("Imported data mismatch");

            // Clean up
            File.Delete(exportPath);
            Console.WriteLine("✅ Test 4 passed\n");
        }

        private static async Task TestMultipleAccounts()
        {
            Console.WriteLine("Test 5: Multiple Accounts");
            Console.WriteLine("─────────────────────────────────");

            var storage = new JsonAccountStorage("./test-sync-data");
            var manager = new ProfileSyncManager(storage);

            // Register multiple providers
            for (int i = 1; i <= 3; i++)
            {
                var provider = new TestAccountProvider($"Platform{i}", $"user{i}");
                manager.RegisterProvider(provider);
            }

            // Backup all
            await manager.BackupAllAccountsAsync();
            Console.WriteLine("✓ All accounts backed up");

            // List accounts
            var accounts = await manager.ListAccountsAsync();
            Console.WriteLine($"✓ Found {accounts.Count} accounts");

            if (accounts.Count != 3)
                throw new Exception($"Expected 3 accounts, found {accounts.Count}");

            // Sync all
            await manager.SyncAllAccountsAsync();
            Console.WriteLine("✓ All accounts synced");

            Console.WriteLine("✅ Test 5 passed\n");
        }

        public static void Main(string[] args)
        {
            RunTestsAsync().Wait();
        }
    }
}
