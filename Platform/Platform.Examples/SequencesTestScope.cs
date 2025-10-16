using System;
using System.IO;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Data.Doublets.Sequences;
using Platform.Disposables;

namespace Platform.Examples
{
    /// <summary>
    /// Provides a test scope for working with sequences in a temporary storage environment.
    /// This class creates temporary files for links storage and provides configured instances
    /// of links and sequences for testing purposes.
    /// </summary>
    public class SequencesTestScope : DisposableBase
    {
        /// <summary>
        /// Gets the core memory adapter for links storage.
        /// </summary>
        public ILinks<ulong> MemoryAdapter { get; }

        /// <summary>
        /// Gets the synchronized links instance.
        /// </summary>
        public SynchronizedLinks<ulong> Links { get; }

        /// <summary>
        /// Gets the sequences instance if sequences are enabled.
        /// </summary>
        public Sequences Sequences { get; }

        /// <summary>
        /// Gets the path to the temporary storage file.
        /// </summary>
        public string TempFilename { get; }

        /// <summary>
        /// Gets the path to the temporary transaction log file.
        /// </summary>
        public string TempTransactionLogFilename { get; }

        private readonly bool _deleteFiles;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencesTestScope"/> class with default options.
        /// </summary>
        /// <param name="deleteFiles">If true, temporary files will be deleted when the scope is disposed.</param>
        /// <param name="useSequences">If true, a Sequences instance will be created and configured.</param>
        /// <param name="useLog">If true, transaction logging will be enabled.</param>
        public SequencesTestScope(bool deleteFiles = true, bool useSequences = false, bool useLog = false)
            : this(new SequencesOptions<ulong>(), deleteFiles, useSequences, useLog)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencesTestScope"/> class with custom sequences options.
        /// </summary>
        /// <param name="sequencesOptions">The options to configure the sequences instance.</param>
        /// <param name="deleteFiles">If true, temporary files will be deleted when the scope is disposed.</param>
        /// <param name="useSequences">If true, a Sequences instance will be created and configured.</param>
        /// <param name="useLog">If true, transaction logging will be enabled.</param>
        public SequencesTestScope(SequencesOptions<ulong> sequencesOptions, bool deleteFiles = true, bool useSequences = false, bool useLog = false)
        {
            _deleteFiles = deleteFiles;
            TempFilename = Path.GetTempFileName();
            TempTransactionLogFilename = Path.GetTempFileName();

            var coreMemoryAdapter = new UInt64UnitedMemoryLinks(TempFilename);

            MemoryAdapter = useLog
                ? (ILinks<ulong>)new UInt64LinksTransactionsLayer(coreMemoryAdapter, TempTransactionLogFilename)
                : coreMemoryAdapter;

            Links = new SynchronizedLinks<ulong>(new UInt64Links(MemoryAdapter));

            if (useSequences)
            {
                Sequences = new Sequences(Links, sequencesOptions);
            }
        }

        /// <summary>
        /// Disposes the resources used by this test scope.
        /// </summary>
        /// <param name="manual">True if called manually, false if called by the finalizer.</param>
        /// <param name="wasDisposed">True if the object was already disposed.</param>
        protected override void Dispose(bool manual, bool wasDisposed)
        {
            if (!wasDisposed)
            {
                Links?.Unsync?.DisposeIfPossible();
                if (_deleteFiles)
                {
                    DeleteFiles();
                }
            }
        }

        /// <summary>
        /// Deletes the temporary files created by this test scope.
        /// </summary>
        public void DeleteFiles()
        {
            try
            {
                if (File.Exists(TempFilename))
                {
                    File.Delete(TempFilename);
                }
                if (File.Exists(TempTransactionLogFilename))
                {
                    File.Delete(TempTransactionLogFilename);
                }
            }
            catch (Exception)
            {
                // Ignore file deletion errors during cleanup
            }
        }
    }
}
