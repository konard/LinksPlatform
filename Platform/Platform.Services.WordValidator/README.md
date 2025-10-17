# Platform.Services.WordValidator

A simple microservice that answers the question: "Is this a word?"

## Overview

This microservice provides a REST API to validate whether a given text is a valid English word. It uses a comprehensive dictionary of over 370,000 English words.

## Features

- Simple REST API endpoint for word validation
- Fast in-memory dictionary lookup
- Case-insensitive word matching
- Comprehensive English word database

## API Endpoints

### Validate Word (Path Parameter)

```
GET /api/word/validate/{word}
```

**Example:**
```bash
curl https://localhost:5001/api/word/validate/hello
```

**Response:**
```json
{
  "word": "hello",
  "isWord": true
}
```

### Validate Word (Query Parameter)

```
GET /api/word/validate?word={word}
```

**Example:**
```bash
curl https://localhost:5001/api/word/validate?word=testing
```

**Response:**
```json
{
  "word": "testing",
  "isWord": true
}
```

### Error Handling

If the word parameter is empty or whitespace:

```json
{
  "error": "Word parameter cannot be empty"
}
```

## Running the Service

### Using .NET CLI

```bash
cd Platform/Platform.Services.WordValidator
dotnet run
```

The service will start on `https://localhost:5001` by default.

### Using Visual Studio

Open `Platform.sln` in Visual Studio and run the `Platform.Services.WordValidator` project.

## Dictionary

The service uses a comprehensive English word list containing over 370,000 words from the [dwyl/english-words](https://github.com/dwyl/english-words) repository.

The dictionary is loaded into memory when the service starts for fast lookups.

## Technical Details

- **Framework:** ASP.NET Core 2.2
- **Language:** C#
- **Architecture:** RESTful API with dependency injection
- **Dictionary Storage:** In-memory HashSet for O(1) lookup performance

## Related Issues

- Part of [#179](https://github.com/konard/LinksPlatform/issues/179) - Reference collections for each small data type
- Can be used in [#203](https://github.com/konard/LinksPlatform/issues/203) - Algorithm to find basic words
- Solves [#204](https://github.com/konard/LinksPlatform/issues/204) - Is this a word?
