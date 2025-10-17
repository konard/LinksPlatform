using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Platform.CodeTransformations.Rules
{
    /// <summary>
    /// Transformation rule for renaming types (classes, interfaces, structs) across the codebase.
    /// </summary>
    public class TypeRenameRule : TransformationRuleBase
    {
        private readonly string _oldTypeName;
        private readonly string _newTypeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeRenameRule"/> class.
        /// </summary>
        /// <param name="oldTypeName">The old type name.</param>
        /// <param name="newTypeName">The new type name.</param>
        /// <param name="fromVersion">The source version.</param>
        /// <param name="toVersion">The target version.</param>
        public TypeRenameRule(string oldTypeName, string newTypeName, string fromVersion, string toVersion)
        {
            _oldTypeName = oldTypeName;
            _newTypeName = newTypeName;
            FromVersion = fromVersion;
            ToVersion = toVersion;
        }

        public override string Name => $"Rename type {_oldTypeName} to {_newTypeName}";

        public override string Description => $"Renames type '{_oldTypeName}' to '{_newTypeName}' and all references to it";

        public override string FromVersion { get; }

        public override string ToVersion { get; }

        public override SyntaxTree Transform(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            var rewriter = new TypeRenameRewriter(_oldTypeName, _newTypeName);
            var newRoot = rewriter.Visit(root);
            return tree.WithRootAndOptions(newRoot, tree.Options);
        }

        public override bool IsApplicable(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            var identifiers = root.DescendantTokens().Where(t => t.IsKind(SyntaxKind.IdentifierToken));
            return identifiers.Any(i => i.Text == _oldTypeName);
        }

        private class TypeRenameRewriter : CSharpSyntaxRewriter
        {
            private readonly string _oldTypeName;
            private readonly string _newTypeName;

            public TypeRenameRewriter(string oldTypeName, string newTypeName)
            {
                _oldTypeName = oldTypeName;
                _newTypeName = newTypeName;
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node.Identifier.Text == _oldTypeName)
                {
                    var newIdentifier = SyntaxFactory.Identifier(_newTypeName);
                    return node.WithIdentifier(newIdentifier);
                }

                return base.VisitClassDeclaration(node);
            }

            public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
            {
                if (node.Identifier.Text == _oldTypeName)
                {
                    var newIdentifier = SyntaxFactory.Identifier(_newTypeName);
                    return node.WithIdentifier(newIdentifier);
                }

                return base.VisitInterfaceDeclaration(node);
            }

            public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
            {
                if (node.Identifier.Text == _oldTypeName)
                {
                    var newIdentifier = SyntaxFactory.Identifier(_newTypeName);
                    return node.WithIdentifier(newIdentifier);
                }

                return base.VisitStructDeclaration(node);
            }

            public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (node.Identifier.Text == _oldTypeName)
                {
                    var newIdentifier = SyntaxFactory.Identifier(_newTypeName);
                    return SyntaxFactory.IdentifierName(newIdentifier);
                }

                return base.VisitIdentifierName(node);
            }
        }
    }
}
