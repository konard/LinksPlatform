/**
 * Node.js Example: Using LinksPlatform MongoDB Emulator
 *
 * This example demonstrates how to use the LinksPlatform MongoDB emulator
 * with a Node.js application using familiar MongoDB API.
 */

const { MongoClient } = require('./linksplatform-mongodb');

async function main() {
    // Connect to LinksPlatform MongoDB Emulator
    // Replace with your LinksPlatform WebTerminal URL
    const url = 'http://localhost:5000';
    const client = await MongoClient.connect(url);

    console.log('✓ Connected to LinksPlatform MongoDB Emulator');

    // Get database and collection
    const db = client.db('myapp');
    const users = db.collection('users');

    try {
        // Insert a single document
        console.log('\n1. Inserting a single user...');
        const insertResult = await users.insertOne({
            name: 'John Doe',
            email: 'john@example.com',
            age: 30,
            city: 'New York'
        });
        console.log('✓ Inserted user with ID:', insertResult.insertedId);

        // Insert multiple documents
        console.log('\n2. Inserting multiple users...');
        const insertManyResult = await users.insertMany([
            { name: 'Alice Smith', email: 'alice@example.com', age: 25, city: 'Los Angeles' },
            { name: 'Bob Johnson', email: 'bob@example.com', age: 35, city: 'Chicago' },
            { name: 'Carol Williams', email: 'carol@example.com', age: 28, city: 'New York' }
        ]);
        console.log('✓ Inserted', insertManyResult.insertedCount, 'users');

        // Find all documents
        console.log('\n3. Finding all users...');
        const allUsers = await users.find();
        console.log('✓ Found', allUsers.length, 'users');
        allUsers.forEach(user => {
            console.log(`  - ${user.name} (${user.email})`);
        });

        // Find with filter
        console.log('\n4. Finding users in New York...');
        const nyUsers = await users.find({ city: 'New York' });
        console.log('✓ Found', nyUsers.length, 'users in New York');
        nyUsers.forEach(user => {
            console.log(`  - ${user.name}`);
        });

        // Find one document
        console.log('\n5. Finding one user...');
        const oneUser = await users.findOne({ email: 'alice@example.com' });
        if (oneUser) {
            console.log('✓ Found user:', oneUser.name);
        }

        // Update a document
        console.log('\n6. Updating user age...');
        const updateResult = await users.updateOne(
            { email: 'john@example.com' },
            { age: 31 }
        );
        console.log('✓ Updated', updateResult.modifiedCount, 'document');

        // Update multiple documents
        console.log('\n7. Updating all users in New York...');
        const updateManyResult = await users.updateMany(
            { city: 'New York' },
            { city: 'New York City' }
        );
        console.log('✓ Updated', updateManyResult.modifiedCount, 'documents');

        // Count documents
        console.log('\n8. Counting users...');
        const count = await users.countDocuments();
        console.log('✓ Total users:', count);

        // Count with filter
        const nyCount = await users.countDocuments({ city: 'New York City' });
        console.log('✓ Users in New York City:', nyCount);

        // Delete a document
        console.log('\n9. Deleting one user...');
        const deleteResult = await users.deleteOne({ email: 'bob@example.com' });
        console.log('✓ Deleted', deleteResult.deletedCount, 'document');

        // List collections
        console.log('\n10. Listing all collections...');
        const collections = await db.listCollections();
        console.log('✓ Collections:', collections.join(', '));

        console.log('\n✓ All operations completed successfully!');

    } catch (error) {
        console.error('Error:', error.message);
    } finally {
        // Close connection
        await client.close();
        console.log('\n✓ Connection closed');
    }
}

// Run the example
main().catch(console.error);
