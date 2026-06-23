/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 23 июня 2026 15:55:12
 * Version: 1.0.68
 */

using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;

namespace Lizerium.AI.LocalizationAssistant.Core.Clients.Ollama
{
    public interface IAiClient
    {
        Task<string> GenerateAsync(PromtConfig config, CancellationToken cancellationToken = default);
    }
}
