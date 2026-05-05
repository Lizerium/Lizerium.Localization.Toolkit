/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 05 мая 2026 07:01:45
 * Version: 1.0.8
 */

using System.Text.Json;

using System.Text.RegularExpressions;

using Lizerium.AI.LocalizationAssistant.Core.Clients.Libre;
using Lizerium.AI.LocalizationAssistant.Core.Clients.Ollama;
using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;

namespace Lizerium.AI.LocalizationAssistant.Core.Services
{
    public sealed class AILocalizationService
    {
        private readonly IAiClient _ollama;
        private readonly PromtConfig _config;

        public AILocalizationService(IAiClient ollama, PromtConfig config)
        {
            _ollama = ollama;
            _config = config;
        }

        public AILocalizationService(AiLocalizationOptions options)
            : this(
                new OllamaClient(options.OllamaBaseUrl, TimeSpan.FromSeconds(options.RequestTimeoutSeconds)),
                options.ToPromtConfig())
        {
        }

        public Task<LocalizationResult?> ProcessAsync(string sourceText)
        {
            return ProcessAsync(sourceText, CancellationToken.None);
        }

        public async Task<LocalizationResult?> ProcessAsync(
            string sourceText,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _config.Prompt = LocalizationPromptBuilder.Build(
                sourceText);

            var raw = await _ollama.GenerateAsync(
                new PromtConfig()
                {
                    Model = _config.Model,
                    Prompt = _config.Prompt,
                    GenerateEndpoint = _config.GenerateEndpoint,
                    RequestTimeoutSeconds = _config.RequestTimeoutSeconds,
                },
                cancellationToken).ConfigureAwait(false);

            Console.WriteLine("FULL RAW:");
            Console.WriteLine(raw);

            if (string.IsNullOrWhiteSpace(raw))
                return null;

            raw = raw.Replace("```json", "")
             .Replace("```css", "")
             .Replace("```", "");

            var start = raw.IndexOf('{');
            var end = raw.LastIndexOf('}');

            if (start < 0 || end <= start)
            {
                Console.WriteLine("JSON not found in response");
                return null;
            }

            raw = raw.Substring(start, end - start + 1);
            raw = raw.Trim();
            raw = raw.TrimStart('\uFEFF');

            LocalizationResult? result;

            try
            {
                result = JsonSerializer.Deserialize<LocalizationResult>(
                    raw,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (JsonException ex)
            {
                Console.WriteLine("JSON parse error:");
                Console.WriteLine(ex.Message);
                Console.WriteLine("BROKEN JSON:");
                Console.WriteLine(raw);

                result = null;
            }
           
            if (result == null)
                result = new LocalizationResult();

            PreserveSourceLanguageValue(sourceText, result);
            ApplyKnownUiGlossary(sourceText, result);

            if (string.IsNullOrWhiteSpace(result.En))
            {
                result.LocErrors.Add(new LocError(LLMError.EN, "LLM: EN is null"));
                result.En = sourceText;
            }

            if (string.IsNullOrWhiteSpace(result.Ru) && !string.IsNullOrEmpty(_config.LibreUrl))
            {
                result.LocErrors.Add(new LocError(LLMError.RU, "LLM: RU is null"));
                try
                {
                    var libre = new LibreTranslateClient(_config.LibreUrl, TimeSpan.FromSeconds(_config.RequestTimeoutSeconds));
                    result.Ru = await libre.TranslateAsync(result.En, "en", "ru", cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
                {
                    result.LocErrors.Add(new LocError(LLMError.RU, "LibreTranslate fallback failed: " + ex.Message));
                    result.Ru = sourceText;
                }
            }

            EnsurePlaceholders(sourceText, result);

            return result;
        }

        private static void PreserveSourceLanguageValue(string sourceText, LocalizationResult result)
        {
            if (string.IsNullOrWhiteSpace(sourceText))
                return;

            if (sourceText.Any(IsCyrillicLetter))
            {
                result.Ru = sourceText;
                return;
            }

            if (sourceText.Any(IsLatinLetter))
                result.En = sourceText;
        }

        private static bool IsCyrillicLetter(char value)
        {
            return char.IsLetter(value) && value >= '\u0400' && value <= '\u04FF';
        }

        private static bool IsLatinLetter(char value)
        {
            return char.IsLetter(value) && ((value >= 'A' && value <= 'Z') || (value >= 'a' && value <= 'z'));
        }

        private static void ApplyKnownUiGlossary(string sourceText, LocalizationResult result)
        {
            var normalized = sourceText.Trim();
            if (KnownUiTranslations.TryGetValue(normalized, out var translation))
            {
                result.En = translation.En;
                result.Ru = translation.Ru;
            }
        }

        private static readonly Dictionary<string, (string En, string Ru)> KnownUiTranslations =
            new Dictionary<string, (string En, string Ru)>(StringComparer.OrdinalIgnoreCase)
            {
                ["English"] = ("English", "Английский"),
                ["Английский"] = ("English", "Английский"),
                ["Russian"] = ("Russian", "Русский"),
                ["Русский"] = ("Russian", "Русский"),
                ["German"] = ("German", "Немецкий"),
                ["Немецкий"] = ("German", "Немецкий"),
                ["French"] = ("French", "Французский"),
                ["Французский"] = ("French", "Французский"),
                ["Spanish"] = ("Spanish", "Испанский"),
                ["Испанский"] = ("Spanish", "Испанский"),
                ["Italian"] = ("Italian", "Итальянский"),
                ["Итальянский"] = ("Italian", "Итальянский"),
                ["Chinese"] = ("Chinese", "Китайский"),
                ["Китайский"] = ("Chinese", "Китайский"),
                ["Japanese"] = ("Japanese", "Японский"),
                ["Японский"] = ("Japanese", "Японский")
            };

        private static void EnsurePlaceholders(string sourceText, LocalizationResult result)
        {
            var placeholders = Regex.Matches(sourceText ?? string.Empty, @"(?<!\{)\{(\d+)(?:[^}]*)\}(?!\})")
                .Cast<Match>()
                .Select(match => "{" + match.Groups[1].Value + "}")
                .Distinct()
                .ToArray();

            if (placeholders.Length == 0)
                return;

            result.En = EnsurePlaceholders(result.En, placeholders);
            result.Ru = EnsurePlaceholders(result.Ru, placeholders);
        }

        private static string EnsurePlaceholders(string? value, string[] placeholders)
        {
            value ??= string.Empty;

            foreach (var placeholder in placeholders)
            {
                if (!value.Contains(placeholder))
                    value = string.IsNullOrWhiteSpace(value) ? placeholder : value.TrimEnd() + " " + placeholder;
            }

            return value;
        }
    }
}
