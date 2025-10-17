using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Platform.Examples.ProfileSync
{
    /// <summary>
    /// Interface for local storage of account data
    /// </summary>
    public interface IAccountStorage
    {
        /// <summary>
        /// Saves account data to local storage
        /// </summary>
        /// <param name="data">Account data to save</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SaveAsync(AccountData data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads account data from local storage
        /// </summary>
        /// <param name="platform">Platform name</param>
        /// <param name="accountId">Account identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Account data or null if not found</returns>
        Task<AccountData> LoadAsync(string platform, string accountId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes account data from local storage
        /// </summary>
        /// <param name="platform">Platform name</param>
        /// <param name="accountId">Account identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteAsync(string platform, string accountId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists all stored accounts
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of account identifiers</returns>
        Task<List<(string Platform, string AccountId)>> ListAccountsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Exports account data to a file
        /// </summary>
        /// <param name="platform">Platform name</param>
        /// <param name="accountId">Account identifier</param>
        /// <param name="exportPath">Path to export file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ExportAsync(string platform, string accountId, string exportPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Imports account data from a file
        /// </summary>
        /// <param name="importPath">Path to import file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Imported account data</returns>
        Task<AccountData> ImportAsync(string importPath, CancellationToken cancellationToken = default);
    }
}
