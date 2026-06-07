/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 07 июня 2026 18:21:57
 * Version: 1.0.52
 */

#if WINDOWS
using System.Windows.Markup;

namespace Lizerium.Localization.Core;

/// <summary>
/// Resolves a localization key directly from WPF XAML.
/// </summary>
[MarkupExtensionReturnType(typeof(string))]
public sealed class LocExtension : MarkupExtension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocExtension"/> class.
    /// </summary>
    public LocExtension()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocExtension"/> class.
    /// </summary>
    /// <param name="key">RESX key to resolve.</param>
    public LocExtension(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Gets or sets the RESX key to resolve.
    /// </summary>
    [ConstructorArgument("key")]
    public string Key { get; set; } = string.Empty;

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return LocalizationService.Instance.GetString(Key);
    }
}
#endif
