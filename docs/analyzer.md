# Lizerium.Localization.Analyzer

Roslyn analyzer and CodeFix provider for `Lizerium.Localization.Toolkit`.

The analyzer detects calls to generated localization methods when the matching `.resx` key is missing. The CodeFix can create the key in `Strings.en.resx` and `Strings.ru.resx`.

## Install

```xml
<PackageReference Include="Lizerium.Localization.Analyzer" Version="1.0.0"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false"
                  PrivateAssets="all" />
```

## Project Setup

```xml
<ItemGroup>
  <AdditionalFiles Include="Resources\Localization\*.resx" />
</ItemGroup>
```

## Diagnostics

| ID | Meaning |
| --- | --- |
| `LOC100` | Code calls a generated localization method but no matching `.resx` key exists |

## CodeFix

Use `Ctrl + . -> Create localization key`.

For a call without arguments:

```csharp
L.FactionView.TooltipHighlight();
```

the CodeFix creates `FactionView_TooltipHighlight`.

For a call with arguments:

```csharp
L.MainWindow.TestParam.CreateValue(path, "param2", 5);
```

the CodeFix creates `MainWindow_TestParam_CreateValue_Format` with `TODO {0} {1} {2}`.

Project repository: https://github.com/Lizerium/Lizerium.Localization.Toolkit
