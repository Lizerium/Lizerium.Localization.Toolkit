# Lizerium.AI.LocalizationAssistant.Core

Библиотека с ядром AI-локализации для Ollama и LibreTranslate. Ее используют AI-анализатор C# строк и XAML VSIX, но пакет можно подключать напрямую в свои инструменты.

## Что внутри

- `AILocalizationService` преобразует исходный текст в значения `en` и `ru`.
- `OllamaClient` вызывает локальный или сетевой Ollama сервер.
- `AiLocalizationOptions` задает адреса серверов, модель и endpoint без хардкода.
- `LocalizationResult` хранит переводы и ошибки восстановления.

## Быстрый старт

```csharp
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

Console.WriteLine(result?.En);
Console.WriteLine(result?.Ru);
```

## Настройка через окружение

Если пакет используется из analyzer или CI, удобно задавать настройки переменными окружения:

```text
LIZERIUM_OLLAMA_URL=http://localhost:11434
LIZERIUM_OLLAMA_MODEL=qwen2.5:7b
LIZERIUM_OLLAMA_GENERATE_ENDPOINT=/api/generate
LIZERIUM_LIBRETRANSLATE_URL=http://localhost:5000
LIZERIUM_AI_TIMEOUT_SECONDS=30
```

```csharp
var options = AiLocalizationOptions.FromEnvironment();
var service = new AILocalizationService(options);
```

## Значения по умолчанию

```text
Ollama URL: http://localhost:11434
Ollama model: qwen2.5:7b
Ollama endpoint: /api/generate
LibreTranslate URL: http://localhost:5000
Request timeout: 30 seconds
```

## Где используется

- `Lizerium.Localization.Ai.Analyzer` читает эти переменные окружения из Visual Studio/процесса сборки.
- `Lizerium.Localization.Xaml.Vsix` имеет отдельную страницу `Tools -> Options` и передает значения в это же ядро.
- Пользовательские утилиты могут напрямую создавать `AiLocalizationOptions`.
