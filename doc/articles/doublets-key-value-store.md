# Using Doublets as a Key-Value Store

## Overview

Doublets is a database implementation based on the [associative model of data](https://en.wikipedia.org/wiki/Associative_model_of_data). While it operates on a low level using links (pairs), it can be effectively used as a key-value store similar to Redis or other NoSQL databases. This guide demonstrates how to use Doublets as a key-value storage system.

## Comparison with Redis

If you're familiar with [Redis (Jedis client)](https://github.com/redis/jedis), you'll find similar concepts in Doublets:

| Redis Concept | Doublets Equivalent | Description |
|---------------|---------------------|-------------|
| Connection | `UnitedMemoryLinks<uint>` | Create a connection to the data store |
| SET key value | `PropertiesOperator.SetValue()` | Store a value with a key |
| GET key | `PropertiesOperator.GetValue()` | Retrieve a value by key |
| Key (string) | Link address or marker | Identifier for your data |
| Value (string/object) | Link or sequence | The stored data |

## Quick Start

### 1. Installation

Add the required NuGet packages to your project:

```bash
dotnet add package Platform.Data.Doublets
```

### 2. Basic Setup

```csharp
using System;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.PropertyOperators;

// Create or open a doublets store (similar to connecting to Redis)
using var links = new UnitedMemoryLinks<uint>("my-store.links");

// Create a properties operator for key-value operations
var properties = new PropertiesOperator<uint>(links);
```

## Basic Operations

### Creating Keys (Property Markers)

Before storing values, create markers that will act as your keys:

```csharp
// Create property markers (keys)
var usernameKey = links.CreatePoint();  // A link that references itself
var emailKey = links.CreatePoint();
var ageKey = links.CreatePoint();
```

### Storing Values (SET Operation)

```csharp
// Create an object (like a Redis hash or document)
var user1 = links.CreatePoint();

// Store simple link values
properties.SetValue(user1, usernameKey, someValueLink);

// For complex data like strings, you'll need to convert them to sequences
// (See "Working with Strings" section below)
```

### Retrieving Values (GET Operation)

```csharp
// Get a value for a specific object and property
var username = properties.GetValue(user1, usernameKey);

if (username != default)
{
    Console.WriteLine($"Username link: {username}");
}
else
{
    Console.WriteLine("Username not found");
}
```

### Deleting Values

```csharp
// To delete a property value, set it to a default or null marker
// Or delete the entire object link
links.Delete(user1);
```

## Working with Strings

Since Doublets operates on links rather than strings directly, you need to convert strings to sequences of character links. Here's a complete example:

```csharp
using System;
using System.Text;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.PropertyOperators;
using Platform.Data.Doublets.Sequences.Converters;
using Platform.Data.Doublets.Unicode;

// Initialize the store
using var links = new UnitedMemoryLinks<uint>("kv-store.links");
var properties = new PropertiesOperator<uint>(links);

// Set up string converters
var unicodeMap = new UnicodeMap<uint>(links);
var addressToNumberConverter = new AddressToRawNumberSequenceConverter<uint>(links);
var stringToSequenceConverter = new CachingConverterDecorator<string, uint>(
    new StringToUnicodeSequenceConverter<uint>(links, unicodeMap, addressToNumberConverter)
);
var sequenceToStringConverter = new UnicodeSequenceToStringConverter<uint>(links, unicodeMap);

// Helper methods
uint StringToLink(string str) => stringToSequenceConverter.Convert(str);
string LinkToString(uint link) => sequenceToStringConverter.Convert(link);

// Create keys
var nameKey = links.CreatePoint();
var emailKey = links.CreatePoint();

// Create a user object
var user = links.CreatePoint();

// Store string values (like Redis SET)
properties.SetValue(user, nameKey, StringToLink("John Doe"));
properties.SetValue(user, emailKey, StringToLink("john@example.com"));

// Retrieve string values (like Redis GET)
var nameLink = properties.GetValue(user, nameKey);
var emailLink = properties.GetValue(user, emailKey);

if (nameLink != default)
{
    Console.WriteLine($"Name: {LinkToString(nameLink)}");
}

if (emailLink != default)
{
    Console.WriteLine($"Email: {LinkToString(emailLink)}");
}
```

## Advanced Patterns

### Hash-like Structure (Redis HSET/HGET)

```csharp
// In Redis: HSET user:1 username "john" email "john@example.com"
// In Doublets:

var user1 = links.CreatePoint();  // Like "user:1"

// Set multiple properties
properties.SetValue(user1, usernameKey, StringToLink("john"));
properties.SetValue(user1, emailKey, StringToLink("john@example.com"));
properties.SetValue(user1, ageKey, links.CreatePoint()); // For numeric values, use appropriate converter

// Get all properties
var username = LinkToString(properties.GetValue(user1, usernameKey));
var email = LinkToString(properties.GetValue(user1, emailKey));
```

### Multiple Objects (Multiple Keys)

```csharp
// Create multiple users
var user1 = links.CreatePoint();
var user2 = links.CreatePoint();
var user3 = links.CreatePoint();

// Store different values for each
properties.SetValue(user1, nameKey, StringToLink("Alice"));
properties.SetValue(user2, nameKey, StringToLink("Bob"));
properties.SetValue(user3, nameKey, StringToLink("Charlie"));

// Retrieve specific user's name
var aliceName = LinkToString(properties.GetValue(user1, nameKey));
Console.WriteLine($"User 1: {aliceName}");
```

### Updating Values

```csharp
// Update is the same as Set - it automatically replaces the old value
properties.SetValue(user1, emailKey, StringToLink("newemail@example.com"));
```

## Complete Example

Here's a full working example demonstrating a simple user management system:

```csharp
using System;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.PropertyOperators;
using Platform.Data.Doublets.Sequences.Converters;
using Platform.Data.Doublets.Unicode;

public class DoubletsKeyValueExample
{
    private readonly UnitedMemoryLinks<uint> _links;
    private readonly PropertiesOperator<uint> _properties;
    private readonly Func<string, uint> _stringToLink;
    private readonly Func<uint, string> _linkToString;

    // Property keys
    private readonly uint _nameKey;
    private readonly uint _emailKey;
    private readonly uint _ageKey;

    public DoubletsKeyValueExample(string dbPath)
    {
        // Initialize storage
        _links = new UnitedMemoryLinks<uint>(dbPath);
        _properties = new PropertiesOperator<uint>(_links);

        // Set up converters
        var unicodeMap = new UnicodeMap<uint>(_links);
        var addressToNumberConverter = new AddressToRawNumberSequenceConverter<uint>(_links);
        var stringToSeqConverter = new CachingConverterDecorator<string, uint>(
            new StringToUnicodeSequenceConverter<uint>(_links, unicodeMap, addressToNumberConverter)
        );
        var seqToStringConverter = new UnicodeSequenceToStringConverter<uint>(_links, unicodeMap);

        _stringToLink = str => stringToSeqConverter.Convert(str);
        _linkToString = link => seqToStringConverter.Convert(link);

        // Create property keys (reuse if they already exist)
        _nameKey = _links.CreatePoint();
        _emailKey = _links.CreatePoint();
        _ageKey = _links.CreatePoint();
    }

    public uint CreateUser(string name, string email)
    {
        var user = _links.CreatePoint();
        _properties.SetValue(user, _nameKey, _stringToLink(name));
        _properties.SetValue(user, _emailKey, _stringToLink(email));
        return user;
    }

    public string GetUserName(uint userId)
    {
        var nameLink = _properties.GetValue(userId, _nameKey);
        return nameLink != default ? _linkToString(nameLink) : null;
    }

    public string GetUserEmail(uint userId)
    {
        var emailLink = _properties.GetValue(userId, _emailKey);
        return emailLink != default ? _linkToString(emailLink) : null;
    }

    public void UpdateUserEmail(uint userId, string newEmail)
    {
        _properties.SetValue(userId, _emailKey, _stringToLink(newEmail));
    }

    public void DeleteUser(uint userId)
    {
        _links.Delete(userId);
    }

    public void Dispose()
    {
        _links?.Dispose();
    }

    public static void Main()
    {
        using var kvStore = new DoubletsKeyValueExample("users.links");

        // Create users (like Redis SET operations)
        var user1 = kvStore.CreateUser("Alice", "alice@example.com");
        var user2 = kvStore.CreateUser("Bob", "bob@example.com");

        // Read users (like Redis GET operations)
        Console.WriteLine($"User 1 name: {kvStore.GetUserName(user1)}");
        Console.WriteLine($"User 1 email: {kvStore.GetUserEmail(user1)}");

        // Update user (like Redis SET with existing key)
        kvStore.UpdateUserEmail(user1, "alice.new@example.com");
        Console.WriteLine($"User 1 updated email: {kvStore.GetUserEmail(user1)}");

        // Delete user (like Redis DEL)
        kvStore.DeleteUser(user2);
        Console.WriteLine($"User 2 after deletion: {kvStore.GetUserName(user2) ?? "Not found"}");
    }
}
```

## Implementation Details

### How It Works Internally

The `PropertiesOperator` implements key-value storage using a three-link pattern:

1. **Object Link**: Represents the entity (like `user:1` in Redis)
2. **Property Link**: Connects the object to its property key (like `username`)
3. **Value Link**: Stores the actual value

The relationship is: `Object -> (Object-Property) -> Value`

When you call `SetValue(object, property, value)`:
- Creates or finds an `object-property` link
- Deletes any existing values for this object-property combination
- Creates a new `(object-property) -> value` link

When you call `GetValue(object, property)`:
- Searches for the `object-property` link
- Finds links where source is `object-property`
- Returns the target of that link (the value)

## Performance Considerations

- **Memory-mapped files**: Doublets uses memory-mapped files for fast I/O
- **Link reuse**: Common subsequences are automatically deduplicated
- **Indexing**: Doublets maintains indices for fast lookups
- **Batch operations**: For better performance, consider batching multiple operations

## References

- [Doublets Repository](https://github.com/linksplatform/Data.Doublets)
- [SQLite vs Doublets Comparison](https://github.com/linksplatform/Comparisons.SQLiteVSDoublets) - Shows how `PropertiesOperator` is used for database-like operations
- [PropertiesOperator Source](https://github.com/linksplatform/Data.Doublets/blob/main/csharp/Platform.Data.Doublets/PropertyOperators/PropertiesOperator.cs)
- [CRUD Examples](https://github.com/linksplatform/Examples.Doublets.CRUD.DotNet)

## Summary

Doublets provides a powerful, flexible key-value storage system based on associative links. While it requires understanding link-based concepts, it offers unique advantages:

- **Structural sharing**: Automatic deduplication of common data patterns
- **Flexibility**: Can represent any data structure, not just key-value
- **Performance**: Memory-mapped storage with efficient indexing
- **Type safety**: Generic implementation with compile-time type checking

For simple key-value operations, use the `PropertiesOperator` class as shown in this guide. For more complex scenarios, you can work directly with the links to create custom data structures.
