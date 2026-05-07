# Changelog / Журнал изменений

## 1.0.5 - 2026-05-07

### English | [Russian](CHANGELOG.ru.md)

#### Fixed

- Fixed a critical `Lizerium.Localization.Xaml.Vsix` bug introduced after the previous release.
- Bumped `Lizerium.Localization.Xaml.Vsix` to `1.0.5` while keeping other library and extension versions independent.
- Bumped NuGet packages to `1.0.5` for the analyzer packaging fix.

## 1.0.4 - 2026-05-07

### English | [Russian](CHANGELOG.ru.md)

#### Added

- Added `Lizerium.Localization.EditorHints`, a separate Visual Studio 2022 VSIX that shows inline C# editor hints for generated localization calls such as `L.MainWindow.Title()`.
- Editor hints resolve values from `Resources/Localization/Strings*.resx`.
- Hint language follows the Visual Studio UI language and falls back to English when a matching RESX file is not available.
- Editor hints refresh RESX values without reopening the C# file by tracking `Strings*.resx` timestamps.
- Release workflow now builds and publishes both Visual Studio extensions:
  - `Lizerium.Localization.Xaml.Vsix`
  - `Lizerium.Localization.EditorHints`
- Release assets now include this bilingual `CHANGELOG.md`.

#### Changed

- Kept the release version aligned at `1.0.4` across NuGet packages and both VSIX extensions.
- Source generator XML documentation now includes localization keys and available `en` / `ru` values, so Quick Info can show useful text even without the editor hints VSIX.
- VSIX packaging was tightened so Visual Studio assemblies are not bundled into the extension package.
- The WPF sample demonstrates generated localization API calls and inline hint behavior.

#### Fixed

- Avoided MEF composition breakage caused by bundling or registering unsafe editor-layer components.
- Fixed stale editor hint values after new RESX keys are generated.
- Fixed hint placement so multiple hints align in a readable right-side column.
- Preserved the existing XAML VSIX behavior and version line while adding the new editor hints VSIX as a separate extension.
