# Lizerium.AI.LocalizationAssistant.Core

AI localization helper library used by `Lizerium.Localization.Ai.Analyzer`.

This package contains:

- `AILocalizationService` for converting source text into localized `en` and `ru` values;
- an Ollama client for local AI translation workflows;
- prompt building helpers;
- fallback-friendly localization result models.

## Usage

```csharp
using Lizerium.AI.LocalizationAssistant.Core.Clients.Ollama;
using Lizerium.AI.LocalizationAssistant.Core.Components.Ollama;
using Lizerium.AI.LocalizationAssistant.Core.Services;

var options = new AiLocalizationOptions
{
    OllamaBaseUrl = "http://localhost:11434",
    OllamaModel = "qwen2.5:7b",
    OllamaGenerateEndpoint = "/api/generate",
    LibreTranslateUrl = "http://localhost:5000"
};

var service = new AILocalizationService(options);

var result = await service.ProcessAsync("Hello World");
Console.WriteLine(result.En);
Console.WriteLine(result.Ru);
```

You can still wire the client manually:

```csharp
var client = new OllamaClient("http://localhost:11434");
var service = new AILocalizationService(client, options.ToPromtConfig());
```

## Configuration

The NuGet package does not require hardcoded server addresses. Use explicit `AiLocalizationOptions`, or let the package read defaults from environment variables:

```csharp
var options = AiLocalizationOptions.FromEnvironment();
var service = new AILocalizationService(options);
```

Supported environment variables:

```text
LIZERIUM_OLLAMA_URL
LIZERIUM_OLLAMA_MODEL
LIZERIUM_OLLAMA_GENERATE_ENDPOINT
LIZERIUM_LIBRETRANSLATE_URL
LIZERIUM_AI_TIMEOUT_SECONDS
```

Defaults:

```text
Ollama URL: http://localhost:11434
Ollama model: qwen2.5:7b
Ollama endpoint: /api/generate
LibreTranslate URL: http://localhost:5000
Request timeout: 30 seconds
```

The analyzer package uses this library internally. Application projects normally install `Lizerium.Localization.Toolkit` instead of referencing this package directly.

`Lizerium.Localization.Ai.Analyzer` also reads the same environment variables, because analyzers do not have a normal application settings file. The XAML VSIX has its own Visual Studio options page and passes those values into the same AI core.
