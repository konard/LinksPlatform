using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Platform.Data.WebTerminal.Models.MongoDB;
using Newtonsoft.Json.Linq;

namespace Platform.Data.WebTerminal.Controllers
{
    /// <summary>
    /// MongoDB-compatible REST API controller for LinksPlatform.
    /// Provides MongoDB-like operations with familiar MongoDB API.
    ///
    /// This is a proof-of-concept implementation demonstrating MongoDB emulation
    /// for easy integration with Node.js and Meteor applications.
    /// Future versions can integrate deeper with LinksPlatform's Triplets storage.
    /// </summary>
    [ApiController]
    [Route("api/mongodb")]
    public class MongoDBEmulatorController : ControllerBase
    {
        // In-memory storage for proof-of-concept
        // TODO: Integrate with LinksPlatform Triplets storage for production use
        private static readonly Dictionary<string, List<MongoDocument>> _collections =
            new Dictionary<string, List<MongoDocument>>();
        private static long _nextId = 1;
        private static readonly object _lock = new object();

        private class MongoDocument
        {
            public string Id { get; set; }
            public JObject Data { get; set; }
        }

        /// <summary>
        /// Insert a document into a collection.
        /// POST /api/mongodb/{collection}/insert
        /// </summary>
        [HttpPost("{collection}/insert")]
        public IActionResult InsertDocument(string collection, [FromBody] JObject document)
        {
            try
            {
                lock (_lock)
                {
                    if (!_collections.ContainsKey(collection))
                    {
                        _collections[collection] = new List<MongoDocument>();
                    }

                    var id = (_nextId++).ToString();
                    var doc = new MongoDocument { Id = id, Data = document };
                    _collections[collection].Add(doc);

                    return Ok(new
                    {
                        success = true,
                        insertedId = id,
                        message = $"Document inserted into collection '{collection}'"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Insert multiple documents into a collection.
        /// POST /api/mongodb/{collection}/insertMany
        /// </summary>
        [HttpPost("{collection}/insertMany")]
        public IActionResult InsertMany(string collection, [FromBody] JArray documents)
        {
            try
            {
                lock (_lock)
                {
                    if (!_collections.ContainsKey(collection))
                    {
                        _collections[collection] = new List<MongoDocument>();
                    }

                    var insertedIds = new List<string>();
                    foreach (var doc in documents)
                    {
                        if (doc is JObject jObj)
                        {
                            var id = (_nextId++).ToString();
                            _collections[collection].Add(new MongoDocument { Id = id, Data = jObj });
                            insertedIds.Add(id);
                        }
                    }

                    return Ok(new
                    {
                        success = true,
                        insertedIds = insertedIds,
                        insertedCount = insertedIds.Count
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Find documents in a collection.
        /// POST /api/mongodb/{collection}/find
        /// </summary>
        [HttpPost("{collection}/find")]
        public IActionResult FindDocuments(string collection, [FromBody] FindQuery query)
        {
            try
            {
                lock (_lock)
                {
                    if (!_collections.ContainsKey(collection))
                    {
                        return Ok(new { success = true, documents = new List<object>() });
                    }

                    var documents = _collections[collection]
                        .Where(doc => query?.Filter == null || MatchesFilter(doc.Data, query.Filter))
                        .Skip(query?.Skip ?? 0)
                        .Take(query?.Limit > 0 ? query.Limit : int.MaxValue)
                        .Select(doc =>
                        {
                            var result = new JObject(doc.Data);
                            result["_id"] = doc.Id;
                            return result;
                        })
                        .ToList();

                    return Ok(new
                    {
                        success = true,
                        documents = documents,
                        count = documents.Count
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Find one document in a collection.
        /// POST /api/mongodb/{collection}/findOne
        /// </summary>
        [HttpPost("{collection}/findOne")]
        public IActionResult FindOne(string collection, [FromBody] FindQuery query)
        {
            try
            {
                lock (_lock)
                {
                    if (!_collections.ContainsKey(collection))
                    {
                        return Ok(new { success = true, document = (object)null });
                    }

                    var doc = _collections[collection]
                        .FirstOrDefault(d => query?.Filter == null || MatchesFilter(d.Data, query.Filter));

                    if (doc == null)
                    {
                        return Ok(new { success = true, document = (object)null });
                    }

                    var result = new JObject(doc.Data);
                    result["_id"] = doc.Id;

                    return Ok(new
                    {
                        success = true,
                        document = result
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Update documents in a collection.
        /// POST /api/mongodb/{collection}/update
        /// </summary>
        [HttpPost("{collection}/update")]
        public IActionResult UpdateDocuments(string collection, [FromBody] UpdateQuery updateQuery)
        {
            try
            {
                lock (_lock)
                {
                    if (!_collections.ContainsKey(collection))
                    {
                        return Ok(new { success = true, modifiedCount = 0 });
                    }

                    var modifiedCount = 0;
                    foreach (var doc in _collections[collection])
                    {
                        if (updateQuery.Filter == null || MatchesFilter(doc.Data, updateQuery.Filter))
                        {
                            // Simple update - merge properties
                            if (updateQuery.Update != null)
                            {
                                foreach (var prop in updateQuery.Update.Properties())
                                {
                                    doc.Data[prop.Name] = prop.Value;
                                }
                            }
                            modifiedCount++;

                            if (!updateQuery.Multi && modifiedCount > 0)
                            {
                                break;
                            }
                        }
                    }

                    return Ok(new
                    {
                        success = true,
                        modifiedCount = modifiedCount
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Delete documents from a collection.
        /// POST /api/mongodb/{collection}/delete
        /// </summary>
        [HttpPost("{collection}/delete")]
        public IActionResult DeleteDocuments(string collection, [FromBody] DeleteQuery deleteQuery)
        {
            try
            {
                lock (_lock)
                {
                    if (!_collections.ContainsKey(collection))
                    {
                        return Ok(new { success = true, deletedCount = 0 });
                    }

                    var toDelete = _collections[collection]
                        .Where(doc => deleteQuery.Filter == null || MatchesFilter(doc.Data, deleteQuery.Filter))
                        .Take(deleteQuery.Single ? 1 : int.MaxValue)
                        .ToList();

                    foreach (var doc in toDelete)
                    {
                        _collections[collection].Remove(doc);
                    }

                    return Ok(new
                    {
                        success = true,
                        deletedCount = toDelete.Count
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Count documents in a collection.
        /// POST /api/mongodb/{collection}/count
        /// </summary>
        [HttpPost("{collection}/count")]
        public IActionResult CountDocuments(string collection, [FromBody] FindQuery query)
        {
            try
            {
                lock (_lock)
                {
                    if (!_collections.ContainsKey(collection))
                    {
                        return Ok(new { success = true, count = 0 });
                    }

                    var count = _collections[collection]
                        .Count(doc => query?.Filter == null || MatchesFilter(doc.Data, query.Filter));

                    return Ok(new
                    {
                        success = true,
                        count = count
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// List all collections.
        /// GET /api/mongodb/collections
        /// </summary>
        [HttpGet("collections")]
        public IActionResult ListCollections()
        {
            try
            {
                lock (_lock)
                {
                    return Ok(new
                    {
                        success = true,
                        collections = _collections.Keys.ToList()
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        #region Helper Methods

        private bool MatchesFilter(JObject document, JObject filter)
        {
            // Simple equality matching
            foreach (var property in filter.Properties())
            {
                var key = property.Name;
                var expectedValue = property.Value;

                if (!document.ContainsKey(key))
                {
                    return false;
                }

                var actualValue = document[key];

                // Simple string comparison
                if (actualValue.ToString() != expectedValue.ToString())
                {
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}
