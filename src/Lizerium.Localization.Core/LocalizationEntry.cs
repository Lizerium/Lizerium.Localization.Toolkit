/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 17 сентября 2026 06:54:47
 * Version: 1.0.154
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
