/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 05 мая 2026 07:01:45
 * Version: 1.0.8
 */

using System.Text.RegularExpressions;

namespace Lizerium.Localization.Generator;

public sealed partial class LocalizationGenerator
{
    private static class PlaceholderCounter
    {
        private static readonly Regex Regex = new(@"(?<!\{)\{(\d+)(?:[^}]*)\}(?!\})", RegexOptions.Compiled);

        /// <summary>
        /// Counts required method parameters from numbered placeholders.
        /// </summary>
        /// <param name="value">Localized value.</param>
        /// <returns>Required generated method argument count.</returns>
        public static int Count(string value)
        {
            var max = -1;
            foreach (Match match in Regex.Matches(value ?? string.Empty))
            {
                if (int.TryParse(match.Groups[1].Value, out var index) && index > max)
                    max = index;
            }

            return max + 1;
        }
    }
}
