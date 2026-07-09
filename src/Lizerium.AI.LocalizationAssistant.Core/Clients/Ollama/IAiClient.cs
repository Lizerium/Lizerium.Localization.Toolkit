/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 09 июля 2026 07:01:51
 * Version: 1.0.84
 */

using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;

namespace Lizerium.AI.LocalizationAssistant.Core.Clients.Ollama
{
    public interface IAiClient
    {
        Task<string> GenerateAsync(PromtConfig config, CancellationToken cancellationToken = default);
    }
}
