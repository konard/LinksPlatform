using System;
using System.IO;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Sequences.Indexes;
using Platform.Memory;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the Universal Search Engine example.
    /// Demonstrates how to use the infinitely precise universal search engine
    /// to index objects with their characteristics and perform precise searches.
    /// </summary>
    public class UniversalSearchEngineCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            var tempFilename = Path.GetTempFileName();

            try
            {
                Console.WriteLine("Universal Search Engine Demo");
                Console.WriteLine("============================\n");

                // Create links storage
                using (var memory = new HeapResizableDirectMemory())
                using (var linksStorage = new UnitedMemoryLinks<ulong>(memory))
                {
                ILinks<ulong> links = linksStorage;
                var index = new SequenceIndex<ulong>(links);

                // Create search engine
                var searchEngine = new UniversalSearchEngine<ulong>(links, index);

                // Demo: Register some objects with characteristics
                Console.WriteLine("Registering objects with characteristics...\n");

                var laptop = searchEngine.RegisterObject("Gaming Laptop");
                searchEngine.AddCharacteristic(laptop, "brand", "ASUS");
                searchEngine.AddCharacteristic(laptop, "processor", "Intel Core i7");
                searchEngine.AddCharacteristic(laptop, "ram", "16GB");
                searchEngine.AddCharacteristic(laptop, "storage", "512GB SSD");
                searchEngine.AddCharacteristic(laptop, "price", "$1200");
                Console.WriteLine("✓ Registered: Gaming Laptop");

                var desktop = searchEngine.RegisterObject("Office Desktop");
                searchEngine.AddCharacteristic(desktop, "brand", "Dell");
                searchEngine.AddCharacteristic(desktop, "processor", "Intel Core i5");
                searchEngine.AddCharacteristic(desktop, "ram", "8GB");
                searchEngine.AddCharacteristic(desktop, "storage", "256GB SSD");
                searchEngine.AddCharacteristic(desktop, "price", "$800");
                Console.WriteLine("✓ Registered: Office Desktop");

                var tablet = searchEngine.RegisterObject("Tablet Pro");
                searchEngine.AddCharacteristic(tablet, "brand", "Microsoft");
                searchEngine.AddCharacteristic(tablet, "processor", "ARM Processor");
                searchEngine.AddCharacteristic(tablet, "ram", "8GB");
                searchEngine.AddCharacteristic(tablet, "storage", "256GB SSD");
                searchEngine.AddCharacteristic(tablet, "price", "$900");
                Console.WriteLine("✓ Registered: Tablet Pro");

                var smartphone = searchEngine.RegisterObject("Premium Phone");
                searchEngine.AddCharacteristic(smartphone, "brand", "Samsung");
                searchEngine.AddCharacteristic(smartphone, "processor", "Snapdragon 888");
                searchEngine.AddCharacteristic(smartphone, "ram", "8GB");
                searchEngine.AddCharacteristic(smartphone, "storage", "128GB");
                searchEngine.AddCharacteristic(smartphone, "price", "$999");
                Console.WriteLine("✓ Registered: Premium Phone\n");

                // Demo: Search by single criterion
                Console.WriteLine("Search Example 1: Find all devices with 8GB RAM");
                Console.WriteLine("------------------------------------------------");
                var results = searchEngine.Search("ram", "8GB");
                foreach (var result in results)
                {
                    var name = searchEngine.GetObjectName(result);
                    Console.WriteLine($"  • {name}");
                }

                Console.WriteLine("\nSearch Example 2: Find all products with 256GB SSD");
                Console.WriteLine("---------------------------------------------------");
                results = searchEngine.Search("storage", "256GB SSD");
                foreach (var result in results)
                {
                    var name = searchEngine.GetObjectName(result);
                    Console.WriteLine($"  • {name}");
                }

                // Demo: Search by multiple criteria
                Console.WriteLine("\nSearch Example 3: Find devices with 8GB RAM AND 256GB SSD");
                Console.WriteLine("----------------------------------------------------------");
                results = searchEngine.SearchMultipleCriteria(
                    ("ram", "8GB"),
                    ("storage", "256GB SSD")
                );
                foreach (var result in results)
                {
                    var name = searchEngine.GetObjectName(result);
                    Console.WriteLine($"  • {name}");
                }

                // Demo: Get all characteristics of an object
                Console.WriteLine("\nObject Details: Gaming Laptop");
                Console.WriteLine("-----------------------------");
                var characteristics = searchEngine.GetCharacteristics(laptop);
                foreach (var (criteria, value) in characteristics)
                {
                    Console.WriteLine($"  {criteria}: {value}");
                }

                Console.WriteLine("\n✓ Demo completed successfully!");
                var anyRestriction = new Link<ulong>(links.Constants.Any, links.Constants.Any);
                Console.WriteLine($"\nTotal links used: {links.Count(anyRestriction)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                if (File.Exists(tempFilename))
                {
                    File.Delete(tempFilename);
                }
            }
        }
    }
}
