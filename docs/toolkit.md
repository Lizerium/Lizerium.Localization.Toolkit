# Lizerium.Localization.Toolkit

All-in-one localization package for .NET projects that use `.resx` translation files.

It includes:

- `Lizerium.Localization.Core` as the runtime dependency;
- an incremental source generator that creates `Generated.Localization.Localization`;
- analyzer diagnostics for missing localization keys;
- a Visual Studio CodeFix that creates missing `.resx` entries from code.

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

For parameterized values, use `_Format` keys with placeholders:

```xml
<data name="MainWindow_Message_Format" xml:space="preserve">
  <value>Current folder: {0}</value>
</data>
```

## CodeFix

Write the generated call first, then use `Ctrl + . -> Create localization key`. Calls with arguments create `_Format` keys and `TODO {0} {1}` placeholder values.

Project repository: https://github.com/Lizerium/Lizerium.Localization.Toolkit
