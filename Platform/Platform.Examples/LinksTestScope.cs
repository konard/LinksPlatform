using System.IO;
using Platform.Disposables;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.United.Specific;
using Platform.Memory;

namespace Platform.Examples
{
    /// <summary>
    /// Provides a test scope for Links that automatically manages temporary file creation and cleanup.
    /// This helper class simplifies test setup by handling file lifecycle and Links initialization.
    /// </summary>
    public class LinksTestScope : DisposableBase
    {
        /// <summary>
        /// Gets the underlying memory adapter for the Links store.
        /// </summary>
        public UInt64UnitedMemoryLinks MemoryAdapter { get; }

        /// <summary>
        /// Gets the synchronized Links instance for thread-safe operations.
        /// </summary>
        public SynchronizedLinks<ulong> Links { get; }

        /// <summary>
        /// Gets the path to the temporary database file.
        /// </summary>
        public string TempFilename { get; }

        private readonly bool _deleteFiles;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinksTestScope"/> class.
        /// </summary>
        /// <param name="deleteFiles">If true, temporary files will be deleted on disposal. Default is true.</param>
        /// <param name="fileSizeStep">The initial file size in bytes. Default is 512 MB.</param>
        public LinksTestScope(bool deleteFiles = true, long fileSizeStep = 512 * 1024 * 1024)
        {
            _deleteFiles = deleteFiles;
            TempFilename = Path.GetTempFileName();
            MemoryAdapter = new UInt64UnitedMemoryLinks(TempFilename, fileSizeStep);
            var coreLinks = new UInt64Links(MemoryAdapter);
            Links = new SynchronizedLinks<ulong>(coreLinks);
        }

        /// <summary>
        /// Disposes the Links resources and optionally deletes temporary files.
        /// </summary>
        protected override void Dispose(bool manual, bool wasDisposed)
        {
            if (!wasDisposed)
            {
                Links?.Unsync.DisposeIfPossible();
                MemoryAdapter?.DisposeIfPossible();
                if (_deleteFiles)
                {
                    DeleteFiles();
                }
            }
        }

        /// <summary>
        /// Deletes the temporary database file.
        /// </summary>
        public void DeleteFiles()
        {
            if (File.Exists(TempFilename))
            {
                File.Delete(TempFilename);
            }
        }
    }
}
