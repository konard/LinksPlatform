using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Platform.Data.EntityFramework.Infrastructure;
using Platform.Data.EntityFramework.Storage;

namespace Platform.Data.EntityFramework.Extensions
{
    /// <summary>
    /// Extension methods for setting up Links database services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class LinksServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the services required by the Links database provider for Entity Framework.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddEntityFrameworkLinks(this IServiceCollection serviceCollection)
        {
            var builder = new EntityFrameworkServicesBuilder(serviceCollection)
                .TryAdd<IDatabaseProvider, DatabaseProvider<LinksOptionsExtension>>()
                .TryAdd<IDatabase, LinksDatabase>();

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
