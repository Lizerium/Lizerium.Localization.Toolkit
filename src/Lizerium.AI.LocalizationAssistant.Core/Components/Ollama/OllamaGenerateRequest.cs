/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 28 мая 2026 17:30:55
 * Version: 1.0.42
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
