/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 05 мая 2026 07:01:45
 * Version: 1.0.8
 */

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Lizerium.Localization.Ai.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AiLocalizationCodeFixProvider)), Shared]
    public class AiLocalizationCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(StringLiteralAnalyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
            if (root == null)
                return;

            foreach (var diagnostic in context.Diagnostics)
            {
                var span = diagnostic.Location.SourceSpan;
                var node = root.FindNode(span);

                var localizableNode = GetLocalizableNode(node);
                if (localizableNode is null)
                    continue;

                context.RegisterCodeFix(
                    CodeAction.Create(
                        GetLocalizedTitle(),
                        token => CreateKeyWithAiAsync(context.Document, span, token),
                        equivalenceKey: "AI_Localization"),
                    diagnostic);
            }
        }

        private async Task<Solution> CreateKeyWithAiAsync(
            Document document,
            TextSpan span,
            CancellationToken token)
        {
            try
            {
                var project = document.Project;
                var solution = project.Solution;

                var root = await document.GetSyntaxRootAsync(token);
                if (root is null)
                    return solution;

                var node = root.FindNode(span, getInnermostNodeForTie: true);
                var localizableNode = GetLocalizableNode(node);
                if (localizableNode is null)
                    return solution;

                var localizableText = GetLocalizableText(localizableNode);
                if (localizableText is null || string.IsNullOrWhiteSpace(localizableText.Value))
                    return solution;

                var sourceText = localizableText.Value;

                var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                var classDecl = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();

                var className = SanitizeIdentifier(classDecl?.Identifier.ValueText ?? "Global");
                var methodName = SanitizeIdentifier(method?.Identifier.ValueText ?? "Method");

                var replacementArguments = localizableText.Arguments;
                var argumentCount = Math.Max(GetPlaceholderCount(sourceText), replacementArguments.Length);

                var resxDocuments = project.AdditionalDocuments
                    .Where(d => d.FilePath?.EndsWith(".resx", StringComparison.OrdinalIgnoreCase) == true)
                    .ToArray();

                var en = FindDocument(resxDocuments, ".en.resx");
                var ru = FindDocument(resxDocuments, ".ru.resx");

                var nextIndex = await GetNextIndexAsync(en ?? ru, className, methodName, token);

                var key = $"{className}_{methodName}_Text{nextIndex}";
                if (argumentCount > 0)
                    key += "_Format";

                var result = await GetSafeLocalizationResultAsync(sourceText, project.FilePath);

                if (en is not null)
                    solution = await AddOrUpdateAsync(solution, en, key, result.En, token);

                if (ru is not null)
                    solution = await AddOrUpdateAsync(solution, ru, key, result.Ru, token);

                solution = await ReplaceStringWithInvocationAsync(
                    document,
                    localizableNode.Span,
                    key,
                    replacementArguments,
                    solution,
                    token);

                return solution;
            }
            catch
            {
                return document.Project.Solution;
            }
        }

        private static async Task<int> GetNextIndexAsync(
            TextDocument? document,
            string className,
            string methodName,
            CancellationToken token)
        {
            if (document is null)
                return 1;

            var text = await document.GetTextAsync(token);
            var xml = text.ToString();

            if (string.IsNullOrWhiteSpace(xml))
                return 1;

            var xdoc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);

            var prefix = $"{className}_{methodName}_Text";

            var max = xdoc.Root?
                .Elements("data")
                .Select(x => x.Attribute("name")?.Value)
                .Where(x => x != null && x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(x => ExtractIndex(x!, className, methodName))
                .DefaultIfEmpty(0)
                .Max() ?? 0;

            return max + 1;
        }

        private static int ExtractIndex(string key, string className, string methodName)
        {
            var value = key;

            if (value.EndsWith("_Format", StringComparison.OrdinalIgnoreCase))
                value = value.Substring(0, value.Length - "_Format".Length);

            var prefix = $"{className}_{methodName}_Text";

            if (!value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return 0;

            var numberText = value.Substring(prefix.Length);

            return int.TryParse(numberText, out var number)
                ? number
                : 0;
        }

        private static int GetPlaceholderCount(string text)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(text, @"(?<!\{)\{(\d+)(?:[^}]*)\}(?!\})");

            if (matches.Count == 0)
                return 0;

            return matches
                .Cast<System.Text.RegularExpressions.Match>()
                .Select(m => int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture))
                .DefaultIfEmpty(-1)
                .Max() + 1;
        }

        private static async Task<Solution> AddOrUpdateAsync(
            Solution solution,
            TextDocument document,
            string key,
            string value,
            CancellationToken token)
        {
            var text = await document.GetTextAsync(token).ConfigureAwait(false);
            var newText = AddOrUpdate(text.ToString(), key, value);

            return solution.WithAdditionalDocumentText(
                document.Id,
                SourceText.From(newText, text.Encoding ?? System.Text.Encoding.UTF8));
        }

        private static string AddOrUpdate(string xml, string key, string value)
        {
            var document = string.IsNullOrWhiteSpace(xml)
                ? CreateDocument()
                : XDocument.Parse(xml, LoadOptions.PreserveWhitespace);

            var root = document.Root ?? throw new InvalidOperationException("Invalid RESX file.");

            var existing = root.Elements("data")
                .FirstOrDefault(item => string.Equals(
                    item.Attribute("name")?.Value,
                    key,
                    StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                root.Add(new XElement("data",
                    new XAttribute("name", key),
                    new XAttribute(XNamespace.Xml + "space", "preserve"),
                    new XElement("value", value)));
            }
            else
            {
                var valueElement = existing.Element("value");

                if (valueElement is null)
                    existing.Add(new XElement("value", value));
                else
                    valueElement.Value = value;
            }

            return document.ToString();
        }


        private static TextDocument? FindDocument(TextDocument[] documents, string suffix)
        {
            // Prefer culture-specific files, but support a neutral Strings.resx in simpler projects.
            return documents.FirstOrDefault(document => document.FilePath?.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) == true)
                ?? documents.FirstOrDefault(document => string.Equals(Path.GetFileName(document.FilePath), "Strings.resx", StringComparison.OrdinalIgnoreCase));
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

        private static string GetLocalizedTitle()
        {
            return "Generate localization key (AI)";
        }

        private static async Task<Solution> ReplaceStringWithInvocationAsync(
            Document document,
            TextSpan span,
            string key,
            ImmutableArray<string> arguments,
            Solution solution,
            CancellationToken token)
        {
            var currentDocument = solution.GetDocument(document.Id) ?? document;
            var root = await currentDocument.GetSyntaxRootAsync(token);
            if (root is null)
                return solution;

            var localizableNode = GetLocalizableNode(root.FindNode(span, getInnermostNodeForTie: true));
            if (localizableNode is null)
                return solution;

            var invocationText = CreateInvocationText(key, arguments);
            if (string.IsNullOrWhiteSpace(invocationText))
                return solution;

            var invocation = SyntaxFactory.ParseExpression(invocationText)
                .WithTriviaFrom(localizableNode);

            var newRoot = root.ReplaceNode(localizableNode, invocation);
            newRoot = EnsureLocalizationAlias(newRoot);

            return solution.WithDocumentSyntaxRoot(document.Id, newRoot);
        }

        private static SyntaxNode EnsureLocalizationAlias(SyntaxNode root)
        {
            if (root is not CompilationUnitSyntax compilationUnit)
                return root;

            var hasAlias = compilationUnit.Usings.Any(item =>
                item.Alias?.Name.Identifier.ValueText == "L" &&
                string.Equals(item.Name?.ToString(), "Generated.Localization.Localization", StringComparison.Ordinal));

            if (hasAlias)
                return root;

            var aliasUsing = SyntaxFactory.UsingDirective(
                    SyntaxFactory.NameEquals("L"),
                    SyntaxFactory.ParseName("Generated.Localization.Localization"))
                .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);

            return compilationUnit.AddUsings(aliasUsing);
        }

        private static async Task<LocalizationResult> GetSafeLocalizationResultAsync(string sourceText, string? projectFilePath)
        {
            LocalizationResult? result = null;

            try
            {
                result = await AiRunner.RunAsync(sourceText, projectFilePath).ConfigureAwait(false);
            }
            catch
            {
                // The code fix must keep the project compilable even when the local AI service is unavailable.
            }

            result ??= new LocalizationResult();

            if (string.IsNullOrWhiteSpace(result.En))
                result.En = sourceText;

            if (string.IsNullOrWhiteSpace(result.Ru))
                result.Ru = sourceText;

            return result;
        }

        private static ImmutableArray<string> GetReplacementArguments(SyntaxNode node, string sourceText)
        {
            var placeholderCount = GetPlaceholderCount(sourceText);
            if (placeholderCount <= 0)
                return ImmutableArray<string>.Empty;

            var literal = node.FirstAncestorOrSelf<LiteralExpressionSyntax>();
            var argument = literal?.FirstAncestorOrSelf<ArgumentSyntax>();
            var invocation = argument?.FirstAncestorOrSelf<InvocationExpressionSyntax>();

            if (argument is not null &&
                invocation is not null &&
                IsStringFormatInvocation(invocation) &&
                invocation.ArgumentList.Arguments.FirstOrDefault() == argument)
            {
                var formatArguments = invocation.ArgumentList.Arguments
                    .Skip(1)
                    .Select(item => item.Expression.ToString())
                    .ToImmutableArray();

                if (formatArguments.Length >= placeholderCount)
                    return formatArguments;
            }

            return Enumerable.Range(0, placeholderCount)
                .Select(index => SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal("{" + index.ToString(CultureInfo.InvariantCulture) + "}")).ToString())
                .ToImmutableArray();
        }

        private static SyntaxNode? GetLocalizableNode(SyntaxNode node)
        {
            var interpolatedString = node.FirstAncestorOrSelf<InterpolatedStringExpressionSyntax>();
            if (interpolatedString is not null)
                return interpolatedString;

            var literal = node.FirstAncestorOrSelf<LiteralExpressionSyntax>();
            return literal?.IsKind(SyntaxKind.StringLiteralExpression) == true
                ? literal
                : null;
        }

        private static LocalizableText? GetLocalizableText(SyntaxNode node)
        {
            if (node is LiteralExpressionSyntax literal)
                return new LocalizableText(literal.Token.ValueText, GetReplacementArguments(node, literal.Token.ValueText));

            if (node is not InterpolatedStringExpressionSyntax interpolatedString)
                return null;

            var builder = new System.Text.StringBuilder();
            var arguments = ImmutableArray.CreateBuilder<string>();

            foreach (var content in interpolatedString.Contents)
            {
                if (content is InterpolatedStringTextSyntax text)
                {
                    builder.Append(text.TextToken.ValueText);
                    continue;
                }

                if (content is not InterpolationSyntax interpolation)
                    continue;

                var index = arguments.Count;
                arguments.Add(interpolation.Expression.ToString());
                builder.Append('{').Append(index.ToString(CultureInfo.InvariantCulture));

                if (interpolation.AlignmentClause is not null)
                    builder.Append(interpolation.AlignmentClause.ToString());

                if (interpolation.FormatClause is not null)
                    builder.Append(interpolation.FormatClause.ToString());

                builder.Append('}');
            }

            return new LocalizableText(builder.ToString(), arguments.ToImmutable());
        }

        private static bool IsStringFormatInvocation(InvocationExpressionSyntax invocation)
        {
            return invocation.Expression switch
            {
                MemberAccessExpressionSyntax memberAccess
                    when memberAccess.Name.Identifier.ValueText == "Format" &&
                         memberAccess.Expression.ToString() is "string" or "String" or "System.String" => true,
                IdentifierNameSyntax identifier
                    when identifier.Identifier.ValueText == "Format" => true,
                _ => false
            };
        }

        private static string CreateInvocationText(string key, ImmutableArray<string> arguments)
        {
            var apiKey = key.EndsWith("_Format", StringComparison.OrdinalIgnoreCase)
                ? key.Substring(0, key.Length - "_Format".Length)
                : key;

            var parts = apiKey
                .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(SanitizeIdentifier)
                .ToArray();

            if (parts.Length == 0)
                return string.Empty;

            var methodName = parts[parts.Length - 1];
            var path = parts.Length == 1
                ? string.Empty
                : "." + string.Join(".", parts.Take(parts.Length - 1));
            var args = string.Join(", ", arguments);

            return $"L{path}.{methodName}({args})";
        }

        private static string SanitizeIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Key";

            var builder = new System.Text.StringBuilder();
            foreach (var ch in value)
                builder.Append(char.IsLetterOrDigit(ch) ? ch : '_');

            if (builder.Length == 0 || char.IsDigit(builder[0]))
                builder.Insert(0, '_');

            return builder.ToString();
        }

        private sealed class LocalizableText
        {
            public LocalizableText(string value, ImmutableArray<string> arguments)
            {
                Value = value;
                Arguments = arguments;
            }

            public string Value { get; }

            public ImmutableArray<string> Arguments { get; }
        }
    }
}
