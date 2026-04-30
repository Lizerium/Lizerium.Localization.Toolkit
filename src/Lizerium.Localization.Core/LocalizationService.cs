/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 30 апреля 2026 09:20:05
 * Version: 1.0.3
 */

using System.Collections.Concurrent;
using System.Globalization;

namespace Lizerium.Localization.Core;

/// <summary>
/// Provides runtime access to localized strings loaded from language-specific RESX files.
/// </summary>
public sealed class LocalizationService
{
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ResxReader _reader = new();
    private string _resourcesDirectory = Path.Combine(AppContext.BaseDirectory, "Resources", "Localization");
    private string _baseName = "Strings";
    private string _defaultLanguage = "en";
    private string _currentLanguage = "en";

    /// <summary>
    /// Gets the shared localization service instance used by generated localization APIs.
    /// </summary>
    public static LocalizationService Instance { get; } = new();

    /// <summary>
    /// Gets the currently active language code.
    /// </summary>
    public string CurrentLanguage => _currentLanguage;

    /// <summary>
    /// Gets a localized string by key.
    /// </summary>
    /// <param name="key">The RESX key to read.</param>
    /// <returns>The localized value, a fallback value, or a marker when the key is missing.</returns>
    public string this[string key] => GetString(key);

    private LocalizationService()
    {
    }

    /// <summary>
    /// Configures where language RESX files are loaded from.
    /// </summary>
    /// <param name="resourcesDirectory">Directory containing files such as <c>Strings.en.resx</c>.</param>
    /// <param name="baseName">Base file name used before the language suffix.</param>
    /// <param name="defaultLanguage">Fallback language used when the current language misses a key.</param>
    public void Configure(string resourcesDirectory, string baseName = "Strings", string defaultLanguage = "en")
    {
        _resourcesDirectory = resourcesDirectory;
        _baseName = string.IsNullOrWhiteSpace(baseName) ? "Strings" : baseName;
        _defaultLanguage = NormalizeLanguage(defaultLanguage);
        _currentLanguage = _defaultLanguage;
        _cache.Clear();
    }

    /// <summary>
    /// Changes the active language and updates current thread culture settings.
    /// </summary>
    /// <param name="language">Language code. Values starting with <c>ru</c> map to Russian; everything else maps to English.</param>
    public void ChangeLanguage(string language)
    {
        _currentLanguage = NormalizeLanguage(language);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(_currentLanguage);
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
    }

    /// <summary>
    /// Returns a localized string for the active language, falling back to the default language.
    /// </summary>
    /// <param name="key">The RESX key to resolve.</param>
    /// <returns>The localized value or a visible missing-key marker.</returns>
    public string GetString(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        if (Load(_currentLanguage).TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            return value;

        if (Load(_defaultLanguage).TryGetValue(key, out value) && !string.IsNullOrWhiteSpace(value))
            return value;

        return "!" + key + "!";
    }

    /// <summary>
    /// Formats a localized string using the current UI culture.
    /// </summary>
    /// <param name="key">The RESX key to resolve.</param>
    /// <param name="args">Arguments used by numbered placeholders such as <c>{0}</c>.</param>
    /// <returns>The formatted localized value.</returns>
    public string Format(string key, params object[] args)
    {
        return string.Format(CultureInfo.CurrentUICulture, GetString(key), args);
    }

    private IReadOnlyDictionary<string, string> Load(string language)
    {
        // Cache parsed RESX files per language; UI code may ask for many strings during a render pass.
        return _cache.GetOrAdd(language, lang =>
        {
            var path = Path.Combine(_resourcesDirectory, $"{_baseName}.{lang}.resx");
            return _reader.Load(path).ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
        });
    }

    private static string NormalizeLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return "en";

        return language.Trim().ToLowerInvariant().StartsWith("ru", StringComparison.Ordinal) ? "ru" : "en";
    }
}
