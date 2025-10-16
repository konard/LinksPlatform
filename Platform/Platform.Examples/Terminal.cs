using System;

namespace Platform.Examples
{
    /// <summary>
    /// Abstract base class for terminal implementations (console, web, etc.).
    /// Provides common functionality for terminal operations and lifecycle management.
    /// </summary>
    public abstract class Terminal
    {
        /// <summary>
        /// Gets or sets whether the terminal is currently running.
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Gets or sets the database file path.
        /// </summary>
        protected string DatabaseFilePath { get; set; }

        /// <summary>
        /// Initializes the terminal with optional database file path.
        /// </summary>
        /// <param name="databaseFilePath">Path to the database file. If null, default location will be used.</param>
        protected Terminal(string databaseFilePath = null)
        {
            DatabaseFilePath = databaseFilePath;
        }

        /// <summary>
        /// Starts the terminal.
        /// </summary>
        public virtual void Start()
        {
            if (IsRunning)
            {
                return;
            }
            IsRunning = true;
            OnStart();
        }

        /// <summary>
        /// Stops the terminal and cleans up resources.
        /// </summary>
        public virtual void Stop()
        {
            if (!IsRunning)
            {
                return;
            }
            OnStop();
            IsRunning = false;
        }

        /// <summary>
        /// Called when the terminal starts. Override to provide custom startup logic.
        /// </summary>
        protected virtual void OnStart()
        {
        }

        /// <summary>
        /// Called when the terminal stops. Override to provide custom shutdown logic.
        /// </summary>
        protected virtual void OnStop()
        {
        }
    }
}
