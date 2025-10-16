# LinksPlatform Chrome Extension

This Chrome extension adds omnibox integration for searching LinksPlatform resources directly from your browser's address bar.

## Installation

### From Source (Developer Mode)

1. Open Chrome and navigate to `chrome://extensions/`
2. Enable "Developer mode" using the toggle in the top right
3. Click "Load unpacked"
4. Select the `browser-extensions/chrome-extension` directory from this repository

### Creating Icons

The extension requires three icon sizes. You can generate them from the included `icon.svg`:

```bash
# Using ImageMagick or similar tools
convert icon.svg -resize 16x16 icon16.png
convert icon.svg -resize 48x48 icon48.png
convert icon.svg -resize 128x128 icon128.png
```

Or use any SVG-to-PNG converter tool to create:
- `icon16.png` (16×16 pixels)
- `icon48.png` (48×48 pixels)
- `icon128.png` (128×128 pixels)

## Usage

1. Type `lp` in your browser's address bar
2. Press `Space` or `Tab`
3. Enter your search query

### Search Modifiers

- **`lp code <query>`** or **`lp c <query>`** - Search code across all LinksPlatform repositories
- **`lp repo <query>`** or **`lp r <query>`** - Search repository names
- **`lp issue <query>`** or **`lp i <query>`** - Search issues and pull requests
- **`lp docs <query>`** or **`lp d <query>`** - Search documentation sites
- **`lp <query>`** (default) - Search code

### Examples

```
lp doublets                    # Search code for "doublets"
lp repo data                   # Search repositories with "data" in name
lp issue memory leak           # Search issues mentioning "memory leak"
lp docs installation           # Search docs for "installation"
```

## Features

- **Quick access**: Type `lp` in the address bar to start searching
- **Context-aware suggestions**: See search type options as you type
- **Multiple search targets**: Code, repositories, issues, and documentation
- **Smart defaults**: Default search is code search across all repos
- **Prefix shortcuts**: Use short prefixes like `r`, `i`, `d`, `c` for quick searches
