/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 27 августа 2026 08:33:05
 * Version: 1.0.133
 */

using Lizerium.AI.LocalizationAssistant.Core.Clients.Ollama;
using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;

namespace Lizerium.AI.LocalizationAssistant.Tests
{
    public class BrokenAiClient : IAiClient
    {
        public Task<string> GenerateAsync(PromtConfig prompt, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult("some garbage text");
        }
    }
}
