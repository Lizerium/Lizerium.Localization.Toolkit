/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 10 июля 2026 11:55:07
 * Version: 1.0.85
 */

using Lizerium.AI.LocalizationAssistant.Core.Clients.Ollama;
using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;

namespace Lizerium.AI.LocalizationAssistant.Tests
{
    public class ThrowingAiClient : IAiClient
    {
        public Task<string> GenerateAsync(PromtConfig prompt, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new HttpRequestException("Ollama is offline");
        }
    }
}
