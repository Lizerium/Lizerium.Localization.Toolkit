# Lizerium.Localization.Generator

Incremental source generator для строго типизированного доступа к `.resx` ключам.

## Как работает

Генератор читает `.resx` файлы из `AdditionalFiles`:

```xml
<AdditionalFiles Include="Resources\Localization\*.resx" />
```

Ключи вида:

```text
MainWindow_Title
Settings_Log_FileCreated_Format
```

становятся API:

```csharp
Localization.MainWindow.Title()
Localization.Settings.Log.FileCreated(arg0)
```

## Placeholders

Если ключ заканчивается на `_Format`, generator ожидает placeholders:

```xml
<data name="MainWindow_Message_Format" xml:space="preserve">
  <value>Current folder: {0}</value>
</data>
```

В коде:

```csharp
var text = L.MainWindow.Message(AppContext.BaseDirectory);
```

## Диагностика

- `LOC001`: ключ есть в одном языке, но отсутствует в другом.
- `LOC002`: количество placeholders отличается между языками.

