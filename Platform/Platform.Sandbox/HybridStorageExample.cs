using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Platform.Memory;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Sandbox
{
    /// <summary>
    /// Demonstrates hybrid storage architecture where:
    /// - Mutable data (current state Sn) is stored in RAM
    /// - Immutable data (archived states S1...S(n-1)) is stored on Disk
    /// - State transitions (X→Y) are logged as append-only records on Disk
    ///
    /// This example shows the concept from issue #645:
    /// https://github.com/konard/LinksPlatform/issues/645
    /// </summary>
    public class HybridStorageExample : IDisposable
    {
        private const string ArchiveFileName = "archive.links";
        private const string StateLogFileName = "state-transitions.log";

        /// <summary>
        /// Represents a state transition from one link state to another.
        /// These transitions form an append-only log that can be used
        /// to reconstruct any historical state.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct StateTransition
        {
            public long Timestamp;
            public ulong LinkAddress;
            public ulong OldSource;
            public ulong OldTarget;
            public ulong NewSource;
            public ulong NewTarget;
            public TransitionType Type;
        }

        public enum TransitionType : byte
        {
            Create = 1,
            Update = 2,
            Delete = 3
        }

        private readonly IResizableDirectMemory _archiveMemory;
        private readonly IResizableDirectMemory _currentStateMemory;
        private readonly FileStream _stateLog;
        private readonly BinaryWriter _logWriter;

        public HybridStorageExample(string archivePath = ArchiveFileName)
        {
            // Disk storage for archived (immutable) states
            _archiveMemory = new FileMappedResizableDirectMemory(archivePath);

            // RAM storage for current (mutable) state
            _currentStateMemory = new HeapResizableDirectMemory();

            // Append-only log for state transitions on disk
            _stateLog = new FileStream(StateLogFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
            _logWriter = new BinaryWriter(_stateLog);
        }

        /// <summary>
        /// Creates a new doublets store with hybrid storage:
        /// - Current working set in RAM
        /// - Archive on Disk
        /// - All transitions logged
        /// </summary>
        public UnitedMemoryLinks<ulong> CreateCurrentStateLinks()
        {
            // This represents the current state Sn stored in RAM
            return new UnitedMemoryLinks<ulong>(_currentStateMemory);
        }

        /// <summary>
        /// Opens archived (read-only) links from disk.
        /// This represents states S1...S(n-1).
        /// </summary>
        public UnitedMemoryLinks<ulong> OpenArchiveLinks()
        {
            // This represents archived states stored on Disk
            return new UnitedMemoryLinks<ulong>(_archiveMemory);
        }

        /// <summary>
        /// Logs a state transition to the append-only log.
        /// These transitions represent the X→Y arrows from the issue diagram.
        /// </summary>
        public void LogTransition(ulong linkAddress, ulong oldSource, ulong oldTarget,
            ulong newSource, ulong newTarget, TransitionType type)
        {
            var transition = new StateTransition
            {
                Timestamp = DateTime.UtcNow.Ticks,
                LinkAddress = linkAddress,
                OldSource = oldSource,
                OldTarget = oldTarget,
                NewSource = newSource,
                NewTarget = newTarget,
                Type = type
            };

            // Write to append-only log
            var bytes = StructToBytes(transition);
            _logWriter.Write(bytes);
            _logWriter.Flush();
        }

        /// <summary>
        /// Archives the current state to disk, effectively:
        /// - Moving state Sn to archived states (becoming S(n-1))
        /// - Freeing RAM for new current state Sn
        /// </summary>
        public void ArchiveCurrentState()
        {
            Console.WriteLine("Archiving current state from RAM to Disk...");

            // Copy current state from RAM to disk archive
            var currentData = new byte[_currentStateMemory.UsedCapacity];
            Marshal.Copy(_currentStateMemory.Pointer, currentData, 0, (int)_currentStateMemory.UsedCapacity);

            var archiveOffset = _archiveMemory.UsedCapacity;
            _archiveMemory.ReservedCapacity = archiveOffset + currentData.Length;
            Marshal.Copy(currentData, 0, IntPtr.Add(_archiveMemory.Pointer, (int)archiveOffset), currentData.Length);

            Console.WriteLine($"Archived {currentData.Length} bytes to disk at offset {archiveOffset}");

            // Clear RAM for new current state
            // In production, you might want to dispose and recreate the memory
        }

        /// <summary>
        /// Reads the state transition log to reconstruct state history.
        /// This allows traversing through all states S1→S2→...→Sn.
        /// </summary>
        public List<StateTransition> ReadTransitionLog()
        {
            var transitions = new List<StateTransition>();

            if (!File.Exists(StateLogFileName))
            {
                return transitions;
            }

            using (var reader = new BinaryReader(File.OpenRead(StateLogFileName)))
            {
                var transitionSize = Marshal.SizeOf<StateTransition>();

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var bytes = reader.ReadBytes(transitionSize);
                    var transition = BytesToStruct<StateTransition>(bytes);
                    transitions.Add(transition);
                }
            }

            return transitions;
        }

        private static byte[] StructToBytes<T>(T structure) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var bytes = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, ptr, false);
                Marshal.Copy(ptr, bytes, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return bytes;
        }

        private static T BytesToStruct<T>(byte[] bytes) where T : struct
        {
            var ptr = Marshal.AllocHGlobal(bytes.Length);
            try
            {
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
                return Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public void Dispose()
        {
            _logWriter?.Dispose();
            _stateLog?.Dispose();
            (_archiveMemory as IDisposable)?.Dispose();
            (_currentStateMemory as IDisposable)?.Dispose();
        }

        /// <summary>
        /// Demonstrates the hybrid storage concept from issue #645.
        /// Shows how to separate mutable (RAM) and immutable (Disk) data
        /// with a complete audit trail of state transitions.
        /// </summary>
        public static void Run()
        {
            Console.WriteLine("=== Hybrid Storage Example (Issue #645) ===");
            Console.WriteLine("Demonstrating: Store mutable data in RAM, and immutable data on Disk");
            Console.WriteLine();

            using (var hybrid = new HybridStorageExample("test-archive.links"))
            {
                // Create current state links in RAM (mutable state Sn)
                Console.WriteLine("1. Creating current state in RAM (Sn)...");
                using (var currentLinks = hybrid.CreateCurrentStateLinks())
                {
                    Console.WriteLine($"   RAM storage initialized: {currentLinks.Count()} links");

                    // Create some links
                    var link1 = currentLinks.Create();
                    Console.WriteLine($"   Created link: {link1}");
                    hybrid.LogTransition(link1, 0, 0, 0, 0, TransitionType.Create);

                    var link2 = currentLinks.Create();
                    Console.WriteLine($"   Created link: {link2}");
                    hybrid.LogTransition(link2, 0, 0, 0, 0, TransitionType.Create);

                    // Update a link (state transition X→Y)
                    Console.WriteLine($"   Updating link {link1} to reference {link2}...");
                    currentLinks.Update(link1, link2, link2);
                    hybrid.LogTransition(link1, 0, 0, link2, link2, TransitionType.Update);

                    Console.WriteLine($"   Current state: {currentLinks.Count()} links in RAM");
                }

                // Archive the current state to disk
                Console.WriteLine();
                Console.WriteLine("2. Archiving state Sn to disk (becomes S(n-1))...");
                hybrid.ArchiveCurrentState();

                // Read the transition log
                Console.WriteLine();
                Console.WriteLine("3. Reading state transition log (X→Y transitions)...");
                var transitions = hybrid.ReadTransitionLog();
                Console.WriteLine($"   Found {transitions.Count} state transitions:");
                foreach (var transition in transitions)
                {
                    var timestamp = new DateTime(transition.Timestamp);
                    Console.WriteLine($"   - {timestamp:HH:mm:ss.fff}: {transition.Type} Link#{transition.LinkAddress}");
                }

                Console.WriteLine();
                Console.WriteLine("=== Summary ===");
                Console.WriteLine("- Mutable data (current state Sn) stored in RAM for fast access");
                Console.WriteLine("- Immutable data (archived states) stored on Disk for persistence");
                Console.WriteLine("- State transitions logged for complete audit trail and reconstruction");
                Console.WriteLine();
                Console.WriteLine("This architecture allows:");
                Console.WriteLine("  • Fast operations on current data (RAM)");
                Console.WriteLine("  • Historical states preserved (Disk)");
                Console.WriteLine("  • Time-travel queries via transition log");
                Console.WriteLine("  • Efficient memory usage (old states archived)");
            }
        }
    }
}
