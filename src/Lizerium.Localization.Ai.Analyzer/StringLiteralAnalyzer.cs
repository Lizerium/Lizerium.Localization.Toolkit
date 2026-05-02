/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 02 мая 2026 19:17:07
 * Version: 1.0.5
 */

using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Lizerium.Localization.Ai.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StringLiteralAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AI001";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "String literal",
            "String literal detected",
            "AI",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(ctx =>
            {
                var literal = (LiteralExpressionSyntax)ctx.Node;

                if (!literal.IsKind(SyntaxKind.StringLiteralExpression))
                    return;

                var diagnostic = Diagnostic.Create(Rule, literal.GetLocation());
                ctx.ReportDiagnostic(diagnostic);

            }, SyntaxKind.StringLiteralExpression);
        }
    }
}
