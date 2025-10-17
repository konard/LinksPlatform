using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

namespace Platform.Data.MongoDB.Tests
{
    /// <summary>
    /// Tests for MongoDBLinksDecorator to verify synchronization with MongoDB.
    /// </summary>
    public class MongoDBLinksDecoratorTests
    {
        private const string MongoConnectionString = "mongodb://localhost:27017";
        private const string TestDatabaseName = "LinksTest";
        private const string TestCollectionName = "links";

        /// <summary>
        /// This is an integration test that requires a running MongoDB instance.
        /// It demonstrates how to use the MongoDBLinksDecorator.
        /// </summary>
        [Fact(Skip = "Requires MongoDB instance running on localhost:27017")]
        public void CreateLinkSyncsToMongoDB()
        {
            // Arrange
            var mongoClient = new MongoClient(MongoConnectionString);
            var database = mongoClient.GetDatabase(TestDatabaseName);
            var collection = database.GetCollection<BsonDocument>(TestCollectionName);

            // Clean up collection before test
            database.DropCollection(TestCollectionName);

            using var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var mongoDecorator = new MongoDBLinksDecorator<uint>(links, collection);

            // Act
            var linkId = mongoDecorator.Create(new List<uint> { 1, 2 });

            // Assert
            Assert.NotEqual(0u, linkId);

            var filter = Builders<BsonDocument>.Filter.Eq("linkId", (long)linkId);
            var document = collection.Find(filter).FirstOrDefault();

            Assert.NotNull(document);
            Assert.Equal((long)linkId, document["linkId"].AsInt64);
            Assert.Equal(1L, document["source"].AsInt64);
            Assert.Equal(2L, document["target"].AsInt64);

            // Clean up
            database.DropCollection(TestCollectionName);
        }

        /// <summary>
        /// This is an integration test that requires a running MongoDB instance.
        /// It demonstrates how updates are synchronized to MongoDB.
        /// </summary>
        [Fact(Skip = "Requires MongoDB instance running on localhost:27017")]
        public void UpdateLinkSyncsToMongoDB()
        {
            // Arrange
            var mongoClient = new MongoClient(MongoConnectionString);
            var database = mongoClient.GetDatabase(TestDatabaseName);
            var collection = database.GetCollection<BsonDocument>(TestCollectionName);

            database.DropCollection(TestCollectionName);

            using var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var mongoDecorator = new MongoDBLinksDecorator<uint>(links, collection);

            var linkId = mongoDecorator.Create(new List<uint> { 1, 2 });

            // Act
            mongoDecorator.Update(new List<uint> { linkId, 1, 2 }, new List<uint> { linkId, 3, 4 });

            // Assert
            var filter = Builders<BsonDocument>.Filter.Eq("linkId", (long)linkId);
            var document = collection.Find(filter).FirstOrDefault();

            Assert.NotNull(document);
            Assert.Equal(3L, document["source"].AsInt64);
            Assert.Equal(4L, document["target"].AsInt64);

            // Clean up
            database.DropCollection(TestCollectionName);
        }

        /// <summary>
        /// This is an integration test that requires a running MongoDB instance.
        /// It demonstrates how deletions are synchronized to MongoDB.
        /// </summary>
        [Fact(Skip = "Requires MongoDB instance running on localhost:27017")]
        public void DeleteLinkRemovesFromMongoDB()
        {
            // Arrange
            var mongoClient = new MongoClient(MongoConnectionString);
            var database = mongoClient.GetDatabase(TestDatabaseName);
            var collection = database.GetCollection<BsonDocument>(TestCollectionName);

            database.DropCollection(TestCollectionName);

            using var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var mongoDecorator = new MongoDBLinksDecorator<uint>(links, collection);

            var linkId = mongoDecorator.Create(new List<uint> { 1, 2 });

            // Act
            mongoDecorator.Delete(new List<uint> { linkId });

            // Assert
            var filter = Builders<BsonDocument>.Filter.Eq("linkId", (long)linkId);
            var document = collection.Find(filter).FirstOrDefault();

            Assert.Null(document);

            // Clean up
            database.DropCollection(TestCollectionName);
        }
    }
}
