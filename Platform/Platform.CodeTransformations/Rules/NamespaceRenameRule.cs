using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Platform.CodeTransformations.Rules
{
    /// <summary>
    /// Transformation rule for renaming namespaces across the codebase.
    /// </summary>
    public class NamespaceRenameRule : TransformationRuleBase
    {
        private readonly string _oldNamespace;
        private readonly string _newNamespace;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamespaceRenameRule"/> class.
        /// </summary>
        /// <param name="oldNamespace">The old namespace name.</param>
        /// <param name="newNamespace">The new namespace name.</param>
        /// <param name="fromVersion">The source version.</param>
        /// <param name="toVersion">The target version.</param>
        public NamespaceRenameRule(string oldNamespace, string newNamespace, string fromVersion, string toVersion)
        {
            _oldNamespace = oldNamespace;
            _newNamespace = newNamespace;
            FromVersion = fromVersion;
            ToVersion = toVersion;
        }

        public override string Name => $"Rename namespace {_oldNamespace} to {_newNamespace}";

        public override string Description => $"Renames namespace '{_oldNamespace}' to '{_newNamespace}' and updates all usings";

        public override string FromVersion { get; }

        public override string ToVersion { get; }

        public override SyntaxTree Transform(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            var rewriter = new NamespaceRenameRewriter(_oldNamespace, _newNamespace);
            var newRoot = rewriter.Visit(root);
            return tree.WithRootAndOptions(newRoot, tree.Options);
        }

        public override bool IsApplicable(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            var namespaceDeclarations = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>();
            var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>();

            return namespaceDeclarations.Any(ns => ns.Name.ToString() == _oldNamespace) ||
                   usingDirectives.Any(u => u.Name.ToString() == _oldNamespace);
        }

        private class NamespaceRenameRewriter : CSharpSyntaxRewriter
        {
            private readonly string _oldNamespace;
            private readonly string _newNamespace;

            public NamespaceRenameRewriter(string oldNamespace, string newNamespace)
            {
                _oldNamespace = oldNamespace;
                _newNamespace = newNamespace;
            }

            public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
            {
                if (node.Name.ToString() == _oldNamespace)
                {
                    var newName = SyntaxFactory.ParseName(_newNamespace);
                    return node.WithName(newName);
                }

                return base.VisitNamespaceDeclaration(node);
            }

            public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
            {
                if (node.Name.ToString() == _oldNamespace)
                {
                    var newName = SyntaxFactory.ParseName(_newNamespace);
                    return node.WithName(newName);
                }

                return base.VisitUsingDirective(node);
            }
        }
    }
}
