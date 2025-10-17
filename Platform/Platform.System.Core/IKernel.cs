namespace Platform.System.Core
{
    /// <summary>
    /// Represents the minimal kernel abstraction providing bare essentials for a functional operating system
    /// Inspired by BareMetal OS design principles
    /// </summary>
    public interface IKernel
    {
        /// <summary>
        /// Gets the processor abstraction
        /// </summary>
        IProcessor Processor { get; }

        /// <summary>
        /// Gets the memory abstraction
        /// </summary>
        IMemory Memory { get; }

        /// <summary>
        /// Gets the network abstraction
        /// </summary>
        INetwork Network { get; }

        /// <summary>
        /// Gets the storage abstraction
        /// </summary>
        IStorage Storage { get; }

        /// <summary>
        /// Initializes the kernel and all subsystems
        /// </summary>
        void Initialize();

        /// <summary>
        /// Shuts down the kernel and all subsystems
        /// </summary>
        void Shutdown();
    }
}
