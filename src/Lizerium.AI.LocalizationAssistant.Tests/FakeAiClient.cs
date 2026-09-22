/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 22 сентября 2026 09:18:39
 * Version: 1.0.159
 */

using Lizerium.AI.LocalizationAssistant.Core.Clients.Ollama;
using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;

namespace Lizerium.AI.LocalizationAssistant.Tests
{
    public class FakeAiClient : IAiClient
    {
        public Task<string> GenerateAsync(PromtConfig config, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult("""
        {
          "key": "TestKey",
          "category": "Error",
          "en": "Test",
          "ru": "Тест"
        }
        """);
        }
    }
}
