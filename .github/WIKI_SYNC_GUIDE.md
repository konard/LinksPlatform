# Wiki Pages Synchronization Guide

This guide helps maintain synchronization between English and Russian wiki pages.

## Overview

The LinksPlatform wiki contains pages in both English and Russian. To ensure consistency and keep both versions up to date, we maintain a mapping of page pairs in `.github/wiki-pages-mapping.json`.

## Page Pairs

The following pages should be kept in sync:

| English | Russian | Description |
|---------|---------|-------------|
| `FAQ.md` | `ЧАВО.md` | Frequently Asked Questions |
| `How-it-all-began.md` | `О-том,-как-всё-начиналось.md` | Philosophical introduction |
| `Coding-conventions.md` | `Соглашения-о-коде.md` | Coding style and conventions |
| `Visualization-primes.md` | `Примитивы-визуализации.md` | Visualization primitives |

## Workflow for Updating Wiki Pages

### When updating an English page:

1. Make your changes to the English wiki page
2. Update the corresponding Russian page with equivalent content
3. Ensure cross-reference links at the top of each page are correct
4. Use the `check-wiki-sync.sh` script to verify sync status

### When updating a Russian page:

1. Make your changes to the Russian wiki page
2. Update the corresponding English page with equivalent content
3. Ensure cross-reference links at the top of each page are correct
4. Use the `check-wiki-sync.sh` script to verify sync status

## Cross-References

Each paired page should have a reference to its translation at the top:

**English pages:**
```markdown
([русская версия](https://github.com/Konard/LinksPlatform/wiki/Russian-Page-Name))
```

**Russian pages:**
```markdown
([english version](https://github.com/Konard/LinksPlatform/wiki/English-Page-Name))
```

## Checking Sync Status

Use the provided script to check if wiki pages need synchronization:

```bash
bash .github/scripts/check-wiki-sync.sh
```

This script will:
- Clone the wiki repository
- Check the last modification dates of paired pages
- Report which pages might be out of sync
- Show the time difference between paired pages

## Best Practices

1. **Update both versions together**: When making significant changes, update both English and Russian versions in the same session to avoid drift.

2. **Maintain structural consistency**: Both versions should have the same structure (headings, sections, code examples).

3. **Preserve meaning**: Translations should preserve the technical meaning and intent of the original content.

4. **Keep cross-references updated**: Always verify that cross-reference links work correctly.

5. **Document changes**: If you make changes to only one version temporarily (e.g., work in progress), leave a comment or note for translators.

## Bilingual Pages

Some pages contain both English and Russian content:
- `Home.md` - Wiki home page
- `Ideas,-идеи.md` - Ideas collection

These pages don't require separate translation updates.

## GitHub Wiki Structure

GitHub wikis are separate git repositories. To edit wiki pages:

1. Clone the wiki repository:
   ```bash
   git clone https://github.com/konard/LinksPlatform.wiki.git
   ```

2. Make your changes to the markdown files

3. Commit and push:
   ```bash
   git add .
   git commit -m "Update wiki pages: [description]"
   git push
   ```

## Automation

While full automation of translation is complex due to technical content requiring human review, this guide and the accompanying tools help maintain awareness of sync status and make the process more manageable.

## Questions?

If you're unsure about translations or sync status, please:
- Open an issue in the main repository
- Ask in the Discord server
- Contact the maintainers

---

*This guide is part of the solution to [issue #70](https://github.com/konard/LinksPlatform/issues/70)*
