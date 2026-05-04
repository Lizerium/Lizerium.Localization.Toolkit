/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 04 мая 2026 06:52:49
 * Version: 1.0.7
 */

using System.Net.Http.Json;

using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;

namespace Lizerium.AI.LocalizationAssistant.Core.Clients.Ollama
{
    public class OllamaClient : IAiClient
    {
        private readonly HttpClient _http;

        public OllamaClient()
            : this(AiLocalizationOptions.DefaultOllamaBaseUrl, null)
        {
        }

        public OllamaClient(string address)
            : this(address, null)
        {
        }

        public OllamaClient(
            string address = AiLocalizationOptions.DefaultOllamaBaseUrl,
            TimeSpan? timeout = null)
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri(address),
                Timeout = timeout ?? TimeSpan.FromSeconds(AiLocalizationOptions.DefaultRequestTimeoutSeconds)
            };
        }

        public async Task<string> GenerateAsync(PromtConfig config, CancellationToken cancellationToken = default)
        {
            var request = new OllamaGenerateRequest()
            {
                Model = config.Model,
                Prompt = config.Prompt,
                Stream = false
            };

            var response = await _http.PostAsJsonAsync(config.GenerateEndpoint, request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
            return json?.Response ?? "";
        }
    }
}
