/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 03 мая 2026 06:52:43
 * Version: 1.0.6
 */

using System.Text.Json;

using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;

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

            var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("translatedText").GetString() ?? text;
        }
    }
}
