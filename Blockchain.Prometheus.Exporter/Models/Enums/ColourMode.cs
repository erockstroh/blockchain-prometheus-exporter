namespace Blockchain.Prometheus.Exporter.Models.Enums
{
    /// <summary>
    /// Defines the colour mode for colour output.
    /// </summary>
    public enum ColourMode
    {
        /// <summary>
        /// Automatically detect colour output mode.
        /// </summary>
        Auto,

        /// <summary>
        /// Use default console colours.
        /// </summary>
        Console,

        /// <summary>
        /// Use XTerm console colours.
        /// </summary>
        XTerm,

        /// <summary>
        /// Disable colour output.
        /// </summary>
        None
    }
}
