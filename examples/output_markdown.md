LinksPlatform API Documentation
# LinksPlatform API Documentation


This API provides access to the LinksPlatform data management system, allowing you to manage links and their relationships.
## Base URL


https://api.linksplatform.com/v1
## Authentication


API key required in header:`X-API-Key: your_api_key`
## Endpoints

### GET/links


**Summary:**Retrieve all links

**Description:**Returns a list of all links in the system with their properties and relationships.
#### Parameters:
**id:**Optional link identifier to filter results (integer)**limit:**Maximum number of results to return (integer, default: 100)**offset:**Number of results to skip for pagination (integer, default: 0)
#### Response:


Returns JSON array of link objects with id, source, target, and metadata.
### GET/links/{id}


**Summary:**Get specific link by ID

**Description:**Retrieves detailed information about a specific link including all its properties and related links.
#### Path Parameters:
**id:**Unique identifier of the link (integer, required)
#### Response:


Returns detailed link object with all properties and relationships.
### POST/links


**Summary:**Create new link

**Description:**Creates a new link relationship between two entities in the system.
#### Request Body:
**source:**Source entity ID (integer, required)**target:**Target entity ID (integer, required)**type:**Link type identifier (integer, optional)
#### Response:


Returns created link object with assigned ID and timestamps.
### PUT/links/{id}


**Summary:**Update existing link

**Description:**Updates properties of an existing link relationship.
#### Path Parameters:
**id:**Unique identifier of the link to update (integer, required)
#### Request Body:
**source:**New source entity ID (integer, optional)**target:**New target entity ID (integer, optional)**type:**New link type identifier (integer, optional)
#### Response:


Returns updated link object with new values and timestamps.
### DELETE/links/{id}


**Summary:**Delete link

**Description:**Permanently removes a link relationship from the system.
#### Path Parameters:
**id:**Unique identifier of the link to delete (integer, required)
#### Response:


Returns confirmation of deletion with status code 204.
## Error Responses


All endpoints may return the following error responses:

- **400 Bad Request:**Invalid request parameters
- **401 Unauthorized:**Missing or invalid API key
- **404 Not Found:**Requested resource not found
- **500 Internal Server Error:**Server processing error
## Rate Limiting


API requests are limited to 1000 requests per hour per API key.