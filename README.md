<h1 align="center">Lizerium.Localization.Toolkit</h1>

<p align="center">
  <img src="https://shields.dvurechensky.pro/nuget/v/Lizerium.Localization.Toolkit?style=for-the-badge&color=0891b2" alt="NuGet Version">
  <img src="https://shields.dvurechensky.pro/nuget/dt/Lizerium.Localization.Toolkit?style=for-the-badge&color=a3e635" alt="NuGet Downloads">
  <img src="https://shields.dvurechensky.pro/github/license/Dvurechensky/Lizerium.Localization.Toolkit?style=for-the-badge&color=f59e0b" alt="License">
  <img src="https://shields.dvurechensky.pro/github/stars/Dvurechensky/Lizerium.Localization.Toolkit?style=for-the-badge&color=facc15" alt="GitHub Stars">
</p>

<div align="center" style="margin: 20px 0; padding: 10px; background: #1c1917; border-radius: 10px;">
  <strong>🌐 Language: </strong>
  
  <a href="./README.ru.md" style="color: #F5F752; margin: 0 10px;">
    🇷🇺 Russian
  </a>
  | 
  <span style="color: #0891b2; margin: 0 10px;">
    ✅ 🇺🇸 English (current)
  </span>
</div>

---

> [!NOTE]
> This project is part of the **Lizerium** ecosystem and belongs to the following project:
>
> - [`Lizerium.Tools.Structs`](https://github.com/Lizerium/Lizerium.Tools.Structs)
>
> If you're looking for related engineering and support tools, start there.

---

`Lizerium.Localization.Toolkit` is a .NET localization workflow for projects that store translations in `.resx` files. It combines runtime loading, a Roslyn source generator, analyzer diagnostics, Visual Studio code fixes, and a standalone WPF editor.

The main package is designed for application projects:

```xml
<PackageReference Include="Lizerium.Localization.Toolkit" Version="1.0.0" />
```

It brings the runtime package and registers the generator/analyzer from the NuGet package under `analyzers/dotnet/cs`.

## Packages

| Package                           | Purpose                                                                         |
| --------------------------------- | ------------------------------------------------------------------------------- |
| `Lizerium.Localization.Toolkit`   | All-in-one package for applications: runtime, generator, analyzer, and code fix |
| `Lizerium.Localization.Core`      | Runtime `.resx` reader/writer and `LocalizationService`                         |
| `Lizerium.Localization.Generator` | Incremental source generator for `Generated.Localization.Localization`          |
| `Lizerium.Localization.Analyzer`  | Analyzer and CodeFix provider for missing localization keys                     |
| `Lizerium.Localization.GUI`       | Standalone WPF translation editor                                               |

Use separate packages only when you need a custom package layout:

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

## Project Setup

Create localization files:

```text
Resources/
  Localization/
    Strings.en.resx
    Strings.ru.resx
```

Add them to your project:

```xml
<ItemGroup>
  <AdditionalFiles Include="Resources\Localization\*.resx" />
  <Content Include="Resources\Localization\*.resx">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

`AdditionalFiles` feeds the generator and analyzer. `Content` copies the `.resx` files next to the application so the runtime service can load them.

## Runtime Usage

Configure the service once during startup:

```csharp
using Lizerium.Localization.Core;

LocalizationService.Instance.Configure(
    Path.Combine(AppContext.BaseDirectory, "Resources", "Localization"));

LocalizationService.Instance.ChangeLanguage("en");
```

Use the generated API:

```csharp
using L = Generated.Localization.Localization;

var title = L.MainWindow.Title();
var message = L.MainWindow.Log.DirectoryCorrect(AppContext.BaseDirectory);
```

## Key Naming

Keys are split by `_` and become nested classes and methods.

| RESX key                          | Generated API                                  |
| --------------------------------- | ---------------------------------------------- |
| `MainWindow_Title`                | `Localization.MainWindow.Title()`              |
| `FactionView_Tooltip_Highlight`   | `Localization.FactionView.Tooltip.Highlight()` |
| `Settings_Log_FileCreated_Format` | `Localization.Settings.Log.FileCreated(arg0)`  |

Use `_Format` for values with placeholders:

```xml
<data name="MainWindow_Log_DirectoryCorrect_Format" xml:space="preserve">
  <value>Directory is correct: {0}</value>
</data>
```

## Diagnostics And Code Fix

The generator reports:

| ID       | Meaning                                                |
| -------- | ------------------------------------------------------ |
| `LOC001` | A key exists in one language but is missing in another |
| `LOC002` | Placeholder counts differ between languages            |

The analyzer reports:

| ID       | Meaning                                                                      |
| -------- | ---------------------------------------------------------------------------- |
| `LOC100` | Code calls a localization method but the matching `.resx` key does not exist |

You can write the generated call first:

```csharp
using L = Generated.Localization.Localization;

check.ToolTip = L.FactionView.TooltipHighlight();
```

Then use:

```text
Ctrl + . -> Create localization key
```

For calls without parameters, the code fix adds:

```xml
<data name="FactionView_TooltipHighlight" xml:space="preserve">
  <value>TODO</value>
</data>
```

For calls with parameters:

```csharp
var text = L.MainWindow.TestParam.CreateValue(path, "param2", 5);
```

the code fix adds:

```xml
<data name="MainWindow_TestParam_CreateValue_Format" xml:space="preserve">
  <value>TODO {0} {1} {2}</value>
</data>
```

Rebuild the project after adding keys so the generator can refresh the strongly typed API.

## GUI Editor

`Lizerium.Localization.GUI` is a standalone WPF editor for translation files. It can open a project folder, find `.resx` files, compare `en` and `ru`, highlight missing translations, detect placeholder mismatches, edit values inline, and save changes.

Publish it as a desktop application:

```powershell
dotnet publish src\Lizerium.Localization.GUI\Lizerium.Localization.GUI.csproj -c Release -r win-x64 --self-contained false -o artifacts\gui
```

You can also register it in Visual Studio through `Tools -> External Tools...`:

```text
Title:     Lizerium Localization
Command:   path\to\Lizerium.Localization.GUI.exe
Arguments: $(ProjectDir)
```

## Build Packages

Create local packages:

```powershell
dotnet pack src\Lizerium.Localization.Core\Lizerium.Localization.Core.csproj -c Release -o artifacts\nuget
dotnet pack src\Lizerium.Localization.Generator\Lizerium.Localization.Generator.csproj -c Release -o artifacts\nuget
dotnet pack src\Lizerium.Localization.Analyzer\Lizerium.Localization.Analyzer.csproj -c Release -o artifacts\nuget
dotnet pack src\Lizerium.Localization.Toolkit\Lizerium.Localization.Toolkit.csproj -c Release -o artifacts\nuget
dotnet pack src\Lizerium.Localization.GUI\Lizerium.Localization.GUI.csproj -c Release -o artifacts\nuget
```

Install from the local feed:

```powershell
dotnet nuget add source .\artifacts\nuget -n LizeriumLocal
dotnet add path\to\YourProject.csproj package Lizerium.Localization.Toolkit --version 1.0.0 --source .\artifacts\nuget
```

If you repack the same version while testing, clear the local NuGet cache:

```powershell
dotnet nuget locals global-packages --clear
```

## Sample

See `samples/WpfSampleApp` for a minimal WPF project with `.resx` files, generated API usage, runtime initialization, and language switching.

```powershell
dotnet build samples\WpfSampleApp\WpfSampleApp.csproj
dotnet run --project samples\WpfSampleApp\WpfSampleApp.csproj
```
