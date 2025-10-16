/**
 * LinksPlatform MongoDB Emulator Client
 *
 * A Node.js client library that provides a MongoDB-like interface
 * for interacting with LinksPlatform's Triplets storage via REST API.
 *
 * This allows easy integration with Meteor and Node.js applications.
 */

const axios = require('axios');

/**
 * MongoDB-compatible Collection class
 */
class Collection {
    constructor(name, client) {
        this.name = name;
        this.client = client;
        this.baseUrl = `${client.baseUrl}/api/mongodb/${name}`;
    }

    /**
     * Insert a single document into the collection
     * @param {Object} document - The document to insert
     * @returns {Promise<Object>} Result with insertedId
     */
    async insertOne(document) {
        try {
            const response = await axios.post(`${this.baseUrl}/insert`, document);
            return {
                acknowledged: true,
                insertedId: response.data.insertedId
            };
        } catch (error) {
            throw this._handleError(error);
        }
    }

    /**
     * Insert multiple documents into the collection
     * @param {Array<Object>} documents - Array of documents to insert
     * @returns {Promise<Object>} Result with insertedIds
     */
    async insertMany(documents) {
        try {
            const response = await axios.post(`${this.baseUrl}/insertMany`, documents);
            return {
                acknowledged: true,
                insertedCount: response.data.insertedCount,
                insertedIds: response.data.insertedIds
            };
        } catch (error) {
            throw this._handleError(error);
        }
    }

    /**
     * Find documents matching a query
     * @param {Object} filter - Query filter
     * @param {Object} options - Query options (limit, skip, projection, sort)
     * @returns {Promise<Array>} Array of matching documents
     */
    async find(filter = {}, options = {}) {
        try {
            const query = {
                filter: filter,
                limit: options.limit || 0,
                skip: options.skip || 0,
                projection: options.projection || null,
                sort: options.sort || null
            };
            const response = await axios.post(`${this.baseUrl}/find`, query);
            return response.data.documents;
        } catch (error) {
            throw this._handleError(error);
        }
    }

    /**
     * Find a single document matching a query
     * @param {Object} filter - Query filter
     * @param {Object} options - Query options
     * @returns {Promise<Object|null>} The matching document or null
     */
    async findOne(filter = {}, options = {}) {
        try {
            const query = {
                filter: filter,
                projection: options.projection || null
            };
            const response = await axios.post(`${this.baseUrl}/findOne`, query);
            return response.data.document;
        } catch (error) {
            throw this._handleError(error);
        }
    }

    /**
     * Update documents matching a query
     * @param {Object} filter - Query filter
     * @param {Object} update - Update operations
     * @param {Object} options - Update options
     * @returns {Promise<Object>} Result with modifiedCount
     */
    async updateMany(filter, update, options = {}) {
        try {
            const query = {
                filter: filter,
                update: update,
                multi: true,
                upsert: options.upsert || false
            };
            const response = await axios.post(`${this.baseUrl}/update`, query);
            return {
                acknowledged: true,
                modifiedCount: response.data.modifiedCount,
                matchedCount: response.data.modifiedCount
            };
        } catch (error) {
            throw this._handleError(error);
        }
    }

    /**
     * Update a single document matching a query
     * @param {Object} filter - Query filter
     * @param {Object} update - Update operations
     * @param {Object} options - Update options
     * @returns {Promise<Object>} Result with modifiedCount
     */
    async updateOne(filter, update, options = {}) {
        try {
            const query = {
                filter: filter,
                update: update,
                multi: false,
                upsert: options.upsert || false
            };
            const response = await axios.post(`${this.baseUrl}/update`, query);
            return {
                acknowledged: true,
                modifiedCount: response.data.modifiedCount,
                matchedCount: response.data.modifiedCount
            };
        } catch (error) {
            throw this._handleError(error);
        }
    }

    /**
     * Delete documents matching a query
     * @param {Object} filter - Query filter
     * @returns {Promise<Object>} Result with deletedCount
     */
    async deleteMany(filter) {
        try {
            const query = {
                filter: filter,
                single: false
            };
            const response = await axios.post(`${this.baseUrl}/delete`, query);
            return {
                acknowledged: true,
                deletedCount: response.data.deletedCount
            };
        } catch (error) {
            throw this._handleError(error);
        }
    }

    /**
     * Delete a single document matching a query
     * @param {Object} filter - Query filter
     * @returns {Promise<Object>} Result with deletedCount
     */
    async deleteOne(filter) {
        try {
            const query = {
                filter: filter,
                single: true
            };
            const response = await axios.post(`${this.baseUrl}/delete`, query);
            return {
                acknowledged: true,
                deletedCount: response.data.deletedCount
            };
        } catch (error) {
            throw this._handleError(error);
        }
    }

    /**
     * Count documents matching a query
     * @param {Object} filter - Query filter
     * @returns {Promise<number>} Count of matching documents
     */
    async countDocuments(filter = {}) {
        try {
            const query = { filter: filter };
            const response = await axios.post(`${this.baseUrl}/count`, query);
            return response.data.count;
        } catch (error) {
            throw this._handleError(error);
        }
    }

    _handleError(error) {
        if (error.response) {
            return new Error(`MongoDB Emulator Error: ${error.response.data.error || error.message}`);
        }
        return new Error(`Network Error: ${error.message}`);
    }
}

/**
 * MongoDB-compatible Database class
 */
class Database {
    constructor(name, client) {
        this.name = name;
        this.client = client;
        this._collections = {};
    }

    /**
     * Get or create a collection
     * @param {string} name - Collection name
     * @returns {Collection} Collection instance
     */
    collection(name) {
        if (!this._collections[name]) {
            this._collections[name] = new Collection(name, this.client);
        }
        return this._collections[name];
    }

    /**
     * List all collections in the database
     * @returns {Promise<Array<string>>} Array of collection names
     */
    async listCollections() {
        try {
            const response = await axios.get(`${this.client.baseUrl}/api/mongodb/collections`);
            return response.data.collections;
        } catch (error) {
            throw new Error(`Failed to list collections: ${error.message}`);
        }
    }
}

/**
 * MongoDB-compatible Client class
 */
class MongoClient {
    constructor(url, options = {}) {
        // Extract base URL (remove database name if present)
        this.baseUrl = url.replace(/\/[^\/]*$/, '');
        this.options = options;
        this._databases = {};
        this.connected = false;
    }

    /**
     * Connect to the LinksPlatform MongoDB emulator
     * @returns {Promise<MongoClient>} Connected client instance
     */
    async connect() {
        try {
            // Test connection by listing collections
            await axios.get(`${this.baseUrl}/api/mongodb/collections`);
            this.connected = true;
            return this;
        } catch (error) {
            throw new Error(`Failed to connect to LinksPlatform MongoDB Emulator: ${error.message}`);
        }
    }

    /**
     * Get a database instance
     * @param {string} name - Database name (currently ignored, as LinksPlatform uses single storage)
     * @returns {Database} Database instance
     */
    db(name = 'default') {
        if (!this._databases[name]) {
            this._databases[name] = new Database(name, this);
        }
        return this._databases[name];
    }

    /**
     * Close the connection
     * @returns {Promise<void>}
     */
    async close() {
        this.connected = false;
    }

    /**
     * Create a MongoDB client compatible with LinksPlatform
     * @param {string} url - Connection URL (e.g., 'http://localhost:5000')
     * @param {Object} options - Connection options
     * @returns {Promise<MongoClient>} Connected client
     */
    static async connect(url, options = {}) {
        const client = new MongoClient(url, options);
        await client.connect();
        return client;
    }
}

module.exports = { MongoClient, Database, Collection };
