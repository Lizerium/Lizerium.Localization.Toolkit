/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 05 мая 2026 07:01:45
 * Version: 1.0.8
 */

using System.Text;

using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;
using Lizerium.AI.LocalizationAssistant.Core.Services;

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
            var requestJson = SimpleJson.CreateObject(
                ("model", config.Model),
                ("prompt", config.Prompt),
                ("stream", false));

            using var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(config.GenerateEndpoint, content, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return SimpleJson.TryGetString(json, "response", out var generated)
                ? generated
                : string.Empty;
        }
    }
}
