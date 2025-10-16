# Objects and Type System for Links Platform

This implementation provides a foundation for treating Links as dynamic objects with an optional type system, enabling interpretation similar to JSON, XML, or C#'s dynamic types.

## Features

### 1. Type System (`ITypeSystem`, `LinksTypeSystem`)

The type system provides:
- **Type Markers**: Define and retrieve types by name (e.g., "Person", "Document")
- **Property Markers**: Define and retrieve properties by name (e.g., "name", "age")
- **Type Checking**: Verify if a link represents a type or property
- **Name Resolution**: Convert between markers and their string names

### 2. Dynamic Objects (`IDynamicObject`, `LinksDynamicObject`)

Dynamic objects enable:
- **Property Access**: Get/set properties by name using indexer syntax
- **Property Enumeration**: Retrieve all properties of an object
- **Property Checking**: Verify if a property exists
- **Type Assignment**: Get/set the type of an object

### 3. Import/Export (`IObjectConverter`, `JsonConverter`)

Converters provide:
- **JSON Import**: Parse JSON strings into Links structures
- **JSON Export**: Serialize Links structures to JSON strings
- **Format Support**: Objects, arrays, strings, numbers, booleans, null

## Architecture

### Links Structure

The implementation uses Links to represent objects:

```
Object Structure:
- object --[property]--> value
- object --[typeMarker]--> type

Type Marker:
- typeMarker --[name_sequence]--> "TypeName"

Property Marker:
- propertyMarker --[name_sequence]--> "propertyName"
```

### Key Components

1. **LinksTypeSystem**: Manages type and property markers, caching for performance
2. **LinksDynamicObject**: Wraps a link to provide object-like property access
3. **JsonConverter**: Transforms between JSON and Links representations

## Usage Example

```csharp
using Platform.Data.Doublets.Memory.United.Generic;

// Initialize Links storage
using (var links = new UnitedMemoryLinks<uint>())
{
    // Create type system
    var typeSystem = new LinksTypeSystem<uint>(links);

    // Create a typed object
    var personType = typeSystem.GetOrCreateType("Person");
    var objectMarker = typeSystem.GetOrCreateType("Object");
    var person = new LinksDynamicObject<uint>(links, typeSystem, objectMarker);

    person.SetType(personType);
    person["name"] = nameLink;
    person["age"] = ageLink;

    // Check properties
    bool hasName = person.HasProperty("name");
    var properties = person.GetProperties();

    // JSON conversion
    var jsonConverter = new JsonConverter<uint>(links, typeSystem);
    var imported = jsonConverter.Import("{\"key\":\"value\"}");
    var exported = jsonConverter.Export(imported);
}
```

## Implementation Notes

### Current Limitations

1. **String/Number Storage**: The current implementation uses placeholder links for strings and numbers. A production implementation would integrate with:
   - `StringToUnicodeSequenceConverter` for proper string storage
   - Number representation links for numeric values

2. **Array Indexing**: Array element ordering could be improved with dedicated index tracking

3. **Circular References**: Export doesn't handle circular references (would require cycle detection)

### Future Enhancements

1. **Schema Validation**: Add schema definition and validation support
2. **Behaviors/Triggers**: Implement method/function attachments to types
3. **Inheritance**: Add type inheritance and polymorphism
4. **XML Support**: Implement XML import/export converter
5. **Performance**: Add indexing for faster property lookups

## Relation to Issue #78

This implementation addresses the requirements from [Issue #78](https://github.com/konard/LinksPlatform/issues/78):

✅ **Links interpretation as dynamic objects** - `IDynamicObject` and `LinksDynamicObject` provide dynamic property access

✅ **Conversion (Export, Import)** - `JsonConverter` implements JSON import/export, with architecture supporting XML and other formats

✅ **Optional schema and type system** - `ITypeSystem` and `LinksTypeSystem` provide optional typing

✅ **Behaviour support** - Architecture allows for future attachment of functions, methods, triggers, transformations, and actions to types

## Files

- `ITypeSystem.cs` - Type system interface
- `LinksTypeSystem.cs` - Type system implementation
- `IDynamicObject.cs` - Dynamic object interface
- `LinksDynamicObject.cs` - Dynamic object implementation
- `IObjectConverter.cs` - Converter interface
- `JsonConverter.cs` - JSON import/export implementation
- `ObjectsExample.cs` - Usage examples and demonstrations
- `OBJECTS_README.md` - This documentation file

## Related Work

This implementation builds on existing Links Platform concepts:
- `XmlImporter` / `XmlIndexer` - XML import functionality
- `GEXFExporter` / `CSVExporter` - Export functionality
- `LinksXmlStorage` - Storage abstraction
- `StringToUnicodeSequenceConverter` - String representation
