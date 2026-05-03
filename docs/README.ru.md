# Документация Lizerium.Localization.Toolkit

Эта папка содержит markdown-документацию и статический сайт для GitHub Pages.

## Основные страницы

- [Toolkit](toolkit.ru.md)
- [Core](core.ru.md)
- [Generator](generator.ru.md)
- [Analyzer](analyzer.ru.md)
- [AI Analyzer](analyzer.ai.ru.md)
- [AI Core](ai-core.ru.md)
- [GUI](gui.ru.md)
- [XAML VSIX](xaml-vsix.ru.md)
- [Публикация релиза](PUSH_RELEASE.ru.md)

## GitHub Pages

Стартовая страница:

```text
docs/index.html
```

Русская версия:

```text
docs/ru/index.html
```

Переключатель языка сохраняет выбор пользователя в `localStorage`:

```text
lizerium.localization.docs.language
```

Если сохраненного выбора нет, сайт смотрит язык браузера и открывает английскую или русскую парную страницу, если она существует.

SEO файлы:

```text
docs/sitemap.xml
docs/robots.txt
```

Файла `.nojekyll` намеренно нет: GitHub Pages должен рендерить markdown-доки как `*.html` страницы.
