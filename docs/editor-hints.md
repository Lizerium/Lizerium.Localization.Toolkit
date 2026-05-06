# Lizerium.Localization.EditorHints

Visual Studio 2022 extension that shows inline editor hints for generated Lizerium localization calls in C# files.

## What It Does

When a C# file contains generated localization calls, the extension reads matching `.resx` values and shows the localized text next to the line:

```csharp
var title = L.MainWindow.Title();
var message = L.MainWindow.Log.DirectoryCorrect(AppContext.BaseDirectory);
```

The hint language follows the Visual Studio UI language when a matching resource value exists and falls back to English.

## Install

Build the release VSIX:

```powershell
dotnet build src\Lizerium.Localization.EditorHints\Lizerium.Localization.EditorHints.csproj -c Release
```

The VSIX is created at:

```text
src\Lizerium.Localization.EditorHints\bin\Release\net472\Lizerium.Localization.EditorHints.1.0.4.vsix
```

Install it into Visual Studio 2022, reopen a C# file that uses the generated localization API, and the hints appear next to visible localization calls.

## Diagnostics

The extension writes troubleshooting details to:

```text
%TEMP%/Lizerium.Localization.EditorHints.log
```

Project repository: https://github.com/Lizerium/Lizerium.Localization.Toolkit
