namespace Platform.Data.WebTerminal.Models
{
    /// <summary>
    /// Error response model
    /// </summary>
    public class ErrorDto
    {
        /// <summary>
        /// Error code
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Additional field information
        /// </summary>
        public string Fields { get; set; }
    }
}
