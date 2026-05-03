# Lizerium.Localization.Analyzer

Analyzer и CodeFix для проверки вызовов сгенерированной локализации.

## Диагностика

`LOC100` появляется, когда код вызывает метод локализации, но соответствующий ключ отсутствует в `.resx`.

```csharp
using L = Generated.Localization.Localization;

var text = L.MainWindow.Title();
```

Если ключа `MainWindow_Title` нет в ресурсах, analyzer предложит CodeFix.

## CodeFix

Действие `Create localization key` добавляет ключ в `.resx`:

```xml
<data name="MainWindow_Title" xml:space="preserve">
  <value>TODO</value>
</data>
```

Для вызовов с параметрами создается `_Format` ключ:

```csharp
var text = L.MainWindow.Status.FileCount(count);
```

```xml
<data name="MainWindow_Status_FileCount_Format" xml:space="preserve">
  <value>TODO {0}</value>
</data>
```

После добавления ключей пересоберите проект, чтобы source generator обновил API.

