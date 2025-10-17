using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BreakingChangesMigrator
{
    /// <summary>
    /// Main code migrator that applies migration rules to source code.
    /// </summary>
    public class CodeMigrator
    {
        private readonly List<MigrationRule> _rules;
        private readonly SemanticModel _semanticModel;

        public CodeMigrator(List<MigrationRule> rules, SemanticModel semanticModel)
        {
            _rules = rules;
            _semanticModel = semanticModel;
        }

        /// <summary>
        /// Migrates a syntax tree according to the loaded rules.
        /// </summary>
        public SyntaxNode Migrate(SyntaxNode root)
        {
            var rewriter = new MigrationRewriter(_rules, _semanticModel);
            return rewriter.Visit(root);
        }

        private class MigrationRewriter : CSharpSyntaxRewriter
        {
            private readonly List<MigrationRule> _rules;
            private readonly SemanticModel _semanticModel;

            public MigrationRewriter(List<MigrationRule> rules, SemanticModel semanticModel)
            {
                _rules = rules;
                _semanticModel = semanticModel;
            }

            public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var symbolInfo = _semanticModel.GetSymbolInfo(node);
                if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
                {
                    foreach (var rule in _rules)
                    {
                        if (rule.ChangeType == "MethodRename" || rule.ChangeType == "MethodChange")
                        {
                            if (MatchesMethod(methodSymbol, rule.OldPattern))
                            {
                                return ApplyMethodMigration(node, rule);
                            }
                        }
                    }
                }

                return base.VisitInvocationExpression(node);
            }

            public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
            {
                var symbolInfo = _semanticModel.GetSymbolInfo(node);
                if (symbolInfo.Symbol is INamedTypeSymbol typeSymbol)
                {
                    foreach (var rule in _rules)
                    {
                        if (rule.ChangeType == "TypeRename")
                        {
                            if (MatchesType(typeSymbol, rule.OldPattern))
                            {
                                return ApplyTypeRename(node, rule);
                            }
                        }
                    }
                }

                return base.VisitIdentifierName(node);
            }

            private bool MatchesMethod(IMethodSymbol method, PatternMatch pattern)
            {
                if (pattern.TypeName != null)
                {
                    var fullTypeName = method.ContainingType.ToDisplayString();
                    if (!fullTypeName.Contains(pattern.TypeName))
                        return false;
                }

                if (pattern.MemberName != null)
                {
                    if (method.Name != pattern.MemberName)
                        return false;
                }

                if (pattern.ParameterTypes.Count > 0)
                {
                    if (method.Parameters.Length != pattern.ParameterTypes.Count)
                        return false;

                    for (int i = 0; i < pattern.ParameterTypes.Count; i++)
                    {
                        var paramType = method.Parameters[i].Type.ToDisplayString();
                        if (!paramType.Contains(pattern.ParameterTypes[i]))
                            return false;
                    }
                }

                return true;
            }

            private bool MatchesType(INamedTypeSymbol type, PatternMatch pattern)
            {
                if (pattern.TypeName != null)
                {
                    var fullTypeName = type.ToDisplayString();
                    return fullTypeName.Contains(pattern.TypeName) || type.Name == pattern.TypeName;
                }

                return false;
            }

            private SyntaxNode ApplyMethodMigration(InvocationExpressionSyntax node, MigrationRule rule)
            {
                if (rule.NewPattern.MemberName != null && node.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    var newName = SyntaxFactory.IdentifierName(rule.NewPattern.MemberName);
                    var newMemberAccess = memberAccess.WithName(newName);
                    return node.WithExpression(newMemberAccess);
                }

                return node;
            }

            private SyntaxNode ApplyTypeRename(IdentifierNameSyntax node, MigrationRule rule)
            {
                if (rule.NewPattern.TypeName != null)
                {
                    var newName = rule.NewPattern.TypeName.Split('.').Last();
                    return SyntaxFactory.IdentifierName(newName);
                }

                return node;
            }
        }
    }
}
