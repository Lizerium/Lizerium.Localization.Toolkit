# Lizerium.Localization.Toolkit Documentation

This folder contains repository markdown docs and a static GitHub Pages site.

## Markdown Docs

- [Toolkit](toolkit.md)
- [Core](core.md)
- [Generator](generator.md)
- [Analyzer](analyzer.md)
- [AI Analyzer](analyzer.ai.md)
- [AI Core](ai-core.md)
- [GUI](gui.md)
- [XAML VSIX](xaml-vsix.md)
- [Release push](PUSH_RELEASE.md)

Russian versions use the `.ru.md` suffix.

The static site language switcher stores the user's choice in `localStorage` under:

```text
lizerium.localization.docs.language
```

If no language was selected yet, the site uses the browser language and redirects to the matching English/Russian page when a paired page exists.

## GitHub Pages

Entry page:

```text
docs/index.html
```

Russian page:

```text
docs/ru/index.html
```

SEO files:

```text
docs/sitemap.xml
docs/robots.txt
```

There is intentionally no `.nojekyll` file: GitHub Pages should render markdown docs as `*.html` pages.
