using System;
using System.Collections.Generic;
using Platform.Data;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// Demonstrates the world model framework that combines physical and knowledge domains
    /// using Links Platform associative memory architecture.
    /// </summary>
    /// <remarks>
    /// This example shows how to represent both physical entities (with properties like mass, charge)
    /// and knowledge concepts (with semantic relationships) in a unified graph structure.
    /// The goal is to create "the most precise world model" by continuously refining representations.
    /// </remarks>
    public class WorldModelExample
    {
        private readonly ILinks<ulong> _links;
        private readonly Dictionary<string, ulong> _namedEntities;

        /// <summary>
        /// Initializes a new instance of the WorldModelExample class.
        /// </summary>
        /// <param name="links">The links storage instance for the associative memory.</param>
        public WorldModelExample(ILinks<ulong> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _namedEntities = new Dictionary<string, ulong>();
        }

        /// <summary>
        /// Creates or retrieves a named entity in the model.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <returns>The link identifier for the entity.</returns>
        public ulong GetOrCreateEntity(string name)
        {
            if (_namedEntities.TryGetValue(name, out var existingLink))
            {
                return existingLink;
            }

            var newLink = _links.Create();
            _namedEntities[name] = newLink;
            return newLink;
        }

        /// <summary>
        /// Creates a property relationship between an entity and a value.
        /// </summary>
        /// <param name="entity">The entity link.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="value">The value link.</param>
        /// <returns>The link representing the property relationship.</returns>
        public ulong CreateProperty(ulong entity, string propertyName, ulong value)
        {
            var property = GetOrCreateEntity(propertyName);
            var propertyValue = _links.Create(property, value);
            return _links.Create(entity, propertyValue);
        }

        /// <summary>
        /// Creates a semantic relationship between two entities.
        /// </summary>
        /// <param name="source">The source entity.</param>
        /// <param name="relationType">The type of relationship.</param>
        /// <param name="target">The target entity.</param>
        /// <returns>The link representing the relationship.</returns>
        public ulong CreateRelation(ulong source, string relationType, ulong target)
        {
            var relation = GetOrCreateEntity(relationType);
            var relationTarget = _links.Create(relation, target);
            return _links.Create(source, relationTarget);
        }

        /// <summary>
        /// Demonstrates physical domain modeling: represents a physical particle with properties.
        /// </summary>
        public void DemoPhysicalDomain()
        {
            Console.WriteLine("=== Physical Domain Demo ===");

            // Create electron entity
            var electron = GetOrCreateEntity("Electron");

            // Create physical properties
            var charge = GetOrCreateEntity("Charge");
            var chargeValue = GetOrCreateEntity("-1.602e-19");
            var chargeUnit = GetOrCreateEntity("Coulombs");

            var mass = GetOrCreateEntity("Mass");
            var massValue = GetOrCreateEntity("9.109e-31");
            var massUnit = GetOrCreateEntity("Kilograms");

            // Link properties to electron
            var chargeProperty = _links.Create(_links.Create(charge, chargeValue), chargeUnit);
            CreateProperty(electron, "Charge", chargeProperty);

            var massProperty = _links.Create(_links.Create(mass, massValue), massUnit);
            CreateProperty(electron, "Mass", massProperty);

            Console.WriteLine($"Created electron entity with charge and mass properties");
            Console.WriteLine($"Total links in model: {_links.Count()}");
        }

        /// <summary>
        /// Demonstrates knowledge domain modeling: represents concepts and their relationships.
        /// </summary>
        public void DemoKnowledgeDomain()
        {
            Console.WriteLine("\n=== Knowledge Domain Demo ===");

            // Create ontological hierarchy
            var animal = GetOrCreateEntity("Animal");
            var mammal = GetOrCreateEntity("Mammal");
            var dog = GetOrCreateEntity("Dog");

            // Create "is-a" relationships
            CreateRelation(mammal, "is-a", animal);
            CreateRelation(dog, "is-a", mammal);

            // Create properties
            var warmBlooded = GetOrCreateEntity("WarmBlooded");
            var hasFur = GetOrCreateEntity("HasFur");

            CreateRelation(mammal, "has-property", warmBlooded);
            CreateRelation(mammal, "has-property", hasFur);

            Console.WriteLine($"Created ontological hierarchy: Dog -> Mammal -> Animal");
            Console.WriteLine($"Total links in model: {_links.Count()}");
        }

        /// <summary>
        /// Demonstrates integration of physical and knowledge domains.
        /// </summary>
        public void DemoIntegratedModel()
        {
            Console.WriteLine("\n=== Integrated Model Demo ===");

            // Create a physical observation
            var observation = GetOrCreateEntity("Observation");
            var temperature = GetOrCreateEntity("Temperature");
            var tempValue = GetOrCreateEntity("100");
            var celsius = GetOrCreateEntity("Celsius");

            var tempMeasurement = _links.Create(_links.Create(temperature, tempValue), celsius);
            CreateProperty(observation, "Measured", tempMeasurement);

            // Link observation to knowledge
            var water = GetOrCreateEntity("Water");
            var boiling = GetOrCreateEntity("Boiling");

            CreateRelation(observation, "observes", water);
            CreateRelation(observation, "state", boiling);

            // Create causal relationship
            var heatingWater = GetOrCreateEntity("HeatingWater");
            var evaporation = GetOrCreateEntity("Evaporation");

            CreateRelation(heatingWater, "causes", evaporation);
            CreateRelation(evaporation, "when", tempMeasurement);

            Console.WriteLine($"Created integrated physical-knowledge model linking observations to concepts");
            Console.WriteLine($"Total links in model: {_links.Count()}");
        }

        /// <summary>
        /// Demonstrates precision tracking for model improvement.
        /// </summary>
        public void DemoPrecisionTracking()
        {
            Console.WriteLine("\n=== Precision Tracking Demo ===");

            // Create a measurement with uncertainty
            var measurement = GetOrCreateEntity("SpeedOfLight");
            var value = GetOrCreateEntity("299792458");
            var unit = GetOrCreateEntity("MetersPerSecond");
            var uncertainty = GetOrCreateEntity("0");
            var confidence = GetOrCreateEntity("1.0");

            var measurementValue = _links.Create(_links.Create(value, unit), uncertainty);
            CreateProperty(measurement, "Value", measurementValue);
            CreateProperty(measurement, "Confidence", confidence);

            // Create timestamp for temporal tracking
            var timestamp = GetOrCreateEntity("2024-01-01");
            CreateProperty(measurement, "Timestamp", timestamp);

            Console.WriteLine($"Created measurement with precision metadata");
            Console.WriteLine($"This enables tracking model accuracy over time");
            Console.WriteLine($"Total links in model: {_links.Count()}");
        }

        /// <summary>
        /// Runs all demonstration examples.
        /// </summary>
        public void RunAllDemos()
        {
            Console.WriteLine("World Model Framework - Demonstration");
            Console.WriteLine("======================================\n");

            DemoPhysicalDomain();
            DemoKnowledgeDomain();
            DemoIntegratedModel();
            DemoPrecisionTracking();

            Console.WriteLine("\n=== Summary ===");
            Console.WriteLine($"Total entities created: {_namedEntities.Count}");
            Console.WriteLine($"Total links in associative memory: {_links.Count()}");
            Console.WriteLine("\nThis demonstrates the foundation for building");
            Console.WriteLine("the most precise world model by continuously");
            Console.WriteLine("refining representations of physical and knowledge domains.");
        }
    }
}
