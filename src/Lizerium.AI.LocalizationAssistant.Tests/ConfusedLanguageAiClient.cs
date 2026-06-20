/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 20 июня 2026 06:52:34
 * Version: 1.0.65
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
