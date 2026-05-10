/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 10 мая 2026 07:42:37
 * Version: 1.0.24
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;

namespace Lizerium.Localization.Generator;

public sealed partial class LocalizationGenerator
{
    private sealed class Node
    {
        private readonly SortedDictionary<string, Node> _children = new(StringComparer.Ordinal);
        private readonly List<ApiEntry> _entries = new();

        private Node(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public static Node Create(string name) => new(name);

        public void Add(ApiEntry entry)
        {
            var node = this;
            foreach (var part in entry.Path)
            {
                if (!node._children.TryGetValue(part, out var child))
                {
                    child = new Node(part);
                    node._children.Add(part, child);
                }

                node = child;
            }

            node._entries.Add(entry);
        }

        /// <summary>
        /// Writes this node as a static class and emits all child nodes and methods.
        /// </summary>
        /// <param name="builder">Destination source builder.</param>
        /// <param name="indent">Indentation level.</param>
        /// <param name="isRoot">Whether the node is the root localization class.</param>
        public void Write(StringBuilder builder, int indent, bool isRoot = false)
        {
            AppendIndent(builder, indent);
            builder.Append("public static class ").Append(Name).AppendLine();
            AppendIndent(builder, indent);
            builder.AppendLine("{");

            foreach (var child in _children.Values)
                child.Write(builder, indent + 1);

            var usedMemberNames = new HashSet<string>(_children.Keys, StringComparer.Ordinal);

            foreach (var group in _entries.GroupBy(item => item.MethodName))
            {
                var index = 0;
                var baseMethodName = group.Key;

                if (usedMemberNames.Contains(baseMethodName))
                    baseMethodName += "Value";

                // If sanitized keys collide, keep the first method name and suffix following overload names.
                foreach (var entry in group)
                {
                    var methodName = index++ == 0 ? baseMethodName : baseMethodName + index;
                    while (usedMemberNames.Contains(methodName))
                        methodName += "Value";

                    usedMemberNames.Add(methodName);
                    WriteMethod(builder, indent + 1, entry, methodName);
                }
            }

            AppendIndent(builder, indent);
            builder.AppendLine("}");
        }

        private static void WriteMethod(StringBuilder builder, int indent, ApiEntry entry, string methodName)
        {
            var parameters = Enumerable.Range(0, entry.ParamCount).Select(i => "object arg" + i).ToArray();
            WriteDocumentation(builder, indent, entry);
            AppendIndent(builder, indent);
            builder.Append("public static string ").Append(methodName).Append('(').Append(string.Join(", ", parameters)).AppendLine(")");
            AppendIndent(builder, indent + 1);
            if (entry.ParamCount == 0)
            {
                builder.Append("=> global::Lizerium.Localization.Core.LocalizationService.Instance.GetString(");
                AppendLiteral(builder, entry.Key);
                builder.AppendLine(");");
                return;
            }

            builder.Append("=> global::Lizerium.Localization.Core.LocalizationService.Instance.Format(");
            AppendLiteral(builder, entry.Key);
            foreach (var parameter in parameters.Select(p => p.Split(' ')[1]))
                builder.Append(", ").Append(parameter);
            builder.AppendLine(");");
        }

        private static void WriteDocumentation(StringBuilder builder, int indent, ApiEntry entry)
        {
            AppendXmlDocRaw(builder, indent, "<summary>");
            AppendXmlDocPara(builder, indent, "Key: " + entry.Key);
            if (!string.IsNullOrWhiteSpace(entry.Ru))
                AppendXmlDocPara(builder, indent, "ru: " + NormalizeDocText(entry.Ru!));
            if (!string.IsNullOrWhiteSpace(entry.En))
                AppendXmlDocPara(builder, indent, "en: " + NormalizeDocText(entry.En!));
            AppendXmlDocRaw(builder, indent, "</summary>");

            for (var i = 0; i < entry.ParamCount; i++)
                AppendXmlDocRaw(builder, indent, "<param name=\"arg" + i + "\">Format argument {" + i + "}.</param>");
        }

        private static void AppendXmlDoc(StringBuilder builder, int indent, string text)
        {
            AppendIndent(builder, indent);
            builder.Append("/// ").AppendLine(EscapeXml(text));
        }

        private static void AppendXmlDocPara(StringBuilder builder, int indent, string text)
        {
            AppendIndent(builder, indent);
            builder.Append("/// <para>").Append(EscapeXml(text)).AppendLine("</para>");
        }

        private static void AppendXmlDocRaw(StringBuilder builder, int indent, string text)
        {
            AppendIndent(builder, indent);
            builder.Append("/// ").AppendLine(text);
        }

        private static string NormalizeDocText(string value)
        {
            var normalized = string.Join(" ", value.Split(new[] { '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Select(part => part.Trim()));
            return normalized.Length <= 180 ? normalized : normalized.Substring(0, 177) + "...";
        }

        private static string EscapeXml(string value)
        {
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }

        private static void AppendLiteral(StringBuilder builder, string value)
        {
            builder.Append("@\"").Append(value.Replace("\"", "\"\"")).Append('"');
        }

        private static void AppendIndent(StringBuilder builder, int indent)
        {
            builder.Append(' ', indent * 4);
        }
    }
}
