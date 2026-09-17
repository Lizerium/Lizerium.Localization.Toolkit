/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 17 сентября 2026 06:54:47
 * Version: 1.0.154
 */

using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;

namespace Lizerium.AI.LocalizationAssistant.Core.Clients.Ollama
{
    public interface IAiClient
    {
        Task<string> GenerateAsync(PromtConfig config, CancellationToken cancellationToken = default);
    }
}
