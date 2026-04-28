# Lizerium.Localization.Generator

Incremental Roslyn source generator for strongly typed `.resx` localization APIs.

The generator reads `.resx` files passed as `AdditionalFiles` and creates:

```csharp
Generated.Localization.Localization
```

## Install

```xml
<PackageReference Include="Lizerium.Localization.Generator" Version="1.0.0"
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

Example key:

```xml
<data name="MainWindow_Log_DirectoryCorrect_Format" xml:space="preserve">
  <value>Directory is correct: {0}</value>
</data>
```

Generated API:

```csharp
Generated.Localization.Localization.MainWindow.Log.DirectoryCorrect(object arg0)
```

The generator reports `LOC001` for missing language keys and `LOC002` for placeholder count mismatches.

Project repository: https://github.com/Lizerium/Lizerium.Localization.Toolkit
