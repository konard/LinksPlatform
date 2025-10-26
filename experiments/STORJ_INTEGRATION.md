# StorJ Integration for Distributed Transaction Log Storage

## Overview

This implementation provides distributed transaction log storage for the Links Platform using [StorJ](https://www.storj.io/), a decentralized cloud storage platform. StorJ offers S3-compatible object storage with enhanced security, privacy, and distributed architecture.

## Why StorJ?

1. **Decentralization**: Data is encrypted, split into pieces, and distributed across a global network of storage nodes
2. **Security**: Client-side encryption ensures data privacy
3. **Cost-effective**: Generally more affordable than traditional cloud storage providers
4. **S3-compatible**: Easy integration using standard AWS S3 SDK
5. **High durability**: Data is stored redundantly across multiple nodes and geographic locations

## Architecture

### Components

1. **IDistributedTransactionLog**: Interface defining the contract for distributed transaction log operations
2. **StorjTransactionLog**: Implementation using StorJ as the storage backend
3. **TransactionLogEntry**: Structured format for transaction data matching the existing local implementation

### Storage Strategy

Transactions are stored in StorJ with the following structure:

```
bucket/
  transaction-log/
    00000000/          # Transactions 0-9999
      00000001.bin
      00000002.bin
      ...
    00000001/          # Transactions 10000-19999
      00010000.bin
      00010001.bin
      ...
```

This hierarchical structure provides:
- Better organization for large transaction volumes
- Efficient listing and retrieval
- Reduced overhead for object storage operations

## Setup

### Prerequisites

1. **StorJ Account**: Sign up at [storj.io](https://www.storj.io/)
2. **Access Credentials**: Create an access grant and S3-compatible credentials
3. **Bucket**: Create a bucket for storing transaction logs

### Installation

Add the AWS S3 SDK NuGet package (used for S3-compatible StorJ access):

```bash
dotnet add package AWSSDK.S3
```

### Configuration

Configure the StorJ connection using environment variables:

```bash
export STORJ_ACCESS_KEY="your-access-key"
export STORJ_SECRET_KEY="your-secret-key"
export STORJ_BUCKET="links-platform-transactions"
```

Or via configuration code:

```csharp
var config = new StorjTransactionLog.StorjConfig
{
    AccessKey = "your-access-key",
    SecretKey = "your-secret-key",
    BucketName = "links-platform-transactions",
    LogPrefix = "transaction-log",
    ServiceUrl = "https://gateway.storjshare.io"
};
```

## Usage

### Basic Usage

```csharp
using Platform.Data.Transactions.Storj;

// Initialize
var config = new StorjTransactionLog.StorjConfig
{
    AccessKey = Environment.GetEnvironmentVariable("STORJ_ACCESS_KEY"),
    SecretKey = Environment.GetEnvironmentVariable("STORJ_SECRET_KEY"),
    BucketName = "links-platform-transactions"
};

using (var transactionLog = new StorjTransactionLog(config))
{
    // Write a transaction
    var transaction = new TransactionLogEntry
    {
        TransactionId = 1,
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        Type = 0, // Creation
        SourceLinkId = 100,
        LinkerLinkId = 200,
        TargetLinkId = 300
    };

    await transactionLog.AppendTransactionAsync(transaction.ToBytes());

    // Read a transaction
    var data = await transactionLog.ReadTransactionAsync(1);
    var readTransaction = TransactionLogEntry.FromBytes(data);

    // Read multiple transactions
    var transactions = await transactionLog.ReadTransactionsAsync(1, 10);
}
```

### Hybrid Local + Distributed Strategy

For optimal performance and reliability, use a hybrid approach:

1. **Write locally first** (using the existing `Platform.Sandbox.Transactions.cs`)
   - Fast writes to memory-mapped file
   - Immediate availability for local operations

2. **Asynchronously replicate to StorJ**
   - Background replication for durability
   - Distributed storage for disaster recovery
   - Cross-node data sharing

```csharp
// Pseudo-code showing hybrid approach
async Task WriteTransaction(TransactionLogEntry transaction)
{
    // 1. Fast local write
    LocalTransactions.Write(transaction);

    // 2. Async StorJ replication (fire and forget with error handling)
    _ = Task.Run(async () =>
    {
        try
        {
            await storjLog.AppendTransactionAsync(transaction.ToBytes());
        }
        catch (Exception ex)
        {
            // Handle replication failure (retry queue, logging, etc.)
            Logger.Error($"StorJ replication failed: {ex.Message}");
        }
    });
}
```

## Features

### Current Implementation

- ✅ Append transactions to distributed log
- ✅ Read individual transactions by ID
- ✅ Read multiple transactions in range
- ✅ Automatic bucket creation
- ✅ Transaction ID tracking
- ✅ Metadata support (timestamp, transaction ID)
- ✅ Hierarchical organization for scalability

### Future Enhancements

- 🔄 Automatic retry with exponential backoff
- 🔄 Transaction log compaction
- 🔄 Cross-node synchronization
- 🔄 Transaction log verification and integrity checks
- 🔄 Snapshot support
- 🔄 Transaction replay functionality
- 🔄 Compression support for reduced storage costs

## Performance Considerations

### Latency

- **Local transactions**: ~microseconds (memory-mapped file)
- **StorJ operations**: ~100-500ms (network-dependent)
  - Upload: 200-500ms average
  - Download: 100-300ms average

### Recommendations

1. **Use hybrid approach**: Write locally first, replicate asynchronously
2. **Batch operations**: Group multiple transactions when possible
3. **Cache frequently accessed transactions** locally
4. **Monitor replication lag**: Track the difference between local and StorJ transaction IDs

## Security

### Encryption

StorJ provides:
- Client-side encryption (data encrypted before leaving your system)
- Zero-knowledge architecture (StorJ cannot access your data)
- Encrypted metadata

### Access Control

- Use separate access grants for read/write operations
- Implement least-privilege access model
- Rotate credentials regularly
- Store credentials securely (environment variables, secret management systems)

## Disaster Recovery

StorJ provides inherent disaster recovery benefits:

1. **Data Redundancy**: Data replicated across multiple storage nodes
2. **Geographic Distribution**: Nodes distributed globally
3. **Node Failure Tolerance**: Data accessible even with node failures
4. **Automatic Repair**: StorJ automatically repairs lost data segments

### Recovery Procedure

If local transaction log is lost:

```csharp
// Restore from StorJ
using (var storjLog = new StorjTransactionLog(config))
{
    long lastTransactionId = GetLastLocalTransactionId();
    long storjLastId = await storjLog.GetLastTransactionIdAsync();

    // Replay missing transactions
    var missingTransactions = await storjLog.ReadTransactionsAsync(
        lastTransactionId + 1,
        storjLastId - lastTransactionId
    );

    foreach (var txData in missingTransactions)
    {
        var tx = TransactionLogEntry.FromBytes(txData);
        LocalTransactions.Replay(tx);
    }
}
```

## Testing

Run the example program:

```bash
cd experiments
dotnet run StorjTransactionLogExample.cs
```

## Troubleshooting

### Connection Issues

**Problem**: Unable to connect to StorJ

**Solutions**:
- Verify credentials are correct
- Check network connectivity
- Ensure ServiceUrl is correct (`https://gateway.storjshare.io`)
- Verify bucket exists and is accessible

### Performance Issues

**Problem**: Slow transaction writes

**Solutions**:
- Implement async replication (don't wait for StorJ writes)
- Use local transaction log for immediate writes
- Batch multiple transactions
- Check network latency to StorJ

### Data Consistency

**Problem**: Mismatch between local and StorJ logs

**Solutions**:
- Implement checksums for verification
- Use transaction log reconciliation
- Set up monitoring for replication lag
- Implement automatic recovery procedures

## Resources

- [StorJ Documentation](https://storj.dev/)
- [StorJ S3 Compatibility](https://storj.dev/dcs/api/s3)
- [StorJ Community Forum](https://forum.storj.io/)
- [AWS S3 SDK for .NET](https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/s3-apis-intro.html)

## Contributing

This implementation is part of the Links Platform side project for distributed transaction storage. Contributions are welcome:

1. Test the implementation with different workloads
2. Add performance benchmarks
3. Implement additional features from the "Future Enhancements" list
4. Improve error handling and resilience
5. Add comprehensive unit tests

## License

This code follows the same license as the Links Platform project.

## Related Issues

- #594: Use StorJ as distributed transaction log storage (this implementation)
- #554: Create examples of Links storage solution as replacement for other database engines

## Contact

For questions or issues with this implementation, please:
1. Open an issue on the GitHub repository
2. Reference issue #594
3. Tag it as "Side Project"
