/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 01 мая 2026 06:52:48
 * Version: 1.0.4
 */

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Lizerium.Localization.Analyzer;

/// <summary>
/// Creates missing RESX keys for diagnostics reported by <see cref="LocalizationInvocationAnalyzer"/>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CreateLocalizationKeyCodeFixProvider))]
[Shared]
public sealed class CreateLocalizationKeyCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds 
        => ImmutableArray.Create(LocalizationInvocationAnalyzer.DiagnosticId);

    /// <inheritdoc />
    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        var node = root?.FindNode(context.Span);

        var literal = node?.FirstAncestorOrSelf<LiteralExpressionSyntax>();

        if (literal == null || !literal.IsKind(SyntaxKind.StringLiteralExpression))
            return;

        var diagnostic = context.Diagnostics.FirstOrDefault();
        if (diagnostic is null || diagnostic.Properties.TryGetValue("key", out var key) is false || string.IsNullOrWhiteSpace(key))
            return;

        var argumentCount = await GetArgumentCountAsync(context.Document, context.Span, context.CancellationToken).ConfigureAwait(false);
        if (diagnostic.Properties.TryGetValue("argumentCount", out var argumentCountText))
        {
            if (int.TryParse(argumentCountText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var diagnosticArgumentCount))
                argumentCount = Math.Max(argumentCount, diagnosticArgumentCount);
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                "Create localization key",
                token => CreateKeyAsync(context.Document.Project, key!, argumentCount, token),
                equivalenceKey: "CreateLocalizationKey"),
            diagnostic);
    }

    private static async Task<int> GetArgumentCountAsync(Document document, TextSpan span, CancellationToken token)
    {
        var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);
        var node = root?.FindNode(span, getInnermostNodeForTie: true);
        return node?.FirstAncestorOrSelf<InvocationExpressionSyntax>()?.ArgumentList.Arguments.Count ?? 0;
    }

    private static async Task<Solution> CreateKeyAsync(Project project, string key, int argumentCount, CancellationToken token)
    {
        var solution = project.Solution;
        // Generated methods with arguments are produced from _Format keys with numbered placeholders.
        var resourceKey = argumentCount > 0 && key.EndsWith("_Format", StringComparison.OrdinalIgnoreCase) is false
            ? key + "_Format"
            : key;

        var resxDocuments = project.AdditionalDocuments
            .Where(document => document.FilePath?.EndsWith(".resx", StringComparison.OrdinalIgnoreCase) == true)
            .ToArray();

        var en = FindDocument(resxDocuments, ".en.resx");
        var ru = FindDocument(resxDocuments, ".ru.resx");

        if (en is not null)
            solution = await AddOrUpdateAsync(solution, en, resourceKey, argumentCount, token).ConfigureAwait(false);

        if (ru is not null)
            solution = await AddOrUpdateAsync(solution, ru, resourceKey, argumentCount, token).ConfigureAwait(false);

        return solution;
    }

    private static TextDocument? FindDocument(TextDocument[] documents, string suffix)
    {
        // Prefer culture-specific files, but support a neutral Strings.resx in simpler projects.
        return documents.FirstOrDefault(document => document.FilePath?.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) == true)
            ?? documents.FirstOrDefault(document => string.Equals(Path.GetFileName(document.FilePath), "Strings.resx", StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<Solution> AddOrUpdateAsync(Solution solution, TextDocument document, string key, int argumentCount, System.Threading.CancellationToken token)
    {
        var text = await document.GetTextAsync(token).ConfigureAwait(false);
        var newText = AddOrUpdate(text.ToString(), key, argumentCount);
        return solution.WithAdditionalDocumentText(document.Id, SourceText.From(newText, text.Encoding ?? System.Text.Encoding.UTF8));
    }

    private static string AddOrUpdate(string xml, string key, int argumentCount)
    {
        var document = string.IsNullOrWhiteSpace(xml)
            ? CreateDocument()
            : XDocument.Parse(xml, LoadOptions.PreserveWhitespace);

        var root = document.Root ?? throw new InvalidOperationException("Invalid RESX file.");
        var existing = root.Elements("data")
            .FirstOrDefault(item => string.Equals(item.Attribute("name")?.Value, key, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            root.Add(new XElement("data",
                new XAttribute("name", key),
                new XAttribute(XNamespace.Xml + "space", "preserve"),
                new XElement("value", CreateTodoValue(argumentCount))));
        }

        return document.ToString();
    }

    private static string CreateTodoValue(int argumentCount)
    {
        if (argumentCount <= 0)
            return "TODO";

        // Keep generated placeholders explicit so the source generator creates the expected signature.
        return "TODO " + string.Join(" ", Enumerable.Range(0, argumentCount).Select(index => "{" + index + "}"));
    }

    private static XDocument CreateDocument()
    {
        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("root",
                new XElement("resheader", new XAttribute("name", "resmimetype"), new XElement("value", "text/microsoft-resx")),
                new XElement("resheader", new XAttribute("name", "version"), new XElement("value", "2.0")),
                new XElement("resheader", new XAttribute("name", "reader"), new XElement("value", "System.Resources.ResXResourceReader, System.Windows.Forms")),
                new XElement("resheader", new XAttribute("name", "writer"), new XElement("value", "System.Resources.ResXResourceWriter, System.Windows.Forms"))));
    }
}
