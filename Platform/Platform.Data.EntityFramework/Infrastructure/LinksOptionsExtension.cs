using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Platform.Data.EntityFramework.Extensions;

namespace Platform.Data.EntityFramework.Infrastructure
{
    /// <summary>
    /// Represents the options extension for the Links database provider.
    /// </summary>
    public class LinksOptionsExtension : IDbContextOptionsExtension
    {
        private string _databaseName;
        private DbContextOptionsExtensionInfo _info;

        public LinksOptionsExtension()
        {
        }

        protected LinksOptionsExtension(LinksOptionsExtension copyFrom)
        {
            _databaseName = copyFrom._databaseName;
        }

        public virtual DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

        protected virtual LinksOptionsExtension Clone() => new LinksOptionsExtension(this);

        public virtual string DatabaseName => _databaseName;

        public virtual LinksOptionsExtension WithDatabaseName(string databaseName)
        {
            var clone = Clone();
            clone._databaseName = databaseName;
            return clone;
        }

        public virtual void ApplyServices(IServiceCollection services)
        {
            services.AddEntityFrameworkLinks();
        }

        public virtual void Validate(IDbContextOptions options)
        {
            if (string.IsNullOrWhiteSpace(_databaseName))
            {
                throw new InvalidOperationException("Database name is required for Links provider.");
            }
        }

        private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
        {
            public ExtensionInfo(IDbContextOptionsExtension extension)
                : base(extension)
            {
            }

            private new LinksOptionsExtension Extension
                => (LinksOptionsExtension)base.Extension;

            public override bool IsDatabaseProvider => true;

            public override string LogFragment
                => $"LinksDatabase={Extension._databaseName}";

            public override int GetServiceProviderHashCode()
                => Extension._databaseName?.GetHashCode() ?? 0;

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
                debugInfo["Links:DatabaseName"] = (Extension._databaseName ?? "").GetHashCode().ToString();
            }

            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
                => other is ExtensionInfo otherInfo
                   && Extension._databaseName == otherInfo.Extension._databaseName;
        }
    }
}
