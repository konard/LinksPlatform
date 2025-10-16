using System;
using System.Collections.Generic;
using Platform.Data.Triplets;

namespace Platform.Sandbox
{
    /// <summary>
    /// Meta / Semantic Links - A minimal set of terms for constructing exact definitions of everything.
    ///
    /// This class implements the core semantic vocabulary needed to describe links using links themselves.
    /// It supports cognitive operations (Analysis, Induction, Deduction, Prognosis) and enables
    /// self-describing systems that can handle both static and dynamic aspects of reality.
    ///
    /// Based on issue #107: https://github.com/konard/LinksPlatform/issues/107
    /// </summary>
    public class SemanticLinks
    {
        /// <summary>
        /// Core semantic terms mapping - the minimal vocabulary for universal descriptions
        /// </summary>
        public enum SemanticMapping
        {
            // Core structural terms (from doublets model)
            Source = 1000,
            Linker,
            Target,

            // Fundamental relations
            Is,              // Identity relation (a is b)
            Has,             // Possession relation (a has b)
            PartOf,          // Composition relation (a is part of b)
            RelatesTo,       // Generic relation

            // Meta-description terms
            Describes,       // Meta-level description
            DefinedAs,       // Explicit definition
            MeaningSame,     // Synonymy
            MeaningOpposite, // Antonymy

            // Temporal/Dynamic terms
            Causes,          // Causal relation
            Precedes,        // Temporal precedence
            Follows,         // Temporal sequence
            ChangesTo,       // State transformation

            // Cognitive operation terms
            Analysis,        // Decomposition/splitting
            Synthesis,       // Combination/building
            Induction,       // Bottom-up reasoning
            Deduction,       // Top-down reasoning
            Prognosis,       // Prediction/consequences

            // Logical terms
            And,             // Conjunction
            Or,              // Disjunction
            Not,             // Negation
            Implies,         // Logical implication

            // Quantification
            All,             // Universal quantifier
            Some,            // Existential quantifier
            None,            // Negated existence

            // Type/Category terms
            Type,            // Type classification
            Instance,        // Instance of type
            Property,        // Property/attribute
            Value,           // Property value

            // Collection terms
            Set,             // Mathematical set
            Sequence,        // Ordered collection
            Element,         // Collection member
            Contains,        // Containment relation

            // Abstract concepts
            Concept,         // Abstract idea
            Concrete,        // Concrete instance
            Abstract,        // Abstract instance

            // Change/Dynamics
            CanChange,       // Mutable characteristic
            Dimension,       // Dimension of change
            State,           // Current state
            Transition,      // State transition

            // Self-reference
            Itself,          // Self-reference marker
            Meta,            // Meta-level marker
            Object,          // Object-level marker
        }

        // Core structural links
        private static Link _source;
        private static Link _linker;
        private static Link _target;
        private static Link _sourceLink;
        private static Link _linkerLink;
        private static Link _targetLink;

        // Fundamental relations
        private static Link _is;
        private static Link _has;
        private static Link _partOf;
        private static Link _relatesTo;

        // Meta-description
        private static Link _describes;
        private static Link _definedAs;
        private static Link _meaningSame;
        private static Link _meaningOpposite;

        // Temporal/Dynamic
        private static Link _causes;
        private static Link _precedes;
        private static Link _follows;
        private static Link _changesTo;

        // Cognitive operations
        private static Link _analysis;
        private static Link _synthesis;
        private static Link _induction;
        private static Link _deduction;
        private static Link _prognosis;

        // Logical
        private static Link _and;
        private static Link _or;
        private static Link _not;
        private static Link _implies;

        // Quantification
        private static Link _all;
        private static Link _some;
        private static Link _none;

        // Type/Category
        private static Link _type;
        private static Link _instance;
        private static Link _property;
        private static Link _value;

        // Collection
        private static Link _set;
        private static Link _sequence;
        private static Link _element;
        private static Link _contains;

        // Abstract
        private static Link _concept;
        private static Link _concrete;
        private static Link _abstract;

        // Change/Dynamics
        private static Link _canChange;
        private static Link _dimension;
        private static Link _state;
        private static Link _transition;

        // Self-reference
        private static Link _itself;
        private static Link _meta;
        private static Link _object;

        /// <summary>
        /// Initialize all semantic links
        /// </summary>
        public static void Initialize()
        {
            // Core structural terms
            _source = Net.CreateMappedThing(SemanticMapping.Source).SetName("source");
            _linker = Net.CreateMappedThing(SemanticMapping.Linker).SetName("linker");
            _target = Net.CreateMappedThing(SemanticMapping.Target).SetName("target");
            _sourceLink = Link.Create(Net.Link, Net.ThatIs, _source).SetName("source link");
            _linkerLink = Link.Create(Net.Link, Net.ThatIs, _linker).SetName("linker link");
            _targetLink = Link.Create(Net.Link, Net.ThatIs, _target).SetName("target link");

            // Fundamental relations
            _is = Net.CreateMappedLink(SemanticMapping.Is).SetName("is");
            _has = Net.CreateMappedLink(SemanticMapping.Has).SetName("has");
            _partOf = Net.CreateMappedLink(SemanticMapping.PartOf).SetName("part of");
            _relatesTo = Net.CreateMappedLink(SemanticMapping.RelatesTo).SetName("relates to");

            // Meta-description
            _describes = Net.CreateMappedLink(SemanticMapping.Describes).SetName("describes");
            _definedAs = Net.CreateMappedLink(SemanticMapping.DefinedAs).SetName("defined as");
            _meaningSame = Net.CreateMappedLink(SemanticMapping.MeaningSame).SetName("meaning same");
            _meaningOpposite = Net.CreateMappedLink(SemanticMapping.MeaningOpposite).SetName("meaning opposite");

            // Temporal/Dynamic
            _causes = Net.CreateMappedLink(SemanticMapping.Causes).SetName("causes");
            _precedes = Net.CreateMappedLink(SemanticMapping.Precedes).SetName("precedes");
            _follows = Net.CreateMappedLink(SemanticMapping.Follows).SetName("follows");
            _changesTo = Net.CreateMappedLink(SemanticMapping.ChangesTo).SetName("changes to");

            // Cognitive operations
            _analysis = Net.CreateMappedThing(SemanticMapping.Analysis).SetName("analysis");
            _synthesis = Net.CreateMappedThing(SemanticMapping.Synthesis).SetName("synthesis");
            _induction = Net.CreateMappedThing(SemanticMapping.Induction).SetName("induction");
            _deduction = Net.CreateMappedThing(SemanticMapping.Deduction).SetName("deduction");
            _prognosis = Net.CreateMappedThing(SemanticMapping.Prognosis).SetName("prognosis");

            // Logical
            _and = Net.CreateMappedLink(SemanticMapping.And).SetName("and");
            _or = Net.CreateMappedLink(SemanticMapping.Or).SetName("or");
            _not = Net.CreateMappedLink(SemanticMapping.Not).SetName("not");
            _implies = Net.CreateMappedLink(SemanticMapping.Implies).SetName("implies");

            // Quantification
            _all = Net.CreateMappedThing(SemanticMapping.All).SetName("all");
            _some = Net.CreateMappedThing(SemanticMapping.Some).SetName("some");
            _none = Net.CreateMappedThing(SemanticMapping.None).SetName("none");

            // Type/Category
            _type = Net.CreateMappedThing(SemanticMapping.Type).SetName("type");
            _instance = Net.CreateMappedThing(SemanticMapping.Instance).SetName("instance");
            _property = Net.CreateMappedThing(SemanticMapping.Property).SetName("property");
            _value = Net.CreateMappedThing(SemanticMapping.Value).SetName("value");

            // Collection
            _set = Net.CreateMappedThing(SemanticMapping.Set).SetName("set");
            _sequence = Net.CreateMappedThing(SemanticMapping.Sequence).SetName("sequence");
            _element = Net.CreateMappedThing(SemanticMapping.Element).SetName("element");
            _contains = Net.CreateMappedLink(SemanticMapping.Contains).SetName("contains");

            // Abstract
            _concept = Net.CreateMappedThing(SemanticMapping.Concept).SetName("concept");
            _concrete = Net.CreateMappedThing(SemanticMapping.Concrete).SetName("concrete");
            _abstract = Net.CreateMappedThing(SemanticMapping.Abstract).SetName("abstract");

            // Change/Dynamics
            _canChange = Net.CreateMappedLink(SemanticMapping.CanChange).SetName("can change");
            _dimension = Net.CreateMappedThing(SemanticMapping.Dimension).SetName("dimension");
            _state = Net.CreateMappedThing(SemanticMapping.State).SetName("state");
            _transition = Net.CreateMappedThing(SemanticMapping.Transition).SetName("transition");

            // Self-reference
            _itself = Net.CreateMappedThing(SemanticMapping.Itself).SetName("itself");
            _meta = Net.CreateMappedThing(SemanticMapping.Meta).SetName("meta");
            _object = Net.CreateMappedThing(SemanticMapping.Object).SetName("object");
        }

        /// <summary>
        /// Creates a self-describing link definition
        /// A link describes itself through its structure
        /// </summary>
        public static Link CreateSelfDescribingLinkDefinition()
        {
            // Link is defined as having source, linker, and target
            var linkStructure = Link.Create(
                Link.Create(Net.Link, _has, _source),
                _and,
                Link.Create(
                    Link.Create(Net.Link, _has, _linker),
                    _and,
                    Link.Create(Net.Link, _has, _target)
                )
            );

            // Link describes itself
            return Link.Create(Net.Link, _describes, linkStructure);
        }

        /// <summary>
        /// Demonstrates analysis operation - decomposing a link into its parts
        /// </summary>
        public static Link[] AnalyzeLink(Link link)
        {
            var sourceAnalysis = Link.Create(
                Link.Create(_analysis, Net.Of, link),
                _describes,
                Link.Create(link, _has, link.Source)
            ).SetName("source analysis");

            var linkerAnalysis = Link.Create(
                Link.Create(_analysis, Net.Of, link),
                _describes,
                Link.Create(link, _has, link.Linker)
            ).SetName("linker analysis");

            var targetAnalysis = Link.Create(
                Link.Create(_analysis, Net.Of, link),
                _describes,
                Link.Create(link, _has, link.Target)
            ).SetName("target analysis");

            return new[] { sourceAnalysis, linkerAnalysis, targetAnalysis };
        }

        /// <summary>
        /// Demonstrates synthesis operation - combining parts into a whole
        /// </summary>
        public static Link SynthesizeLink(Link source, Link linker, Link target)
        {
            var synthesisOperation = Link.Create(_synthesis, Net.Of,
                Link.Create(
                    Link.Create(source, _is, _source),
                    _and,
                    Link.Create(
                        Link.Create(linker, _is, _linker),
                        _and,
                        Link.Create(target, _is, _target)
                    )
                )
            ).SetName("synthesis operation");

            var result = Link.Create(source, linker, target);

            Link.Create(synthesisOperation, _causes, result).SetName("synthesis result");

            return result;
        }

        /// <summary>
        /// Demonstrates induction - generalizing from specific instances to general rule
        /// </summary>
        public static Link InducePattern(Link[] instances)
        {
            if (instances == null || instances.Length == 0)
                return null;

            // Create set of instances
            var instancesSet = Net.CreateSet().SetName("instances set");
            foreach (var instance in instances)
            {
                Link.Create(instance, Net.ContainedBy, instancesSet);
            }

            // Induction operation: from instances to pattern
            var pattern = Net.CreateThing().SetName("induced pattern");
            var inductionOp = Link.Create(
                Link.Create(_induction, Net.Of, instancesSet),
                _causes,
                pattern
            ).SetName("induction operation");

            return pattern;
        }

        /// <summary>
        /// Demonstrates deduction - applying general rule to specific case
        /// </summary>
        public static Link DeduceInstance(Link pattern, Link context)
        {
            var instance = Net.CreateThing().SetName("deduced instance");
            var deductionOp = Link.Create(
                Link.Create(_deduction, Net.Of,
                    Link.Create(pattern, _and, context)
                ),
                _causes,
                instance
            ).SetName("deduction operation");

            return instance;
        }

        /// <summary>
        /// Demonstrates prognosis - predicting consequences
        /// </summary>
        public static Link PrognoseFuture(Link currentState, Link[] transformations)
        {
            var futureState = Net.CreateThing().SetName("future state");

            // Current state with transformations leads to future state
            var transformationsSet = Net.CreateSet().SetName("transformations");
            if (transformations != null)
            {
                foreach (var transformation in transformations)
                {
                    Link.Create(transformation, Net.ContainedBy, transformationsSet);
                }
            }

            var prognosisOp = Link.Create(
                Link.Create(_prognosis, Net.Of,
                    Link.Create(currentState, _and, transformationsSet)
                ),
                _causes,
                futureState
            ).SetName("prognosis operation");

            return futureState;
        }

        /// <summary>
        /// Creates a dynamic dimension - something that can change
        /// </summary>
        public static Link CreateDynamicDimension(Link entity, Link characteristic)
        {
            var dimension = Link.Create(
                Link.Create(entity, _has, characteristic),
                _and,
                Link.Create(characteristic, _canChange, _itself)
            ).SetName($"{characteristic} dimension");

            return dimension;
        }

        /// <summary>
        /// Creates a state transition
        /// </summary>
        public static Link CreateTransition(Link fromState, Link toState, Link cause)
        {
            var transition = Link.Create(
                Link.Create(fromState, _changesTo, toState),
                _and,
                Link.Create(cause, _causes, toState)
            ).SetName("state transition");

            return transition;
        }

        /// <summary>
        /// Run demonstration of semantic links capabilities
        /// </summary>
        public static void RunDemo()
        {
            Console.WriteLine("=== Semantic Links Demo ===\n");

            Initialize();
            Console.WriteLine("✓ Initialized semantic vocabulary");

            // 1. Self-describing link
            Console.WriteLine("\n1. Creating self-describing link definition...");
            var selfDesc = CreateSelfDescribingLinkDefinition();
            Console.WriteLine($"   Created: {selfDesc}");

            // 2. Analysis
            Console.WriteLine("\n2. Demonstrating analysis (decomposition)...");
            var testLink = Link.Create(Net.Thing, Net.IsA, Net.Thing);
            testLink.SetName("test link");
            var analysisResults = AnalyzeLink(testLink);
            Console.WriteLine($"   Analyzed link into {analysisResults.Length} parts");

            // 3. Synthesis
            Console.WriteLine("\n3. Demonstrating synthesis (combination)...");
            var synthesized = SynthesizeLink(Net.Link, _is, _concept);
            Console.WriteLine($"   Synthesized: {synthesized}");

            // 4. Induction
            Console.WriteLine("\n4. Demonstrating induction (bottom-up)...");
            var instances = new[] {
                Link.Create(Net.CreateThing(), _is, Net.Thing),
                Link.Create(Net.CreateThing(), _is, Net.Thing),
                Link.Create(Net.CreateThing(), _is, Net.Thing)
            };
            var pattern = InducePattern(instances);
            Console.WriteLine($"   Induced pattern from {instances.Length} instances");

            // 5. Deduction
            Console.WriteLine("\n5. Demonstrating deduction (top-down)...");
            var deduced = DeduceInstance(pattern, Net.Thing);
            Console.WriteLine($"   Deduced: {deduced}");

            // 6. Prognosis
            Console.WriteLine("\n6. Demonstrating prognosis (prediction)...");
            var currentState = Link.Create(Net.Thing, _has, Net.CreateThing().SetName("property A"));
            var transformations = new[] {
                Link.Create(_analysis, _causes, Net.CreateThing().SetName("change 1"))
            };
            var futureState = PrognoseFuture(currentState, transformations);
            Console.WriteLine($"   Predicted future state: {futureState}");

            // 7. Dynamic dimensions
            Console.WriteLine("\n7. Creating dynamic dimension (changeable characteristic)...");
            var entity = Net.CreateThing().SetName("entity");
            var characteristic = Net.CreateThing().SetName("color");
            var dimension = CreateDynamicDimension(entity, characteristic);
            Console.WriteLine($"   Created dimension: {dimension}");

            // 8. State transition
            Console.WriteLine("\n8. Creating state transition...");
            var state1 = Net.CreateThing().SetName("state 1");
            var state2 = Net.CreateThing().SetName("state 2");
            var cause = Net.CreateThing().SetName("cause");
            var transition = CreateTransition(state1, state2, cause);
            Console.WriteLine($"   Created transition: {transition}");

            Console.WriteLine("\n=== Demo Complete ===");
            Console.WriteLine("\nSemantic links enable:");
            Console.WriteLine("  • Self-description: Links can describe themselves");
            Console.WriteLine("  • Analysis: Decomposing complex structures");
            Console.WriteLine("  • Synthesis: Building new structures");
            Console.WriteLine("  • Induction: Generalizing from examples");
            Console.WriteLine("  • Deduction: Applying general rules");
            Console.WriteLine("  • Prognosis: Predicting future states");
            Console.WriteLine("  • Dynamics: Modeling change and transformations");
        }

        // Public accessors for semantic terms
        public static Link Source => _source;
        public static Link Linker => _linker;
        public static Link Target => _target;
        public static Link Is => _is;
        public static Link Has => _has;
        public static Link PartOf => _partOf;
        public static Link Describes => _describes;
        public static Link DefinedAs => _definedAs;
        public static Link Causes => _causes;
        public static Link Analysis => _analysis;
        public static Link Synthesis => _synthesis;
        public static Link Induction => _induction;
        public static Link Deduction => _deduction;
        public static Link Prognosis => _prognosis;
    }
}
