/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 02 мая 2026 19:17:07
 * Version: 1.0.5
 */

using System.Text.RegularExpressions;

namespace Lizerium.Localization.Core;

/// <summary>
/// Counts numbered format placeholders in localized strings.
/// </summary>
public static class PlaceholderAnalyzer
{
    private static readonly Regex PlaceholderRegex = new(@"(?<!\{)\{(\d+)(?:[^}]*)\}(?!\})", RegexOptions.Compiled);

    /// <summary>
    /// Counts the number of required arguments based on the highest placeholder index.
    /// </summary>
    /// <param name="value">The localized value to inspect.</param>
    /// <returns>The required argument count.</returns>
    public static int CountParams(string value)
    {
        if (string.IsNullOrEmpty(value))
            return 0;

        var max = -1;
        foreach (Match match in PlaceholderRegex.Matches(value))
        {
            if (int.TryParse(match.Groups[1].Value, out var index) && index > max)
                max = index;
        }

        return max + 1;
    }
}
