/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 06 мая 2026 10:30:02
 * Version: 1.0.12
 */

namespace Lizerium.Localization.Core;

/// <summary>
/// Represents one localization entry loaded from a RESX file.
/// </summary>
public sealed class LocalizationEntry
{
    /// <summary>
    /// Gets or sets the RESX data name.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the localized string value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of numbered placeholders detected in <see cref="Value"/>.
    /// </summary>
    public int ParamCount { get; set; }
}
