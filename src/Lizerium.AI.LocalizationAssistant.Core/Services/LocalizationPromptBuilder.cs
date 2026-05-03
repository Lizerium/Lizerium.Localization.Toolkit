/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 03 мая 2026 06:52:43
 * Version: 1.0.6
 */

namespace Lizerium.AI.LocalizationAssistant.Core.Services
{
    public static class LocalizationPromptBuilder
    {
        public static string Build(string sourceText)
        {
            return $@"
You are a strict JSON translation engine for .NET RESX localization.

Translate the SOURCE_TEXT between English and Russian.

Rules:
- Return ONLY one valid JSON object.
- Do NOT return markdown.
- Do NOT explain anything.
- Preserve placeholders exactly: {{0}}, {{1}}, {{2}}.
- Do NOT rename, reorder, remove, or duplicate placeholders.
- Keep punctuation, casing, and formatting as close to SOURCE_TEXT as possible.
- Do not copy the JSON schema values literally.

Language rules:
- If SOURCE_TEXT is English, set ""en"" to the original SOURCE_TEXT and ""ru"" to the Russian translation.
- If SOURCE_TEXT is Russian, set ""ru"" to the original SOURCE_TEXT and ""en"" to the English translation.

SOURCE_TEXT:
<<<
{sourceText}
>>>

Return JSON in this shape, replacing EN_VALUE and RU_VALUE with real values:
{{
  ""en"": ""EN_VALUE"",
  ""ru"": ""RU_VALUE""
}}";
        }
    }
}
