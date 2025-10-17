using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Experiments.CodeGraphVCS
{
    /// <summary>
    /// Represents a code entity type in the version control system
    /// </summary>
    public enum CodeEntityType
    {
        Namespace,
        Class,
        Interface,
        Struct,
        Method,
        Property,
        Field,
        Parameter,
        LocalVariable
    }

    /// <summary>
    /// Represents a relationship type between code entities
    /// </summary>
    public enum RelationType
    {
        Contains,        // Parent contains child (e.g., Class contains Method)
        DependsOn,       // Entity depends on another (e.g., Method depends on Class)
        Calls,           // Method calls another method
        Inherits,        // Class inherits from another
        Implements,      // Class implements interface
        References,      // General reference relationship
        Overrides,       // Method overrides another
        Version          // Links to a previous version
    }

    /// <summary>
    /// Represents a code entity as a Link with unique identifier
    /// </summary>
    public class CodeEntity
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public CodeEntityType Type { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string Author { get; set; }
        public Dictionary<string, string> Metadata { get; set; }

        public CodeEntity()
        {
            Metadata = new Dictionary<string, string>();
            Timestamp = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"[{Id}] {Type}: {Name}";
        }
    }

    /// <summary>
    /// Represents a relationship (Link) between two code entities
    /// </summary>
    public class CodeRelation
    {
        public ulong Id { get; set; }
        public ulong SourceId { get; set; }
        public ulong TargetId { get; set; }
        public RelationType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string> Metadata { get; set; }

        public CodeRelation()
        {
            Metadata = new Dictionary<string, string>();
            Timestamp = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"[{Id}] {SourceId} --{Type}--> {TargetId}";
        }
    }

    /// <summary>
    /// Represents a snapshot/commit in the version control system
    /// </summary>
    public class CodeSnapshot
    {
        public ulong Id { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string Author { get; set; }
        public HashSet<ulong> EntityIds { get; set; }
        public HashSet<ulong> RelationIds { get; set; }
        public ulong? ParentSnapshotId { get; set; }

        public CodeSnapshot()
        {
            EntityIds = new HashSet<ulong>();
            RelationIds = new HashSet<ulong>();
            Timestamp = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"Snapshot [{Id}]: {Message} by {Author} at {Timestamp}";
        }
    }

    /// <summary>
    /// Main Code Graph Version Control System using Links Platform concepts
    /// </summary>
    public class CodeGraphVCS
    {
        private ulong _nextEntityId = 1;
        private ulong _nextRelationId = 1;
        private ulong _nextSnapshotId = 1;

        private Dictionary<ulong, CodeEntity> _entities;
        private Dictionary<ulong, CodeRelation> _relations;
        private Dictionary<ulong, CodeSnapshot> _snapshots;
        private Dictionary<string, HashSet<ulong>> _nameIndex;

        public CodeGraphVCS()
        {
            _entities = new Dictionary<ulong, CodeEntity>();
            _relations = new Dictionary<ulong, CodeRelation>();
            _snapshots = new Dictionary<ulong, CodeSnapshot>();
            _nameIndex = new Dictionary<string, HashSet<ulong>>();
        }

        /// <summary>
        /// Creates a new code entity
        /// </summary>
        public CodeEntity CreateEntity(string name, CodeEntityType type, string content = "", string author = "")
        {
            var entity = new CodeEntity
            {
                Id = _nextEntityId++,
                Name = name,
                Type = type,
                Content = content,
                Author = author
            };

            _entities[entity.Id] = entity;

            // Index by name for quick lookup
            if (!_nameIndex.ContainsKey(name))
            {
                _nameIndex[name] = new HashSet<ulong>();
            }
            _nameIndex[name].Add(entity.Id);

            return entity;
        }

        /// <summary>
        /// Creates a relationship between two entities
        /// </summary>
        public CodeRelation CreateRelation(ulong sourceId, ulong targetId, RelationType type)
        {
            if (!_entities.ContainsKey(sourceId) || !_entities.ContainsKey(targetId))
            {
                throw new ArgumentException("Source or target entity does not exist");
            }

            var relation = new CodeRelation
            {
                Id = _nextRelationId++,
                SourceId = sourceId,
                TargetId = targetId,
                Type = type
            };

            _relations[relation.Id] = relation;
            return relation;
        }

        /// <summary>
        /// Creates a new version of an entity (tracking changes at function/class level)
        /// </summary>
        public CodeEntity CreateNewVersion(ulong entityId, string newContent, string author = "")
        {
            if (!_entities.ContainsKey(entityId))
            {
                throw new ArgumentException("Entity does not exist");
            }

            var oldEntity = _entities[entityId];
            var newEntity = CreateEntity(oldEntity.Name, oldEntity.Type, newContent, author);

            // Create a version link to previous version
            CreateRelation(newEntity.Id, oldEntity.Id, RelationType.Version);

            return newEntity;
        }

        /// <summary>
        /// Creates a snapshot (similar to git commit) of current state
        /// </summary>
        public CodeSnapshot CreateSnapshot(string message, string author, HashSet<ulong> entityIds)
        {
            var snapshot = new CodeSnapshot
            {
                Id = _nextSnapshotId++,
                Message = message,
                Author = author,
                EntityIds = new HashSet<ulong>(entityIds)
            };

            // Find all relations between included entities
            var relevantRelations = _relations.Values
                .Where(r => entityIds.Contains(r.SourceId) && entityIds.Contains(r.TargetId))
                .Select(r => r.Id);

            foreach (var relId in relevantRelations)
            {
                snapshot.RelationIds.Add(relId);
            }

            _snapshots[snapshot.Id] = snapshot;
            return snapshot;
        }

        /// <summary>
        /// Gets the dependency graph starting from an entity
        /// </summary>
        public HashSet<ulong> GetDependencyGraph(ulong entityId, int maxDepth = -1)
        {
            var result = new HashSet<ulong> { entityId };
            var queue = new Queue<(ulong id, int depth)>();
            queue.Enqueue((entityId, 0));

            while (queue.Count > 0)
            {
                var (currentId, depth) = queue.Dequeue();

                if (maxDepth >= 0 && depth >= maxDepth)
                    continue;

                var dependencies = _relations.Values
                    .Where(r => r.SourceId == currentId &&
                               (r.Type == RelationType.DependsOn ||
                                r.Type == RelationType.Calls ||
                                r.Type == RelationType.References))
                    .Select(r => r.TargetId);

                foreach (var depId in dependencies)
                {
                    if (result.Add(depId))
                    {
                        queue.Enqueue((depId, depth + 1));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets version history of an entity
        /// </summary>
        public List<CodeEntity> GetVersionHistory(ulong entityId)
        {
            var history = new List<CodeEntity>();
            var currentId = entityId;

            while (_entities.ContainsKey(currentId))
            {
                history.Add(_entities[currentId]);

                var versionLink = _relations.Values
                    .FirstOrDefault(r => r.SourceId == currentId && r.Type == RelationType.Version);

                if (versionLink == null)
                    break;

                currentId = versionLink.TargetId;
            }

            return history;
        }

        /// <summary>
        /// Finds entities by name
        /// </summary>
        public IEnumerable<CodeEntity> FindByName(string name)
        {
            if (_nameIndex.ContainsKey(name))
            {
                return _nameIndex[name].Select(id => _entities[id]);
            }
            return Enumerable.Empty<CodeEntity>();
        }

        /// <summary>
        /// Gets all entities of a specific type
        /// </summary>
        public IEnumerable<CodeEntity> GetEntitiesByType(CodeEntityType type)
        {
            return _entities.Values.Where(e => e.Type == type);
        }

        /// <summary>
        /// Gets all relations of a specific type
        /// </summary>
        public IEnumerable<CodeRelation> GetRelationsByType(RelationType type)
        {
            return _relations.Values.Where(r => r.Type == type);
        }

        /// <summary>
        /// Gets the complete code graph as a visual representation
        /// </summary>
        public string VisualizeGraph()
        {
            var result = new System.Text.StringBuilder();
            result.AppendLine("=== Code Graph ===");
            result.AppendLine();

            result.AppendLine("Entities:");
            foreach (var entity in _entities.Values.OrderBy(e => e.Id))
            {
                result.AppendLine($"  {entity}");
            }

            result.AppendLine();
            result.AppendLine("Relations:");
            foreach (var relation in _relations.Values.OrderBy(r => r.Id))
            {
                result.AppendLine($"  {relation}");
            }

            return result.ToString();
        }

        public Dictionary<ulong, CodeEntity> Entities => _entities;
        public Dictionary<ulong, CodeRelation> Relations => _relations;
        public Dictionary<ulong, CodeSnapshot> Snapshots => _snapshots;
    }
}
