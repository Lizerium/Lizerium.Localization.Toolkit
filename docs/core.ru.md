# Lizerium.Localization.Core

Runtime-библиотека для чтения и записи `.resx`, переключения языка и WPF XAML локализации через `{loc:Loc Key}`.

## Подключение

```xml
<PackageReference Include="Lizerium.Localization.Core" Version="1.0.0" />
```

## Runtime

```csharp
using Lizerium.Localization.Core;

LocalizationService.Instance.Configure(
    Path.Combine(AppContext.BaseDirectory, "Resources", "Localization"));

LocalizationService.Instance.ChangeLanguage("ru");
```

## XAML MarkupExtension

Добавьте namespace:

```xml
xmlns:loc="clr-namespace:Lizerium.Localization.Core;assembly=Lizerium.Localization.Core"
```

Используйте ключи прямо в XAML:

```xml
<Button Content="{loc:Loc MainWindow_Button_English}" />
<TextBlock Text="{loc:Loc MainWindow_Title}" />
```

## XamlLocalizationService

Сервис можно использовать из инструментов, VSIX или собственных команд:

```csharp
var xaml = new XamlLocalizationService();
xaml.LocalizeText(
    "MainWindow.xaml",
    "English",
    "MainWindow_Button_English",
    "Resources/Localization");
```

Он заменяет `Content="English"` на `{loc:Loc ...}`, добавляет `xmlns:loc` и пишет ключи в `Strings.en.resx` / `Strings.ru.resx`.

