<h1 align="center">Lizerium.Localization.Toolkit</h1>

<p align="center">
  <img src="https://shields.dvurechensky.pro/nuget/v/Lizerium.Localization.Toolkit?style=for-the-badge&color=0891b2" alt="NuGet Version">
  <img src="https://shields.dvurechensky.pro/nuget/dt/Lizerium.Localization.Toolkit?style=for-the-badge&color=a3e635" alt="NuGet Downloads">
  <img src="https://shields.dvurechensky.pro/github/license/Dvurechensky/Lizerium.Localization.Toolkit?style=for-the-badge&color=f59e0b" alt="License">
  <img src="https://shields.dvurechensky.pro/github/stars/Dvurechensky/Lizerium.Localization.Toolkit?style=for-the-badge&color=facc15" alt="GitHub Stars">
</p>

<div align="center" style="margin: 20px 0; padding: 10px; background: #1c1917; border-radius: 10px;">
  <strong>🌐 Language: </strong>
  
  <span style="color: #0891b2; margin: 0 10px;">
    ✅ 🇷🇺 Russian (current)
  </span>
  | 
  <a href="./README.md" style="color: #F5F752; margin: 0 10px;">
    🇺🇸 English
  </a>
</div>

---

> [!NOTE]
> Этот проект является частью экосистемы **Lizerium** и относится к направлению:
>
> - [`Lizerium.Tools.Structs`](https://github.com/Lizerium/Lizerium.Tools.Structs)
>
> Если вы ищете связанные инженерные и вспомогательные инструменты, начните оттуда.

---

`Lizerium.Localization.Toolkit` — рабочий набор для локализации .NET-проектов, которые хранят переводы в `.resx`. В одном процессе он закрывает runtime-загрузку переводов, генерацию строго типизированного API, диагностику Roslyn, CodeFix в Visual Studio и отдельный WPF-редактор переводов.

Основной пакет для приложений:

```xml
<PackageReference Include="Lizerium.Localization.Toolkit" Version="1.0.0" />
```

Он подключает runtime-пакет и регистрирует generator/analyzer из NuGet-пакета через `analyzers/dotnet/cs`.

## Пакеты

| Пакет                             | Назначение                                                             |
| --------------------------------- | ---------------------------------------------------------------------- |
| `Lizerium.Localization.Toolkit`   | Пакет “всё в одном”: runtime, generator, analyzer и code fix           |
| `Lizerium.Localization.Core`      | Runtime-чтение/запись `.resx` и `LocalizationService`                  |
| `Lizerium.Localization.Generator` | Incremental source generator для `Generated.Localization.Localization` |
| `Lizerium.Localization.Analyzer`  | Analyzer и CodeFix provider для отсутствующих ключей                   |
| `Lizerium.Localization.GUI`       | Отдельный WPF-редактор переводов                                       |

Раздельные пакеты нужны только если тебе требуется свой layout зависимостей:

```xml
<PackageReference Include="Lizerium.Localization.Core" Version="1.0.0" />

<PackageReference Include="Lizerium.Localization.Generator" Version="1.0.0"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false"
                  PrivateAssets="all" />

<PackageReference Include="Lizerium.Localization.Analyzer" Version="1.0.0"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false"
                  PrivateAssets="all" />
```

## Настройка проекта

Создай файлы переводов:

```text
Resources/
  Localization/
    Strings.en.resx
    Strings.ru.resx
```

Добавь их в `.csproj`:

```xml
<ItemGroup>
  <AdditionalFiles Include="Resources\Localization\*.resx" />
  <Content Include="Resources\Localization\*.resx">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

`AdditionalFiles` нужны generator/analyzer. `Content` копирует `.resx` рядом с приложением, чтобы runtime-сервис мог их прочитать.

## Использование Runtime

Один раз настрой сервис при запуске:

```csharp
using Lizerium.Localization.Core;

LocalizationService.Instance.Configure(
    Path.Combine(AppContext.BaseDirectory, "Resources", "Localization"));

LocalizationService.Instance.ChangeLanguage("en");
```

Используй сгенерированный API:

```csharp
using L = Generated.Localization.Localization;

var title = L.MainWindow.Title();
var message = L.MainWindow.Log.DirectoryCorrect(AppContext.BaseDirectory);
```

## Именование ключей

Ключи делятся по `_` и превращаются во вложенные классы и методы.

| RESX-ключ                         | Сгенерированный API                            |
| --------------------------------- | ---------------------------------------------- |
| `MainWindow_Title`                | `Localization.MainWindow.Title()`              |
| `FactionView_Tooltip_Highlight`   | `Localization.FactionView.Tooltip.Highlight()` |
| `Settings_Log_FileCreated_Format` | `Localization.Settings.Log.FileCreated(arg0)`  |

Для строк с параметрами используй `_Format`:

```xml
<data name="MainWindow_Log_DirectoryCorrect_Format" xml:space="preserve">
  <value>Directory is correct: {0}</value>
</data>
```

## Диагностика и CodeFix

Generator сообщает:

| ID       | Значение                                         |
| -------- | ------------------------------------------------ |
| `LOC001` | Ключ есть в одном языке, но отсутствует в другом |
| `LOC002` | Количество placeholders отличается между языками |

Analyzer сообщает:

| ID       | Значение                                                          |
| -------- | ----------------------------------------------------------------- |
| `LOC100` | В коде вызван метод локализации, но подходящего `.resx`-ключа нет |

Можно сначала написать вызов:

```csharp
using L = Generated.Localization.Localization;

check.ToolTip = L.FactionView.TooltipHighlight();
```

Затем использовать:

```text
Ctrl + . -> Create localization key
```

Для вызова без параметров CodeFix добавит:

```xml
<data name="FactionView_TooltipHighlight" xml:space="preserve">
  <value>TODO</value>
</data>
```

Для вызова с параметрами:

```csharp
var text = L.MainWindow.TestParam.CreateValue(path, "param2", 5);
```

CodeFix добавит:

```xml
<data name="MainWindow_TestParam_CreateValue_Format" xml:space="preserve">
  <value>TODO {0} {1} {2}</value>
</data>
```

После добавления ключей сделай rebuild проекта, чтобы generator обновил строго типизированный API.

## GUI-редактор

`Lizerium.Localization.GUI` — отдельный WPF-редактор переводов. Он открывает папку проекта, находит `.resx`, сравнивает `en` и `ru`, показывает отсутствующие переводы, находит mismatch placeholders, позволяет редактировать значения inline и сохраняет изменения.

Публикация desktop-приложения:

```powershell
dotnet publish src\Lizerium.Localization.GUI\Lizerium.Localization.GUI.csproj -c Release -r win-x64 --self-contained false -o artifacts\gui
```

Его можно добавить в Visual Studio через `Tools -> External Tools...`:

```text
Title:     Lizerium Localization
Command:   path\to\Lizerium.Localization.GUI.exe
Arguments: $(ProjectDir)
```

## Сборка пакетов

Локальная сборка NuGet-пакетов:

```powershell
dotnet pack src\Lizerium.Localization.Core\Lizerium.Localization.Core.csproj -c Release -o artifacts\nuget
dotnet pack src\Lizerium.Localization.Generator\Lizerium.Localization.Generator.csproj -c Release -o artifacts\nuget
dotnet pack src\Lizerium.Localization.Analyzer\Lizerium.Localization.Analyzer.csproj -c Release -o artifacts\nuget
dotnet pack src\Lizerium.Localization.Toolkit\Lizerium.Localization.Toolkit.csproj -c Release -o artifacts\nuget
dotnet pack src\Lizerium.Localization.GUI\Lizerium.Localization.GUI.csproj -c Release -o artifacts\nuget
```

Установка из локального feed:

```powershell
dotnet nuget add source .\artifacts\nuget -n LizeriumLocal
dotnet add path\to\YourProject.csproj package Lizerium.Localization.Toolkit --version 1.0.0 --source .\artifacts\nuget
```

Если во время тестов пересобираешь тот же version `1.0.0`, очисти локальный cache NuGet:

```powershell
dotnet nuget locals global-packages --clear
```

## Пример

Смотри `samples/WpfSampleApp`: там есть минимальный WPF-проект с `.resx`, использованием generated API, runtime-настройкой и переключением языка.

```powershell
dotnet build samples\WpfSampleApp\WpfSampleApp.csproj
dotnet run --project samples\WpfSampleApp\WpfSampleApp.csproj
```
