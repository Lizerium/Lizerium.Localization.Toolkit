/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 03 мая 2026 06:52:43
 * Version: 1.0.6
 */

using Lizerium.AI.LocalizationAssistant.Core.Clients.Ollama;
using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;
using Lizerium.AI.LocalizationAssistant.Core.Services;

namespace Lizerium.Localization.Ai.Analyzer
{
    internal class AiRunner
    {
        public static async Task<LocalizationResult?> RunAsync(string text)
        {
            var options = AiLocalizationOptions.FromEnvironment();
            var ollama = new OllamaClient(
                options.OllamaBaseUrl,
                TimeSpan.FromSeconds(options.RequestTimeoutSeconds));
            var ai = new AILocalizationService(ollama, options.ToPromtConfig());

            var result = await ai.ProcessAsync(
                    sourceText: text);
            return result;
        }
    }
}
