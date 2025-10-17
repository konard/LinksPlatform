using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Platform.Examples.MethodImplInliningTransformer
{
    /// <summary>
    /// <para>
    /// Represents a code transformer that adds or removes [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// attributes to/from all methods in C# code.
    /// </para>
    /// <para>
    /// This tool uses regular expressions to transform C# source code by either adding or removing
    /// the MethodImpl attribute with AggressiveInlining option. This can be useful for performance
    /// testing and optimization experiments.
    /// </para>
    /// <para>
    /// The tool is designed to work with Platform.RegularExpressions.Transformer library and follows
    /// the same patterns as other transformers in the LinksPlatform ecosystem.
    /// </para>
    /// </summary>
    public static class MethodImplAggressiveInliningTransformer
    {
        /// <summary>
        /// <para>
        /// Gets substitution rules for adding [MethodImpl(MethodImplOptions.AggressiveInlining)] to methods.
        /// </para>
        /// <para>
        /// These rules will:
        /// 1. Add the MethodImpl attribute before method declarations
        /// 2. Add the MethodImpl attribute before property getters/setters
        /// 3. Handle various method signatures including generic methods, async methods, etc.
        /// 4. Avoid adding duplicate attributes
        /// </para>
        /// </summary>
        public static List<(Regex Pattern, string Replacement)> GetAddRules()
        {
            var rules = new List<(Regex, string)>();

            // Match method declarations and add MethodImpl attribute before them
            // This pattern matches:
            // - Optional access modifiers (public, private, protected, internal)
            // - Optional modifiers (static, virtual, override, abstract, async, sealed, extern, readonly, unsafe, new, partial)
            // - Return type
            // - Method name
            // - Parameters in parentheses
            // - Optional generic constraints (where T : ...)
            // - Method body start ({ or =>)
            // But excludes class/interface/struct/enum declarations
            var methodPattern = new Regex(
                @"(?<indent>\r?\n[ \t]*)(?<modifiers>(?:public|private|protected|internal|static|virtual|override|abstract|async|sealed|extern|readonly|unsafe|new|partial)\s+)*(?!class|interface|struct|enum|namespace|using)(?<signature>\S+\s+\S+\s*\<[^>]+\>\s*\([^)]*\)\s*(?:where\s+[^\r\n{]+)?(?:\{|=>))",
                RegexOptions.Multiline | RegexOptions.Compiled
            );
            rules.Add((methodPattern, "${indent}[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]${indent}${modifiers}${signature}"));

            // Match method declarations without generics
            var simpleMethodPattern = new Regex(
                @"(?<indent>\r?\n[ \t]*)(?<modifiers>(?:public|private|protected|internal|static|virtual|override|abstract|async|sealed|extern|readonly|unsafe|new|partial)\s+)*(?!class|interface|struct|enum|namespace|using)(?<signature>\S+\s+\S+\s*\([^)]*\)\s*(?:where\s+[^\r\n{]+)?(?:\{|=>))",
                RegexOptions.Multiline | RegexOptions.Compiled
            );
            rules.Add((simpleMethodPattern, "${indent}[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]${indent}${modifiers}${signature}"));

            // Match property getters and setters (auto-properties)
            var propertyPattern = new Regex(
                @"(?<indent>\r?\n[ \t]*)(?<getter>\{\s*get;)",
                RegexOptions.Multiline | RegexOptions.Compiled
            );
            rules.Add((propertyPattern, "${indent}[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]${indent}${getter}"));

            // Match standalone get/set in properties with bodies
            var propertyGetSetPattern = new Regex(
                @"(?<indent>\r?\n[ \t]+)(?<accessor>(?:get|set)\s*\{)",
                RegexOptions.Multiline | RegexOptions.Compiled
            );
            rules.Add((propertyGetSetPattern, "${indent}[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]${indent}${accessor}"));

            return rules;
        }

        /// <summary>
        /// <para>
        /// Gets substitution rules for removing [MethodImpl(MethodImplOptions.AggressiveInlining)] from methods.
        /// </para>
        /// <para>
        /// These rules will remove the MethodImpl attribute regardless of how it's written:
        /// - Fully qualified: [System.Runtime.CompilerServices.MethodImpl(...)]
        /// - Short form: [MethodImpl(...)]
        /// </para>
        /// </summary>
        public static List<(Regex Pattern, string Replacement)> GetRemoveRules()
        {
            var rules = new List<(Regex, string)>();

            // Remove fully qualified MethodImpl attribute
            var fullyQualifiedPattern = new Regex(
                @"[ \t]*\[System\.Runtime\.CompilerServices\.MethodImpl\(System\.Runtime\.CompilerServices\.MethodImplOptions\.AggressiveInlining\)\]\s*\r?\n",
                RegexOptions.Multiline | RegexOptions.Compiled
            );
            rules.Add((fullyQualifiedPattern, ""));

            // Remove short form MethodImpl attribute
            var shortFormPattern = new Regex(
                @"[ \t]*\[MethodImpl\(MethodImplOptions\.AggressiveInlining\)\]\s*\r?\n",
                RegexOptions.Multiline | RegexOptions.Compiled
            );
            rules.Add((shortFormPattern, ""));

            return rules;
        }

        /// <summary>
        /// <para>
        /// Adds [MethodImpl(MethodImplOptions.AggressiveInlining)] to all methods in the source code.
        /// </para>
        /// </summary>
        /// <param name="sourceCode">The C# source code to transform</param>
        /// <returns>Transformed source code with MethodImpl attributes added</returns>
        public static string AddMethodImplAttributes(string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return sourceCode;
            }

            var result = sourceCode;
            var rules = GetAddRules();

            // Apply each rule
            foreach (var (pattern, replacement) in rules)
            {
                var previousResult = result;
                result = pattern.Replace(result, replacement);

                // Avoid infinite loops
                if (result == previousResult)
                {
                    continue;
                }

                // Remove duplicates that might have been created
                result = RemoveDuplicateAttributes(result);
            }

            // Add using directive if needed
            result = EnsureUsingDirective(result);

            return result;
        }

        /// <summary>
        /// <para>
        /// Removes [MethodImpl(MethodImplOptions.AggressiveInlining)] from all methods in the source code.
        /// </para>
        /// </summary>
        /// <param name="sourceCode">The C# source code to transform</param>
        /// <returns>Transformed source code with MethodImpl attributes removed</returns>
        public static string RemoveMethodImplAttributes(string sourceCode)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                return sourceCode;
            }

            var result = sourceCode;
            var rules = GetRemoveRules();

            // Apply each rule multiple times to ensure all attributes are removed
            foreach (var (pattern, replacement) in rules)
            {
                // Keep applying until no more matches
                while (pattern.IsMatch(result))
                {
                    result = pattern.Replace(result, replacement);
                }
            }

            return result;
        }

        /// <summary>
        /// <para>
        /// Transforms a C# source file by adding MethodImpl attributes.
        /// </para>
        /// </summary>
        /// <param name="inputFilePath">Path to the input C# file</param>
        /// <param name="outputFilePath">Path to the output file (optional, defaults to input path)</param>
        public static void TransformFileAdd(string inputFilePath, string outputFilePath = null)
        {
            if (!File.Exists(inputFilePath))
            {
                throw new FileNotFoundException($"Input file not found: {inputFilePath}");
            }

            var sourceCode = File.ReadAllText(inputFilePath);
            var transformedCode = AddMethodImplAttributes(sourceCode);

            var targetPath = outputFilePath ?? inputFilePath;
            File.WriteAllText(targetPath, transformedCode);
        }

        /// <summary>
        /// <para>
        /// Transforms a C# source file by removing MethodImpl attributes.
        /// </para>
        /// </summary>
        /// <param name="inputFilePath">Path to the input C# file</param>
        /// <param name="outputFilePath">Path to the output file (optional, defaults to input path)</param>
        public static void TransformFileRemove(string inputFilePath, string outputFilePath = null)
        {
            if (!File.Exists(inputFilePath))
            {
                throw new FileNotFoundException($"Input file not found: {inputFilePath}");
            }

            var sourceCode = File.ReadAllText(inputFilePath);
            var transformedCode = RemoveMethodImplAttributes(sourceCode);

            var targetPath = outputFilePath ?? inputFilePath;
            File.WriteAllText(targetPath, transformedCode);
        }

        /// <summary>
        /// <para>
        /// Ensures that "using System.Runtime.CompilerServices;" is present in the source code.
        /// </para>
        /// </summary>
        private static string EnsureUsingDirective(string sourceCode)
        {
            // Check if attribute is used and using directive is missing
            if (sourceCode.Contains("[System.Runtime.CompilerServices.MethodImpl") &&
                !sourceCode.Contains("using System.Runtime.CompilerServices;"))
            {
                // Find the position after the last using directive
                var usingMatches = Regex.Matches(sourceCode, @"^using\s+[^;]+;", RegexOptions.Multiline);

                if (usingMatches.Count > 0)
                {
                    var lastUsing = usingMatches[usingMatches.Count - 1];
                    var insertPosition = lastUsing.Index + lastUsing.Length;
                    sourceCode = sourceCode.Insert(insertPosition, Environment.NewLine + "using System.Runtime.CompilerServices;");
                }
                else
                {
                    // No using directives found, add at the beginning
                    sourceCode = "using System.Runtime.CompilerServices;" + Environment.NewLine + Environment.NewLine + sourceCode;
                }
            }

            return sourceCode;
        }

        /// <summary>
        /// <para>
        /// Removes duplicate MethodImpl attributes that might appear consecutively.
        /// </para>
        /// </summary>
        private static string RemoveDuplicateAttributes(string sourceCode)
        {
            // Remove consecutive duplicate attributes
            var duplicatePattern = new Regex(
                @"(\[System\.Runtime\.CompilerServices\.MethodImpl\(System\.Runtime\.CompilerServices\.MethodImplOptions\.AggressiveInlining\)\]\s*\r?\n\s*)+",
                RegexOptions.Multiline | RegexOptions.Compiled
            );

            return duplicatePattern.Replace(sourceCode, match =>
            {
                // Keep only one attribute
                var lines = match.Value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var indent = "";
                foreach (var line in lines)
                {
                    if (line.Contains("[System.Runtime.CompilerServices.MethodImpl"))
                    {
                        // Extract the indentation
                        var trimmed = line.TrimStart();
                        indent = line.Substring(0, line.Length - trimmed.Length);
                        return indent + "[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]" + Environment.NewLine;
                    }
                }
                return match.Value;
            });
        }
    }
}
