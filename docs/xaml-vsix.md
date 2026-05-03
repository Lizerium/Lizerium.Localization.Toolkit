# Lizerium.Localization.Xaml.Vsix

Visual Studio extension that adds XAML light bulb actions for WPF localization.

## What It Does

When the caret is inside a XAML literal or text is selected, the extension offers:

```text
Create XAML localization key
```

The action:

- replaces the selected/literal text with `{loc:Loc Key}`;
- adds `xmlns:loc="clr-namespace:Lizerium.Localization.Core;assembly=Lizerium.Localization.Core"` when missing;
- writes the key to `Resources/Localization/Strings.en.resx`;
- writes the key to `Resources/Localization/Strings.ru.resx`;
- uses the Lizerium AI localization service for `en` and `ru` values when Ollama is available;
- falls back to the source text if AI is unavailable;
- works for common UI properties and arbitrary selected XAML text.

## Visual Studio Options

The VSIX adds a Visual Studio options page:

```text
Tools -> Options -> Lizerium Localization -> AI Servers
```

Available settings:

- `Use AI translations`: enable/disable AI-generated resource values.
- `Ollama base URL`: defaults to `http://localhost:11434`.
- `Ollama model`: defaults to `qwen2.5:7b`.
- `Ollama generate endpoint`: defaults to `/api/generate`.
- `Request timeout seconds`: defaults to `30`.
- `LibreTranslate URL`: defaults to `http://localhost:5000`.
- `Fallback to source text`: writes the original text when AI translation fails; otherwise writes `TODO`.

Environment variables are also supported as defaults before the Visual Studio options page is saved:

```text
LIZERIUM_OLLAMA_URL
LIZERIUM_OLLAMA_MODEL
LIZERIUM_OLLAMA_GENERATE_ENDPOINT
LIZERIUM_LIBRETRANSLATE_URL
LIZERIUM_AI_TIMEOUT_SECONDS
```

The suggested action passes Visual Studio's cancellation token to the AI request. Pressing `Cancel` stops the action, and an unavailable AI server falls back after the configured timeout instead of waiting for the default HTTP timeout. If the first request starts a cold Ollama model, increase `Request timeout seconds` to `30` or more.

## Examples

Attribute value:

```xml
<Button Content="English" />
```

becomes:

```xml
<Button Content="{loc:Loc MainWindow_Button_Content}" />
```

Text node:

```xml
<TextBlock>Hello world</TextBlock>
```

becomes:

```xml
<TextBlock Text="{loc:Loc MainWindow_TextBlock_Text}"></TextBlock>
```

## Build

```powershell
dotnet build src\Lizerium.Localization.Xaml.Vsix\Lizerium.Localization.Xaml.Vsix.csproj
```

The VSIX is created at:

```text
src/Lizerium.Localization.Xaml.Vsix/bin/Debug/net472/Lizerium.Localization.Xaml.Vsix.vsix
```

Install it into Visual Studio 2022, reopen a WPF `.xaml` file, place the caret inside a literal or select text, and use the light bulb.

## Diagnostics

The extension writes a small troubleshooting log to:

```text
%TEMP%/Lizerium.Localization.Xaml.Vsix.log
```

If XAML changed but RESX did not, check that log first. The action writes RESX files before editing XAML, so failed file access is now visible there and should prevent half-applied changes.

If a previous run already produced `{loc:Loc Some_Key}` but the RESX entry is missing, place the caret inside that markup extension and run the light bulb again. The extension will create or update the RESX entries for the existing key.

## Local Install Notes

When a VSIX is installed directly from a local folder, Visual Studio's Extensions dialog only shows metadata from `source.extension.vsixmanifest`: icon, display name, author, version, description, and the More Info link. It does not render this markdown documentation as a Marketplace details page. The full formatted documentation is visible after publishing to Visual Studio Marketplace, or by opening this file in the repository.

## Publish To Visual Studio Marketplace

1. Build a release VSIX:

```powershell
dotnet build src\Lizerium.Localization.Xaml.Vsix\Lizerium.Localization.Xaml.Vsix.csproj -c Release
```

2. Verify the artifact:

```text
src/Lizerium.Localization.Xaml.Vsix/bin/Release/net472/Lizerium.Localization.Xaml.Vsix.vsix
```

3. Open the Visual Studio Marketplace publisher portal:

```text
https://marketplace.visualstudio.com/manage
```

4. Create or select the `Dvurechensky` publisher.

5. Choose `New extension`, upload the `.vsix`, and fill in:

- Display name: `Lizerium Localization XAML Tools`
- Categories: `Coding`, `Programming Languages`, `Other`
- Tags: `Localization`, `XAML`, `WPF`, `RESX`, `Lizerium`
- Repository: `https://github.com/Lizerium/Lizerium.Localization.Toolkit`

6. Add screenshots that show the light bulb, the generated `{loc:Loc Key}` markup, and the created RESX entries.

7. Publish as private first, install from Marketplace, test on `samples/WpfSampleApp`, then switch visibility to public.
