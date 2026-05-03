# Lizerium.Localization.Core

Runtime library for `Lizerium.Localization.Toolkit`.

This package contains:

- `LocalizationService` for loading `.resx` files at runtime;
- language switching;
- string formatting through numbered placeholders;
- `.resx` read/write helpers used by the toolkit and editor;
- WPF `LocExtension` for XAML values;
- `XamlLocalizationService` for converting XAML literals to localization keys.

## Install

```xml
<PackageReference Include="Lizerium.Localization.Core" Version="1.0.0" />
```

## Usage

```csharp
using Lizerium.Localization.Core;

LocalizationService.Instance.Configure(
    Path.Combine(AppContext.BaseDirectory, "Resources", "Localization"));

LocalizationService.Instance.ChangeLanguage("en");

var title = LocalizationService.Instance.GetString("MainWindow_Title");
var message = LocalizationService.Instance.Format("MainWindow_Message_Format", "value");
```

## WPF XAML

For WPF projects targeting `net8.0-windows`, add the namespace:

```xml
xmlns:loc="clr-namespace:Lizerium.Localization.Core;assembly=Lizerium.Localization.Core"
```

Then bind literal UI text to RESX keys:

```xml
<Button Content="{loc:Loc MainWindow_Button_English}" />
<TextBlock Text="{loc:Loc MainWindow_Title}" />
```

`LocExtension` resolves keys through `LocalizationService.Instance.GetString`.

## XAML Conversion Helper

`XamlLocalizationService` can update a XAML file and create matching RESX entries:

```csharp
var xaml = new XamlLocalizationService();
xaml.LocalizeText(
    xamlPath: "MainWindow.xaml",
    text: "English",
    key: "MainWindow_Button_English",
    resourcesDirectory: "Resources/Localization");
```

It replaces localizable attributes such as `Content`, `Text`, `Header`, `Title`, and `ToolTip` with `{loc:Loc Key}` and writes `Strings.en.resx` and `Strings.ru.resx`.

The Visual Studio extension project `Lizerium.Localization.Xaml.Vsix` uses this runtime format to provide a XAML light bulb action.

For strongly typed access, install `Lizerium.Localization.Toolkit` or combine this package with `Lizerium.Localization.Generator`.

Project repository: https://github.com/Lizerium/Lizerium.Localization.Toolkit
