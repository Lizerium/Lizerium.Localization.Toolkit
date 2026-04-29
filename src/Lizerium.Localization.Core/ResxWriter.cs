/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 29 апреля 2026 06:52:46
 * Version: 1.0.2
 */

using System.Xml.Linq;

namespace Lizerium.Localization.Core;

/// <summary>
/// Adds and updates localization values in RESX files.
/// </summary>
public sealed class ResxWriter
{
    /// <summary>
    /// Adds a new RESX entry or updates an existing one.
    /// </summary>
    /// <param name="path">Path to the RESX file.</param>
    /// <param name="key">Localization key to write.</param>
    /// <param name="value">Localized value to store.</param>
    public void AddOrUpdate(string path, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("RESX path is required.", nameof(path));

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Localization key is required.", nameof(key));

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);

        var document = File.Exists(path)
            ? XDocument.Load(path, LoadOptions.PreserveWhitespace)
            : CreateDocument();

        var root = document.Root ?? throw new InvalidOperationException("Invalid RESX file: root element is missing.");
        var existing = root.Elements("data")
            .FirstOrDefault(item => string.Equals(item.Attribute("name")?.Value, key, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            root.Add(new XElement("data",
                new XAttribute("name", key),
                new XAttribute(XNamespace.Xml + "space", "preserve"),
                new XElement("value", value ?? string.Empty)));
        }
        else
        {
            var valueNode = existing.Element("value");
            if (valueNode is null)
                existing.Add(new XElement("value", value ?? string.Empty));
            else
                valueNode.Value = value ?? string.Empty;
        }

        document.Save(path);
    }

    private static XDocument CreateDocument()
    {
        // Create the standard RESX headers so new files can be opened by common .NET tooling.
        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("root",
                new XElement("resheader", new XAttribute("name", "resmimetype"), new XElement("value", "text/microsoft-resx")),
                new XElement("resheader", new XAttribute("name", "version"), new XElement("value", "2.0")),
                new XElement("resheader", new XAttribute("name", "reader"), new XElement("value", "System.Resources.ResXResourceReader, System.Windows.Forms")),
                new XElement("resheader", new XAttribute("name", "writer"), new XElement("value", "System.Resources.ResXResourceWriter, System.Windows.Forms"))));
    }
}
