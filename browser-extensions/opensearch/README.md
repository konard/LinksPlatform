# LinksPlatform OpenSearch Plugins

OpenSearch XML descriptors for adding LinksPlatform search engines to your web browser. These plugins work with Firefox, Edge, Safari (with extensions), and other browsers that support the OpenSearch standard.

## Available Search Plugins

### 1. LinksPlatform GitHub Search (`linksplatform-github.xml`)
Search code across all LinksPlatform repositories (both `linksplatform` and `konard` organizations).

**Keyword suggestion**: `lp` or `linksplatform`

### 2. LinksPlatform Repositories (`linksplatform-repos.xml`)
Search for repositories by name in the LinksPlatform organization.

**Keyword suggestion**: `lprepo` or `lp-repo`

### 3. LinksPlatform Issues (`linksplatform-issues.xml`)
Search issues and pull requests across all LinksPlatform repositories.

**Keyword suggestion**: `lpissue` or `lp-issue`

### 4. LinksPlatform Documentation (`linksplatform-docs.xml`)
Search documentation on linksplatform.github.io sites.

**Keyword suggestion**: `lpdocs` or `lp-docs`

## Installation

### Firefox

#### Method 1: Manual Installation
1. Open Firefox and navigate to `about:preferences#search`
2. Scroll down to "Search Shortcuts"
3. Click "Add" at the bottom
4. Browse to the XML file you want to install
5. The search engine will be added to your search shortcuts

#### Method 2: Using Add-ons
1. Install the "Add custom search engine" add-on from Mozilla Add-ons
2. Visit a page hosting the OpenSearch XML file
3. Click the add-on icon to install the search engine

#### Method 3: Direct Link (if hosted)
If these XML files are hosted on a web server, Firefox can auto-detect them when the page includes:
```html
<link rel="search" type="application/opensearchdescription+xml"
      title="LinksPlatform" href="/path/to/linksplatform-github.xml">
```

### Microsoft Edge

1. Visit a website that has the OpenSearch descriptor linked in its HTML
2. Edge will automatically detect and offer to add the search engine
3. Alternatively, manually add via: `edge://settings/searchEngines`
   - Click "Add"
   - Enter the search URL: `https://github.com/search?q=org%3Alinksplatform+org%3Akonard+%s&type=code`
   - Replace `%s` with your search term

### Google Chrome

Chrome doesn't directly support OpenSearch XML installation, but you can manually add search engines:

1. Go to `chrome://settings/searchEngines`
2. Click "Add" under "Site Search"
3. Fill in:
   - **Name**: LinksPlatform GitHub
   - **Shortcut**: lp
   - **URL**: `https://github.com/search?q=org%3Alinksplatform+org%3Akonard+%s&type=code`

For better Chrome integration, use the [LinksPlatform Chrome Extension](../chrome-extension/) instead.

### Safari

Safari requires an extension for custom search engines:

1. Install "Any Search" or "Keyword Search" extension from the App Store
2. Use the extension to add custom search engines using the URLs from the XML files

## Manual Search Engine Configuration

If your browser doesn't support OpenSearch XML, you can manually add these search URLs:

| Search Type | URL Template |
|------------|--------------|
| **Code Search** | `https://github.com/search?q=org%3Alinksplatform+org%3Akonard+{searchTerms}&type=code` |
| **Repository Search** | `https://github.com/search?q=org%3Alinksplatform+{searchTerms}+in%3Aname&type=repositories` |
| **Issues Search** | `https://github.com/search?q=org%3Alinksplatform+org%3Akonard+{searchTerms}&type=issues` |
| **Docs Search** | `https://www.google.com/search?q=site%3Alinksplatform.github.io+{searchTerms}` |

Replace `{searchTerms}` with `%s` for most browsers.

## Usage

After installation:

1. In your browser's address bar, type your search keyword (e.g., `lp`)
2. Press `Tab` or `Space`
3. Type your search query
4. Press `Enter`

Alternatively, you can select the search engine from your browser's search bar dropdown.

## Hosting the OpenSearch Files

To enable auto-detection in browsers, host these XML files on a web server and add this to your HTML `<head>`:

```html
<!-- LinksPlatform Search Plugins -->
<link rel="search" type="application/opensearchdescription+xml"
      title="LinksPlatform GitHub"
      href="https://yourdomain.com/browser-extensions/opensearch/linksplatform-github.xml">
<link rel="search" type="application/opensearchdescription+xml"
      title="LinksPlatform Repositories"
      href="https://yourdomain.com/browser-extensions/opensearch/linksplatform-repos.xml">
<link rel="search" type="application/opensearchdescription+xml"
      title="LinksPlatform Issues"
      href="https://yourdomain.com/browser-extensions/opensearch/linksplatform-issues.xml">
<link rel="search" type="application/opensearchdescription+xml"
      title="LinksPlatform Docs"
      href="https://yourdomain.com/browser-extensions/opensearch/linksplatform-docs.xml">
```

## Technical Details

These OpenSearch descriptors follow the [OpenSearch 1.1 specification](http://www.opensearch.org/Specifications/OpenSearch/1.1).

### File Format

```xml
<?xml version="1.0" encoding="UTF-8"?>
<OpenSearchDescription xmlns="http://a9.com/-/spec/opensearch/1.1/">
  <ShortName>Engine Name</ShortName>
  <Description>Description of search engine</Description>
  <Tags>tags keywords</Tags>
  <Contact>email@example.com</Contact>
  <Url type="text/html" template="https://example.com/search?q={searchTerms}"/>
  <Image height="16" width="16" type="image/x-icon">https://example.com/favicon.ico</Image>
  <InputEncoding>UTF-8</InputEncoding>
  <SearchForm>https://example.com</SearchForm>
</OpenSearchDescription>
```

## Contributing

To add or modify search plugins:

1. Edit or create a new `.xml` file in this directory
2. Follow the OpenSearch 1.1 specification
3. Test the plugin in multiple browsers
4. Submit a pull request

## References

- [OpenSearch specification](http://www.opensearch.org/Specifications/OpenSearch/1.1)
- [MDN: OpenSearch description format](https://developer.mozilla.org/en-US/docs/Web/OpenSearch)
- [How to create search plugins (HowToGeek)](http://www.howtogeek.com/114176/how-to-easily-create-search-plugins-add-any-search-engine-to-your-browser/)
