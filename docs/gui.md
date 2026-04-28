# Lizerium.Localization.GUI

Standalone WPF editor for `.resx` translation files used by `Lizerium.Localization.Toolkit`.

The editor can:

- open a project folder;
- find localization `.resx` files;
- compare English and Russian resources;
- highlight missing translations;
- detect placeholder mismatches;
- edit values inline;
- save changes.

## Build

```powershell
dotnet publish src\Lizerium.Localization.GUI\Lizerium.Localization.GUI.csproj -c Release -r win-x64 --self-contained false -o artifacts\gui
```

## Visual Studio External Tool

```text
Title:     Lizerium Localization
Command:   path\to\Lizerium.Localization.GUI.exe
Arguments: $(ProjectDir)
```

Project repository: https://github.com/Lizerium/Lizerium.Localization.Toolkit
