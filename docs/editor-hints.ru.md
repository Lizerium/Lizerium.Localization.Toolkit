# Lizerium.Localization.EditorHints

Расширение Visual Studio 2022, которое показывает inline-подсказки для сгенерированных C# вызовов локализации Lizerium.

## Что делает

Когда C# файл содержит вызовы сгенерированной локализации, расширение читает соответствующие значения `.resx` и показывает локализованный текст рядом со строкой:

```csharp
var title = L.MainWindow.Title();
var message = L.MainWindow.Log.DirectoryCorrect(AppContext.BaseDirectory);
```

Язык подсказки следует языку интерфейса Visual Studio, если подходящее значение найдено, и иначе откатывается на английский текст.

## Установка

Соберите release VSIX:

```powershell
dotnet build src\Lizerium.Localization.EditorHints\Lizerium.Localization.EditorHints.csproj -c Release
```

VSIX создаётся здесь:

```text
src\Lizerium.Localization.EditorHints\bin\Release\net472\Lizerium.Localization.EditorHints.1.0.4.vsix
```

Установите его в Visual Studio 2022, заново откройте C# файл с вызовами сгенерированного API локализации, и подсказки появятся рядом с видимыми вызовами.

## Диагностика

Лог для диагностики пишется сюда:

```text
%TEMP%/Lizerium.Localization.EditorHints.log
```

Репозиторий проекта: https://github.com/Lizerium/Lizerium.Localization.Toolkit
