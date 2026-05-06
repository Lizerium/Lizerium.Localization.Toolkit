/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 06 мая 2026 10:30:02
 * Version: 1.0.12
 */

using Lizerium.AI.LocalizationAssistant.Core.Clients.Ollama;
using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;
using Lizerium.AI.LocalizationAssistant.Core.Services;

namespace Lizerium.Localization.Ai.Analyzer
{
    internal class AiRunner
    {
        public static async Task<LocalizationResult?> RunAsync(string text, string? projectFilePath = null)
        {
            var options = AiLocalizationOptions.FromProject(projectFilePath);
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
