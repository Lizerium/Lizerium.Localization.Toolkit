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
