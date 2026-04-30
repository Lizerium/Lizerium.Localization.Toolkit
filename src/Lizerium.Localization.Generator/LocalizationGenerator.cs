/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 30 апреля 2026 09:20:05
 * Version: 1.0.3
 */

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Lizerium.Localization.Generator;

/// <summary>
/// Generates a strongly typed localization API from RESX files supplied as additional files.
/// </summary>
[Generator]
public sealed class LocalizationGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor MissingKeyRule = new(
        "LOC001",
        "Localization key is missing in a language",
        "Localization key '{0}' is missing in {1}",
        "Localization",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ParamMismatchRule = new(
        "LOC002",
        "Localization placeholder count mismatch",
        "Localization key '{0}' has different placeholder counts: ru={1}, en={2}",
        "Localization",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Registers the incremental pipeline that reads RESX files and emits generated source.
    /// </summary>
    /// <param name="context">Generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var resxFiles = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".resx", StringComparison.OrdinalIgnoreCase))
            .Select((file, token) => ResxFile.Parse(file, token))
            .Collect();

        context.RegisterSourceOutput(resxFiles, Execute);
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<ResxFile> files)
    {
        // Only the currently supported languages participate in generated API and consistency diagnostics.
        var languageFiles = files
            .Where(file => file.Language is "ru" or "en")
            .GroupBy(file => file.Language, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.SelectMany(file => file.Entries).ToDictionary(item => item.Key, item => item, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

        languageFiles.TryGetValue("ru", out var ru);
        languageFiles.TryGetValue("en", out var en);
        ru ??= new Dictionary<string, LocalizationEntry>(StringComparer.OrdinalIgnoreCase);
        en ??= new Dictionary<string, LocalizationEntry>(StringComparer.OrdinalIgnoreCase);

        var keys = ru.Keys.Concat(en.Keys).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(key => key, StringComparer.OrdinalIgnoreCase).ToArray();

        // Keep missing translations and placeholder mismatches visible without stopping compilation.
        foreach (var key in keys)
        {
            var hasRu = ru.TryGetValue(key, out var ruEntry);
            var hasEn = en.TryGetValue(key, out var enEntry);

            if (!hasRu)
                context.ReportDiagnostic(Diagnostic.Create(MissingKeyRule, Location.None, key, "ru"));
            if (!hasEn)
                context.ReportDiagnostic(Diagnostic.Create(MissingKeyRule, Location.None, key, "en"));
            if (hasRu && hasEn && ruEntry!.ParamCount != enEntry!.ParamCount)
                context.ReportDiagnostic(Diagnostic.Create(ParamMismatchRule, Location.None, key, ruEntry.ParamCount, enEntry.ParamCount));
        }

        context.AddSource("Localization.g.cs", SourceText.From(CodeBuilder.Build(keys.Select(key =>
        {
            // English is preferred for placeholder metadata, with Russian as a fallback for one-sided keys.
            var entry = en.TryGetValue(key, out var enEntry) ? enEntry : ru[key];
            return ApiEntry.From(entry);
        })), Encoding.UTF8));
    }

    private sealed record ResxFile(string Language, ImmutableArray<LocalizationEntry> Entries)
    {
        /// <summary>
        /// Parses a RESX additional file into generator-friendly entries.
        /// </summary>
        /// <param name="file">The additional file to parse.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>The parsed RESX file representation.</returns>
        public static ResxFile Parse(AdditionalText file, CancellationToken token)
        {
            var language = GetLanguage(file.Path);
            var text = file.GetText(token)?.ToString();
            if (string.IsNullOrWhiteSpace(text))
                return new ResxFile(language, ImmutableArray<LocalizationEntry>.Empty);

            try
            {
                var document = XDocument.Parse(text);
                var entries = document.Root?
                    .Elements("data")
                    .Select(item =>
                    {
                        var value = item.Element("value")?.Value ?? string.Empty;
                        return new LocalizationEntry(
                            item.Attribute("name")?.Value ?? string.Empty,
                            value,
                            PlaceholderCounter.Count(value));
                    })
                    .Where(item => !string.IsNullOrWhiteSpace(item.Key))
                    .ToImmutableArray() ?? ImmutableArray<LocalizationEntry>.Empty;

                return new ResxFile(language, entries);
            }
            catch
            {
                // Invalid RESX markup is handled by MSBuild/IDE diagnostics; the generator stays non-fatal.
                return new ResxFile(language, ImmutableArray<LocalizationEntry>.Empty);
            }
        }

        private static string GetLanguage(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            var dot = name.LastIndexOf('.');
            return dot >= 0 ? name.Substring(dot + 1).ToLowerInvariant() : string.Empty;
        }
    }

    private sealed record LocalizationEntry(string Key, string Value, int ParamCount);

    private sealed record ApiEntry(string Key, ImmutableArray<string> Path, string MethodName, int ParamCount, bool IsFormat)
    {
        /// <summary>
        /// Converts a localization entry into a generated API method description.
        /// </summary>
        /// <param name="entry">Localization entry read from RESX.</param>
        /// <returns>API shape used by the code builder.</returns>
        public static ApiEntry From(LocalizationEntry entry)
        {
            var key = entry.Key;
            var isFormat = key.EndsWith("_Format", StringComparison.OrdinalIgnoreCase);
            var parts = key.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (isFormat && parts.Length > 1)
                parts = parts.Take(parts.Length - 1).ToArray();

            // Every key segment becomes either a nested class name or the final method name.
            var method = Sanitize(parts.Length == 0 ? key : parts[parts.Length - 1]);
            var path = parts.Length <= 1
                ? ImmutableArray<string>.Empty
                : parts.Take(parts.Length - 1).Select(Sanitize).ToImmutableArray();

            return new ApiEntry(key, path, method, entry.ParamCount, isFormat || entry.ParamCount > 0);
        }

        private static string Sanitize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Key";

            var builder = new StringBuilder();
            foreach (var ch in value)
                builder.Append(char.IsLetterOrDigit(ch) ? ch : '_');

            if (builder.Length == 0 || char.IsDigit(builder[0]))
                builder.Insert(0, '_');

            return builder.ToString();
        }
    }

    private static class PlaceholderCounter
    {
        private static readonly Regex Regex = new(@"(?<!\{)\{(\d+)(?:[^}]*)\}(?!\})", RegexOptions.Compiled);

        /// <summary>
        /// Counts required method parameters from numbered placeholders.
        /// </summary>
        /// <param name="value">Localized value.</param>
        /// <returns>Required generated method argument count.</returns>
        public static int Count(string value)
        {
            var max = -1;
            foreach (Match match in Regex.Matches(value ?? string.Empty))
            {
                if (int.TryParse(match.Groups[1].Value, out var index) && index > max)
                    max = index;
            }

            return max + 1;
        }
    }

    private static class CodeBuilder
    {
        /// <summary>
        /// Builds the generated C# source for all localization API entries.
        /// </summary>
        /// <param name="entries">API entries derived from RESX keys.</param>
        /// <returns>Generated C# source text.</returns>
        public static string Build(IEnumerable<ApiEntry> entries)
        {
            var root = Node.Create("Localization");
            foreach (var entry in entries)
                root.Add(entry);

            var builder = new StringBuilder();
            builder.AppendLine("// <auto-generated />");
            builder.AppendLine("#nullable enable");
            builder.AppendLine("namespace Generated.Localization");
            builder.AppendLine("{");
            root.Write(builder, 1, isRoot: true);
            builder.AppendLine("}");
            return builder.ToString();
        }
    }

    private sealed class Node
    {
        private readonly SortedDictionary<string, Node> _children = new(StringComparer.Ordinal);
        private readonly List<ApiEntry> _entries = new();

        private Node(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public static Node Create(string name) => new(name);

        public void Add(ApiEntry entry)
        {
            var node = this;
            foreach (var part in entry.Path)
            {
                if (!node._children.TryGetValue(part, out var child))
                {
                    child = new Node(part);
                    node._children.Add(part, child);
                }

                node = child;
            }

            node._entries.Add(entry);
        }

        /// <summary>
        /// Writes this node as a static class and emits all child nodes and methods.
        /// </summary>
        /// <param name="builder">Destination source builder.</param>
        /// <param name="indent">Indentation level.</param>
        /// <param name="isRoot">Whether the node is the root localization class.</param>
        public void Write(StringBuilder builder, int indent, bool isRoot = false)
        {
            AppendIndent(builder, indent);
            builder.Append("public static class ").Append(Name).AppendLine();
            AppendIndent(builder, indent);
            builder.AppendLine("{");

            foreach (var child in _children.Values)
                child.Write(builder, indent + 1);

            foreach (var group in _entries.GroupBy(item => item.MethodName))
            {
                var index = 0;
                // If sanitized keys collide, keep the first method name and suffix following overload names.
                foreach (var entry in group)
                    WriteMethod(builder, indent + 1, entry, index++ == 0 ? entry.MethodName : entry.MethodName + index);
            }

            AppendIndent(builder, indent);
            builder.AppendLine("}");
        }

        private static void WriteMethod(StringBuilder builder, int indent, ApiEntry entry, string methodName)
        {
            var parameters = Enumerable.Range(0, entry.ParamCount).Select(i => "object arg" + i).ToArray();
            AppendIndent(builder, indent);
            builder.Append("public static string ").Append(methodName).Append('(').Append(string.Join(", ", parameters)).AppendLine(")");
            AppendIndent(builder, indent + 1);
            if (entry.ParamCount == 0)
            {
                builder.Append("=> global::Lizerium.Localization.Core.LocalizationService.Instance.GetString(");
                AppendLiteral(builder, entry.Key);
                builder.AppendLine(");");
                return;
            }

            builder.Append("=> global::Lizerium.Localization.Core.LocalizationService.Instance.Format(");
            AppendLiteral(builder, entry.Key);
            foreach (var parameter in parameters.Select(p => p.Split(' ')[1]))
                builder.Append(", ").Append(parameter);
            builder.AppendLine(");");
        }

        private static void AppendLiteral(StringBuilder builder, string value)
        {
            builder.Append("@\"").Append(value.Replace("\"", "\"\"")).Append('"');
        }

        private static void AppendIndent(StringBuilder builder, int indent)
        {
            builder.Append(' ', indent * 4);
        }
    }
}
