namespace Platform.System.Core
{
    /// <summary>
    /// Represents the network abstraction for low-level network access
    /// </summary>
    public interface INetwork
    {
        /// <summary>
        /// Gets a value indicating whether the network is available
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Sends data over the network
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <param name="destination">Destination address</param>
        void Send(byte[] data, string destination);

        /// <summary>
        /// Receives data from the network
        /// </summary>
        /// <param name="buffer">Buffer to store received data</param>
        /// <returns>Number of bytes received</returns>
        int Receive(byte[] buffer);
    }
}
