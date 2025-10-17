# Profile Sync System

A comprehensive solution for backing up, syncing, and migrating account data across different platforms (social networks, messengers, forums, etc.).

## Overview

This system allows users to:
- **Own their data**: Keep a local copy of everything they put on the internet
- **Backup accounts**: Protect against bans, service crashes, or accidental deletion
- **Sync data**: Keep data synchronized across multiple platforms
- **Migrate accounts**: Easily move data from one platform to another
- **Export/Import**: Transfer data between devices or create portable backups

## Architecture

### Core Components

1. **IAccountProvider** - Interface for platform-specific data access
   - Download data from platforms
   - Upload data to platforms
   - Bidirectional synchronization

2. **IAccountStorage** - Interface for local data storage
   - Save/load account data
   - Export/import functionality
   - Account management

3. **ProfileSyncManager** - Main orchestrator
   - Manages providers and storage
   - Coordinates backup/sync/migration operations
   - Handles multiple accounts simultaneously

4. **AccountData** - Universal data model
   - Profile information
   - Messages/posts
   - Contacts/friends
   - Media files
   - Extensible metadata

### Data Model

```
AccountData
├── Profile (username, email, bio, etc.)
├── Messages (texts, posts, comments)
├── Contacts (friends, followers)
├── Media (photos, videos, documents)
└── Metadata (platform-specific data)
```

## Usage Examples

### Basic Backup

```csharp
var storage = new JsonAccountStorage("./my-data");
var manager = new ProfileSyncManager(storage);

// Register provider
var provider = new WhatsAppProvider("user123");
manager.RegisterProvider(provider);

// Backup account
await manager.BackupAccountAsync("WhatsApp", "user123");
```

### Syncing Data

```csharp
// Bidirectional sync
await manager.SyncAccountAsync("WhatsApp", "user123");

// Sync all registered accounts
await manager.SyncAllAccountsAsync();
```

### Migration Between Platforms

```csharp
// Migrate from WhatsApp to Telegram
await manager.MigrateAccountAsync(
    "WhatsApp", "user123",
    "Telegram", "user456"
);
```

### Export/Import

```csharp
// Export to file
await manager.ExportAccountAsync(
    "WhatsApp", "user123",
    "./backup.json"
);

// Import from file
var data = await manager.ImportAccountAsync("./backup.json");
```

## Command-Line Interface

```bash
# List all accounts
ProfileSync list

# Backup specific account
ProfileSync backup WhatsApp user123

# Backup all accounts
ProfileSync backup-all

# Sync account
ProfileSync sync WhatsApp user123

# Migrate account
ProfileSync migrate WhatsApp user123 Telegram user456

# Export account
ProfileSync export WhatsApp user123 ./backup.json

# Import account
ProfileSync import ./backup.json
```

## Implementing Platform Providers

To add support for a new platform, implement `IAccountProvider`:

```csharp
public class MyPlatformProvider : BaseAccountProvider
{
    public override string PlatformName => "MyPlatform";
    public override string AccountId { get; }

    public override async Task<AccountData> DownloadDataAsync(
        CancellationToken cancellationToken = default)
    {
        // Implement platform-specific download logic
        // Using platform's API or export functionality
    }

    public override async Task UploadDataAsync(
        AccountData data,
        CancellationToken cancellationToken = default)
    {
        // Implement platform-specific upload logic
    }
}
```

## Use Cases

### 1. WhatsApp Migration (iOS to Android)
```csharp
var whatsappProvider = new WhatsAppProvider("user123");
manager.RegisterProvider(whatsappProvider);

// Backup on iOS
await manager.BackupAccountAsync("WhatsApp", "user123");

// Transfer backup file to Android device

// Restore on Android
await manager.RestoreAccountAsync("WhatsApp", "user123");
```

### 2. Unified Messenger Interface
```csharp
// Sync all messengers
var whatsapp = new WhatsAppProvider("user1");
var telegram = new TelegramProvider("user2");
var signal = new SignalProvider("user3");

manager.RegisterProvider(whatsapp);
manager.RegisterProvider(telegram);
manager.RegisterProvider(signal);

// Download all messages to unified local storage
await manager.BackupAllAccountsAsync();

// Send message through specific platform
// (requires extension of the provider interface)
```

### 3. HR Profile Synchronization
```csharp
// Sync professional profile across job sites
var linkedin = new LinkedInProvider("user@email.com");
var indeed = new IndeedProvider("user@email.com");

manager.RegisterProvider(linkedin);
manager.RegisterProvider(indeed);

// Update LinkedIn profile
await manager.SyncAccountAsync("LinkedIn", "user@email.com");

// Mirror to Indeed
await manager.MigrateAccountAsync(
    "LinkedIn", "user@email.com",
    "Indeed", "user@email.com"
);
```

### 4. Disaster Recovery
```csharp
// Regular automated backups
var timer = new Timer(async _ => {
    await manager.BackupAllAccountsAsync();
}, null, TimeSpan.Zero, TimeSpan.FromHours(24));

// In case of account ban or data loss
await manager.RestoreAccountAsync("Twitter", "myaccount");
```

## Storage Implementations

### JSON Storage (Default)
```csharp
var storage = new JsonAccountStorage("./data");
```

### Links Platform Storage (Future)
```csharp
// Using associative data model
var links = new SynchronizedLinks<ulong>(...);
var storage = new LinksAccountStorage(links);
```

## Security Considerations

- **Encryption**: Sensitive data should be encrypted at rest
- **Authentication**: Provider implementations must handle secure authentication
- **Privacy**: Local storage gives users full control over their data
- **Compliance**: Respect platform terms of service and data protection laws

## Future Enhancements

1. **Real-time Sync**: WebSocket-based live synchronization
2. **Conflict Resolution**: Advanced merge strategies for conflicting data
3. **Incremental Backup**: Only sync changed data
4. **Compression**: Reduce storage footprint
5. **Cloud Storage**: Optional cloud backup integration
6. **End-to-End Encryption**: Secure data at rest and in transit
7. **Provider Marketplace**: Community-contributed platform providers

## Benefits

- **Data Ownership**: Users control their own data
- **Platform Independence**: Not locked into any single platform
- **Data Portability**: Easy migration between services
- **Backup Protection**: Guard against data loss
- **Privacy**: Local storage, no third-party access
- **Flexibility**: Extensible architecture for any platform

## Contributing

To add support for a new platform:

1. Implement `IAccountProvider` interface
2. Handle platform-specific API authentication
3. Map platform data to `AccountData` model
4. Add tests for the provider
5. Document platform-specific requirements

## License

This implementation is part of the Links Platform project and follows the same license terms.
