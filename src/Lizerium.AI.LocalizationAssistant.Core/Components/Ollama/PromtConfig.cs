/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 14 мая 2026 10:37:33
 * Version: 1.0.28
 */

namespace Lizerium.AI.LocalizationAssistant.Core.Components.Ollama
{
    public class PromtConfig
    {
        public string Prompt { get; set; } = string.Empty;
        public string Model { get; set; } = AiLocalizationOptions.DefaultOllamaModel;
        public string GenerateEndpoint { get; set; } = AiLocalizationOptions.DefaultOllamaGenerateEndpoint;
        public string LibreUrl { get; set; } = AiLocalizationOptions.DefaultLibreTranslateUrl;
        public int RequestTimeoutSeconds { get; set; } = AiLocalizationOptions.DefaultRequestTimeoutSeconds;
    }
}
