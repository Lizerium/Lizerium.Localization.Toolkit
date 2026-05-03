# Lizerium.Localization.Ai.Analyzer

Roslyn analyzer и CodeFix для автоматической локализации C# строк через AI.

## Что делает

Анализатор находит:

- обычные строки, например `"Hello World"`;
- interpolated strings, например `$"Log directory: {AppContext.BaseDirectory} | {5}"`.

В Visual Studio появляется действие:

```text
Ctrl + . -> Generate localization key (AI)
```

CodeFix:

- создает ключ в `.resx`;
- пишет значения в `Strings.en.resx` и `Strings.ru.resx`;
- сохраняет placeholders `{0}`, `{1}`;
- заменяет строку на вызов сгенерированного API `L.*`.

## Пример

```csharp
var details = $"Log directory: {AppContext.BaseDirectory} | {5}";
```

После CodeFix ресурс хранится как:

```text
Log directory: {0} | {1}
```

А код становится вызовом вида:

```csharp
var details = L.MainWindow.Render.Text2(AppContext.BaseDirectory, 5);
```

## Настройка AI серверов

NuGet analyzer не имеет собственной страницы `Tools -> Options`, потому что запускается внутри Roslyn/IDE. Для него настройки задаются переменными окружения до запуска Visual Studio:

```text
LIZERIUM_OLLAMA_URL=http://localhost:11434
LIZERIUM_OLLAMA_MODEL=qwen2.5:7b
LIZERIUM_OLLAMA_GENERATE_ENDPOINT=/api/generate
LIZERIUM_LIBRETRANSLATE_URL=http://localhost:5000
LIZERIUM_AI_TIMEOUT_SECONDS=30
```

Если Ollama работает локально на `localhost:11434`, можно ничего не настраивать.

## Fallback

Если AI сервер недоступен или вернул неполный ответ, CodeFix использует исходный текст. Проект остается собираемым, а переводы можно поправить позже.

## XAML

Для XAML editor используется отдельный VSIX: `Lizerium.Localization.Xaml.Vsix`. У него есть страница настроек в Visual Studio.
