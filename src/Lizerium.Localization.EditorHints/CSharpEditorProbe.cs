/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 19 июня 2026 06:52:42
 * Version: 1.0.64
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Utilities;

namespace Lizerium.Localization.EditorHints;

[Export(typeof(IWpfTextViewCreationListener))]
[ContentType("CSharp")]
[TextViewRole(PredefinedTextViewRoles.Document)]
internal sealed class CSharpEditorProbe : IWpfTextViewCreationListener
{
    [Import]
    internal ITextDocumentFactoryService TextDocumentFactoryService { get; set; } = null!;

    public void TextViewCreated(IWpfTextView textView)
    {
        try
        {
            _ = new CSharpLocalizationHintsView(textView, TextDocumentFactoryService);
        }
        catch (Exception ex)
        {
            Log("Failed to initialize hints. " + ex);
        }
    }

    internal static void Log(string message)
    {
        try
        {
            File.AppendAllText(
                Path.Combine(Path.GetTempPath(), "Lizerium.Localization.EditorHints.log"),
                DateTime.Now.ToString("O", CultureInfo.InvariantCulture) + " " + message + Environment.NewLine);
        }
        catch
        {
        }
    }
}

internal sealed class CSharpLocalizationHintsView
{
    private const int MaxHints = 20;

    private readonly IWpfTextView view;
    private readonly ITextDocumentFactoryService textDocumentFactoryService;
    private readonly IAdornmentLayer layer;
    private readonly object adornmentTag = new();
    private readonly DispatcherTimer keepAliveTimer;
    private LocalizationResourceCache? resourceCache;
    private bool redrawQueued;
    private bool isDrawing;
    private int drawCount;

    public CSharpLocalizationHintsView(IWpfTextView view, ITextDocumentFactoryService textDocumentFactoryService)
    {
        this.view = view;
        this.textDocumentFactoryService = textDocumentFactoryService;
        layer = view.GetAdornmentLayer(PredefinedAdornmentLayers.Selection);
        keepAliveTimer = new DispatcherTimer(
            DispatcherPriority.ApplicationIdle,
            view.VisualElement.Dispatcher)
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        keepAliveTimer.Tick += OnKeepAliveTimerTick;
        view.LayoutChanged += OnLayoutChanged;
        view.Closed += OnClosed;
        keepAliveTimer.Start();
        QueueDraw();
    }

    private void OnClosed(object sender, EventArgs e)
    {
        view.LayoutChanged -= OnLayoutChanged;
        view.Closed -= OnClosed;
        keepAliveTimer.Stop();
        keepAliveTimer.Tick -= OnKeepAliveTimerTick;
        layer.RemoveAdornmentsByTag(adornmentTag);
    }

    private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
    {
        QueueDraw();
    }

    private void OnKeepAliveTimerTick(object sender, EventArgs e)
    {
        QueueDraw();
    }

    private void QueueDraw()
    {
        if (redrawQueued || view.IsClosed)
        {
            return;
        }

        redrawQueued = true;
        view.VisualElement.Dispatcher.BeginInvoke(
            new Action(() =>
            {
                redrawQueued = false;
                Draw();
            }),
            DispatcherPriority.ApplicationIdle);
    }

    private void Draw()
    {
        if (isDrawing || view.IsClosed)
        {
            return;
        }

        try
        {
            isDrawing = true;
            layer.RemoveAdornmentsByTag(adornmentTag);

            var scanned = 0;
            var matched = 0;
            var added = 0;
            var visibleLines = GetVisibleLines().ToArray();
            var hintColumnLeft = CalculateHintColumnLeft(visibleLines);
            foreach (var line in visibleLines)
            {
                scanned++;
                var lineText = line.Extent.GetText();
                if (!TryCreateHint(line, hintColumnLeft, out var hint))
                {
                    continue;
                }

                matched++;
                if (AddHint(line, hint))
                {
                    added++;
                }

                if (added >= MaxHints)
                {
                    break;
                }
            }

            drawCount++;
            _ = scanned;
            _ = matched;
            _ = added;
        }
        catch (Exception ex)
        {
            CSharpEditorProbe.Log("Hints draw failed. " + ex.Message);
        }
        finally
        {
            isDrawing = false;
        }
    }

    private double CalculateHintColumnLeft(IReadOnlyCollection<ITextViewLine> visibleLines)
    {
        var maxRight = visibleLines.Count == 0
            ? view.ViewportLeft + 520
            : visibleLines.Max(line => line.Right);

        return Math.Max(maxRight + 32, view.ViewportLeft + 520);
    }

    private IEnumerable<ITextViewLine> GetVisibleLines()
    {
        var lines = view.TextViewLines;
        if (lines == null || !lines.IsValid)
        {
            return Enumerable.Empty<ITextViewLine>();
        }

        return lines.Where(line => line.VisibilityState != VisibilityState.Hidden);
    }

    private bool AddHint(ITextViewLine line, LocalizationHint hint)
    {
        var text = new TextBlock
        {
            Text = hint.DisplayText,
            FontSize = 12,
            FontStyle = FontStyles.Italic,
            Foreground = new SolidColorBrush(Color.FromArgb(155, 150, 150, 150)),
            IsHitTestVisible = false
        };

        Canvas.SetLeft(text, Math.Max(hint.Left, view.ViewportLeft + 520));
        Canvas.SetTop(text, line.Top + 1);

        return layer.AddAdornment(
            AdornmentPositioningBehavior.TextRelative,
            line.Extent,
            adornmentTag,
            text,
            null);
    }

    private bool TryCreateHint(ITextViewLine line, double hintColumnLeft, out LocalizationHint hint)
    {
        hint = default;

        var lineText = line.Extent.GetText();
        var start = lineText.IndexOf("L.", StringComparison.Ordinal);
        if (start < 0)
        {
            return false;
        }

        var pathStart = start + 2;
        var openParen = lineText.IndexOf('(', pathStart);
        if (openParen <= pathStart)
        {
            return false;
        }

        var path = lineText.Substring(pathStart, openParen - pathStart).Trim();
        if (path.Length == 0 || path.Any(c => !(char.IsLetterOrDigit(c) || c == '_' || c == '.')))
        {
            return false;
        }

        var key = path.Replace('.', '_');
        var display = ResolveDisplayText(key);
        hint = new LocalizationHint(
            display,
            hintColumnLeft);
        return true;
    }

    private string ResolveDisplayText(string key)
    {
        var cache = GetResourceCache();
        if (cache == null)
        {
            return key;
        }

        var value = cache.GetValue(key) ?? cache.GetValue(key + "_Format");
        if (string.IsNullOrWhiteSpace(value))
        {
            return key;
        }

        return cache.Language + ": " + Trim(value!, 42);
    }

    private LocalizationResourceCache? GetResourceCache()
    {
        if (resourceCache != null && !resourceCache.IsStale())
        {
            return resourceCache;
        }

        if (!textDocumentFactoryService.TryGetTextDocument(view.TextBuffer, out var document))
        {
            return null;
        }

        var directory = FindProjectDirectory(document.FilePath);
        if (directory == null)
        {
            return null;
        }

        var resourcesDirectory = Path.Combine(directory, "Resources", "Localization");
        resourceCache = LocalizationResourceCache.Load(resourcesDirectory);
        return resourceCache;
    }

    private static string? FindProjectDirectory(string filePath)
    {
        var directory = new FileInfo(filePath).Directory;
        while (directory != null)
        {
            if (Directory.EnumerateFiles(directory.FullName, "*.csproj", SearchOption.TopDirectoryOnly).Any())
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static string Trim(string value, int maxLength)
    {
        var normalized = value.Replace(Environment.NewLine, " ").Trim();
        return normalized.Length <= maxLength
            ? normalized
            : normalized.Substring(0, maxLength - 1) + "...";
    }
}

internal sealed class LocalizationResourceCache
{
    private readonly Dictionary<string, string> values;
    private readonly DateTime stampUtc;
    private readonly string resourcesDirectory;

    private LocalizationResourceCache(string resourcesDirectory, string language, DateTime stampUtc, Dictionary<string, string> values)
    {
        this.resourcesDirectory = resourcesDirectory;
        Language = language;
        this.stampUtc = stampUtc;
        this.values = values;
    }

    public string Language { get; }

    public string? GetValue(string key)
    {
        return values.TryGetValue(key, out var value) ? value : null;
    }

    public bool IsStale()
    {
        return GetStampUtc(resourcesDirectory) != stampUtc;
    }

    public static LocalizationResourceCache Load(string resourcesDirectory)
    {
        var requestedLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var effectiveLanguage = HasLanguageFile(resourcesDirectory, requestedLanguage)
            ? requestedLanguage
            : "en";

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        LoadFile(Path.Combine(resourcesDirectory, "Strings.en.resx"), values);
        if (!string.Equals(effectiveLanguage, "en", StringComparison.OrdinalIgnoreCase))
        {
            LoadFile(Path.Combine(resourcesDirectory, "Strings." + effectiveLanguage + ".resx"), values);
        }

        return new LocalizationResourceCache(resourcesDirectory, effectiveLanguage, GetStampUtc(resourcesDirectory), values);
    }

    private static bool HasLanguageFile(string resourcesDirectory, string language)
    {
        if (string.IsNullOrWhiteSpace(language) || string.Equals(language, "en", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return File.Exists(Path.Combine(resourcesDirectory, "Strings." + language + ".resx"));
    }

    private static void LoadFile(string path, Dictionary<string, string> values)
    {
        if (!File.Exists(path))
        {
            return;
        }

        var document = XDocument.Load(path);
        foreach (var item in document.Root?.Elements("data") ?? Enumerable.Empty<XElement>())
        {
            var name = item.Attribute("name")?.Value;
            var value = item.Element("value")?.Value;
            if (!string.IsNullOrWhiteSpace(name) && value != null)
            {
                values[name!] = value;
            }
        }
    }

    private static DateTime GetStampUtc(string resourcesDirectory)
    {
        if (!Directory.Exists(resourcesDirectory))
        {
            return DateTime.MinValue;
        }

        return Directory.EnumerateFiles(resourcesDirectory, "Strings*.resx", SearchOption.TopDirectoryOnly)
            .Select(File.GetLastWriteTimeUtc)
            .DefaultIfEmpty(DateTime.MinValue)
            .Max();
    }
}

internal readonly struct LocalizationHint
{
    public LocalizationHint(string displayText, double left)
    {
        DisplayText = displayText;
        Left = left;
    }

    public string DisplayText { get; }

    public double Left { get; }
}
