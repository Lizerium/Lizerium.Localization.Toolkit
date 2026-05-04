/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 04 мая 2026 06:52:49
 * Version: 1.0.7
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
                switch (ctx.Node)
                {
                    case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression):
                        ctx.ReportDiagnostic(Diagnostic.Create(Rule, literal.GetLocation()));
                        break;
                    case InterpolatedStringExpressionSyntax interpolatedString:
                        ctx.ReportDiagnostic(Diagnostic.Create(Rule, interpolatedString.GetLocation()));
                        break;
                }

            }, SyntaxKind.StringLiteralExpression, SyntaxKind.InterpolatedStringExpression);
        }
    }
}
