/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 15 сентября 2026 07:37:20
 * Version: 1.0.152
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
