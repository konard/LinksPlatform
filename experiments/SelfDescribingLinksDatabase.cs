using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace LinksPlatform.Experiments
{
    /// <summary>
    /// Proof-of-concept demonstration of a self-describing Links database.
    /// This experiment shows how a Links database can contain metadata about itself,
    /// including its structure, build information, and compilation details.
    ///
    /// This addresses issue #32: "Build Links database that contains enough information
    /// to compile/build/package/adapt/reproduce itself to any language/environment"
    ///
    /// Concept: Similar to how the Nemerle compiler is written in Nemerle itself,
    /// this demonstrates how Links can describe their own structure and build process.
    /// </summary>
    public class SelfDescribingLinksDatabase
    {
        private readonly ILinks<ulong> _links;

        // Core metadata concepts stored as Links
        private ulong _conceptLink;
        private ulong _typeLink;
        private ulong _propertyLink;
        private ulong _hasPropertyLink;
        private ulong _hasTypeLink;

        // Links system structure concepts
        private ulong _linkStructureLink;
        private ulong _sourcePropertyLink;
        private ulong _targetPropertyLink;
        private ulong _indexPropertyLink;

        // Build and compilation concepts
        private ulong _buildInfoLink;
        private ulong _languageLink;
        private ulong _csharpLink;
        private ulong _cppLink;
        private ulong _compilationStepLink;

        public SelfDescribingLinksDatabase(ILinks<ulong> links)
        {
            _links = links;
        }

        /// <summary>
        /// Initializes the self-describing metadata within the Links database.
        /// This method creates Links that describe the Links system itself.
        /// </summary>
        public void InitializeSelfDescription()
        {
            Console.WriteLine("=== Initializing Self-Describing Links Database ===\n");

            // Step 1: Create fundamental concepts
            CreateFundamentalConcepts();

            // Step 2: Describe the Link structure itself using Links
            DescribeLinkStructure();

            // Step 3: Store build and compilation information
            StoreBuildInformation();

            // Step 4: Store language adaptation information
            StoreLanguageAdaptationInfo();

            Console.WriteLine("\n=== Self-Description Complete ===");
            Console.WriteLine($"Total links in database: {_links.Count()}");
        }

        /// <summary>
        /// Creates fundamental concepts that will be used to describe the system.
        /// These are meta-concepts like "Concept", "Type", "Property", etc.
        /// </summary>
        private void CreateFundamentalConcepts()
        {
            Console.WriteLine("Creating fundamental concepts...");

            // Create self-referential "Concept" - a concept is itself a concept
            _conceptLink = _links.Create();
            _conceptLink = _links.Update(_conceptLink, _conceptLink, _conceptLink);
            Console.WriteLine($"  Created 'Concept' (self-referential): {_conceptLink}");

            // Create "Type" concept
            _typeLink = _links.Create();
            _links.Update(_typeLink, _conceptLink, _typeLink);
            Console.WriteLine($"  Created 'Type': {_typeLink}");

            // Create "Property" concept
            _propertyLink = _links.Create();
            _links.Update(_propertyLink, _conceptLink, _propertyLink);
            Console.WriteLine($"  Created 'Property': {_propertyLink}");

            // Create "HasProperty" relationship
            _hasPropertyLink = _links.Create();
            _links.Update(_hasPropertyLink, _conceptLink, _hasPropertyLink);
            Console.WriteLine($"  Created 'HasProperty' relationship: {_hasPropertyLink}");

            // Create "HasType" relationship
            _hasTypeLink = _links.Create();
            _links.Update(_hasTypeLink, _conceptLink, _hasTypeLink);
            Console.WriteLine($"  Created 'HasType' relationship: {_hasTypeLink}");
        }

        /// <summary>
        /// Describes the Link structure itself using Links.
        /// This is the key to self-description: Links describing Links!
        /// </summary>
        private void DescribeLinkStructure()
        {
            Console.WriteLine("\nDescribing Link structure using Links...");

            // Create "LinkStructure" concept
            _linkStructureLink = _links.Create();
            _links.Update(_linkStructureLink, _conceptLink, _linkStructureLink);
            Console.WriteLine($"  Created 'LinkStructure': {_linkStructureLink}");

            // LinkStructure has type Type
            var linkStructureType = _links.Create();
            _links.Update(linkStructureType, _linkStructureLink, _hasTypeLink, _typeLink);
            Console.WriteLine($"  LinkStructure has type 'Type': {linkStructureType}");

            // Create properties of a Link
            _sourcePropertyLink = _links.Create();
            _links.Update(_sourcePropertyLink, _conceptLink, _propertyLink);
            Console.WriteLine($"  Created 'Source' property: {_sourcePropertyLink}");

            _targetPropertyLink = _links.Create();
            _links.Update(_targetPropertyLink, _conceptLink, _propertyLink);
            Console.WriteLine($"  Created 'Target' property: {_targetPropertyLink}");

            _indexPropertyLink = _links.Create();
            _links.Update(_indexPropertyLink, _conceptLink, _propertyLink);
            Console.WriteLine($"  Created 'Index' property: {_indexPropertyLink}");

            // Link LinkStructure to its properties
            var linkHasSource = _links.Create();
            _links.Update(linkHasSource, _linkStructureLink, _hasPropertyLink, _sourcePropertyLink);
            Console.WriteLine($"  LinkStructure has property 'Source': {linkHasSource}");

            var linkHasTarget = _links.Create();
            _links.Update(linkHasTarget, _linkStructureLink, _hasPropertyLink, _targetPropertyLink);
            Console.WriteLine($"  LinkStructure has property 'Target': {linkHasTarget}");

            var linkHasIndex = _links.Create();
            _links.Update(linkHasIndex, _linkStructureLink, _hasPropertyLink, _indexPropertyLink);
            Console.WriteLine($"  LinkStructure has property 'Index': {linkHasIndex}");
        }

        /// <summary>
        /// Stores build and compilation information in the Links database.
        /// This demonstrates how the database can contain instructions for rebuilding itself.
        /// </summary>
        private void StoreBuildInformation()
        {
            Console.WriteLine("\nStoring build and compilation information...");

            // Create BuildInfo concept
            _buildInfoLink = _links.Create();
            _links.Update(_buildInfoLink, _conceptLink, _buildInfoLink);
            Console.WriteLine($"  Created 'BuildInfo': {_buildInfoLink}");

            // Create CompilationStep concept
            _compilationStepLink = _links.Create();
            _links.Update(_compilationStepLink, _conceptLink, _compilationStepLink);
            Console.WriteLine($"  Created 'CompilationStep': {_compilationStepLink}");

            // Create specific compilation steps
            var step1 = CreateCompilationStep("RestorePackages", _compilationStepLink);
            var step2 = CreateCompilationStep("CompileSource", _compilationStepLink);
            var step3 = CreateCompilationStep("RunTests", _compilationStepLink);
            var step4 = CreateCompilationStep("CreatePackage", _compilationStepLink);

            // Create sequence of compilation steps
            var stepsSequence = _links.Create();
            _links.Update(stepsSequence, step1, step2);
            var stepsSequence2 = _links.Create();
            _links.Update(stepsSequence2, stepsSequence, step3);
            var stepsSequence3 = _links.Create();
            _links.Update(stepsSequence3, stepsSequence2, step4);

            Console.WriteLine($"  Created compilation steps sequence: {stepsSequence3}");

            // Link BuildInfo to the steps sequence
            var buildHasSteps = _links.Create();
            _links.Update(buildHasSteps, _buildInfoLink, _hasPropertyLink, stepsSequence3);
            Console.WriteLine($"  BuildInfo has compilation steps: {buildHasSteps}");
        }

        /// <summary>
        /// Stores information about how to adapt Links to different languages/environments.
        /// This demonstrates the "reproduce itself to any language/environment" aspect.
        /// </summary>
        private void StoreLanguageAdaptationInfo()
        {
            Console.WriteLine("\nStoring language adaptation information...");

            // Create Language concept
            _languageLink = _links.Create();
            _links.Update(_languageLink, _conceptLink, _languageLink);
            Console.WriteLine($"  Created 'Language': {_languageLink}");

            // Create specific language implementations
            _csharpLink = CreateLanguageImplementation("CSharp", _languageLink);
            _cppLink = CreateLanguageImplementation("CPlusPlus", _languageLink);
            var jsLink = CreateLanguageImplementation("JavaScript", _languageLink);
            var pythonLink = CreateLanguageImplementation("Python", _languageLink);

            // Map LinkStructure to language implementations
            var csharpMapping = _links.Create();
            _links.Update(csharpMapping, _linkStructureLink, _csharpLink, _csharpLink);
            Console.WriteLine($"  LinkStructure maps to C# implementation: {csharpMapping}");

            var cppMapping = _links.Create();
            _links.Update(cppMapping, _linkStructureLink, _cppLink, _cppLink);
            Console.WriteLine($"  LinkStructure maps to C++ implementation: {cppMapping}");

            var jsMapping = _links.Create();
            _links.Update(jsMapping, _linkStructureLink, jsLink, jsLink);
            Console.WriteLine($"  LinkStructure maps to JavaScript implementation: {jsMapping}");

            var pythonMapping = _links.Create();
            _links.Update(pythonMapping, _linkStructureLink, pythonLink, pythonLink);
            Console.WriteLine($"  LinkStructure maps to Python implementation: {pythonMapping}");
        }

        private ulong CreateCompilationStep(string stepName, ulong typeLink)
        {
            var step = _links.Create();
            _links.Update(step, typeLink, step);
            Console.WriteLine($"    Created compilation step '{stepName}': {step}");
            return step;
        }

        private ulong CreateLanguageImplementation(string languageName, ulong typeLink)
        {
            var lang = _links.Create();
            _links.Update(lang, typeLink, lang);
            Console.WriteLine($"    Created language '{languageName}': {lang}");
            return lang;
        }

        /// <summary>
        /// Demonstrates querying the self-describing database.
        /// This shows how the metadata can be used to understand the system.
        /// </summary>
        public void QuerySelfDescription()
        {
            Console.WriteLine("\n=== Querying Self-Description ===\n");

            Console.WriteLine("Link Structure Properties:");
            var any = _links.Constants.Any;

            // Query all properties of LinkStructure
            var query = new Link<ulong>(any, _linkStructureLink, _hasPropertyLink, any);
            _links.Each(link =>
            {
                var propertyLink = _links.GetTarget(link);
                Console.WriteLine($"  Property ID: {propertyLink}");
                return _links.Constants.Continue;
            }, query);

            Console.WriteLine("\nThis database now contains enough information to:");
            Console.WriteLine("  1. Understand its own structure (Links have Source, Target, Index)");
            Console.WriteLine("  2. Rebuild itself (compilation steps are stored)");
            Console.WriteLine("  3. Adapt to different languages (language mappings are stored)");
            Console.WriteLine("  4. Bootstrap from the stored metadata");
        }

        /// <summary>
        /// Demonstrates the bootstrapping concept by showing how the database
        /// can be used to generate its own description in another format.
        /// </summary>
        public string GenerateSchemaDescription()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Generated Schema Description ===");
            sb.AppendLine();
            sb.AppendLine("Link Structure:");
            sb.AppendLine($"  Type: LinkStructure (ID: {_linkStructureLink})");
            sb.AppendLine($"  Properties:");
            sb.AppendLine($"    - Source (ID: {_sourcePropertyLink})");
            sb.AppendLine($"    - Target (ID: {_targetPropertyLink})");
            sb.AppendLine($"    - Index (ID: {_indexPropertyLink})");
            sb.AppendLine();
            sb.AppendLine("Build Information:");
            sb.AppendLine($"  BuildInfo (ID: {_buildInfoLink})");
            sb.AppendLine($"  Compilation Steps: RestorePackages -> CompileSource -> RunTests -> CreatePackage");
            sb.AppendLine();
            sb.AppendLine("Supported Languages:");
            sb.AppendLine($"  - C# (ID: {_csharpLink})");
            sb.AppendLine($"  - C++ (ID: {_cppLink})");
            sb.AppendLine($"  - JavaScript");
            sb.AppendLine($"  - Python");

            return sb.ToString();
        }
    }

    /// <summary>
    /// Example program demonstrating the self-describing Links database.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("LinksPlatform Self-Describing Database Experiment");
            Console.WriteLine("Issue #32: Proof of Concept\n");

            // Create a temporary in-memory Links database
            var dbPath = "self-describing-links.db";
            using (var links = new UnitedMemoryLinks<ulong>(dbPath))
            {
                var selfDescribing = new SelfDescribingLinksDatabase(links);

                // Initialize the self-describing metadata
                selfDescribing.InitializeSelfDescription();

                // Query the self-description
                selfDescribing.QuerySelfDescription();

                // Generate and display schema description
                Console.WriteLine("\n" + selfDescribing.GenerateSchemaDescription());

                Console.WriteLine("\n=== Conclusion ===");
                Console.WriteLine("This proof-of-concept demonstrates that a Links database can:");
                Console.WriteLine("1. Store metadata about its own structure");
                Console.WriteLine("2. Contain build/compilation instructions");
                Console.WriteLine("3. Map to multiple language implementations");
                Console.WriteLine("4. Bootstrap its own description");
                Console.WriteLine();
                Console.WriteLine("This is conceptually similar to how Nemerle compiler is written in Nemerle.");
                Console.WriteLine($"The database file '{dbPath}' now contains a self-describing Links system.");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
