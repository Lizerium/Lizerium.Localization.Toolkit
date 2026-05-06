# Lizerium.Localization.Toolkit

All-in-one NuGet пакет для .NET/WPF проектов, которые хранят переводы в `.resx`.

## Что входит

- runtime `Lizerium.Localization.Core`;
- source generator для `Generated.Localization.Localization`;
- analyzer для отсутствующих ключей;
- CodeFix для создания `.resx` записей;
- AI CodeFix для обычных и interpolated C# строк;
- XAML локализация через `{loc:Loc Key}`;
- inline-подсказки Visual Studio для сгенерированных C# вызовов локализации.

## Установка

```xml
<PackageReference Include="Lizerium.Localization.Toolkit" Version="1.0.0" />
```

## Структура ресурсов

```text
Resources/
  Localization/
    Strings.en.resx
    Strings.ru.resx
```

```xml
<ItemGroup>
  <AdditionalFiles Include="Resources\Localization\*.resx" />
  <Content Include="Resources\Localization\*.resx">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

## Использование

```csharp
using Lizerium.Localization.Core;
using L = Generated.Localization.Localization;

LocalizationService.Instance.Configure(
    Path.Combine(AppContext.BaseDirectory, "Resources", "Localization"));

LocalizationService.Instance.ChangeLanguage("ru");

var title = L.MainWindow.Title();
```

## AI локализация C# строк

Analyzer предлагает CodeFix на строках:

```csharp
var text = "Hello World";
var details = $"Log directory: {AppContext.BaseDirectory} | {5}";
```

Для NuGet analyzer настройки AI задаются через переменные окружения:

```text
LIZERIUM_OLLAMA_URL
LIZERIUM_OLLAMA_MODEL
LIZERIUM_OLLAMA_GENERATE_ENDPOINT
LIZERIUM_LIBRETRANSLATE_URL
```

Если Ollama запущен на `http://localhost:11434`, можно оставить значения по умолчанию.

## XAML

Для runtime XAML:

```xml
xmlns:loc="clr-namespace:Lizerium.Localization.Core;assembly=Lizerium.Localization.Core"
```

```xml
<Button Content="{loc:Loc MainWindow_Button_English}" />
```

Для лампочки в XAML editor используйте `Lizerium.Localization.Xaml.Vsix`.

## C# inline-подсказки

Для подсказок в C# файлах соберите и установите `Lizerium.Localization.EditorHints`.

```text
src\Lizerium.Localization.EditorHints\bin\Release\net472\Lizerium.Localization.EditorHints.1.0.4.vsix
```

Расширение показывает значения `.resx` рядом с вызовами вроде `L.MainWindow.Title()` и пишет диагностику в `%TEMP%/Lizerium.Localization.EditorHints.log`.
