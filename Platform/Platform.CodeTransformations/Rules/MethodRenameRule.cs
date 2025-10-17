using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Platform.CodeTransformations.Rules
{
    /// <summary>
    /// Transformation rule for renaming methods across the codebase.
    /// </summary>
    public class MethodRenameRule : TransformationRuleBase
    {
        private readonly string _oldMethodName;
        private readonly string _newMethodName;
        private readonly string _className;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodRenameRule"/> class.
        /// </summary>
        /// <param name="className">The class containing the method (can be null for any class).</param>
        /// <param name="oldMethodName">The old method name.</param>
        /// <param name="newMethodName">The new method name.</param>
        /// <param name="fromVersion">The source version.</param>
        /// <param name="toVersion">The target version.</param>
        public MethodRenameRule(string className, string oldMethodName, string newMethodName, string fromVersion, string toVersion)
        {
            _className = className;
            _oldMethodName = oldMethodName;
            _newMethodName = newMethodName;
            FromVersion = fromVersion;
            ToVersion = toVersion;
        }

        public override string Name => $"Rename method {_oldMethodName} to {_newMethodName}";

        public override string Description =>
            string.IsNullOrEmpty(_className)
                ? $"Renames all calls to method '{_oldMethodName}' to '{_newMethodName}'"
                : $"Renames calls to method '{_oldMethodName}' to '{_newMethodName}' in class '{_className}'";

        public override string FromVersion { get; }

        public override string ToVersion { get; }

        public override SyntaxTree Transform(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            var rewriter = new MethodRenameRewriter(_className, _oldMethodName, _newMethodName);
            var newRoot = rewriter.Visit(root);
            return tree.WithRootAndOptions(newRoot, tree.Options);
        }

        public override bool IsApplicable(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (var invocation in invocations)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    if (memberAccess.Name.Identifier.Text == _oldMethodName)
                    {
                        return true;
                    }
                }
                else if (invocation.Expression is IdentifierNameSyntax identifier)
                {
                    if (identifier.Identifier.Text == _oldMethodName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private class MethodRenameRewriter : CSharpSyntaxRewriter
        {
            private readonly string _className;
            private readonly string _oldMethodName;
            private readonly string _newMethodName;

            public MethodRenameRewriter(string className, string oldMethodName, string newMethodName)
            {
                _className = className;
                _oldMethodName = oldMethodName;
                _newMethodName = newMethodName;
            }

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (node.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    if (memberAccess.Name.Identifier.Text == _oldMethodName)
                    {
                        var newName = SyntaxFactory.IdentifierName(_newMethodName);
                        var newMemberAccess = memberAccess.WithName((SimpleNameSyntax)newName);
                        return node.WithExpression(newMemberAccess);
                    }
                }
                else if (node.Expression is IdentifierNameSyntax identifier)
                {
                    if (identifier.Identifier.Text == _oldMethodName)
                    {
                        var newIdentifier = SyntaxFactory.IdentifierName(_newMethodName);
                        return node.WithExpression(newIdentifier);
                    }
                }

                return base.VisitInvocationExpression(node);
            }

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                if (node.Identifier.Text == _oldMethodName)
                {
                    if (string.IsNullOrEmpty(_className))
                    {
                        var newIdentifier = SyntaxFactory.Identifier(_newMethodName);
                        return node.WithIdentifier(newIdentifier);
                    }
                }

                return base.VisitMethodDeclaration(node);
            }
        }
    }
}
