/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 05 мая 2026 07:01:45
 * Version: 1.0.8
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

            foreach (var group in _entries.GroupBy(item => item.MethodName))
            {
                var index = 0;
                // If sanitized keys collide, keep the first method name and suffix following overload names.
                foreach (var entry in group)
                    WriteMethod(builder, indent + 1, entry, index++ == 0 ? entry.MethodName : entry.MethodName + index);
            }

            AppendIndent(builder, indent);
            builder.AppendLine("}");
        }

        private static void WriteMethod(StringBuilder builder, int indent, ApiEntry entry, string methodName)
        {
            var parameters = Enumerable.Range(0, entry.ParamCount).Select(i => "object arg" + i).ToArray();
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
