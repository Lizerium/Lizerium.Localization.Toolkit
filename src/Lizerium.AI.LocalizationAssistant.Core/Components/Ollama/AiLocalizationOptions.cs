/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 03 мая 2026 06:52:43
 * Version: 1.0.6
 */

using System;

namespace Lizerium.AI.LocalizationAssistant.Core.Components.Ollama
{
    public sealed class AiLocalizationOptions
    {
        public const string DefaultOllamaBaseUrl = "http://localhost:11434";
        public const string DefaultOllamaModel = "qwen2.5:7b";
        public const string DefaultOllamaGenerateEndpoint = "/api/generate";
        public const string DefaultLibreTranslateUrl = "http://localhost:5000";
        public const int DefaultRequestTimeoutSeconds = 30;

        public string OllamaBaseUrl { get; set; } = DefaultOllamaBaseUrl;

        public string OllamaModel { get; set; } = DefaultOllamaModel;

        public string OllamaGenerateEndpoint { get; set; } = DefaultOllamaGenerateEndpoint;

        public string LibreTranslateUrl { get; set; } = DefaultLibreTranslateUrl;

        public int RequestTimeoutSeconds { get; set; } = DefaultRequestTimeoutSeconds;

        public static AiLocalizationOptions FromEnvironment()
        {
            var options = new AiLocalizationOptions();
            options.OllamaBaseUrl = Normalize(Environment.GetEnvironmentVariable("LIZERIUM_OLLAMA_URL"), options.OllamaBaseUrl);
            options.OllamaModel = Normalize(Environment.GetEnvironmentVariable("LIZERIUM_OLLAMA_MODEL"), options.OllamaModel);
            options.OllamaGenerateEndpoint = Normalize(Environment.GetEnvironmentVariable("LIZERIUM_OLLAMA_GENERATE_ENDPOINT"), options.OllamaGenerateEndpoint);
            options.LibreTranslateUrl = Normalize(Environment.GetEnvironmentVariable("LIZERIUM_LIBRETRANSLATE_URL"), options.LibreTranslateUrl);
            options.RequestTimeoutSeconds = NormalizePositiveInt(Environment.GetEnvironmentVariable("LIZERIUM_AI_TIMEOUT_SECONDS"), options.RequestTimeoutSeconds);
            return options;
        }

        public PromtConfig ToPromtConfig()
        {
            return new PromtConfig
            {
                Model = OllamaModel,
                GenerateEndpoint = OllamaGenerateEndpoint,
                LibreUrl = LibreTranslateUrl,
                RequestTimeoutSeconds = RequestTimeoutSeconds
            };
        }

        private static string Normalize(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static int NormalizePositiveInt(string value, int fallback)
        {
            return int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
        }
    }
}
