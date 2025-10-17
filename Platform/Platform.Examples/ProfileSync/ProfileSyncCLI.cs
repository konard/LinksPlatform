using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Platform.Examples.ProfileSync
{
    /// <summary>
    /// Command-line interface for Profile Sync system
    /// </summary>
    public class ProfileSyncCLI : ICommandLineInterface
    {
        private readonly ProfileSyncManager _manager;
        private readonly CancellationTokenSource _cancellationSource;

        public ProfileSyncCLI(string storagePath)
        {
            var storage = new JsonAccountStorage(storagePath ?? "./profile-sync-data");
            _manager = new ProfileSyncManager(storage);
            _cancellationSource = new CancellationTokenSource();
        }

        public void Run(params string[] args)
        {
            RunAsync(args).Wait();
        }

        private async Task RunAsync(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            var command = args[0].ToLower();

            try
            {
                switch (command)
                {
                    case "list":
                        await ListAccountsAsync();
                        break;

                    case "backup":
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Usage: backup <platform> <accountId>");
                            return;
                        }
                        await BackupAccountAsync(args[1], args[2]);
                        break;

                    case "backup-all":
                        await BackupAllAccountsAsync();
                        break;

                    case "restore":
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Usage: restore <platform> <accountId>");
                            return;
                        }
                        await RestoreAccountAsync(args[1], args[2]);
                        break;

                    case "sync":
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Usage: sync <platform> <accountId>");
                            return;
                        }
                        await SyncAccountAsync(args[1], args[2]);
                        break;

                    case "sync-all":
                        await SyncAllAccountsAsync();
                        break;

                    case "migrate":
                        if (args.Length < 5)
                        {
                            Console.WriteLine("Usage: migrate <sourcePlatform> <sourceAccountId> <targetPlatform> <targetAccountId>");
                            return;
                        }
                        await MigrateAccountAsync(args[1], args[2], args[3], args[4]);
                        break;

                    case "export":
                        if (args.Length < 4)
                        {
                            Console.WriteLine("Usage: export <platform> <accountId> <exportPath>");
                            return;
                        }
                        await ExportAccountAsync(args[1], args[2], args[3]);
                        break;

                    case "import":
                        if (args.Length < 2)
                        {
                            Console.WriteLine("Usage: import <importPath>");
                            return;
                        }
                        await ImportAccountAsync(args[1]);
                        break;

                    case "help":
                        ShowHelp();
                        break;

                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        ShowHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private async Task ListAccountsAsync()
        {
            Console.WriteLine("Listing all accounts...");
            var accounts = await _manager.ListAccountsAsync(_cancellationSource.Token);

            if (accounts.Count == 0)
            {
                Console.WriteLine("No accounts found.");
                return;
            }

            Console.WriteLine($"\nFound {accounts.Count} account(s):");
            foreach (var (platform, accountId) in accounts)
            {
                Console.WriteLine($"  - {platform}/{accountId}");
            }
        }

        private async Task BackupAccountAsync(string platform, string accountId)
        {
            Console.WriteLine($"Backing up {platform}/{accountId}...");
            await _manager.BackupAccountAsync(platform, accountId, _cancellationSource.Token);
            Console.WriteLine("Backup completed successfully.");
        }

        private async Task BackupAllAccountsAsync()
        {
            Console.WriteLine("Backing up all accounts...");
            await _manager.BackupAllAccountsAsync(_cancellationSource.Token);
            Console.WriteLine("All accounts backed up successfully.");
        }

        private async Task RestoreAccountAsync(string platform, string accountId)
        {
            Console.WriteLine($"Restoring {platform}/{accountId}...");
            await _manager.RestoreAccountAsync(platform, accountId, _cancellationSource.Token);
            Console.WriteLine("Restore completed successfully.");
        }

        private async Task SyncAccountAsync(string platform, string accountId)
        {
            Console.WriteLine($"Syncing {platform}/{accountId}...");
            await _manager.SyncAccountAsync(platform, accountId, _cancellationSource.Token);
            Console.WriteLine("Sync completed successfully.");
        }

        private async Task SyncAllAccountsAsync()
        {
            Console.WriteLine("Syncing all accounts...");
            await _manager.SyncAllAccountsAsync(_cancellationSource.Token);
            Console.WriteLine("All accounts synced successfully.");
        }

        private async Task MigrateAccountAsync(string sourcePlatform, string sourceAccountId, string targetPlatform, string targetAccountId)
        {
            Console.WriteLine($"Migrating from {sourcePlatform}/{sourceAccountId} to {targetPlatform}/{targetAccountId}...");
            await _manager.MigrateAccountAsync(sourcePlatform, sourceAccountId, targetPlatform, targetAccountId, _cancellationSource.Token);
            Console.WriteLine("Migration completed successfully.");
        }

        private async Task ExportAccountAsync(string platform, string accountId, string exportPath)
        {
            Console.WriteLine($"Exporting {platform}/{accountId} to {exportPath}...");
            await _manager.ExportAccountAsync(platform, accountId, exportPath, _cancellationSource.Token);
            Console.WriteLine("Export completed successfully.");
        }

        private async Task ImportAccountAsync(string importPath)
        {
            Console.WriteLine($"Importing from {importPath}...");
            var data = await _manager.ImportAccountAsync(importPath, _cancellationSource.Token);
            Console.WriteLine($"Import completed successfully: {data.Platform}/{data.AccountId}");
        }

        private void ShowHelp()
        {
            Console.WriteLine("Profile Sync CLI - Manage your data across all platforms");
            Console.WriteLine("\nUsage: ProfileSync <command> [arguments]");
            Console.WriteLine("\nCommands:");
            Console.WriteLine("  list                                             - List all stored accounts");
            Console.WriteLine("  backup <platform> <accountId>                    - Backup specific account");
            Console.WriteLine("  backup-all                                       - Backup all registered accounts");
            Console.WriteLine("  restore <platform> <accountId>                   - Restore account from backup");
            Console.WriteLine("  sync <platform> <accountId>                      - Sync account (bidirectional)");
            Console.WriteLine("  sync-all                                         - Sync all registered accounts");
            Console.WriteLine("  migrate <srcPlatform> <srcId> <tgtPlatform> <tgtId> - Migrate account data");
            Console.WriteLine("  export <platform> <accountId> <exportPath>       - Export account to file");
            Console.WriteLine("  import <importPath>                              - Import account from file");
            Console.WriteLine("  help                                             - Show this help message");
            Console.WriteLine("\nExamples:");
            Console.WriteLine("  ProfileSync backup WhatsApp user123");
            Console.WriteLine("  ProfileSync migrate WhatsApp user123 Telegram user456");
            Console.WriteLine("  ProfileSync export Twitter myaccount ./backup.json");
        }
    }
}
