/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 08 мая 2026 06:52:16
 * Version: 1.0.22
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
