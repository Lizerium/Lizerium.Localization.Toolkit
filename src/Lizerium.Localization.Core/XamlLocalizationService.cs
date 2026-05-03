/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 03 мая 2026 06:52:43
 * Version: 1.0.6
 */

using System.IO;
using System.Xml.Linq;

namespace Lizerium.Localization.Core;

/// <summary>
/// Converts literal WPF XAML text values to <see cref="LocExtension"/> markup extension references
/// and creates matching RESX entries.
/// </summary>
public sealed class XamlLocalizationService
{
    private const string LocNamespace = "https://schemas.lizerium.dev/localization";
    private readonly ResxWriter _resxWriter = new();

    /// <summary>
    /// Replaces matching XAML text with a <c>{loc:Loc Key}</c> markup extension and writes RESX entries.
    /// </summary>
    /// <param name="xamlPath">Path to the XAML file to update.</param>
    /// <param name="text">Exact text selected by the user or discovered by tooling.</param>
    /// <param name="key">Localization key to write.</param>
    /// <param name="resourcesDirectory">Directory containing <c>Strings.en.resx</c> and <c>Strings.ru.resx</c>.</param>
    /// <param name="englishValue">English value. When omitted, <paramref name="text"/> is used.</param>
    /// <param name="russianValue">Russian value. When omitted, <paramref name="text"/> is used.</param>
    /// <param name="baseName">RESX base file name.</param>
    /// <returns>The number of XAML values replaced.</returns>
    public int LocalizeText(
        string xamlPath,
        string text,
        string key,
        string resourcesDirectory,
        string? englishValue = null,
        string? russianValue = null,
        string baseName = "Strings",
        bool replaceAll = false)
    {
        if (string.IsNullOrWhiteSpace(xamlPath))
            throw new ArgumentException("XAML path is required.", nameof(xamlPath));

        if (!File.Exists(xamlPath))
            throw new FileNotFoundException("XAML file was not found.", xamlPath);

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text is required.", nameof(text));

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Localization key is required.", nameof(key));

        if (string.IsNullOrWhiteSpace(resourcesDirectory))
            throw new ArgumentException("Resources directory is required.", nameof(resourcesDirectory));

        var document = XDocument.Load(xamlPath, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        var replacement = "{loc:Loc " + key + "}";
        var replacements = ReplaceMatchingValues(document, text, replacement, replaceAll);

        if (replacements == 0)
            return 0;

        EnsureLocNamespace(document);
        document.Save(xamlPath);

        _resxWriter.AddOrUpdate(Path.Combine(resourcesDirectory, $"{baseName}.en.resx"), key, englishValue ?? text);
        _resxWriter.AddOrUpdate(Path.Combine(resourcesDirectory, $"{baseName}.ru.resx"), key, russianValue ?? text);

        return replacements;
    }

    /// <summary>
    /// Creates a conventional XAML localization key from file, element and property names.
    /// </summary>
    /// <param name="xamlPath">Path to the XAML file.</param>
    /// <param name="elementName">Element name or role, for example <c>EnglishButton</c>.</param>
    /// <param name="propertyName">Property name, for example <c>Content</c> or <c>Text</c>.</param>
    /// <returns>A sanitized localization key.</returns>
    public static string CreateKey(string xamlPath, string elementName, string propertyName)
    {
        var viewName = Path.GetFileNameWithoutExtension(xamlPath);
        return string.Join("_", new[]
        {
            Sanitize(viewName),
            Sanitize(elementName),
            Sanitize(propertyName)
        }.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static int ReplaceMatchingValues(XDocument document, string text, string replacement, bool replaceAll)
    {
        var count = 0;

        foreach (var attribute in document.Descendants().Attributes().ToArray())
        {
            if (!IsLocalizableAttribute(attribute) || attribute.Value != text)
                continue;

            attribute.Value = replacement;
            count++;
            if (!replaceAll)
                return count;
        }

        foreach (var textNode in document.DescendantNodes().OfType<XText>().ToArray())
        {
            if (textNode.Value.Trim() != text)
                continue;

            var parent = textNode.Parent;
            if (parent is null)
                continue;

            parent.SetAttributeValue(GetContentProperty(parent), replacement);
            textNode.Remove();
            count++;
            if (!replaceAll)
                return count;
        }

        return count;
    }

    private static string GetContentProperty(XElement element)
    {
        return element.Name.LocalName is "TextBlock" or "TextBox" or "Run"
            ? "Text"
            : "Content";
    }

    private static bool IsLocalizableAttribute(XAttribute attribute)
    {
        if (attribute.IsNamespaceDeclaration)
            return false;

        if (attribute.Value.StartsWith("{", StringComparison.Ordinal))
            return false;

        var name = attribute.Name.LocalName;
        if (name is "Class" or "Name" or "Key" or "Uid" or "Click" or "Command" or "CommandParameter")
            return false;

        if (name is "Width" or "Height" or "MinWidth" or "MinHeight" or "MaxWidth" or "MaxHeight" or "Margin" or "Padding")
            return false;

        return true;
    }

    private static void EnsureLocNamespace(XDocument document)
    {
        var root = document.Root;
        if (root is null)
            return;

        if (root.Attributes().Any(attribute => attribute.IsNamespaceDeclaration && attribute.Value == LocNamespace))
            return;

        var prefix = GetAvailablePrefix(root);
        root.Add(new XAttribute(XNamespace.Xmlns + prefix, LocNamespace));
    }

    private static string GetAvailablePrefix(XElement root)
    {
        const string preferred = "loc";
        if (root.GetNamespaceOfPrefix(preferred) is null)
            return preferred;

        var index = 1;
        while (root.GetNamespaceOfPrefix(preferred + index.ToString(System.Globalization.CultureInfo.InvariantCulture)) is not null)
            index++;

        return preferred + index.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string Sanitize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var builder = new System.Text.StringBuilder();
        foreach (var ch in value)
            builder.Append(char.IsLetterOrDigit(ch) ? ch : '_');

        return builder.ToString().Trim('_');
    }
}
