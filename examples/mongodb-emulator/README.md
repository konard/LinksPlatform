# LinksPlatform MongoDB Emulator

A MongoDB-compatible interface for LinksPlatform that enables easy integration with Node.js and Meteor applications.

## Overview

This MongoDB emulator provides a familiar MongoDB API backed by LinksPlatform's powerful associative data storage (Triplets). It allows developers to use LinksPlatform's unique data model while maintaining compatibility with existing MongoDB-based applications.

## Features

- **MongoDB-Compatible API**: Use familiar methods like `insertOne`, `find`, `updateMany`, `deleteOne`, etc.
- **Node.js Integration**: Drop-in replacement for MongoDB in Node.js applications
- **Meteor Support**: Easy integration with Meteor framework
- **REST API Backend**: C# ASP.NET Core API that bridges to LinksPlatform Triplets storage
- **Associative Storage**: Leverage LinksPlatform's associative data model for flexible data relationships

## Architecture

```
┌─────────────────────────────────────────┐
│  Node.js / Meteor Application           │
│  (uses familiar MongoDB API)             │
└──────────────┬──────────────────────────┘
               │
               │ HTTP/REST
               │
┌──────────────▼──────────────────────────┐
│  MongoDB Emulator REST API               │
│  (ASP.NET Core Controller)               │
└──────────────┬──────────────────────────┘
               │
               │ C# API
               │
┌──────────────▼──────────────────────────┐
│  LinksPlatform Triplets Storage          │
│  (Associative Data Model)                │
└──────────────────────────────────────────┘
```

## Installation

### Prerequisites

1. **LinksPlatform WebTerminal** running (default: `http://localhost:5000`)
2. **Node.js** 12.0 or later
3. **npm** or **yarn**

### Install Dependencies

```bash
npm install
```

## Usage

### Node.js Example

```javascript
const { MongoClient } = require('./linksplatform-mongodb');

async function main() {
    // Connect to LinksPlatform
    const client = await MongoClient.connect('http://localhost:5000');
    const db = client.db('myapp');
    const users = db.collection('users');

    // Insert a document
    await users.insertOne({
        name: 'John Doe',
        email: 'john@example.com',
        age: 30
    });

    // Find documents
    const allUsers = await users.find();
    console.log('Users:', allUsers);

    // Update a document
    await users.updateOne(
        { email: 'john@example.com' },
        { age: 31 }
    );

    // Delete a document
    await users.deleteOne({ email: 'john@example.com' });

    await client.close();
}

main();
```

### Run the Example

```bash
npm run example
```

## API Reference

### MongoClient

#### `MongoClient.connect(url, options)`

Connect to the LinksPlatform MongoDB emulator.

- `url`: Base URL of LinksPlatform WebTerminal (e.g., `http://localhost:5000`)
- `options`: Connection options (optional)
- Returns: Promise<MongoClient>

#### `client.db(name)`

Get a database instance.

- `name`: Database name (currently used for logical grouping only)
- Returns: Database

### Database

#### `db.collection(name)`

Get or create a collection.

- `name`: Collection name
- Returns: Collection

#### `db.listCollections()`

List all collections.

- Returns: Promise<Array<string>>

### Collection

#### `collection.insertOne(document)`

Insert a single document.

- `document`: The document to insert
- Returns: Promise<{acknowledged, insertedId}>

#### `collection.insertMany(documents)`

Insert multiple documents.

- `documents`: Array of documents to insert
- Returns: Promise<{acknowledged, insertedCount, insertedIds}>

#### `collection.find(filter, options)`

Find documents matching a query.

- `filter`: Query filter object
- `options`: Query options (limit, skip, projection, sort)
- Returns: Promise<Array>

#### `collection.findOne(filter, options)`

Find a single document.

- `filter`: Query filter object
- `options`: Query options
- Returns: Promise<Object|null>

#### `collection.updateOne(filter, update, options)`

Update a single document.

- `filter`: Query filter
- `update`: Update operations
- `options`: Update options (upsert)
- Returns: Promise<{acknowledged, modifiedCount, matchedCount}>

#### `collection.updateMany(filter, update, options)`

Update multiple documents.

- Similar to updateOne but updates all matching documents

#### `collection.deleteOne(filter)`

Delete a single document.

- `filter`: Query filter
- Returns: Promise<{acknowledged, deletedCount}>

#### `collection.deleteMany(filter)`

Delete multiple documents.

- `filter`: Query filter
- Returns: Promise<{acknowledged, deletedCount}>

#### `collection.countDocuments(filter)`

Count documents matching a query.

- `filter`: Query filter
- Returns: Promise<number>

## Meteor Integration

See `meteor-example.js` for detailed Meteor integration examples including:

- Meteor Methods with LinksPlatform
- Reactive publications
- React component integration
- Environment configuration

## REST API Endpoints

The following REST endpoints are available on the LinksPlatform WebTerminal:

- `POST /api/mongodb/{collection}/insert` - Insert a document
- `POST /api/mongodb/{collection}/insertMany` - Insert multiple documents
- `POST /api/mongodb/{collection}/find` - Find documents
- `POST /api/mongodb/{collection}/findOne` - Find one document
- `POST /api/mongodb/{collection}/update` - Update documents
- `POST /api/mongodb/{collection}/delete` - Delete documents
- `POST /api/mongodb/{collection}/count` - Count documents
- `GET /api/mongodb/collections` - List all collections

## How It Works

The MongoDB emulator stores documents in LinksPlatform's Triplets storage using the following structure:

1. **Collections** are represented as Links with a Collection type
2. **Documents** are Links associated with their collection
3. **Fields** are stored as Triplets: (Document, FieldName, FieldValue)
4. **Queries** are translated to Link searches in the associative storage

This approach leverages LinksPlatform's associative model while providing a familiar MongoDB interface.

## Limitations

Current implementation has the following limitations:

- Basic query operators only (equality matching)
- No aggregation pipeline support
- Simple field updates (no complex update operators like `$inc`, `$push`)
- No transaction support
- No index optimization

These can be extended based on requirements.

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

This project is part of LinksPlatform and follows the same license.

## Links

- [LinksPlatform](https://github.com/konard/LinksPlatform)
- [LinksPlatform Documentation](https://linksplatform.github.io/)
- [MongoDB Documentation](https://docs.mongodb.com/)
