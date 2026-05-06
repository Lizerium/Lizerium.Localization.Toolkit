/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 06 мая 2026 10:30:02
 * Version: 1.0.12
 */

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Lizerium.AI.LocalizationAssistant.Core.Services
{
    internal static class SimpleJson
    {
        public static string CreateObject(params (string Name, object? Value)[] properties)
        {
            var builder = new StringBuilder();
            builder.Append('{');

            for (var i = 0; i < properties.Length; i++)
            {
                if (i > 0)
                    builder.Append(',');

                builder.Append('"').Append(Escape(properties[i].Name)).Append("\":");

                var value = properties[i].Value;
                if (value is bool boolean)
                    builder.Append(boolean ? "true" : "false");
                else
                    builder.Append('"').Append(Escape(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty)).Append('"');
            }

            builder.Append('}');
            return builder.ToString();
        }

        public static bool TryGetString(string json, string propertyName, out string value)
        {
            var escapedName = Regex.Escape(propertyName);
            var match = Regex.Match(
                json,
                "\"(?:" + escapedName + ")\"\\s*:\\s*\"(?<value>(?:\\\\.|[^\"\\\\])*)\"",
                RegexOptions.Singleline);

            if (!match.Success)
            {
                value = string.Empty;
                return false;
            }

            value = Unescape(match.Groups["value"].Value);
            return true;
        }

        public static string Escape(string value)
        {
            var builder = new StringBuilder(value.Length);

            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        if (char.IsControl(ch))
                            builder.Append("\\u").Append(((int)ch).ToString("x4", CultureInfo.InvariantCulture));
                        else
                            builder.Append(ch);
                        break;
                }
            }

            return builder.ToString();
        }

        private static string Unescape(string value)
        {
            var builder = new StringBuilder(value.Length);

            for (var i = 0; i < value.Length; i++)
            {
                var ch = value[i];
                if (ch != '\\' || i + 1 >= value.Length)
                {
                    builder.Append(ch);
                    continue;
                }

                var escaped = value[++i];
                switch (escaped)
                {
                    case '"':
                    case '\\':
                    case '/':
                        builder.Append(escaped);
                        break;
                    case 'b':
                        builder.Append('\b');
                        break;
                    case 'f':
                        builder.Append('\f');
                        break;
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 'r':
                        builder.Append('\r');
                        break;
                    case 't':
                        builder.Append('\t');
                        break;
                    case 'u':
                        if (i + 4 < value.Length &&
                            int.TryParse(value.Substring(i + 1, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var code))
                        {
                            builder.Append((char)code);
                            i += 4;
                        }
                        else
                        {
                            builder.Append("\\u");
                        }
                        break;
                    default:
                        builder.Append(escaped);
                        break;
                }
            }

            return builder.ToString();
        }
    }
}
