/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 04 октября 2026 11:17:30
 * Version: 1.0.171
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
