/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 05 октября 2026 08:12:00
 * Version: 1.0.172
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
