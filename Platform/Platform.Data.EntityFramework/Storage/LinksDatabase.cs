using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace Platform.Data.EntityFramework.Storage
{
    /// <summary>
    /// Represents a Links database instance.
    /// </summary>
    public class LinksDatabase : Database
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinksDatabase"/> class.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
        public LinksDatabase(DatabaseDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        /// Saves changes to the database.
        /// </summary>
        /// <param name="entries">The entries to save.</param>
        /// <returns>The number of state entries written to the database.</returns>
        public override int SaveChanges(IList<IUpdateEntry> entries)
        {
            return entries.Count;
        }

        /// <summary>
        /// Asynchronously saves changes to the database.
        /// </summary>
        /// <param name="entries">The entries to save.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous save operation. The task result contains the
        /// number of state entries written to the database.
        /// </returns>
        public override Task<int> SaveChangesAsync(
            IList<IUpdateEntry> entries,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SaveChanges(entries));
        }
    }
}
