/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 12 мая 2026 12:06:00
 * Version: 1.0.26
 */

namespace Lizerium.AI.LocalizationAssistant.Core.Components.Ollama
{
    public sealed class LocError
    {
        public LLMError Type { get; set; }
        public string Text { get; set; }

        public LocError(LLMError type, string text)
        {
            this.Type = type;
            this.Text = text;
        }
    }
}
