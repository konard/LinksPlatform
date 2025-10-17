using System;
using Platform.Examples;

namespace Experiments
{
    // Example classes with the same interface but different names
    public class UserRepository
    {
        public void Add(object item) { }
        public void Remove(object item) { }
        public object Get(int id) { return null; }
    }

    public class CustomerStore
    {
        public void Add(object item) { }
        public void Remove(object item) { }
        public object Get(int id) { return null; }
    }

    public class DataManager
    {
        public void Add(object item) { }
        public void Remove(object item) { }
        public object Get(int id) { return null; }
    }

    // Different interface
    public class FileHandler
    {
        public void Write(string data) { }
        public string Read() { return ""; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var service = new ClassNameService();

            // Add various classes to the database
            service.AddClass(typeof(UserRepository));
            service.AddClass(typeof(CustomerStore));
            service.AddClass(typeof(DataManager));
            service.AddClass(typeof(FileHandler));

            Console.WriteLine("=== Class Name Service Demo ===\n");
            Console.WriteLine($"Total classes: {service.ClassCount}");
            Console.WriteLine($"Unique interface signatures: {service.SignatureCount}\n");

            // Query for similar names based on interface
            Console.WriteLine("Classes with the same interface as 'UserRepository':");
            var similarNames = service.GetSimilarNames("UserRepository");
            foreach (var name in similarNames)
            {
                Console.WriteLine($"  - {name}");
            }

            Console.WriteLine("\nClasses with the same interface as 'FileHandler':");
            similarNames = service.GetSimilarNames("FileHandler");
            foreach (var name in similarNames)
            {
                Console.WriteLine($"  - {name}");
            }

            // Show interface signatures
            Console.WriteLine("\n=== Interface Signatures ===");
            foreach (var signature in service.GetAllSignatures())
            {
                var names = service.GetNamesForSignature(signature);
                Console.WriteLine($"\nSignature: {signature}");
                Console.WriteLine($"Used names: {string.Join(", ", names)}");
            }

            Console.WriteLine("\n=== Demonstration Complete ===");
        }
    }
}
