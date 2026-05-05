/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 05 мая 2026 07:01:45
 * Version: 1.0.8
 */

using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;
using Lizerium.AI.LocalizationAssistant.Core.Services;

namespace Lizerium.AI.LocalizationAssistant.Core.Clients.Libre
{
    public class LibreTranslateClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public LibreTranslateClient(string baseUrl, TimeSpan? timeout = null)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _http = new HttpClient
            {
                Timeout = timeout ?? TimeSpan.FromSeconds(AiLocalizationOptions.DefaultRequestTimeoutSeconds)
            };
        }

        public async Task<string> TranslateAsync(
            string text,
            string from,
            string to,
            CancellationToken cancellationToken = default)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["q"] = text,
                ["source"] = from,
                ["target"] = to,
                ["format"] = "text"
            });

            var response = await _http.PostAsync($"{_baseUrl}/translate", content, cancellationToken).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return SimpleJson.TryGetString(json, "translatedText", out var translated)
                ? translated
                : text;
        }
    }
}
