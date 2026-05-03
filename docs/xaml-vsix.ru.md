# Lizerium.Localization.Xaml.Vsix

Расширение Visual Studio 2022, которое добавляет лампочку для локализации WPF XAML.

## Что делает

Если курсор стоит внутри XAML текста или текст выделен, VSIX предлагает:

```text
Create XAML localization key
```

Действие:

- заменяет текст на `{loc:Loc Key}`;
- автоматически добавляет `xmlns:loc`;
- пишет ключ в `Resources/Localization/Strings.en.resx`;
- пишет ключ в `Resources/Localization/Strings.ru.resx`;
- использует AI Core для перевода через Ollama/LibreTranslate;
- делает fallback на исходный текст, если AI недоступен.

## Настройки Visual Studio

После установки VSIX откройте:

```text
Tools -> Options -> Lizerium Localization -> AI Servers
```

Поля:

- `Use AI translations`;
- `Ollama base URL`;
- `Ollama model`;
- `Ollama generate endpoint`;
- `Request timeout seconds`;
- `LibreTranslate URL`;
- `Fallback to source text`.

Для XAML VSIX переменные окружения не обязательны: удобнее задавать серверы прямо в Visual Studio.

Кнопка `Cancel` в окне suggested action теперь пробрасывается до AI-запроса. Если Ollama или LibreTranslate не отвечают, действие переходит в fallback после `Request timeout seconds`, а не ждёт стандартный HTTP timeout. По умолчанию timeout равен `30` секундам. Если первый запрос поднимает холодную модель Ollama, можно временно поставить больше.

## Пример

```xml
<Button Content="English" />
```

становится:

```xml
<Button Content="{loc:Loc MainWindow_Button_Content_English}" />
```

Если `xmlns:loc` отсутствовал, расширение добавит его в корневой `Window`, `UserControl`, `Page` или `ResourceDictionary`.

## Диагностика

Лог пишется сюда:

```text
%TEMP%/Lizerium.Localization.Xaml.Vsix.log
```

## Локальная установка

При установке `.vsix` из папки Visual Studio показывает только metadata из manifest: логотип, название, описание, автора, версию и ссылку More Info. Полная документация красиво отображается на Marketplace или GitHub Pages.
