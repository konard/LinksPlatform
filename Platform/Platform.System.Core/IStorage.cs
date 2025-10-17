namespace Platform.System.Core
{
    /// <summary>
    /// Represents the storage abstraction for low-level drive access
    /// </summary>
    public interface IStorage
    {
        /// <summary>
        /// Gets the total storage capacity in bytes
        /// </summary>
        long TotalBytes { get; }

        /// <summary>
        /// Gets the available free storage in bytes
        /// </summary>
        long FreeBytes { get; }

        /// <summary>
        /// Reads data from storage
        /// </summary>
        /// <param name="offset">Offset position to read from</param>
        /// <param name="buffer">Buffer to store read data</param>
        /// <returns>Number of bytes read</returns>
        int Read(long offset, byte[] buffer);

        /// <summary>
        /// Writes data to storage
        /// </summary>
        /// <param name="offset">Offset position to write to</param>
        /// <param name="data">Data to write</param>
        /// <returns>Number of bytes written</returns>
        int Write(long offset, byte[] data);
    }
}
