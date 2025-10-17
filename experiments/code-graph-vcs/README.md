# Code Graph VCS - Function/Class-Level Version Control

## Overview

This is a proof-of-concept implementation of a version control system that operates at the **function and class level** instead of the traditional file level. It uses the Links Platform concept to represent code as a graph of interconnected entities.

## Problem with Traditional VCS

Traditional version control systems like Git operate on files:
- **Track changes at file level** - even if you change one function, the whole file is marked as changed
- **Difficult to see precise changes** - need to diff files to understand what actually changed
- **Dependencies are implicit** - can't easily see which functions depend on which
- **Code graph is hidden** - relationships between classes/functions exist in code but not in VCS

## Solution: Code Graph VCS

Code Graph VCS treats each code entity (class, method, function) as a separate versioned entity (Link):

### Key Features

1. **Entity-Level Versioning**
   - Each class, method, property, etc. is a separate entity with its own version history
   - Changing one method creates a new version of just that method, not the entire file

2. **Explicit Relationships**
   - Track relationships: Contains, DependsOn, Calls, Inherits, Implements, References
   - See exact dependencies between code entities
   - Understand impact of changes automatically

3. **Code Graph Visualization**
   - Complete graph of all code entities and their relationships
   - Navigate code by relationships, not file structure
   - See the "shape" of your codebase

4. **Better Change Tracking**
   - Know exactly which function/class changed
   - See version history at method level
   - Track who changed what at fine granularity

5. **Dependency Analysis**
   - Automatically compute dependency graphs
   - See what code depends on a changed function
   - Understand change impact before making it

## Architecture

### Core Components

1. **CodeEntity** - Represents any code element (class, method, property, etc.)
   - Unique ID (Link identifier)
   - Type (namespace, class, interface, method, etc.)
   - Content (actual code)
   - Metadata (author, timestamp, etc.)

2. **CodeRelation** - Represents relationships between entities
   - Source and Target entity IDs
   - Relation type (contains, calls, depends on, etc.)
   - Metadata for the relationship

3. **CodeSnapshot** - Equivalent to a "commit"
   - Set of entity IDs and relation IDs
   - Message, author, timestamp
   - Links to parent snapshot (for history)

4. **CodeGraphVCS** - Main system managing entities, relations, and snapshots
   - Create and version entities
   - Track relationships
   - Query dependencies
   - Create snapshots

## Benefits Over Git

| Traditional Git | Code Graph VCS |
|----------------|----------------|
| Tracks files | Tracks functions/classes |
| Diff shows file changes | Diff shows exact method changes |
| Dependencies implicit in code | Dependencies explicit in graph |
| History per file | History per function/class |
| Must parse code to understand structure | Structure is the data model |
| Change impact requires analysis | Change impact is in the graph |

## Use Cases

1. **AI/GPT-4 Code Generation**
   - Easier for AI to understand and modify specific functions
   - Can reason about code at method level
   - Dependencies are explicit for AI to see

2. **Microservices & Function-Level Deployment**
   - Version and deploy individual functions
   - Track function-level changes across services

3. **Code Review**
   - Review changes at method/class level
   - See exactly what changed and its dependencies
   - Better understanding of change impact

4. **Dependency Management**
   - Know what depends on what you're changing
   - Detect circular dependencies
   - Refactoring with confidence

5. **Code Analytics**
   - Which functions change most often?
   - Which functions have most dependencies?
   - Identify code hotspots

## Example Usage

```csharp
var vcs = new CodeGraphVCS();

// Create a class
var calcClass = vcs.CreateEntity("Calculator", CodeEntityType.Class,
    "public class Calculator { }");

// Create a method
var addMethod = vcs.CreateEntity("Add", CodeEntityType.Method,
    "public int Add(int a, int b) { return a + b; }");

// Link method to class
vcs.CreateRelation(calcClass.Id, addMethod.Id, RelationType.Contains);

// Modify just the method (not the whole file!)
var addMethodV2 = vcs.CreateNewVersion(addMethod.Id,
    "public int Add(int a, int b) { \n    // Added validation\n    return a + b; \n}");

// See version history of just this method
var history = vcs.GetVersionHistory(addMethodV2.Id);

// Get dependency graph
var deps = vcs.GetDependencyGraph(addMethod.Id);
```

## How It Works

### 1. Code Representation
Each code element becomes a Link/Entity with:
- Unique identifier
- Type (class, method, etc.)
- Content (actual code)
- Metadata

### 2. Relationship Tracking
Relationships between entities are explicit Links:
- Class **Contains** Method
- Method **Calls** Method
- Class **Inherits** Class
- Method **DependsOn** Class

### 3. Versioning
Instead of versioning files:
- Version individual entities
- New version = new entity + Version link to old entity
- History is a chain of Version links

### 4. Snapshots
Like Git commits, but:
- Snapshot references entity IDs, not files
- Can include subset of entities
- Tracks relations between included entities

## Running the Example

```bash
cd experiments/code-graph-vcs
dotnet run
```

This will demonstrate:
- Creating code entities (classes, methods)
- Establishing relationships
- Versioning individual methods
- Tracking dependencies
- Creating snapshots

## Future Enhancements

1. **Integration with AST Parsers**
   - Automatically parse source files into code graph
   - Support multiple languages (C#, Java, Python, etc.)

2. **Diff and Merge**
   - Smart merge at method level
   - Conflict resolution for entity changes
   - 3-way merge for code graphs

3. **Storage Backend**
   - Integrate with actual Links Platform storage
   - Persistent graph database
   - Efficient querying

4. **IDE Integration**
   - Visual Studio / VS Code extensions
   - Navigate code by dependencies
   - Visualize code graph

5. **Advanced Queries**
   - Find all methods calling X
   - Find all classes implementing interface Y
   - Find circular dependencies
   - Code complexity metrics

6. **Branching and Merging**
   - Graph-based branching
   - Merge strategies for code graphs
   - Conflict visualization

## Links Platform Integration

This implementation uses Links Platform concepts:

- **Links as Code Entities** - Each function/class is a Link
- **Associative Memory** - Relationships are first-class
- **Graph Structure** - Code is naturally a graph
- **Versioning via Links** - Version links connect versions
- **Sequences** - Can represent code sequences using Link sequences

## References

- Issue: [#669 - Better than git: source control not based on files but classes/functions](https://github.com/konard/LinksPlatform/issues/669)
- Research: [Historage - Fine-grained version control system for Java](https://www.researchgate.net/publication/220875471_Historage_Fine-grained_version_control_system_for_Java)
- Research: [A fine-grained and flexible version control for software artifacts](https://www.researchgate.net/publication/220961654_A_fine-grained_and_flexible_version_control_for_software_artifacts)
- Concept: [Semantic Code Graph](https://arxiv.org/html/2310.02128v2)

## License

Same as LinksPlatform main repository.
