namespace Blockchain.Prometheus.Exporter.Models.Ethereum
{
    /// <summary>
    /// Collection class that collects various node metrics.
    /// </summary>
    public static class EthereumMetricsCollection
    {
        /// <summary>
        /// Defines a set of basic block metrics.
        /// </summary>
        public static EthereumBlockMetrics BlockMetrics { get; }

        /// <summary>
        /// Defines a set of peer specific metrics.
        /// </summary>
        public static EthereumPeerMetrics PeerMetrics { get; }

        /// <summary>
        /// Defines a set of node metrics.
        /// </summary>
        public static EthereumNodeMetrics NodeMetrics { get; }

        /// <summary>
        /// Creates a new <see cref="EthereumMetricsCollection"/> instance.
        /// </summary>
        static EthereumMetricsCollection()
        {
            BlockMetrics = new EthereumBlockMetrics();
            PeerMetrics = new EthereumPeerMetrics();
            NodeMetrics = new EthereumNodeMetrics();
        }
    }
}
