/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 02 мая 2026 19:17:07
 * Version: 1.0.5
 */

using System.Xml.Linq;

namespace Lizerium.Localization.Core;

/// <summary>
/// Reads localization entries from RESX files.
/// </summary>
public sealed class ResxReader
{
    /// <summary>
    /// Loads all <c>data</c> entries from a RESX file.
    /// </summary>
    /// <param name="path">Path to the RESX file.</param>
    /// <returns>Entries sorted by key. Missing files return an empty list.</returns>
    public List<LocalizationEntry> Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("RESX path is required.", nameof(path));

        if (!File.Exists(path))
            return new List<LocalizationEntry>();

        var document = XDocument.Load(path, LoadOptions.PreserveWhitespace);
        // RESX data entries are the only nodes that represent localizable user-facing values.
        return document.Root?
            .Elements("data")
            .Select(item =>
            {
                var value = item.Element("value")?.Value ?? string.Empty;
                return new LocalizationEntry
                {
                    Key = item.Attribute("name")?.Value ?? string.Empty,
                    Value = value,
                    ParamCount = PlaceholderAnalyzer.CountParams(value)
                };
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.Key))
            .OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<LocalizationEntry>();
    }
}
