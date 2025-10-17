using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Platform.CodeTransformations
{
    /// <summary>
    /// Engine for applying transformation rules to source code files.
    /// </summary>
    public class TransformationEngine
    {
        private readonly List<ITransformationRule> _rules;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformationEngine"/> class.
        /// </summary>
        public TransformationEngine()
        {
            _rules = new List<ITransformationRule>();
        }

        /// <summary>
        /// Registers a transformation rule.
        /// </summary>
        /// <param name="rule">The rule to register.</param>
        public void RegisterRule(ITransformationRule rule)
        {
            _rules.Add(rule);
        }

        /// <summary>
        /// Registers multiple transformation rules.
        /// </summary>
        /// <param name="rules">The rules to register.</param>
        public void RegisterRules(IEnumerable<ITransformationRule> rules)
        {
            _rules.AddRange(rules);
        }

        /// <summary>
        /// Transforms a source file from one version to another.
        /// </summary>
        /// <param name="sourceFilePath">Path to the source file.</param>
        /// <param name="fromVersion">The current version of the code.</param>
        /// <param name="toVersion">The target version to transform to.</param>
        /// <returns>The transformed source code.</returns>
        public string TransformFile(string sourceFilePath, string fromVersion, string toVersion)
        {
            var sourceCode = File.ReadAllText(sourceFilePath);
            return TransformCode(sourceCode, fromVersion, toVersion);
        }

        /// <summary>
        /// Transforms source code from one version to another.
        /// </summary>
        /// <param name="sourceCode">The source code to transform.</param>
        /// <param name="fromVersion">The current version of the code.</param>
        /// <param name="toVersion">The target version to transform to.</param>
        /// <returns>The transformed source code.</returns>
        public string TransformCode(string sourceCode, string fromVersion, string toVersion)
        {
            var tree = CSharpSyntaxTree.ParseText(sourceCode);
            var applicableRules = GetApplicableRules(fromVersion, toVersion);

            foreach (var rule in applicableRules)
            {
                if (rule.IsApplicable(tree))
                {
                    tree = rule.Transform(tree);
                }
            }

            return tree.GetRoot().ToFullString();
        }

        /// <summary>
        /// Gets the transformation rules applicable for transforming from one version to another.
        /// </summary>
        /// <param name="fromVersion">The source version.</param>
        /// <param name="toVersion">The target version.</param>
        /// <returns>List of applicable transformation rules.</returns>
        public IEnumerable<ITransformationRule> GetApplicableRules(string fromVersion, string toVersion)
        {
            var path = FindTransformationPath(fromVersion, toVersion);
            return path;
        }

        private IEnumerable<ITransformationRule> FindTransformationPath(string fromVersion, string toVersion)
        {
            // Simple implementation: find direct path or sequential path
            var directRules = _rules.Where(r => r.FromVersion == fromVersion && r.ToVersion == toVersion).ToList();
            if (directRules.Any())
            {
                return directRules;
            }

            // Try to find a path through intermediate versions
            var visited = new HashSet<string>();
            var queue = new Queue<(string version, List<ITransformationRule> path)>();
            queue.Enqueue((fromVersion, new List<ITransformationRule>()));

            while (queue.Count > 0)
            {
                var (currentVersion, currentPath) = queue.Dequeue();

                if (currentVersion == toVersion)
                {
                    return currentPath;
                }

                if (visited.Contains(currentVersion))
                {
                    continue;
                }

                visited.Add(currentVersion);

                var nextRules = _rules.Where(r => r.FromVersion == currentVersion);
                foreach (var rule in nextRules)
                {
                    var newPath = new List<ITransformationRule>(currentPath) { rule };
                    queue.Enqueue((rule.ToVersion, newPath));
                }
            }

            return Enumerable.Empty<ITransformationRule>();
        }

        /// <summary>
        /// Transforms all C# files in a directory.
        /// </summary>
        /// <param name="directoryPath">Path to the directory.</param>
        /// <param name="fromVersion">The current version of the code.</param>
        /// <param name="toVersion">The target version to transform to.</param>
        /// <param name="recursive">Whether to search subdirectories.</param>
        public void TransformDirectory(string directoryPath, string fromVersion, string toVersion, bool recursive = true)
        {
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(directoryPath, "*.cs", searchOption);

            foreach (var file in files)
            {
                var transformedCode = TransformFile(file, fromVersion, toVersion);
                File.WriteAllText(file, transformedCode);
            }
        }
    }
}
