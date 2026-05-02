/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 02 мая 2026 19:17:07
 * Version: 1.0.5
 */

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
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

        //public override Task RegisterCodeFixesAsync(CodeFixContext context)
        //{
        //    var title = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru"
        //        ? "Создать ключ локализации (AI)"
        //        : "Generate localization key (AI)";

        //    context.RegisterCodeFix(
        //        CodeAction.Create(
        //            title,
        //            ct => Task.FromResult(context.Document.Project.Solution),
        //            equivalenceKey: "AI_Localization"),
        //        context.Diagnostics);

        //    return Task.CompletedTask;
        //}

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Id != StringLiteralAnalyzer.DiagnosticId)
                    continue;

                context.RegisterCodeFix(
                    CodeAction.Create(
                        "AI TEST COMMAND",
                        async ct =>
                        {
                            var doc = context.Document;
                            var root = await doc.GetSyntaxRootAsync(ct);

                            SyntaxNode newRoot = default;

                            if(root != null)
                                newRoot = root.WithTrailingTrivia(
                                    root.GetTrailingTrivia().Add(SyntaxFactory.Whitespace(" ")));

                            return doc.WithSyntaxRoot(newRoot).Project.Solution;
                        },
                        equivalenceKey: "AI_TEST"),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private static string GetLocalizedTitle()
        {
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            switch(culture)
            {
                case "ru":
                    return  "Создать ключ локализации (AI)";
                    break;
                default:
                    return  "Generate localization key (AI)";
                    break;
            };
        }

        private async Task<Solution> CreateKeyWithAiAsync(Document document, TextSpan span, CancellationToken token)
        {
            var project = document.Project;
            var solution = project.Solution;

            var text = await GetStringLiteralAsync(document, span, token);
            if (string.IsNullOrWhiteSpace(text))
                return solution;

            // get context
            var semanticModel = await document.GetSemanticModelAsync(token);
            var root = await document.GetSyntaxRootAsync(token);
            var node = root?.FindNode(span);

            var method = node?.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            var classDecl = node?.FirstAncestorOrSelf<ClassDeclarationSyntax>();

            var className = classDecl?.Identifier.Text ?? "Global";
            var methodName = method?.Identifier.Text ?? "Method";
            return solution;
        }

        //private static async Task<Solution> CreateKeyWithAiAsync(
        // Document document,
        // TextSpan span,
        // string fallbackKey,
        // int argumentCount,
        // CancellationToken token)
        //{
        //    //// get AI Core
        //    //var ai = CreateAiService();

        //    //var aiResult = await ai.ProcessAsync(
        //    //    sourceText: text!,
        //    //    className: className,
        //    //    methodName: methodName,
        //    //    usageType: "Error" 
        //    //);

        //    //if (aiResult is null)
        //    //    return solution;

        //    //var resourceKey = argumentCount > 0 && aiResult.Key.EndsWith("_Format", StringComparison.OrdinalIgnoreCase) is false
        //    //    ? aiResult.Key + "_Format"
        //    //    : aiResult.Key;

        //    //var resxDocuments = project.AdditionalDocuments
        //    //    .Where(d => d.FilePath?.EndsWith(".resx", StringComparison.OrdinalIgnoreCase) == true)
        //    //    .ToArray();

        //    //var en = FindDocument(resxDocuments, ".en.resx");
        //    //var ru = FindDocument(resxDocuments, ".ru.resx");

        //    //if (en is not null)
        //    //    solution = await AddOrUpdateWithValueAsync(solution, en, resourceKey, aiResult.En, token);

        //    //if (ru is not null)
        //    //    solution = await AddOrUpdateWithValueAsync(solution, ru, resourceKey, aiResult.Ru, token);

        //    //// 👉 опционально: заменить строку на L.Class.Key()
        //    //solution = await ReplaceStringWithInvocationAsync(document, span, className, resourceKey, solution, token);

        //    return solution;
        //}

        private static async Task<Solution> AddOrUpdateWithValueAsync(
            Solution solution,
            TextDocument document,
            string key,
            string value,
            CancellationToken token)
        {
            var text = await document.GetTextAsync(token);
            var newXml = AddOrUpdateValue(text.ToString(), key, value);

            return solution.WithAdditionalDocumentText(
                document.Id,
                SourceText.From(newXml, text.Encoding ?? System.Text.Encoding.UTF8));
        }

        private static string AddOrUpdateValue(string xml, string key, string value)
        {
            var doc = string.IsNullOrWhiteSpace(xml)
                ? CreateDocument()
                : XDocument.Parse(xml, LoadOptions.PreserveWhitespace);

            var root = doc.Root;

            if(root == null)
                return doc.ToString();

            var existing = root.Elements("data")
                .FirstOrDefault(x => string.Equals(x.Attribute("name")?.Value, key, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                root.Add(new XElement("data",
                    new XAttribute("name", key),
                    new XAttribute(XNamespace.Xml + "space", "preserve"),
                    new XElement("value", value)));
            }
            else
            {
                existing.Element("value").Value = value;
            }

            return doc.ToString();
        }

        private static async Task<Solution> ReplaceStringWithInvocationAsync(
            Document document,
            TextSpan span,
            string className,
            string key,
            Solution solution,
            CancellationToken token)
        {
            var root = await document.GetSyntaxRootAsync(token);
            if(root == null) return null;

            var literal = root?.FindNode(span).FirstAncestorOrSelf<LiteralExpressionSyntax>();
            if (literal == null)
                return solution;

            var invocation = SyntaxFactory.ParseExpression($"L.{className}.{key}()");

            var newRoot = root.ReplaceNode(literal, invocation);
            var newDoc = document.WithSyntaxRoot(newRoot);

            return newDoc.Project.Solution;
        }

        private static async Task<string> GetStringLiteralAsync(Document document, TextSpan span, CancellationToken token)
        {
            var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);
            var node = root?.FindNode(span, getInnermostNodeForTie: true);

            var literal = node?.FirstAncestorOrSelf<LiteralExpressionSyntax>();
            if (literal == null)
                return null;

            return literal.Token.ValueText; // ← без кавычек
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
}
