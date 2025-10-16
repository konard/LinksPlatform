using System;

namespace Platform.Data.Triggers
{
    /// <summary>
    /// Provides pattern matching functionality for link triplets.
    /// Supports wildcards and comparison predicates.
    /// </summary>
    public class PatternMatcher
    {
        /// <summary>
        /// Represents a wildcard that matches any link.
        /// </summary>
        public static readonly object Wildcard = new object();

        /// <summary>
        /// Determines whether a link triplet matches the given pattern.
        /// </summary>
        /// <param name="source">The source link to match.</param>
        /// <param name="linker">The linker link to match.</param>
        /// <param name="target">The target link to match.</param>
        /// <param name="patternSource">The pattern source (can be Wildcard).</param>
        /// <param name="patternLinker">The pattern linker (can be Wildcard).</param>
        /// <param name="patternTarget">The pattern target (can be Wildcard).</param>
        /// <returns>True if the triplet matches the pattern; otherwise, false.</returns>
        public static bool Matches(object source, object linker, object target,
                                   object patternSource, object patternLinker, object patternTarget)
        {
            return MatchesSingle(source, patternSource) &&
                   MatchesSingle(linker, patternLinker) &&
                   MatchesSingle(target, patternTarget);
        }

        private static bool MatchesSingle(object value, object pattern)
        {
            if (ReferenceEquals(pattern, Wildcard) || pattern == null)
                return true;

            if (pattern is Predicate<object> predicate)
                return predicate(value);

            return Equals(value, pattern);
        }
    }
}
