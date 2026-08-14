/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 14 августа 2026 08:16:54
 * Version: 1.0.120
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
