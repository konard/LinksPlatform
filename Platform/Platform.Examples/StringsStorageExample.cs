using System;
using Platform.Memory;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Sequences.Frequencies.Cache;
using Platform.Data.Doublets.Sequences.Frequencies.Counters;

namespace Platform.Examples
{
    public static class StringsStorageExample
    {
        public static void Run()
        {
            using (var memory = new HeapResizableDirectMemory())
            using (var links = new UnitedMemoryLinks<uint>(memory))
            {
                var totalSequenceSymbolFrequencyCounter = new TotalSequenceSymbolFrequencyCounter<uint>(links);
                var cache = new LinkFrequenciesCache<uint>(links, totalSequenceSymbolFrequencyCounter);
                var storage = new StringsStorage<uint>(links, indexSequenceBeforeCreation: true, cache);

                Console.WriteLine("=== Strings Storage Service Example ===\n");

                // Store some strings
                Console.WriteLine("Storing strings...");
                var helloLink = storage.Store("Hello");
                var worldLink = storage.Store("World");
                var platformLink = storage.Store("LinksPlatform");

                Console.WriteLine($"Stored 'Hello' with link: {helloLink}");
                Console.WriteLine($"Stored 'World' with link: {worldLink}");
                Console.WriteLine($"Stored 'LinksPlatform' with link: {platformLink}");

                // Retrieve strings
                Console.WriteLine("\nRetrieving strings...");
                Console.WriteLine($"Link {helloLink} contains: '{storage.Get(helloLink)}'");
                Console.WriteLine($"Link {worldLink} contains: '{storage.Get(worldLink)}'");
                Console.WriteLine($"Link {platformLink} contains: '{storage.Get(platformLink)}'");

                // Check if strings exist
                Console.WriteLine("\nChecking if strings exist...");
                Console.WriteLine($"Contains 'Hello': {storage.Contains("Hello")}");
                Console.WriteLine($"Contains 'World': {storage.Contains("World")}");
                Console.WriteLine($"Contains 'LinksPlatform': {storage.Contains("LinksPlatform")}");
                Console.WriteLine($"Contains 'NonExistent': {storage.Contains("NonExistent")}");

                // GetOrCreate - should return existing link
                Console.WriteLine("\nUsing GetOrCreate...");
                var helloLink2 = storage.GetOrCreate("Hello");
                Console.WriteLine($"GetOrCreate 'Hello' (should be same as first Store): {helloLink2}");

                // GetOrCreate - should create new link
                var newStringLink = storage.GetOrCreate("New String");
                Console.WriteLine($"GetOrCreate 'New String': {newStringLink}");
                Console.WriteLine($"Contains 'New String': {storage.Contains("New String")}");

                Console.WriteLine($"\nTotal links in database: {links.Count(new uint[] { links.Constants.Any })}");
            }
        }
    }
}
