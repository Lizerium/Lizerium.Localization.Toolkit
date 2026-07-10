/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 10 июля 2026 11:55:07
 * Version: 1.0.85
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
