/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 21 октября 2026 10:05:55
 * Version: 1.0.187
 */

namespace Lizerium.AI.LocalizationAssistant.Core.Components.Ollama
{
    public sealed class OllamaGenerateRequest
    {
        public string Model { get; set; } = "";

        public string Prompt { get; set; } = "";

        public bool Stream { get; set; }
    }
}
