using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using Platform.Data.Doublets;

namespace Platform.Sandbox
{
    /// <include file='../Documentation/API/QueryExecutorExtensions.xml' path='docs/members[@name="QueryExecutorExtensions"]/QueryExecutorExtensions/*'/>
    public static class QueryExecutorExtensions
    {
        /// <include file='../Documentation/API/QueryExecutorExtensions.xml' path='docs/members[@name="QueryExecutorExtensions"]/Execute/*'/>
        public static IEnumerable<T> Execute<T>(this SynchronizedLinks<ulong> links,
            Expression<Func<SynchronizedLinks<ulong>, IEnumerable<T>>> query)
        {
            var queryId = query.ToString();

            var compiledQuery = CompiledQueriesCache<T>.CompiledQueries.GetOrAdd(queryId, key => query.Compile());

            return compiledQuery(links);
        }

        private static class CompiledQueriesCache<T>
        {
            public static readonly ConcurrentDictionary<string, Func<SynchronizedLinks<ulong>, IEnumerable<T>>>
                CompiledQueries =
                    new ConcurrentDictionary<string, Func<SynchronizedLinks<ulong>, IEnumerable<T>>>();
        }
    }
}