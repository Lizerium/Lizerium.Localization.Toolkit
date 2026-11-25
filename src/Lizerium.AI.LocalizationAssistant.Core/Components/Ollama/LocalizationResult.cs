/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 25 ноября 2026 11:15:15
 * Version: 1.0.222
 */

namespace Lizerium.AI.LocalizationAssistant.Core.Components.Ollama
{
    public sealed class LocalizationResult
    {
        public string Ru { get; set; } = "";
        public string En { get; set; } = "";
        public List<LocError> LocErrors { get; set; } = new List<LocError>();

        public override string ToString()
        {
            return $"Ru: {Ru} | En: {En}";
        }
    }
}
