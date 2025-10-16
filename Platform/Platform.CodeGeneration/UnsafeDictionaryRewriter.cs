using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Platform.CodeGeneration
{
    /// <summary>
    /// Transforms Dictionary source code to UnsafeDictionary by:
    /// 1. Making the Entries field public
    /// 2. Removing exception checks (throw statements)
    /// 3. Renaming Dictionary to UnsafeDictionary
    /// </summary>
    public class UnsafeDictionaryRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            // Rename Dictionary to UnsafeDictionary
            if (node.Identifier.Text == "Dictionary")
            {
                node = node.WithIdentifier(SyntaxFactory.Identifier("UnsafeDictionary"));
            }

            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            // Handle any struct renaming if needed (e.g., Dictionary.Entry)
            return base.VisitStructDeclaration(node);
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            // Make _entries or entries field public if it's private/internal
            var variable = node.Declaration.Variables.FirstOrDefault();
            if (variable != null)
            {
                var variableName = variable.Identifier.Text;
                if (variableName.Contains("entries") || variableName.Contains("_entries"))
                {
                    // Remove private/internal modifiers and add public
                    var modifiers = node.Modifiers
                        .Where(m => !m.IsKind(SyntaxKind.PrivateKeyword) &&
                                   !m.IsKind(SyntaxKind.InternalKeyword))
                        .ToList();

                    // Add public if not already present
                    if (!modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                    {
                        modifiers.Insert(0, SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                    }

                    node = node.WithModifiers(SyntaxFactory.TokenList(modifiers));
                }
            }

            return base.VisitFieldDeclaration(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            // Make Entries property public if it exists
            if (node.Identifier.Text == "Entries" || node.Identifier.Text.Contains("entries"))
            {
                var modifiers = node.Modifiers
                    .Where(m => !m.IsKind(SyntaxKind.PrivateKeyword) &&
                               !m.IsKind(SyntaxKind.InternalKeyword))
                    .ToList();

                if (!modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                {
                    modifiers.Insert(0, SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                }

                node = node.WithModifiers(SyntaxFactory.TokenList(modifiers));
            }

            return base.VisitPropertyDeclaration(node);
        }

        public override SyntaxNode VisitThrowStatement(ThrowStatementSyntax node)
        {
            // Remove all throw statements to eliminate exception checks
            // Return an empty statement instead
            return SyntaxFactory.EmptyStatement();
        }

        public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            // Check if the if statement only contains a throw statement
            if (node.Statement is BlockSyntax block)
            {
                var statements = block.Statements;
                if (statements.Count == 1 && statements[0] is ThrowStatementSyntax)
                {
                    // Remove the entire if statement if it only throws
                    return SyntaxFactory.EmptyStatement();
                }
            }
            else if (node.Statement is ThrowStatementSyntax)
            {
                // Remove if statement with single throw
                return SyntaxFactory.EmptyStatement();
            }

            return base.VisitIfStatement(node);
        }

        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            // Check for ThrowHelper method calls and remove them
            if (node.Expression is InvocationExpressionSyntax invocation)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    if (memberAccess.Expression is IdentifierNameSyntax identifier &&
                        identifier.Identifier.Text == "ThrowHelper")
                    {
                        return SyntaxFactory.EmptyStatement();
                    }
                }
                else if (invocation.Expression is IdentifierNameSyntax identifier &&
                         identifier.Identifier.Text.StartsWith("Throw"))
                {
                    return SyntaxFactory.EmptyStatement();
                }
            }

            return base.VisitExpressionStatement(node);
        }
    }
}
