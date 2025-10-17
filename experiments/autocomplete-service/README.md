# Global Autocomplete Service Experiments

This directory contains experimental scripts for testing the Global Autocomplete Service.

## Files

- `test-api.sh` - Shell script to test the API endpoints
- `sample-text.txt` - Sample text corpus for testing

## Running the Service

1. Navigate to the service directory:
   ```bash
   cd Platform/Platform.Data.AutocompleteService
   ```

2. Run the service:
   ```bash
   dotnet run
   ```

3. The service will start on http://localhost:5000

## Testing the API

After starting the service, run the test script:

```bash
chmod +x experiments/autocomplete-service/test-api.sh
./experiments/autocomplete-service/test-api.sh
```

## API Endpoints

### POST /api/autocomplete/index
Index text into the autocomplete service.

**Request:**
```json
{
  "text": "Your text to index here"
}
```

### GET /api/autocomplete/suggest
Get autocomplete suggestions.

**Parameters:**
- `prefix` - The text prefix to autocomplete
- `maxResults` - Maximum number of suggestions (default: 10)

**Example:**
```
GET /api/autocomplete/suggest?prefix=Hello&maxResults=5
```

### POST /api/autocomplete/suggest
Get autocomplete suggestions (POST version).

**Request:**
```json
{
  "prefix": "Hello",
  "maxResults": 5
}
```

### GET /api/autocomplete/stats
Get service statistics.

**Response:**
```json
{
  "indexedSequences": 1234,
  "uniqueSequences": 567
}
```
