# Lizerium.Localization.GUI

WPF-редактор `.resx` переводов.

## Возможности

- открыть папку проекта;
- найти `Resources/Localization/*.resx`;
- сравнить `en` и `ru`;
- подсветить отсутствующие переводы;
- проверить несовпадение placeholders;
- редактировать значения и сохранять `.resx`.

## Публикация

```powershell
dotnet publish src\Lizerium.Localization.GUI\Lizerium.Localization.GUI.csproj -c Release -r win-x64 --self-contained false -o artifacts\gui
```

## Подключение к Visual Studio

Можно добавить как внешний инструмент:

```text
Tools -> External Tools...
Title: Lizerium Localization
Command: path\to\Lizerium.Localization.GUI.exe
Arguments: $(ProjectDir)
```

