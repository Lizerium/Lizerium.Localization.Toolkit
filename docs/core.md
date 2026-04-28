# Lizerium.Localization.Core

Runtime library for `Lizerium.Localization.Toolkit`.

This package contains:

- `LocalizationService` for loading `.resx` files at runtime;
- language switching;
- string formatting through numbered placeholders;
- `.resx` read/write helpers used by the toolkit and editor.

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

For strongly typed access, install `Lizerium.Localization.Toolkit` or combine this package with `Lizerium.Localization.Generator`.

Project repository: https://github.com/Lizerium/Lizerium.Localization.Toolkit
