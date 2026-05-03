/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 03 мая 2026 06:52:43
 * Version: 1.0.6
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Lizerium.AI.LocalizationAssistant.Core.Clients.Ollama;
using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;
using Lizerium.AI.LocalizationAssistant.Core.Services;
using Lizerium.Localization.Core;

using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace Lizerium.Localization.Xaml.Vsix;

[Export(typeof(ISuggestedActionsSourceProvider))]
[Name(nameof(XamlLocalizationSuggestedActionProvider))]
[ContentType("XAML")]
internal sealed class XamlLocalizationSuggestedActionProvider : ISuggestedActionsSourceProvider
{
    [Import]
    internal ITextDocumentFactoryService TextDocumentFactoryService { get; set; } = null!;

    [Import]
    internal ITextStructureNavigatorSelectorService NavigatorSelectorService { get; set; } = null!;

    public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer)
    {
        return new XamlLocalizationSuggestedActionsSource(textView, textBuffer, TextDocumentFactoryService, NavigatorSelectorService);
    }
}

internal sealed class XamlLocalizationSuggestedActionsSource : ISuggestedActionsSource
{
    private static readonly Regex AttributeRegex = new(
        @"(?<name>[A-Za-z_][\w:.-]*)\s*=\s*""(?<value>[^""]*)""",
        RegexOptions.Compiled);

    private readonly ITextView _textView;
    private readonly ITextBuffer _textBuffer;
    private readonly ITextDocumentFactoryService _textDocumentFactoryService;
    private readonly ITextStructureNavigatorSelectorService _navigatorSelectorService;

    public XamlLocalizationSuggestedActionsSource(
        ITextView textView,
        ITextBuffer textBuffer,
        ITextDocumentFactoryService textDocumentFactoryService,
        ITextStructureNavigatorSelectorService navigatorSelectorService)
    {
        _textView = textView;
        _textBuffer = textBuffer;
        _textDocumentFactoryService = textDocumentFactoryService;
        _navigatorSelectorService = navigatorSelectorService;
    }

#pragma warning disable CS0067
    public event EventHandler<EventArgs>? SuggestedActionsChanged;
#pragma warning restore CS0067

    public void Dispose()
    {
    }

    public Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
    {
        return Task.FromResult(TryCreateCandidate(range, out _));
    }

    public IEnumerable<SuggestedActionSet> GetSuggestedActions(
        ISuggestedActionCategorySet requestedActionCategories,
        SnapshotSpan range,
        CancellationToken cancellationToken)
    {
        if (!TryCreateCandidate(range, out var candidate))
            return Enumerable.Empty<SuggestedActionSet>();

        var action = new XamlLocalizationSuggestedAction(_textBuffer, candidate);
        return new[]
        {
            new SuggestedActionSet(
                PredefinedSuggestedActionCategoryNames.Refactoring,
                new ISuggestedAction[] { action },
                title: null,
                priority: SuggestedActionSetPriority.Medium)
        };
    }

    public bool TryGetTelemetryId(out Guid telemetryId)
    {
        telemetryId = new Guid("D1E38CF2-D4CC-4C48-A307-43D8826145D1");
        return true;
    }

    private bool TryCreateCandidate(SnapshotSpan range, out XamlLocalizationCandidate candidate)
    {
        candidate = default!;

        if (!_textDocumentFactoryService.TryGetTextDocument(_textBuffer, out var textDocument))
            return false;

        var xamlPath = textDocument.FilePath;
        if (string.IsNullOrWhiteSpace(xamlPath) || !xamlPath.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
            return false;

        var snapshot = _textBuffer.CurrentSnapshot;
        var selection = GetSelection(snapshot);
        if (selection.HasValue && !string.IsNullOrWhiteSpace(selection.Value.GetText()))
            return TryCreateSelectionCandidate(xamlPath, snapshot, selection.Value, out candidate);

        var point = range.Start.Position;
        return TryCreateExistingLocCandidate(xamlPath, snapshot, point, out candidate)
            || TryCreateAttributeCandidate(xamlPath, snapshot, point, out candidate)
            || TryCreateTextNodeCandidate(xamlPath, snapshot, point, out candidate);
    }

    private SnapshotSpan? GetSelection(ITextSnapshot snapshot)
    {
        if (_textView.Selection.IsEmpty)
            return null;

        var selectedSpans = _textView.Selection.SelectedSpans;
        if (selectedSpans.Count == 0)
            return null;

        var span = selectedSpans[0].TranslateTo(snapshot, SpanTrackingMode.EdgeInclusive);
        return span.Length > 0 ? span : null;
    }

    private bool TryCreateSelectionCandidate(string xamlPath, ITextSnapshot snapshot, SnapshotSpan selection, out XamlLocalizationCandidate candidate)
    {
        candidate = default!;
        var selectedText = selection.GetText().Trim();
        if (!IsLocalizableValue(selectedText))
            return false;

        var point = selection.Start.Position;
        var attributeCandidate = TryCreateAttributeCandidate(xamlPath, snapshot, point, out var attribute)
            ? attribute
            : null;

        var element = attributeCandidate?.ElementName ?? FindElementName(snapshot, point) ?? Path.GetFileNameWithoutExtension(xamlPath);
        var property = attributeCandidate?.PropertyName ?? "Text";
        var key = XamlLocalizationService.CreateKey(xamlPath, element, property);
        key = EnsureKeyHasTextHint(key, selectedText);

        candidate = new XamlLocalizationCandidate(
            xamlPath,
            selection.Span,
            selectedText,
            key,
            element,
            property,
            isTextNode: false,
            startTagInsertPosition: null);

        return true;
    }

    private bool TryCreateAttributeCandidate(string xamlPath, ITextSnapshot snapshot, int point, out XamlLocalizationCandidate candidate)
    {
        candidate = default!;
        var line = snapshot.GetLineFromPosition(point);
        var lineText = line.GetText();
        var lineStart = line.Start.Position;

        foreach (Match match in AttributeRegex.Matches(lineText))
        {
            var valueGroup = match.Groups["value"];
            var valueStart = lineStart + valueGroup.Index;
            var valueEnd = valueStart + valueGroup.Length;
            if (point < valueStart || point > valueEnd)
                continue;

            var value = valueGroup.Value;
            if (!IsLocalizableValue(value))
                return false;

            var propertyName = match.Groups["name"].Value;
            if (!IsLocalizableAttribute(propertyName, value))
                return false;

            var elementName = FindElementName(snapshot, point) ?? Path.GetFileNameWithoutExtension(xamlPath);
            var key = EnsureKeyHasTextHint(XamlLocalizationService.CreateKey(xamlPath, elementName, propertyName), value);

            candidate = new XamlLocalizationCandidate(
                xamlPath,
                new Span(valueStart, valueGroup.Length),
                value,
                key,
                elementName,
                propertyName,
                isTextNode: false,
                startTagInsertPosition: null);

            return true;
        }

        return false;
    }

    private bool TryCreateExistingLocCandidate(string xamlPath, ITextSnapshot snapshot, int point, out XamlLocalizationCandidate candidate)
    {
        candidate = default!;
        var line = snapshot.GetLineFromPosition(point);
        var lineText = line.GetText();
        var lineStart = line.Start.Position;

        foreach (Match match in AttributeRegex.Matches(lineText))
        {
            var valueGroup = match.Groups["value"];
            var valueStart = lineStart + valueGroup.Index;
            var valueEnd = valueStart + valueGroup.Length;
            if (point < valueStart || point > valueEnd)
                continue;

            var locMatch = Regex.Match(valueGroup.Value, @"^\{loc:Loc\s+(?<key>[^}]+)\}$");
            if (!locMatch.Success)
                return false;

            var propertyName = match.Groups["name"].Value;
            var key = locMatch.Groups["key"].Value.Trim();
            var elementName = FindElementName(snapshot, point) ?? Path.GetFileNameWithoutExtension(xamlPath);

            candidate = new XamlLocalizationCandidate(
                xamlPath,
                new Span(valueStart, valueGroup.Length),
                InferValueFromKey(key),
                key,
                elementName,
                propertyName,
                isTextNode: false,
                startTagInsertPosition: null);

            return true;
        }

        return false;
    }

    private bool TryCreateTextNodeCandidate(string xamlPath, ITextSnapshot snapshot, int point, out XamlLocalizationCandidate candidate)
    {
        candidate = default!;
        var navigator = _navigatorSelectorService.GetTextStructureNavigator(_textBuffer);
        var extent = navigator.GetExtentOfWord(new SnapshotPoint(snapshot, point));
        var text = extent.Span.GetText().Trim();
        if (!IsLocalizableValue(text))
            return false;

        var elementName = FindElementName(snapshot, point) ?? Path.GetFileNameWithoutExtension(xamlPath);
        var propertyName = elementName is "TextBlock" or "TextBox" or "Run" ? "Text" : "Content";
        var key = EnsureKeyHasTextHint(XamlLocalizationService.CreateKey(xamlPath, elementName, propertyName), text);
        var startTagInsertPosition = FindStartTagInsertPosition(snapshot, point);

        if (startTagInsertPosition is null)
            return false;

        candidate = new XamlLocalizationCandidate(
            xamlPath,
            extent.Span.Span,
            text,
            key,
            elementName,
            propertyName,
            isTextNode: true,
            startTagInsertPosition: startTagInsertPosition);

        return true;
    }

    private static string? FindElementName(ITextSnapshot snapshot, int point)
    {
        var before = snapshot.GetText(0, Math.Min(point, snapshot.Length));
        var tagStart = before.LastIndexOf('<');
        if (tagStart < 0 || tagStart + 1 >= snapshot.Length)
            return null;

        var rest = snapshot.GetText(tagStart, Math.Min(snapshot.Length - tagStart, 300));
        var match = Regex.Match(rest, @"<(?<name>[A-Za-z_][\w:.-]*)");
        if (!match.Success)
            return null;

        var named = Regex.Match(rest, @"(?:x:Name|Name)\s*=\s*""(?<name>[^""]+)""");
        return named.Success ? named.Groups["name"].Value : match.Groups["name"].Value;
    }

    private static int? FindStartTagInsertPosition(ITextSnapshot snapshot, int point)
    {
        var before = snapshot.GetText(0, Math.Min(point, snapshot.Length));
        var tagStart = before.LastIndexOf('<');
        if (tagStart < 0)
            return null;

        var after = snapshot.GetText(tagStart, Math.Min(snapshot.Length - tagStart, 500));
        var close = after.IndexOf('>');
        if (close < 0)
            return null;

        var absoluteClose = tagStart + close;
        return after[Math.Max(0, close - 1)] == '/' ? absoluteClose - 1 : absoluteClose;
    }

    private static bool IsLocalizableAttribute(string name, string value)
    {
        if (name.StartsWith("xmlns", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("x:", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("Grid.", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("Canvas.", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("DockPanel.", StringComparison.OrdinalIgnoreCase))
            return false;

        if (name is "Name" or "Click" or "Command" or "CommandParameter" or "Style" or "Template")
            return false;

        if (value.StartsWith("{", StringComparison.Ordinal) || value.Contains("\\"))
            return false;

        return true;
    }

    private static bool IsLocalizableValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (value.StartsWith("{", StringComparison.Ordinal))
            return false;

        return value.Any(char.IsLetter);
    }

    private static string EnsureKeyHasTextHint(string key, string text)
    {
        var hint = new string(text.Where(char.IsLetterOrDigit).Take(24).ToArray());
        return string.IsNullOrWhiteSpace(hint)
            ? key
            : key + "_" + hint;
    }

    private static string InferValueFromKey(string key)
    {
        var last = key.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        return string.IsNullOrWhiteSpace(last) ? "TODO" : last;
    }
}

internal sealed class XamlLocalizationSuggestedAction : ISuggestedAction
{
    private readonly ITextBuffer _textBuffer;
    private readonly XamlLocalizationCandidate _candidate;

    public XamlLocalizationSuggestedAction(ITextBuffer textBuffer, XamlLocalizationCandidate candidate)
    {
        _textBuffer = textBuffer;
        _candidate = candidate;
    }

    public string DisplayText => "Create XAML localization key";

    public ImageMoniker IconMoniker => default;

    public string IconAutomationText => DisplayText;

    public string InputGestureText => string.Empty;

    public bool HasActionSets => false;

    public bool HasPreview => true;

    public void Dispose()
    {
    }

    public Task<IEnumerable<SuggestedActionSet>?> GetActionSetsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IEnumerable<SuggestedActionSet>?>(Enumerable.Empty<SuggestedActionSet>());
    }

    public Task<object?> GetPreviewAsync(CancellationToken cancellationToken)
    {
        var preview = _candidate.IsTextNode
            ? $"<{_candidate.ElementName} {_candidate.PropertyName}=\"{{loc:Loc {_candidate.Key}}}\">"
            : $"{_candidate.PropertyName}=\"{{loc:Loc {_candidate.Key}}}\"";

        return Task.FromResult<object?>(preview);
    }

    public void Invoke(CancellationToken cancellationToken)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteResources(_candidate, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            var snapshot = _textBuffer.CurrentSnapshot;
            var replacement = "{loc:Loc " + _candidate.Key + "}";
            using var edit = _textBuffer.CreateEdit();

            EnsureLocNamespace(snapshot, edit);

            if (_candidate.IsTextNode && _candidate.StartTagInsertPosition.HasValue)
            {
                edit.Insert(_candidate.StartTagInsertPosition.Value, $" {_candidate.PropertyName}=\"{replacement}\"");
                edit.Delete(_candidate.ValueSpan);
            }
            else
            {
                edit.Replace(_candidate.ValueSpan, replacement);
            }

            edit.Apply();
            Log("Localized XAML value. File=" + _candidate.XamlPath + "; Key=" + _candidate.Key);
        }
        catch (Exception ex)
        {
            Log("Failed to localize XAML value. " + ex);
            throw;
        }
    }

    public bool TryGetTelemetryId(out Guid telemetryId)
    {
        telemetryId = new Guid("6635C760-282D-46DF-AC86-E8F57B54BFB2");
        return true;
    }

    private static void EnsureLocNamespace(ITextSnapshot snapshot, ITextEdit edit)
    {
        var text = snapshot.GetText();
        const string locNamespace = "https://schemas.lizerium.dev/localization";
        const string legacyLocNamespace = "clr-namespace:Lizerium.Localization.Core;assembly=Lizerium.Localization.Core";
        if (text.Contains(locNamespace))
            return;

        var legacyIndex = text.IndexOf(legacyLocNamespace, StringComparison.Ordinal);
        if (legacyIndex >= 0)
        {
            edit.Replace(new Span(legacyIndex, legacyLocNamespace.Length), locNamespace);
            return;
        }

        var rootMatch = Regex.Match(
            text,
            @"<(?<name>Window|UserControl|Page|ResourceDictionary)\b(?<attrs>[^>]*?)>",
            RegexOptions.Singleline);

        if (!rootMatch.Success)
            return;

        var openingTag = rootMatch.Value;
        if (openingTag.IndexOf("xmlns:loc=", StringComparison.Ordinal) >= 0)
            return;

        var indent = Environment.NewLine + "        ";
        var updatedTag = openingTag.Insert(
            openingTag.Length - 1,
            indent + "xmlns:loc=\"" + locNamespace + "\"");

        edit.Replace(new Span(rootMatch.Index, rootMatch.Length), updatedTag);
    }

    private static void WriteResources(XamlLocalizationCandidate candidate, CancellationToken cancellationToken)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var resourcesDirectory = FindResourcesDirectory(candidate.XamlPath);
        Directory.CreateDirectory(resourcesDirectory);
        var localization = GetLocalization(candidate.Value, cancellationToken);

        var writer = new ResxWriter();
        writer.AddOrUpdate(Path.Combine(resourcesDirectory, "Strings.en.resx"), candidate.Key, localization.En);
        writer.AddOrUpdate(Path.Combine(resourcesDirectory, "Strings.ru.resx"), candidate.Key, localization.Ru);
        Log("Updated resources. Directory=" + resourcesDirectory + "; Key=" + candidate.Key);
    }

    private static LocalizationResult GetLocalization(string sourceText, CancellationToken cancellationToken)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        cancellationToken.ThrowIfCancellationRequested();
        var options = ReadOptions();
        if (!options.UseAiTranslations)
        {
            Log("AI localization disabled in Visual Studio options.");
            return CreateFallbackLocalization(sourceText, options);
        }

        try
        {
            Log("AI localization starting. Url=" + options.OllamaBaseUrl + "; Model=" + options.OllamaModel + "; Endpoint=" + options.OllamaGenerateEndpoint + "; Libre=" + options.LibreTranslateUrl + "; TimeoutSeconds=" + options.RequestTimeoutSeconds + "; Source=" + sourceText);
            var ollama = new OllamaClient(options.OllamaBaseUrl);
            var service = new AILocalizationService(
                ollama,
                new PromtConfig
                {
                    Model = options.OllamaModel,
                    GenerateEndpoint = options.OllamaGenerateEndpoint,
                    LibreUrl = options.LibreTranslateUrl,
                    RequestTimeoutSeconds = options.RequestTimeoutSeconds
                });
#pragma warning disable VSTHRD002
            var result = RunWithTimeout(service, sourceText, options.RequestTimeoutSeconds + 5, cancellationToken);
#pragma warning restore VSTHRD002

            if (result is not null)
            {
                if (string.IsNullOrWhiteSpace(result.En))
                    result.En = sourceText;

                if (string.IsNullOrWhiteSpace(result.Ru))
                    result.Ru = sourceText;

                Log("AI localization completed. Url=" + options.OllamaBaseUrl + "; Model=" + options.OllamaModel + "; Source=" + sourceText + "; En=" + result.En + "; Ru=" + result.Ru);
                return result;
            }
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            Log("AI localization timed out after " + options.RequestTimeoutSeconds + " seconds, falling back to source text. " + ex.Message);
        }
        catch (OperationCanceledException)
        {
            Log("XAML localization action canceled.");
            throw;
        }
        catch (Exception ex)
        {
            Log("AI localization failed, falling back to source text. " + ex.Message);
        }

        return CreateFallbackLocalization(sourceText, options);
    }

    private static LocalizationResult? RunWithTimeout(
        AILocalizationService service,
        string sourceText,
        int timeoutSeconds,
        CancellationToken cancellationToken)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);

        var aiTask = Task.Run(
            async () => await service.ProcessAsync(sourceText, linked.Token).ConfigureAwait(false),
            linked.Token);
        var delayTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds), linked.Token);
#pragma warning disable VSTHRD002
        var completed = Task.WhenAny(aiTask, delayTask).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002

        cancellationToken.ThrowIfCancellationRequested();

        if (completed != aiTask)
            throw new OperationCanceledException("AI localization request timed out.");

        linked.Cancel();
#pragma warning disable VSTHRD002
        return aiTask.GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
    }

    private static VsixLocalizationOptions ReadOptions()
    {
        try
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var shell = Package.GetGlobalService(typeof(SVsShell)) as IVsShell;
            if (shell is null)
                return VsixLocalizationOptions.FromEnvironment();

            var packageGuid = new Guid(LizeriumLocalizationPackage.PackageGuidString);
            if (shell.LoadPackage(ref packageGuid, out var package) < 0 || package is not Package visualStudioPackage)
                return VsixLocalizationOptions.FromEnvironment();

            var page = (LizeriumLocalizationOptionsPage)visualStudioPackage.GetDialogPage(typeof(LizeriumLocalizationOptionsPage));
            return VsixLocalizationOptions.FromPage(page);
        }
        catch (Exception ex)
        {
            Log("Failed to read Visual Studio options, using environment/default settings. " + ex.Message);
            return VsixLocalizationOptions.FromEnvironment();
        }
    }

    private static LocalizationResult CreateFallbackLocalization(string sourceText, VsixLocalizationOptions options)
    {
        if (!options.FallbackToSourceText)
        {
            return new LocalizationResult
            {
                En = "TODO",
                Ru = "TODO"
            };
        }

        return new LocalizationResult
        {
            En = sourceText,
            Ru = sourceText
        };
    }

    private static string FindResourcesDirectory(string xamlPath)
    {
        var directory = new FileInfo(xamlPath).Directory;
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "Resources", "Localization");
            if (Directory.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        return Path.Combine(new FileInfo(xamlPath).DirectoryName ?? Environment.CurrentDirectory, "Resources", "Localization");
    }

    private static void Log(string message)
    {
        try
        {
            File.AppendAllText(
                Path.Combine(Path.GetTempPath(), "Lizerium.Localization.Xaml.Vsix.log"),
                DateTime.Now.ToString("O", CultureInfo.InvariantCulture) + " " + message + Environment.NewLine);
        }
        catch
        {
        }
    }
}

internal sealed class VsixLocalizationOptions
{
    public bool UseAiTranslations { get; private set; } = true;

    public string OllamaBaseUrl { get; private set; } = "http://localhost:11434";

    public string OllamaModel { get; private set; } = "qwen2.5:7b";

    public string OllamaGenerateEndpoint { get; private set; } = "/api/generate";

    public string LibreTranslateUrl { get; private set; } = "http://localhost:5000";

    public int RequestTimeoutSeconds { get; private set; } = 30;

    public bool FallbackToSourceText { get; private set; } = true;

    public static VsixLocalizationOptions FromPage(LizeriumLocalizationOptionsPage page)
    {
        var options = FromEnvironment();
        options.UseAiTranslations = page.UseAiTranslations;
        options.OllamaBaseUrl = Normalize(page.OllamaBaseUrl, options.OllamaBaseUrl);
        options.OllamaModel = Normalize(page.OllamaModel, options.OllamaModel);
        options.OllamaGenerateEndpoint = Normalize(page.OllamaGenerateEndpoint, options.OllamaGenerateEndpoint);
        options.LibreTranslateUrl = NormalizeOptional(page.LibreTranslateUrl);
        options.RequestTimeoutSeconds = NormalizePositiveInt(page.RequestTimeoutSeconds, options.RequestTimeoutSeconds);
        options.FallbackToSourceText = page.FallbackToSourceText;
        return options;
    }

    public static VsixLocalizationOptions FromEnvironment()
    {
        var options = new VsixLocalizationOptions();
        options.OllamaBaseUrl = Normalize(Environment.GetEnvironmentVariable("LIZERIUM_OLLAMA_URL"), options.OllamaBaseUrl);
        options.OllamaModel = Normalize(Environment.GetEnvironmentVariable("LIZERIUM_OLLAMA_MODEL"), options.OllamaModel);
        options.OllamaGenerateEndpoint = Normalize(Environment.GetEnvironmentVariable("LIZERIUM_OLLAMA_GENERATE_ENDPOINT"), options.OllamaGenerateEndpoint);
        options.LibreTranslateUrl = Normalize(Environment.GetEnvironmentVariable("LIZERIUM_LIBRETRANSLATE_URL"), options.LibreTranslateUrl);
        options.RequestTimeoutSeconds = NormalizePositiveInt(Environment.GetEnvironmentVariable("LIZERIUM_AI_TIMEOUT_SECONDS"), options.RequestTimeoutSeconds);
        return options;
    }

    private static string Normalize(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value!.Trim();
    }

    private static string NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value!.Trim();
    }

    private static int NormalizePositiveInt(string? value, int fallback)
    {
        return int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
    }

    private static int NormalizePositiveInt(int value, int fallback)
    {
        return value > 0 ? value : fallback;
    }
}

internal sealed class XamlLocalizationCandidate
{
    public XamlLocalizationCandidate(
        string xamlPath,
        Span valueSpan,
        string value,
        string key,
        string elementName,
        string propertyName,
        bool isTextNode,
        int? startTagInsertPosition)
    {
        XamlPath = xamlPath;
        ValueSpan = valueSpan;
        Value = value;
        Key = key;
        ElementName = elementName;
        PropertyName = propertyName;
        IsTextNode = isTextNode;
        StartTagInsertPosition = startTagInsertPosition;
    }

    public string XamlPath { get; }

    public Span ValueSpan { get; }

    public string Value { get; }

    public string Key { get; }

    public string ElementName { get; }

    public string PropertyName { get; }

    public bool IsTextNode { get; }

    public int? StartTagInsertPosition { get; }
}
