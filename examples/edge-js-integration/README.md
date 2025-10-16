# LinksPlatform Edge.js Integration

This example demonstrates JavaScript integration for LinksPlatform using [edge-js](https://github.com/agracio/edge-js), which enables running .NET and Node.js code in-process.

## Features

- ✅ Call .NET LinksPlatform APIs from JavaScript
- ✅ In-process communication (32x faster than HTTP)
- ✅ Cross-platform support (Windows, macOS, Linux)
- ✅ Full access to Platform.Data.Doublets functionality
- ✅ Async/await support in both JavaScript and C#

## Prerequisites

- Node.js 16.x or later
- .NET 8.0 SDK
- Linux: gcc, g++, make (for building native modules)

## Installation

```bash
npm install
```

## Building the .NET Component

The C# adapter needs to be built before running the examples:

```bash
dotnet build EdgeJsIntegration.csproj -c Release
```

## Examples

### 1. Simple Edge.js Example

Demonstrates basic edge-js functionality with inline C# code:

```bash
node simple-example.js
```

This shows:
- Basic string manipulation between .NET and JavaScript
- Inline C# code execution
- Simple data exchange

### 2. Full Integration Example

Demonstrates complete LinksPlatform integration:

```bash
node example.js
```

This example shows:
- Creating links
- Reading links
- Updating links
- Deleting links
- Getting link counts
- Using GetOrCreate pattern

### 3. Using as a Module

```javascript
const LinksClient = require('./index');

async function main() {
    const links = new LinksClient('./data.links');

    // Create a link
    const result = await links.createLink(1, 1);
    console.log('Created link:', result.link);

    // Get or create a link
    const link = await links.getOrCreate(2, 3);
    console.log('Link:', link);

    // Count links
    const count = await links.count();
    console.log('Total links:', count.count);
}

main();
```

## API Reference

### LinksClient

#### Constructor

```javascript
const links = new LinksClient(dataFilePath);
```

Creates a new Links client connected to the specified data file.

#### Methods

##### `createLink(source, target)`

Creates a new link with the specified source and target.

```javascript
const result = await links.createLink(1, 2);
// Returns: { success: true, link: <link_id> }
```

##### `getOrCreate(source, target)`

Gets an existing link or creates a new one if it doesn't exist.

```javascript
const result = await links.getOrCreate(1, 2);
// Returns: { success: true, link: <link_id> }
```

##### `update(link, newSource, newTarget)`

Updates an existing link with new source and target values.

```javascript
const result = await links.update(linkId, 3, 4);
// Returns: { success: true }
```

##### `delete(link)`

Deletes the specified link.

```javascript
const result = await links.delete(linkId);
// Returns: { success: true }
```

##### `getLink(link)`

Retrieves information about a specific link.

```javascript
const result = await links.getLink(linkId);
// Returns: { success: true, index: <id>, source: <source>, target: <target> }
```

##### `count()`

Returns the total number of links in the database.

```javascript
const result = await links.count();
// Returns: { success: true, count: <number> }
```

## Architecture

```
┌─────────────────┐
│   Node.js       │
│   JavaScript    │
└────────┬────────┘
         │ edge-js
         │ (in-process)
┌────────▼────────┐
│   LinksAdapter  │
│   C# Wrapper    │
└────────┬────────┘
         │
┌────────▼─────────────────┐
│  Platform.Data.Doublets  │
│  Core Library            │
└──────────────────────────┘
```

## Performance

Edge.js provides in-process communication between Node.js and .NET, which is approximately **32x faster** than HTTP-based interop according to benchmarks.

## Publishing as npm Package

To prepare this for publishing as an npm package:

1. Update `package.json` with your repository information
2. Ensure the .NET assembly is built and included
3. Add `.npmignore` to exclude unnecessary files
4. Publish: `npm publish --access public`

## References

- [Edge.js Documentation](https://github.com/agracio/edge-js)
- [LinksPlatform Documentation](https://github.com/linksplatform)
- [Platform.Data.Doublets](https://github.com/linksplatform/Data.Doublets)

## License

MIT

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
