namespace Blockchain.Prometheus.Exporter.Models
{
    /// <summary>
    /// Defines version information about a node client.
    /// </summary>
    public class NodeClientVersion
    {
        /// <summary>
        /// Client that is running on the node.
        /// </summary>
        public string Client { get; set; }

        /// <summary>
        /// Version of running client.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Short version of running client.
        /// </summary>
        public string ShortVersion { get; set; }

        /// <summary>
        /// Client build architecture.
        /// </summary>
        public string Architecture { get; set; }

        /// <summary>
        /// Client build language.
        /// </summary>
        public string Language { get; set; }
    }
}
