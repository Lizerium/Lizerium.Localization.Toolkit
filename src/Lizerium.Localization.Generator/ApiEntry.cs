/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 23 июня 2026 15:55:12
 * Version: 1.0.68
 */

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;

namespace Lizerium.Localization.Generator;

public sealed partial class LocalizationGenerator
{
    private sealed record ApiEntry(
        string Key,
        ImmutableArray<string> Path,
        string MethodName,
        int ParamCount,
        bool IsFormat,
        string? En,
        string? Ru)
    {
        /// <summary>
        /// Converts a localization entry into a generated API method description.
        /// </summary>
        /// <param name="entry">Localization entry read from RESX.</param>
        /// <returns>API shape used by the code builder.</returns>
        public static ApiEntry From(LocalizationEntry entry, string? en, string? ru)
        {
            var key = entry.Key;
            var isFormat = key.EndsWith("_Format", StringComparison.OrdinalIgnoreCase);
            var parts = key.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (isFormat && parts.Length > 1)
                parts = parts.Take(parts.Length - 1).ToArray();

            // Every key segment becomes either a nested class name or the final method name.
            var method = Sanitize(parts.Length == 0 ? key : parts[parts.Length - 1]);
            var path = parts.Length <= 1
                ? ImmutableArray<string>.Empty
                : parts.Take(parts.Length - 1).Select(Sanitize).ToImmutableArray();

            return new ApiEntry(key, path, method, entry.ParamCount, isFormat || entry.ParamCount > 0, en, ru);
        }

        private static string Sanitize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Key";

            var builder = new StringBuilder();
            foreach (var ch in value)
                builder.Append(char.IsLetterOrDigit(ch) ? ch : '_');

            if (builder.Length == 0 || char.IsDigit(builder[0]))
                builder.Insert(0, '_');

            return builder.ToString();
        }
    }
}
