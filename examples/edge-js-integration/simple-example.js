// Simple inline C# example using edge-js
const edge = require('edge-js');

// Example 1: Simple synchronous-style function
const helloWorld = edge.func(function () {/*
    async (input) => {
        return ".NET Welcomes " + input.ToString();
    }
*/});

// Example 2: Using Platform.Data.Doublets types
const linksExample = edge.func(function () {/*
    #r "System.Runtime.dll"
    #r "System.Collections.dll"

    using System;
    using System.Threading.Tasks;

    public class Startup
    {
        public async Task<object> Invoke(dynamic input)
        {
            var message = input.message?.ToString() ?? "Links";
            return new
            {
                success = true,
                message = $"LinksPlatform says: {message}",
                timestamp = DateTime.Now.ToString("o")
            };
        }
    }
*/});

async function main() {
    console.log('=== Simple Edge.js Examples ===\n');

    // Test hello world
    console.log('1. Hello World Example:');
    helloWorld('JavaScript', function (error, result) {
        if (error) throw error;
        console.log('   Result:', result);
        console.log();

        // Test links example
        console.log('2. LinksPlatform Example:');
        linksExample({ message: 'Hello from Node.js!' }, function (error, result) {
            if (error) throw error;
            console.log('   Result:', JSON.stringify(result, null, 2));
            console.log();
            console.log('✅ Simple examples completed!');
        });
    });
}

main();
