/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 15 июля 2026 11:54:26
 * Version: 1.0.90
 */

using Lizerium.AI.LocalizationAssistant.Core.Clients.Ollama;
using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;

namespace Lizerium.AI.LocalizationAssistant.Tests
{
    public class ConfusedLanguageAiClient : IAiClient
    {
        public Task<string> GenerateAsync(PromtConfig config, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult("""
        {
          "en": "Russian",
          "ru": "Р°РЅРіР»РёР№СЃРєРёР№"
        }
        """);
        }
    }
}
