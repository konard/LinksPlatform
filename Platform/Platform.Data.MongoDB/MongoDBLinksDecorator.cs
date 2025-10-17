using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MongoDB.Bson;
using MongoDB.Driver;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Decorators;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.MongoDB
{
    /// <summary>
    /// <para>
    /// Represents a decorator that synchronizes all changes from ILinks interface to a MongoDB collection.
    /// </para>
    /// <para>
    /// Представляет декоратор, который синхронизирует все изменения из интерфейса ILinks в коллекцию MongoDB.
    /// </para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of link address.</typeparam>
    /// <remarks>
    /// This decorator pushes all changes (Create, Update, Delete operations) to MongoDB after each operation,
    /// enabling integration with reactive frameworks like Meteor.
    /// </remarks>
    public class MongoDBLinksDecorator<TLinkAddress> : LinksDecoratorBase<TLinkAddress>
        where TLinkAddress : IConvertible
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly string _linkIdField;
        private readonly string _sourceField;
        private readonly string _targetField;

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="MongoDBLinksDecorator{TLinkAddress}"/> class.
        /// </para>
        /// <para>
        /// Инициализирует новый экземпляр класса <see cref="MongoDBLinksDecorator{TLinkAddress}"/>.
        /// </para>
        /// </summary>
        /// <param name="links">The underlying ILinks implementation to decorate.</param>
        /// <param name="mongoUrl">MongoDB connection string.</param>
        /// <param name="databaseName">Name of the database to use.</param>
        /// <param name="collectionName">Name of the collection to store links.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MongoDBLinksDecorator(
            ILinks<TLinkAddress> links,
            string mongoUrl,
            string databaseName,
            string collectionName = "links")
            : this(links, new MongoClient(mongoUrl).GetDatabase(databaseName).GetCollection<BsonDocument>(collectionName))
        {
        }

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="MongoDBLinksDecorator{TLinkAddress}"/> class.
        /// </para>
        /// <para>
        /// Инициализирует новый экземпляр класса <see cref="MongoDBLinksDecorator{TLinkAddress}"/>.
        /// </para>
        /// </summary>
        /// <param name="links">The underlying ILinks implementation to decorate.</param>
        /// <param name="collection">MongoDB collection to synchronize with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MongoDBLinksDecorator(
            ILinks<TLinkAddress> links,
            IMongoCollection<BsonDocument> collection)
            : base(links)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _linkIdField = "linkId";
            _sourceField = "source";
            _targetField = "target";

            // Create index on linkId for faster lookups
            try
            {
                var indexKeys = Builders<BsonDocument>.IndexKeys.Ascending(_linkIdField);
                var indexModel = new CreateIndexModel<BsonDocument>(indexKeys, new CreateIndexOptions { Unique = true });
                _collection.Indexes.CreateOne(indexModel);
            }
            catch
            {
                // Index may already exist, ignore the error
            }
        }

        /// <summary>
        /// <para>
        /// Creates a link and synchronizes it to MongoDB.
        /// </para>
        /// <para>
        /// Создаёт связь и синхронизирует её с MongoDB.
        /// </para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Create(IList<TLinkAddress> restrictions)
        {
            var result = base.Create(restrictions);

            if (!EqualityComparer<TLinkAddress>.Default.Equals(result, _constants.Null))
            {
                SyncLinkToMongoDB(result);
            }

            return result;
        }

        /// <summary>
        /// <para>
        /// Updates a link and synchronizes the changes to MongoDB.
        /// </para>
        /// <para>
        /// Обновляет связь и синхронизирует изменения с MongoDB.
        /// </para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Update(IList<TLinkAddress> restrictions, IList<TLinkAddress> substitution)
        {
            var result = base.Update(restrictions, substitution);

            if (!EqualityComparer<TLinkAddress>.Default.Equals(result, _constants.Null))
            {
                var linkId = restrictions != null && restrictions.Count > 0 ? restrictions[0] : result;
                SyncLinkToMongoDB(linkId);
            }

            return result;
        }

        /// <summary>
        /// <para>
        /// Deletes a link and removes it from MongoDB.
        /// </para>
        /// <para>
        /// Удаляет связь и удаляет её из MongoDB.
        /// </para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Delete(IList<TLinkAddress> restrictions)
        {
            TLinkAddress linkIdToDelete = default;

            if (restrictions != null && restrictions.Count > 0)
            {
                linkIdToDelete = restrictions[0];
            }

            base.Delete(restrictions);

            if (!EqualityComparer<TLinkAddress>.Default.Equals(linkIdToDelete, default(TLinkAddress)))
            {
                DeleteLinkFromMongoDB(linkIdToDelete);
            }
        }

        /// <summary>
        /// Synchronizes a link to MongoDB by inserting or updating it.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SyncLinkToMongoDB(TLinkAddress linkId)
        {
            try
            {
                var link = _links.GetLink(linkId);
                var sourceIndex = _constants.SourcePart;
                var targetIndex = _constants.TargetPart;

                var document = new BsonDocument
                {
                    { _linkIdField, BsonValue.Create(Convert.ToInt64(linkId)) },
                    { _sourceField, BsonValue.Create(Convert.ToInt64(link[sourceIndex])) },
                    { _targetField, BsonValue.Create(Convert.ToInt64(link[targetIndex])) },
                    { "timestamp", DateTime.UtcNow }
                };

                var filter = Builders<BsonDocument>.Filter.Eq(_linkIdField, Convert.ToInt64(linkId));
                var options = new ReplaceOptions { IsUpsert = true };

                _collection.ReplaceOne(filter, document, options);
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw to avoid breaking the links operation
                Console.Error.WriteLine($"Failed to sync link {linkId} to MongoDB: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a link from MongoDB.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DeleteLinkFromMongoDB(TLinkAddress linkId)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq(_linkIdField, Convert.ToInt64(linkId));
                _collection.DeleteOne(filter);
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw to avoid breaking the links operation
                Console.Error.WriteLine($"Failed to delete link {linkId} from MongoDB: {ex.Message}");
            }
        }
    }
}
