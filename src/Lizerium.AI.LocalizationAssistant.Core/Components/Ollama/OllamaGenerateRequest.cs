/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 23 сентября 2026 09:41:39
 * Version: 1.0.160
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
