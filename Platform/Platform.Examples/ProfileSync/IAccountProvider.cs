using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Platform.Examples.ProfileSync
{
    /// <summary>
    /// Interface for account data providers across different platforms
    /// (social networks, forums, messengers, etc.)
    /// </summary>
    public interface IAccountProvider
    {
        /// <summary>
        /// Gets the name of the platform (e.g., "WhatsApp", "Twitter", "Facebook")
        /// </summary>
        string PlatformName { get; }

        /// <summary>
        /// Gets the unique identifier for the account on this platform
        /// </summary>
        string AccountId { get; }

        /// <summary>
        /// Downloads all account data from the platform
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Account data</returns>
        Task<AccountData> DownloadDataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Uploads data to the platform
        /// </summary>
        /// <param name="data">Account data to upload</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task UploadDataAsync(AccountData data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Synchronizes data with the platform (bidirectional)
        /// </summary>
        /// <param name="localData">Local account data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Synchronized account data</returns>
        Task<AccountData> SyncDataAsync(AccountData localData, CancellationToken cancellationToken = default);
    }
}
