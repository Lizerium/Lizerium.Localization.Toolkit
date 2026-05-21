/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 21 мая 2026 11:38:42
 * Version: 1.0.35
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
