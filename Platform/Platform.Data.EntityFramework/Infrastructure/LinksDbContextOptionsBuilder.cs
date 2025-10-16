using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Platform.Data.EntityFramework.Infrastructure
{
    /// <summary>
    /// Provides a simple API surface for configuring <see cref="LinksOptionsExtension"/>.
    /// </summary>
    public class LinksDbContextOptionsBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinksDbContextOptionsBuilder"/> class.
        /// </summary>
        /// <param name="optionsBuilder">The core options builder.</param>
        public LinksDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
        {
            OptionsBuilder = optionsBuilder;
        }

        /// <summary>
        /// Gets the core options builder.
        /// </summary>
        protected virtual DbContextOptionsBuilder OptionsBuilder { get; }
    }
}
