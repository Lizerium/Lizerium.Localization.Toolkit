# Lizerium.Localization.Toolkit

All-in-one localization package for .NET projects that use `.resx` translation files.

It includes:

- `Lizerium.Localization.Core` as the runtime dependency;
- an incremental source generator that creates `Generated.Localization.Localization`;
- analyzer diagnostics for missing localization keys;
- a Visual Studio CodeFix that creates missing `.resx` entries from code;
- AI CodeFix support for normal and interpolated C# strings;
- WPF XAML runtime localization through `{loc:Loc Key}`.

## Install

```xml
<PackageReference Include="Lizerium.Localization.Toolkit" Version="1.0.0" />
```

## Project Setup

```xml
<ItemGroup>
  <AdditionalFiles Include="Resources\Localization\*.resx" />
  <Content Include="Resources\Localization\*.resx">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

Expected files:

```text
Resources/
  Localization/
    Strings.en.resx
    Strings.ru.resx
```

## Usage

```csharp
using Lizerium.Localization.Core;
using L = Generated.Localization.Localization;

LocalizationService.Instance.Configure(
    Path.Combine(AppContext.BaseDirectory, "Resources", "Localization"));
LocalizationService.Instance.ChangeLanguage("en");

var title = L.MainWindow.Title();
```

For WPF XAML values, add the namespace:

```xml
xmlns:loc="clr-namespace:Lizerium.Localization.Core;assembly=Lizerium.Localization.Core"
```

and use RESX keys directly:

```xml
<Button Content="{loc:Loc MainWindow_Button_English}" />
<TextBlock Text="{loc:Loc MainWindow_Title}" />
```

For parameterized values, use `_Format` keys with placeholders:

```xml
<data name="MainWindow_Message_Format" xml:space="preserve">
  <value>Current folder: {0}</value>
</data>
```

## CodeFix

Write the generated call first, then use `Ctrl + . -> Create localization key`. Calls with arguments create `_Format` keys and `TODO {0} {1}` placeholder values.

The AI CodeFix also appears on string literals and interpolated strings:

```csharp
var text = "Hello World";
var details = $"Log directory: {AppContext.BaseDirectory} | {5}";
```

For interpolated strings, the resource value is stored as `Log directory: {0} | {1}` and the generated call receives the original interpolation expressions as arguments.

The NuGet analyzer reads AI server settings from environment variables:

```text
LIZERIUM_OLLAMA_URL
LIZERIUM_OLLAMA_MODEL
LIZERIUM_OLLAMA_GENERATE_ENDPOINT
LIZERIUM_LIBRETRANSLATE_URL
```

Direct consumers of `Lizerium.AI.LocalizationAssistant.Core` can use `AiLocalizationOptions` instead of hardcoded URLs.

## XAML Conversion

`Lizerium.Localization.Core.XamlLocalizationService` provides the shared conversion logic used by tooling:

```csharp
var xaml = new XamlLocalizationService();
xaml.LocalizeText(
    "MainWindow.xaml",
    "English",
    "MainWindow_Button_English",
    "Resources/Localization");
```

It replaces XAML literals like `Content="English"` with `Content="{loc:Loc MainWindow_Button_English}"`, adds `xmlns:loc` when missing, and writes keys to `Strings.en.resx` and `Strings.ru.resx`.

For an editor light bulb in Visual Studio 2022, build and install `Lizerium.Localization.Xaml.Vsix`. It offers `Создать ключ локализации для XAML` for selected XAML text or literal attribute values.

Project repository: https://github.com/Lizerium/Lizerium.Localization.Toolkit
