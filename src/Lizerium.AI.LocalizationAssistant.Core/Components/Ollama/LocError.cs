/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 05 мая 2026 07:01:45
 * Version: 1.0.8
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
