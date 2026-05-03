/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 03 мая 2026 06:52:43
 * Version: 1.0.6
 */

using System.Text.Json.Serialization;

namespace Lizerium.AI.LocalizationAssistant.Core.Components.Ollama
{
    public sealed class OllamaGenerateResponse
    {
        [JsonPropertyName("response")]
        public string Response { get; set; } = "";
    }
}
