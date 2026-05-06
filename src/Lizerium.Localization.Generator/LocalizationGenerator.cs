/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 06 мая 2026 10:30:02
 * Version: 1.0.12
 */

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Lizerium.Localization.Generator;

/// <summary>
/// Generates a strongly typed localization API from RESX files supplied as additional files.
/// </summary>
[Generator]
public sealed partial class LocalizationGenerator : IIncrementalGenerator
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
}
