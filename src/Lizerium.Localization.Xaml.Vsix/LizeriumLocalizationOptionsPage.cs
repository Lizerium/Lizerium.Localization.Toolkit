/*
 * Author: Nikolay Dvurechensky
 * Site: https://dvurechensky.pro/
 * Gmail: dvurechenskysoft@gmail.com
 * Last Updated: 05 мая 2026 07:01:45
 * Version: 1.0.8
 */

using System.ComponentModel;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Shell;

namespace Lizerium.Localization.Xaml.Vsix;

[Guid("1F417D41-F073-4AC1-A46D-FFCFD128AF8C")]
public sealed class LizeriumLocalizationOptionsPage : DialogPage
{
    [Category("AI Translation")]
    [DisplayName("Use AI translations")]
    [Description("Use the configured Ollama server to generate en/ru resource values. When disabled, the source text is written to resources.")]
    public bool UseAiTranslations { get; set; } = true;

    [Category("Ollama")]
    [DisplayName("Ollama base URL")]
    [Description("Base URL of the Ollama server used by the XAML localization action.")]
    public string OllamaBaseUrl { get; set; } = "http://localhost:11434";

    [Category("Ollama")]
    [DisplayName("Ollama model")]
    [Description("Model name passed to Ollama for translation generation.")]
    public string OllamaModel { get; set; } = "qwen2.5:7b";

    [Category("Ollama")]
    [DisplayName("Ollama generate endpoint")]
    [Description("Generate endpoint path used by the Ollama API.")]
    public string OllamaGenerateEndpoint { get; set; } = "/api/generate";

    [Category("AI Translation")]
    [DisplayName("Request timeout seconds")]
    [Description("Maximum time for each AI HTTP request before falling back. Keep it short so the XAML light bulb remains responsive.")]
    public int RequestTimeoutSeconds { get; set; } = 30;

    [Category("LibreTranslate")]
    [DisplayName("LibreTranslate URL")]
    [Description("Optional LibreTranslate server URL used when Ollama does not return a Russian value. Leave empty to disable LibreTranslate fallback.")]
    public string LibreTranslateUrl { get; set; } = "http://localhost:5000";

    [Category("Fallback")]
    [DisplayName("Fallback to source text")]
    [Description("When AI translation fails, write the source text into en/ru resources instead of TODO.")]
    public bool FallbackToSourceText { get; set; } = true;
}
