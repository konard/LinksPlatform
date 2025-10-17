using System;
using System.Collections.Generic;
using MongoDB.Driver;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

namespace Platform.Data.MongoDB.Tests
{
    /// <summary>
    /// Example demonstrating how to use MongoDBLinksDecorator for synchronization with MongoDB.
    /// This enables integration with reactive frameworks like Meteor.
    /// </summary>
    public class MongoDBLinksExample
    {
        public static void Run()
        {
            // Configuration
            const string mongoUrl = "mongodb://localhost:27017";
            const string databaseName = "LinksDatabase";
            const string collectionName = "links";

            Console.WriteLine("MongoDB Links Synchronization Example");
            Console.WriteLine("======================================");
            Console.WriteLine();

            try
            {
                // Initialize the underlying links storage
                using var memory = new HeapResizableDirectMemory();
                var links = new UnitedMemoryLinks<ulong>(memory);

                // Create MongoDB decorator that will sync all changes
                var mongoDecorator = new MongoDBLinksDecorator<ulong>(
                    links,
                    mongoUrl,
                    databaseName,
                    collectionName
                );

                Console.WriteLine("Connected to MongoDB at: " + mongoUrl);
                Console.WriteLine("Database: " + databaseName);
                Console.WriteLine("Collection: " + collectionName);
                Console.WriteLine();

                // Create a link - this will be automatically synced to MongoDB
                Console.WriteLine("Creating a link...");
                var link1 = mongoDecorator.Create(new List<ulong> { 1, 2 });
                Console.WriteLine($"Created link with ID: {link1}");
                Console.WriteLine($"  Source: 1, Target: 2");
                Console.WriteLine();

                // Create another link
                Console.WriteLine("Creating another link...");
                var link2 = mongoDecorator.Create(new List<ulong> { 3, 4 });
                Console.WriteLine($"Created link with ID: {link2}");
                Console.WriteLine($"  Source: 3, Target: 4");
                Console.WriteLine();

                // Update a link - this will sync the update to MongoDB
                Console.WriteLine($"Updating link {link1}...");
                mongoDecorator.Update(
                    new List<ulong> { link1, 1, 2 },
                    new List<ulong> { link1, 5, 6 }
                );
                Console.WriteLine($"Updated link {link1}");
                Console.WriteLine($"  New Source: 5, New Target: 6");
                Console.WriteLine();

                // Delete a link - this will remove it from MongoDB
                Console.WriteLine($"Deleting link {link2}...");
                mongoDecorator.Delete(new List<ulong> { link2 });
                Console.WriteLine($"Deleted link {link2}");
                Console.WriteLine();

                Console.WriteLine("All operations have been synchronized to MongoDB!");
                Console.WriteLine();
                Console.WriteLine("You can now:");
                Console.WriteLine("1. Query the MongoDB collection to see the synced data");
                Console.WriteLine("2. Use MongoDB change streams for reactive updates");
                Console.WriteLine("3. Integrate with Meteor or other reactive frameworks");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.Error.WriteLine();
                Console.Error.WriteLine("Make sure MongoDB is running on localhost:27017");
            }
        }
    }
}
