# Lizerium.Localization.Ai.Analyzer

Roslyn analyzer and AI CodeFix provider for `Lizerium.Localization.Toolkit`.

The analyzer detects localizable C# text and offers a CodeFix that asks the AI localization service for translations, writes keys to `Strings.en.resx` and `Strings.ru.resx`, and replaces source text with generated localization API calls.

## Install

```xml
<PackageReference Include="Lizerium.Localization.Analyzer" Version="1.0.0"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false"
                  PrivateAssets="all" />
```

## Project Setup

```xml
<ItemGroup>
  <AdditionalFiles Include="Resources\Localization\*.resx" />
</ItemGroup>
```

## Diagnostics

| ID | Meaning |
| --- | --- |
| `AI001` | Localizable C# string or interpolated string was detected |

## CodeFix

Use `Ctrl + . -> Create localization key (AI)`.

For a plain string:

```csharp
var text = "Hello World";
```

the CodeFix creates a key such as `MainWindow_Render_Text1`, writes AI-generated `en` and `ru` values, and replaces the literal with:

```csharp
var text = L.MainWindow.Render.Text1();
```

For an interpolated string:

```csharp
var details = $"Log directory: {AppContext.BaseDirectory} | {5}";
```

the CodeFix stores the resource value as `Log directory: {0} | {1}` and replaces the source with:

```csharp
var details = L.MainWindow.Render.Text2(AppContext.BaseDirectory, 5);
```

If the local AI service is unavailable or returns incomplete data, the CodeFix falls back to the original source text so the project remains compilable.

## AI Server Configuration

The analyzer is distributed through the NuGet/toolkit package, so it cannot use a normal application settings file. Configure its AI servers with environment variables before starting Visual Studio:

```text
LIZERIUM_OLLAMA_URL=http://localhost:11434
LIZERIUM_OLLAMA_MODEL=qwen2.5:7b
LIZERIUM_OLLAMA_GENERATE_ENDPOINT=/api/generate
LIZERIUM_LIBRETRANSLATE_URL=http://localhost:5000
LIZERIUM_AI_TIMEOUT_SECONDS=30
```

If variables are not set, these same values are used as defaults. `Lizerium.AI.LocalizationAssistant.Core` also exposes `AiLocalizationOptions` for direct NuGet usage from custom tools or applications.

## XAML

Plain Roslyn analyzers do not provide native quick actions inside the Visual Studio XAML editor. XAML conversion is exposed through `Lizerium.Localization.Core.XamlLocalizationService` and the WPF `{loc:Loc Key}` markup extension. A VSIX or editor command can reuse that service to convert selected XAML text and write RESX keys.

Project repository: https://github.com/Lizerium/Lizerium.Localization.Toolkit
