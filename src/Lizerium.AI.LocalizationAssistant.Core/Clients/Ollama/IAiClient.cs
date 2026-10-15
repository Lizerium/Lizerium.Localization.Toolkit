/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 15 октября 2026 07:44:31
 * Version: 1.0.181
 */

using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;

namespace Lizerium.AI.LocalizationAssistant.Core.Clients.Ollama
{
    public interface IAiClient
    {
        Task<string> GenerateAsync(PromtConfig config, CancellationToken cancellationToken = default);
    }
}
