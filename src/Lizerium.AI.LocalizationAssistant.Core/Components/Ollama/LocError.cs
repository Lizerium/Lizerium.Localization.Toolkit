/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 03 июня 2026 15:16:57
 * Version: 1.0.48
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
