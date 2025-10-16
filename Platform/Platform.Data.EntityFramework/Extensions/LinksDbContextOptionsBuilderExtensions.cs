using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Platform.Data.EntityFramework.Infrastructure;

namespace Platform.Data.EntityFramework.Extensions
{
    /// <summary>
    /// Extension methods for configuring a DbContext to use Links as the database provider.
    /// </summary>
    public static class LinksDbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// Configures the context to connect to a Links database.
        /// </summary>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="databaseName">The name of the Links database.</param>
        /// <param name="linksOptionsAction">An optional action to allow additional Links-specific configuration.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder UseLinks(
            this DbContextOptionsBuilder optionsBuilder,
            string databaseName,
            Action<LinksDbContextOptionsBuilder> linksOptionsAction = null)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new ArgumentException("Database name cannot be null or empty.", nameof(databaseName));
            }

            var extension = GetOrCreateExtension(optionsBuilder);
            extension = extension.WithDatabaseName(databaseName);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            linksOptionsAction?.Invoke(new LinksDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        /// <summary>
        /// Configures the context to connect to a Links database.
        /// </summary>
        /// <typeparam name="TContext">The type of context being configured.</typeparam>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="databaseName">The name of the Links database.</param>
        /// <param name="linksOptionsAction">An optional action to allow additional Links-specific configuration.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder<TContext> UseLinks<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            string databaseName,
            Action<LinksDbContextOptionsBuilder> linksOptionsAction = null)
            where TContext : DbContext
        {
            return (DbContextOptionsBuilder<TContext>)UseLinks(
                (DbContextOptionsBuilder)optionsBuilder, databaseName, linksOptionsAction);
        }

        private static LinksOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.Options.FindExtension<LinksOptionsExtension>()
                ?? new LinksOptionsExtension();
    }
}
