using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Platform.Data.Doublets;
using Platform.Data;

namespace Platform.Examples
{
    /// <summary>
    /// Compares two links databases and detects differences (added, removed, and modified links).
    /// </summary>
    public class LinksDiff
    {
        /// <summary>
        /// Represents a single change in the links structure.
        /// </summary>
        public enum ChangeType
        {
            Added,
            Removed,
            Modified
        }

        /// <summary>
        /// Represents a detected change between two links databases.
        /// </summary>
        public class LinkChange
        {
            public ChangeType Type { get; set; }
            public ulong Index { get; set; }
            public ulong? OldSource { get; set; }
            public ulong? OldTarget { get; set; }
            public ulong? NewSource { get; set; }
            public ulong? NewTarget { get; set; }

            public override string ToString()
            {
                switch (Type)
                {
                    case ChangeType.Added:
                        return $"+ Link {Index}: {NewSource} -> {NewTarget}";
                    case ChangeType.Removed:
                        return $"- Link {Index}: {OldSource} -> {OldTarget}";
                    case ChangeType.Modified:
                        return $"~ Link {Index}: ({OldSource} -> {OldTarget}) => ({NewSource} -> {NewTarget})";
                    default:
                        return $"? Link {Index}";
                }
            }
        }

        /// <summary>
        /// Compares two links databases and returns a list of changes.
        /// </summary>
        /// <param name="oldLinks">The old/previous state of links.</param>
        /// <param name="newLinks">The new/current state of links.</param>
        /// <param name="cancellationToken">Cancellation token to stop the operation.</param>
        /// <returns>A list of detected changes.</returns>
        public List<LinkChange> Compare(SynchronizedLinks<ulong> oldLinks, SynchronizedLinks<ulong> newLinks, CancellationToken cancellationToken)
        {
            var changes = new List<LinkChange>();
            var oldLinksMap = new Dictionary<ulong, (ulong source, ulong target)>();
            var newLinksMap = new Dictionary<ulong, (ulong source, ulong target)>();

            // Build map of old links
            oldLinks.Each(link =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return oldLinks.Constants.Break;
                }
                var index = link[oldLinks.Constants.IndexPart];
                var source = link[oldLinks.Constants.SourcePart];
                var target = link[oldLinks.Constants.TargetPart];
                oldLinksMap[index] = (source, target);
                return oldLinks.Constants.Continue;
            });

            // Build map of new links
            newLinks.Each(link =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return newLinks.Constants.Break;
                }
                var index = link[newLinks.Constants.IndexPart];
                var source = link[newLinks.Constants.SourcePart];
                var target = link[newLinks.Constants.TargetPart];
                newLinksMap[index] = (source, target);
                return newLinks.Constants.Continue;
            });

            if (cancellationToken.IsCancellationRequested)
            {
                return changes;
            }

            // Find removed and modified links
            foreach (var kvp in oldLinksMap)
            {
                var index = kvp.Key;
                var oldValue = kvp.Value;

                if (!newLinksMap.TryGetValue(index, out var newValue))
                {
                    // Link was removed
                    changes.Add(new LinkChange
                    {
                        Type = ChangeType.Removed,
                        Index = index,
                        OldSource = oldValue.source,
                        OldTarget = oldValue.target
                    });
                }
                else if (oldValue.source != newValue.source || oldValue.target != newValue.target)
                {
                    // Link was modified
                    changes.Add(new LinkChange
                    {
                        Type = ChangeType.Modified,
                        Index = index,
                        OldSource = oldValue.source,
                        OldTarget = oldValue.target,
                        NewSource = newValue.source,
                        NewTarget = newValue.target
                    });
                }
            }

            // Find added links
            foreach (var kvp in newLinksMap)
            {
                var index = kvp.Key;
                var newValue = kvp.Value;

                if (!oldLinksMap.ContainsKey(index))
                {
                    // Link was added
                    changes.Add(new LinkChange
                    {
                        Type = ChangeType.Added,
                        Index = index,
                        NewSource = newValue.source,
                        NewTarget = newValue.target
                    });
                }
            }

            return changes;
        }

        /// <summary>
        /// Compares two links databases and writes the differences to a file.
        /// </summary>
        /// <param name="oldLinks">The old/previous state of links.</param>
        /// <param name="newLinks">The new/current state of links.</param>
        /// <param name="outputPath">Path to the output file where diff will be written.</param>
        /// <param name="cancellationToken">Cancellation token to stop the operation.</param>
        public void CompareAndExport(SynchronizedLinks<ulong> oldLinks, SynchronizedLinks<ulong> newLinks, string outputPath, CancellationToken cancellationToken)
        {
            var changes = Compare(oldLinks, newLinks, cancellationToken);

            using (var file = File.OpenWrite(outputPath))
            using (var writer = new StreamWriter(file, Encoding.UTF8))
            {
                writer.WriteLine($"Links Diff Report");
                writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"Total changes: {changes.Count}");
                writer.WriteLine();

                var addedCount = 0;
                var removedCount = 0;
                var modifiedCount = 0;

                foreach (var change in changes)
                {
                    switch (change.Type)
                    {
                        case ChangeType.Added:
                            addedCount++;
                            break;
                        case ChangeType.Removed:
                            removedCount++;
                            break;
                        case ChangeType.Modified:
                            modifiedCount++;
                            break;
                    }
                }

                writer.WriteLine($"Summary:");
                writer.WriteLine($"  Added: {addedCount}");
                writer.WriteLine($"  Removed: {removedCount}");
                writer.WriteLine($"  Modified: {modifiedCount}");
                writer.WriteLine();
                writer.WriteLine("Changes:");
                writer.WriteLine("--------");

                foreach (var change in changes)
                {
                    writer.WriteLine(change.ToString());
                }
            }
        }
    }
}
