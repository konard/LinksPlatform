using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Platform.Examples.MethodImplInliningTransformer
{
    /// <summary>
    /// Transformer for adding [MethodImpl(MethodImplOptions.AggressiveInlining)] to C# methods
    /// </summary>
    public class AddMethodImplInliningTransformer
    {
        /// <summary>
        /// Gets the substitution rules for adding MethodImpl attributes
        /// </summary>
        public static IList<(string Pattern, string Replacement)> GetRules()
        {
            return new List<(string, string)>
            {
                // Add using directive if not present
                (@"^(using System\.Runtime\.CompilerServices;)", ""),

                // Pattern for methods with access modifiers (public, private, protected, internal, static, etc.)
                // Captures: access modifiers + return type + method name + parameters
                // Example: public void MethodName() => [MethodImpl...]\n        public void MethodName()
                (
                    @"(\r?\n\s*)((?:public|private|protected|internal|static|virtual|override|abstract|async|sealed|extern|readonly|unsafe|new)\s+)*(?!class|interface|struct|enum)(\S+\s+\S+\s*\([^)]*\)\s*(?:where\s+\S+\s*:\s*\S+(?:\s*,\s*\S+)*\s*)?(?:{|=>))",
                    "$1[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]$1$2$3"
                ),

                // Pattern for property getters and setters
                (
                    @"(\r?\n\s*)(\{\s*(?:get|set)\s*)(;)",
                    "$1[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]$1$2$3"
                ),

                // Remove duplicate attributes that may have been added
                (
                    @"(\[System\.Runtime\.CompilerServices\.MethodImpl\(System\.Runtime\.CompilerServices\.MethodImplOptions\.AggressiveInlining\)\]\s*\r?\n\s*)+(\[System\.Runtime\.CompilerServices\.MethodImpl\(System\.Runtime\.CompilerServices\.MethodImplOptions\.AggressiveInlining\)\])",
                    "$2"
                )
            };
        }

        /// <summary>
        /// Transforms C# code by adding MethodImpl attributes to all methods
        /// </summary>
        public static string Transform(string sourceCode)
        {
            var result = sourceCode;
            var rules = GetRules();

            foreach (var (pattern, replacement) in rules)
            {
                var regex = new Regex(pattern, RegexOptions.Multiline);
                result = regex.Replace(result, replacement);
            }

            // Add using directive at the top if not present and attributes were added
            if (result.Contains("[System.Runtime.CompilerServices.MethodImpl") &&
                !result.Contains("using System.Runtime.CompilerServices;"))
            {
                // Find the last using directive
                var usingMatch = Regex.Match(result, @"^using\s+[^;]+;", RegexOptions.Multiline);
                if (usingMatch.Success)
                {
                    var lastUsing = usingMatch;
                    foreach (Match match in Regex.Matches(result, @"^using\s+[^;]+;", RegexOptions.Multiline))
                    {
                        lastUsing = match;
                    }

                    var insertPosition = lastUsing.Index + lastUsing.Length;
                    result = result.Insert(insertPosition, Environment.NewLine + "using System.Runtime.CompilerServices;");
                }
                else
                {
                    // If no using directives, add at the beginning
                    result = "using System.Runtime.CompilerServices;" + Environment.NewLine + Environment.NewLine + result;
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Transformer for removing [MethodImpl(MethodImplOptions.AggressiveInlining)] from C# methods
    /// </summary>
    public class RemoveMethodImplInliningTransformer
    {
        /// <summary>
        /// Gets the substitution rules for removing MethodImpl attributes
        /// </summary>
        public static IList<(string Pattern, string Replacement)> GetRules()
        {
            return new List<(string, string)>
            {
                // Remove [MethodImpl(MethodImplOptions.AggressiveInlining)] attribute
                (
                    @"\s*\[System\.Runtime\.CompilerServices\.MethodImpl\(System\.Runtime\.CompilerServices\.MethodImplOptions\.AggressiveInlining\)\]\s*\r?\n",
                    ""
                ),

                // Remove [MethodImpl(MethodImplOptions.AggressiveInlining)] with shorter syntax
                (
                    @"\s*\[MethodImpl\(MethodImplOptions\.AggressiveInlining\)\]\s*\r?\n",
                    ""
                ),

                // Remove using System.Runtime.CompilerServices if it's no longer needed
                // (This is commented out to be safe - we don't want to remove it if it's used for other things)
                // (@"^\s*using System\.Runtime\.CompilerServices;\s*\r?\n", "")
            };
        }

        /// <summary>
        /// Transforms C# code by removing MethodImpl attributes from all methods
        /// </summary>
        public static string Transform(string sourceCode)
        {
            var result = sourceCode;
            var rules = GetRules();

            foreach (var (pattern, replacement) in rules)
            {
                var regex = new Regex(pattern, RegexOptions.Multiline);
                result = regex.Replace(result, replacement);
            }

            return result;
        }
    }
}
