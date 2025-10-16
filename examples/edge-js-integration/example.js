const LinksClient = require('./index');
const path = require('path');
const fs = require('fs');

async function main() {
    console.log('LinksPlatform Edge.js Integration Example\n');

    // Create a temporary database file
    const dbPath = path.join(__dirname, 'test.links');

    // Clean up existing test database if it exists
    if (fs.existsSync(dbPath)) {
        fs.unlinkSync(dbPath);
        console.log('Cleaned up existing test database\n');
    }

    try {
        // Initialize Links client
        console.log('Initializing Links client...');
        const links = new LinksClient(dbPath);

        // Get initial count
        console.log('\n1. Getting initial count...');
        let countResult = await links.count();
        console.log('Result:', JSON.stringify(countResult, null, 2));

        // Create a link
        console.log('\n2. Creating a link with source=1, target=1...');
        let createResult = await links.createLink(1, 1);
        console.log('Result:', JSON.stringify(createResult, null, 2));

        if (createResult.success) {
            const linkId = createResult.link;

            // Get the created link
            console.log(`\n3. Getting link ${linkId}...`);
            let getResult = await links.getLink(linkId);
            console.log('Result:', JSON.stringify(getResult, null, 2));

            // Update the link
            console.log(`\n4. Updating link ${linkId} to source=2, target=3...`);
            let updateResult = await links.update(linkId, 2, 3);
            console.log('Result:', JSON.stringify(updateResult, null, 2));

            // Get the updated link
            console.log(`\n5. Getting updated link ${linkId}...`);
            getResult = await links.getLink(linkId);
            console.log('Result:', JSON.stringify(getResult, null, 2));

            // Get count after operations
            console.log('\n6. Getting count after operations...');
            countResult = await links.count();
            console.log('Result:', JSON.stringify(countResult, null, 2));

            // Use GetOrCreate
            console.log('\n7. Using GetOrCreate with source=2, target=3 (should return existing link)...');
            let getOrCreateResult = await links.getOrCreate(2, 3);
            console.log('Result:', JSON.stringify(getOrCreateResult, null, 2));

            console.log('\n8. Using GetOrCreate with source=4, target=5 (should create new link)...');
            getOrCreateResult = await links.getOrCreate(4, 5);
            console.log('Result:', JSON.stringify(getOrCreateResult, null, 2));

            // Final count
            console.log('\n9. Getting final count...');
            countResult = await links.count();
            console.log('Result:', JSON.stringify(countResult, null, 2));

            // Delete a link
            console.log(`\n10. Deleting link ${linkId}...`);
            let deleteResult = await links.delete(linkId);
            console.log('Result:', JSON.stringify(deleteResult, null, 2));

            // Count after deletion
            console.log('\n11. Getting count after deletion...');
            countResult = await links.count();
            console.log('Result:', JSON.stringify(countResult, null, 2));
        }

        console.log('\n✅ Example completed successfully!');
    } catch (error) {
        console.error('\n❌ Error:', error.message);
        console.error(error.stack);
    }
}

main();
