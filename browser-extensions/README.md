# LinksPlatform Browser Extensions

Browser search adapters and extensions for LinksPlatform, enabling console-like search experience from your browser's address bar (omnibox).

## Overview

This directory contains browser extensions and search plugins that allow you to quickly search LinksPlatform resources directly from your browser without visiting GitHub or documentation sites first.

**Goal**: Use browser search field as a universal input/search/command field to enable console-like (Unix bash/shell, Windows cmd) experience for LinksPlatform resources.

## What's Included

### 1. [OpenSearch Plugins](opensearch/)
Universal search plugins (XML descriptors) compatible with Firefox, Edge, Safari, and other browsers supporting OpenSearch standard.

**Available searches**:
- **Code Search** - Search code across all LinksPlatform repositories
- **Repository Search** - Find repositories by name
- **Issues Search** - Search issues and pull requests
- **Documentation Search** - Search LinksPlatform documentation sites

### 2. [Chrome Extension](chrome-extension/)
Advanced Chrome/Chromium extension with omnibox API integration for rich search suggestions and prefixes.

**Features**:
- Type `lp` in address bar to activate
- Smart prefix modifiers: `repo`, `issue`, `docs`, `code`
- Auto-suggestions as you type
- Works in Chrome, Edge, Brave, and other Chromium browsers

## Quick Start

### For Chrome/Chromium Users
Use the [Chrome Extension](chrome-extension/) for the best experience:

1. Navigate to `chrome://extensions/`
2. Enable "Developer mode"
3. Click "Load unpacked"
4. Select the `browser-extensions/chrome-extension` directory
5. Type `lp` in your address bar and press Space/Tab

### For Firefox Users
Install [OpenSearch plugins](opensearch/):

1. Go to `about:preferences#search`
2. Add the search engines manually using the XML files

### For Other Browsers
See individual README files:
- [OpenSearch README](opensearch/README.md)
- [Chrome Extension README](chrome-extension/README.md)

## Usage Examples

Once installed, search directly from your address bar:

```
lp doublets                    # Search code for "doublets"
lp repo data                   # Search repositories containing "data"
lp issue memory leak           # Search issues about "memory leak"
lp docs ILinks                 # Search documentation for "ILinks"
```

## Architecture

### OpenSearch XML Plugins
Standard OpenSearch 1.1 descriptors that define:
- Search name and description
- Search URL template with `{searchTerms}` placeholder
- Icon and encoding information
- Optional suggestion API endpoints

### Chrome Extension
Manifest V3 extension using:
- **Omnibox API** - Captures `lp` keyword in address bar
- **Background Service Worker** - Handles search logic and URL generation
- **Dynamic Suggestions** - Provides context-aware search options

## Search Targets

All search plugins cover these LinksPlatform resources:

1. **GitHub Organizations**:
   - `linksplatform` - Main organization
   - `konard` - Legacy/related repositories

2. **Content Types**:
   - Source code (`.cs`, `.cpp`, `.rs`, `.py`, etc.)
   - Repository names and descriptions
   - Issues, pull requests, discussions
   - Documentation sites (`*.github.io`)

3. **Search Scopes**:
   - Cross-repository code search
   - Organization-wide issue search
   - Documentation full-text search via Google

## Browser Compatibility

| Browser | OpenSearch XML | Chrome Extension |
|---------|----------------|------------------|
| Chrome | ⚠️ Manual only | ✅ Full support |
| Firefox | ✅ Native | ❌ N/A |
| Edge | ✅ Native | ✅ Compatible |
| Safari | ⚠️ Requires extension | ❌ N/A |
| Brave | ⚠️ Manual only | ✅ Compatible |
| Opera | ⚠️ Manual only | ✅ Compatible |

## Contributing

Contributions are welcome! Here's how you can help:

### Adding New Search Engines
1. Create a new OpenSearch XML file in `opensearch/`
2. Follow the [OpenSearch 1.1 specification](http://www.opensearch.org/Specifications/OpenSearch/1.1)
3. Test in multiple browsers

### Enhancing Chrome Extension
1. Edit `chrome-extension/background.js` for search logic
2. Update `chrome-extension/manifest.json` if adding permissions
3. Test with `chrome://extensions/` in developer mode

### Improving Documentation
- Update README files with clearer instructions
- Add screenshots and examples
- Document browser-specific quirks

## Technical References

- [OpenSearch 1.1 Specification](http://www.opensearch.org/Specifications/OpenSearch/1.1)
- [Chrome Omnibox API](https://developer.chrome.com/docs/extensions/reference/omnibox/)
- [How to create search plugins - HowToGeek](http://www.howtogeek.com/114176/how-to-easily-create-search-plugins-add-any-search-engine-to-your-browser/)
- [Custom Chrome URL suggestions - Stack Overflow](http://stackoverflow.com/questions/13411509/custom-chrome-url-suggestions)

## Related Issues

- [Issue #129: Add search adapters to current web browsers](https://github.com/konard/LinksPlatform/issues/129)

## License

This project follows the same license as the main LinksPlatform repository. See [LICENSE](../LICENSE) for details.
