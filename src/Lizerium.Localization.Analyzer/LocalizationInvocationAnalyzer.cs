/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 04 мая 2026 06:52:49
 * Version: 1.0.7
 */

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Lizerium.Localization.Analyzer;

/// <summary>
/// Reports generated localization method calls that do not have matching RESX keys.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LocalizationInvocationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic identifier used for missing localization keys.
    /// </summary>
    public const string DiagnosticId = "LOC101";

    /// <summary>
    /// Descriptor for missing generated localization keys.
    /// </summary>
    public static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Localization key does not exist",
        "Localization key '{0}' does not exist",
        "Localization",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (TryBuildKey(invocation.Expression, context.SemanticModel, context.CancellationToken, out var key, out var methodName) is false)
            return;

        var argumentCount = invocation.ArgumentList.Arguments.Count;
        var knownKeys = LoadKnownKeys(context.Options.AdditionalFiles, context.CancellationToken);
        if (HasMatchingKey(knownKeys, key, argumentCount))
            return;

        var properties = ImmutableDictionary<string, string?>.Empty
            .Add("key", key)
            .Add("method", methodName)
            .Add("argumentCount", argumentCount.ToString(System.Globalization.CultureInfo.InvariantCulture));

        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Expression.GetLocation(), properties, key));
    }

    /// <summary>
    /// Converts a generated localization member access into its RESX key form.
    /// </summary>
    /// <param name="expression">Invocation expression such as <c>L.MainWindow.Title</c>.</param>
    /// <param name="semanticModel">Semantic model used to resolve aliases.</param>
    /// <param name="token">Cancellation token.</param>
    /// <param name="key">Resolved RESX key without a trailing <c>_Format</c>.</param>
    /// <param name="methodName">Final method name from the member access chain.</param>
    /// <returns><c>true</c> when the expression targets the generated localization API.</returns>
    internal static bool TryBuildKey(ExpressionSyntax expression, SemanticModel semanticModel, System.Threading.CancellationToken token, out string key, out string methodName)
    {
        var parts = new Stack<string>();
        var current = expression;

        while (current is MemberAccessExpressionSyntax memberAccess)
        {
            parts.Push(memberAccess.Name.Identifier.ValueText);
            current = memberAccess.Expression;
        }

        if (current is IdentifierNameSyntax identifier)
            parts.Push(identifier.Identifier.ValueText);

        var values = parts.ToArray();
        if (values.Length < 2 || IsLocalizationRoot(current, semanticModel, token) is false)
        {
            key = string.Empty;
            methodName = string.Empty;
            return false;
        }

        methodName = values[values.Length - 1];
        key = string.Join("_", values.Skip(1));
        return true;
    }

    private static bool IsLocalizationRoot(ExpressionSyntax expression, SemanticModel semanticModel, System.Threading.CancellationToken token)
    {
        if (expression is not IdentifierNameSyntax identifier)
            return false;

        if (identifier.Identifier.ValueText == "Localization")
            return true;

        // Alias support lets the analyzer recognize common usage such as:
        // using L = Generated.Localization.Localization;
        var alias = semanticModel.GetAliasInfo(identifier, token);
        if (alias?.Target is INamedTypeSymbol aliasTarget && IsGeneratedLocalizationType(aliasTarget))
            return true;

        return semanticModel.GetSymbolInfo(identifier, token).Symbol is INamedTypeSymbol symbol
            && IsGeneratedLocalizationType(symbol);
    }

    private static bool IsGeneratedLocalizationType(INamedTypeSymbol symbol)
    {
        return symbol.Name == "Localization"
            && symbol.ContainingNamespace.ToDisplayString() == "Generated.Localization";
    }

    private static bool HasMatchingKey(ImmutableDictionary<string, int> knownKeys, string key, int argumentCount)
    {
        if (argumentCount == 0)
            return knownKeys.ContainsKey(key) || knownKeys.ContainsKey(key + "_Format");

        // Parameterized generated calls must map to keys with enough placeholders.
        return knownKeys.TryGetValue(key, out var parameterCount) && parameterCount >= argumentCount
            || knownKeys.TryGetValue(key + "_Format", out parameterCount) && parameterCount >= argumentCount;
    }

    /// <summary>
    /// Loads known RESX keys and their placeholder counts from project additional files.
    /// </summary>
    /// <param name="files">Additional files supplied by the consuming project.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A dictionary keyed by RESX data name.</returns>
    internal static ImmutableDictionary<string, int> LoadKnownKeys(ImmutableArray<AdditionalText> files, System.Threading.CancellationToken token)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            if (!file.Path.EndsWith(".resx", StringComparison.OrdinalIgnoreCase))
                continue;

            var text = file.GetText(token)?.ToString();
            if (string.IsNullOrWhiteSpace(text))
                continue;

            try
            {
                var document = XDocument.Parse(text);
                foreach (var item in document.Root?.Elements("data") ?? Enumerable.Empty<XElement>())
                {
                    var key = item.Attribute("name")?.Value;
                    if (string.IsNullOrWhiteSpace(key))
                        continue;

                    builder[key!] = CountPlaceholders(item.Element("value")?.Value ?? string.Empty);
                }
            }
            catch
            {
                // Invalid resx files are reported by MSBuild/IDE; this analyzer keeps quiet.
            }
        }

        return builder.ToImmutable();
    }

    private static int CountPlaceholders(string value)
    {
        var max = -1;
        foreach (Match match in Regex.Matches(value ?? string.Empty, @"(?<!\{)\{(\d+)(?:[^}]*)\}(?!\})"))
        {
            if (int.TryParse(match.Groups[1].Value, out var index) && index > max)
                max = index;
        }

        return max + 1;
    }
}
