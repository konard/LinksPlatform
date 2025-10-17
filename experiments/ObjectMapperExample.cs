using System;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Examples;

namespace Platform.Experiments
{
    /// <summary>
    /// Demonstrates the usage of ObjectMapper for saving and loading C# objects to/from Links storage.
    /// This example shows how stateful managed objects can be persisted using automatic IL-generated mappers.
    /// </summary>
    public class ObjectMapperExample
    {
        // Example class with fields to be persisted
        public class Person
        {
            private string _name;
            private int _age;
            private bool _isActive;

            public Person()
            {
                // Default constructor required for ObjectMapper
            }

            public Person(string name, int age, bool isActive)
            {
                _name = name;
                _age = age;
                _isActive = isActive;
            }

            public string GetName() => _name;
            public int GetAge() => _age;
            public bool GetIsActive() => _isActive;

            public void SetName(string name) => _name = name;
            public void SetAge(int age) => _age = age;
            public void SetIsActive(bool isActive) => _isActive = isActive;

            public override string ToString()
            {
                return $"Person(Name: {_name}, Age: {_age}, Active: {_isActive})";
            }
        }

        public static void Run()
        {
            Console.WriteLine("=== Object Mapper Example ===");
            Console.WriteLine();

            // Create in-memory links storage
            using (var links = new UnitedMemoryLinks<uint>())
            {
                var mapper = new ObjectMapper<uint>(links);

                // Create and save an object
                var person = new Person("Alice", 30, true);
                Console.WriteLine($"Original object: {person}");

                var linkId = mapper.Save(person);
                Console.WriteLine($"Object saved to link: {linkId}");
                Console.WriteLine();

                // Load the object back
                var loadedPerson = mapper.Load<Person>(linkId);
                Console.WriteLine($"Loaded object: {loadedPerson}");
                Console.WriteLine();

                // Verify the data matches
                Console.WriteLine("Verification:");
                Console.WriteLine($"  Name matches: {person.GetName() == loadedPerson.GetName()}");
                Console.WriteLine($"  Age matches: {person.GetAge() == loadedPerson.GetAge()}");
                Console.WriteLine($"  IsActive matches: {person.GetIsActive() == loadedPerson.GetIsActive()}");
            }

            Console.WriteLine();
            Console.WriteLine("=== Example Complete ===");
        }

        public static void Main(string[] args)
        {
            try
            {
                Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
