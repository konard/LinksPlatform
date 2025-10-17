# Platform.Data.CMS

Data layer adapters for CMS (Content Management System) integration with LinksPlatform's Doublets.

## Overview

This library provides an experimental data layer that allows CMS systems to use LinksPlatform's Doublets as their underlying storage mechanism. It demonstrates how traditional entity-based CMS architectures (like those used in Orchard, Umbraco, or DNN) can be mapped to the associative link-based data structure.

## Concept

Traditional CMS systems use relational databases with tables for:
- Content items (pages, posts, media)
- Properties (title, content, metadata)
- Relationships (parent-child, references)

This library maps these concepts to Links (Doublets):
- **Content Items**: Stored as links with semantic markers
- **Properties**: Represented as key-value pairs connected to content items
- **Relationships**: Direct links between content items with type markers
- **Content Types**: Defined as string markers that categorize content

## Core Components

### ICMSContentStorage<TLink, TContentId>

Generic interface for CMS content operations:
- `CreateContent`: Create new content items with properties
- `GetContent`: Retrieve content and its properties
- `UpdateContent`: Modify existing content properties
- `DeleteContent`: Remove content items
- `QueryContent`: Find content by type and filters
- `CreateRelationship`: Link content items together
- `GetRelatedContent`: Navigate relationships

### LinksBasedCMSStorage<TLink>

Implementation using LinksPlatform's Doublets:
- Maps CMS entities to binary links
- Uses semantic markers for type differentiation
- Stores strings as Unicode sequences
- Maintains relationships as typed links

## Example Usage

```csharp
using var links = new UnitedMemoryLinks<uint>();
var cmsStorage = new LinksBasedCMSStorage<uint>(links);

// Create a blog post
var postId = cmsStorage.CreateContent("BlogPost", new Dictionary<string, object>
{
    { "Title", "My First Post" },
    { "Content", "Hello World!" },
    { "Status", "Published" }
});

// Create a media item
var imageId = cmsStorage.CreateContent("Media", new Dictionary<string, object>
{
    { "FileName", "header.jpg" },
    { "FilePath", "/media/header.jpg" }
});

// Link them together
cmsStorage.CreateRelationship(postId, imageId, "featured-image");

// Query published posts
var posts = cmsStorage.QueryContent("BlogPost", new Dictionary<string, object>
{
    { "Status", "Published" }
});

// Get related content
var relatedMedia = cmsStorage.GetRelatedContent(postId, "featured-image");
```

## Target CMS Systems

This library is designed to explore integration with:

1. **Orchard CMS** - Modular ASP.NET CMS with extensible content types
2. **Umbraco** - .NET Core CMS with flexible content modeling
3. **DNN Platform** - Enterprise-grade .NET CMS with module architecture

## Architecture

```
CMS Application Layer
        ↓
ICMSContentStorage (abstraction)
        ↓
LinksBasedCMSStorage (implementation)
        ↓
ILinks<TLink> (Platform.Data.Doublets)
        ↓
Physical Storage (memory/file)
```

## Benefits of Links-Based Storage

1. **Flexibility**: No fixed schema, content types can evolve
2. **Relationships**: Native support for complex content relationships
3. **Performance**: Optimized binary link operations
4. **Simplicity**: Single storage abstraction for all CMS data
5. **Associativity**: Natural representation of content interconnections

## Limitations

Current implementation is experimental and has limitations:
- String-to-link conversion is simplified
- No transaction support
- Basic querying capabilities
- In-memory demonstration only

## Future Enhancements

- Full string reconstruction from Unicode sequences
- Advanced query optimization
- Transaction support
- Persistence layer integration
- Indexing for faster queries
- Migration tools from traditional CMS databases
- Specific adapters for Orchard/Umbraco/DNN APIs

## Related Issues

- [#456](https://github.com/konard/LinksPlatform/issues/456) - Attempt to implement data layer for CMS systems
- [#455](https://github.com/konard/LinksPlatform/issues/455) - Become a part of .NET Foundation

## License

Licensed under the same terms as LinksPlatform (see repository LICENSE file).
