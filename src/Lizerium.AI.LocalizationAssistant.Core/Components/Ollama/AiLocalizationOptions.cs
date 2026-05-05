/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 05 мая 2026 07:01:45
 * Version: 1.0.8
 */

using System;
using System.IO;
using System.Text.RegularExpressions;

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
            ApplyEnvironment(options);
            return options;
        }

        public static AiLocalizationOptions FromProject(string? projectFilePath)
        {
            var options = new AiLocalizationOptions();
            ApplyConfigFile(options, projectFilePath);
            ApplyEnvironment(options);
            return options;
        }

        private static void ApplyEnvironment(AiLocalizationOptions options)
        {
            options.OllamaBaseUrl = Normalize(Environment.GetEnvironmentVariable("LIZERIUM_OLLAMA_URL"), options.OllamaBaseUrl);
            options.OllamaModel = Normalize(Environment.GetEnvironmentVariable("LIZERIUM_OLLAMA_MODEL"), options.OllamaModel);
            options.OllamaGenerateEndpoint = Normalize(Environment.GetEnvironmentVariable("LIZERIUM_OLLAMA_GENERATE_ENDPOINT"), options.OllamaGenerateEndpoint);
            options.LibreTranslateUrl = Normalize(Environment.GetEnvironmentVariable("LIZERIUM_LIBRETRANSLATE_URL"), options.LibreTranslateUrl);
            options.RequestTimeoutSeconds = NormalizePositiveInt(Environment.GetEnvironmentVariable("LIZERIUM_AI_TIMEOUT_SECONDS"), options.RequestTimeoutSeconds);
        }

        private static void ApplyConfigFile(AiLocalizationOptions options, string? projectFilePath)
        {
            var directory = string.IsNullOrWhiteSpace(projectFilePath)
                ? Environment.CurrentDirectory
                : Path.GetDirectoryName(Path.GetFullPath(projectFilePath)) ?? Environment.CurrentDirectory;

            while (!string.IsNullOrWhiteSpace(directory))
            {
                var configPath = Path.Combine(directory, ".lizerium-localization.json");
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    options.OllamaBaseUrl = Normalize(ReadString(json, "ollamaBaseUrl"), options.OllamaBaseUrl);
                    options.OllamaModel = Normalize(ReadString(json, "ollamaModel"), options.OllamaModel);
                    options.OllamaGenerateEndpoint = Normalize(ReadString(json, "ollamaGenerateEndpoint"), options.OllamaGenerateEndpoint);
                    options.LibreTranslateUrl = Normalize(ReadString(json, "libreTranslateUrl"), options.LibreTranslateUrl);
                    options.RequestTimeoutSeconds = NormalizePositiveInt(ReadString(json, "requestTimeoutSeconds"), options.RequestTimeoutSeconds);
                    return;
                }

                directory = Directory.GetParent(directory)?.FullName;
            }
        }

        private static string ReadString(string json, string propertyName)
        {
            var escapedName = Regex.Escape(propertyName);
            var stringMatch = Regex.Match(
                json,
                "\"(?:" + escapedName + ")\"\\s*:\\s*\"(?<value>(?:\\\\.|[^\"\\\\])*)\"",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (stringMatch.Success)
                return Unescape(stringMatch.Groups["value"].Value);

            var numberMatch = Regex.Match(
                json,
                "\"(?:" + escapedName + ")\"\\s*:\\s*(?<value>\\d+)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            return numberMatch.Success ? numberMatch.Groups["value"].Value : string.Empty;
        }

        private static string Unescape(string value)
        {
            return value
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t");
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
