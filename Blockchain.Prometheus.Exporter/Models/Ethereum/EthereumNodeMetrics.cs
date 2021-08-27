using Prometheus;

namespace Blockchain.Prometheus.Exporter.Models.Ethereum
{
    /// <summary>
    /// Defines basic metrics for a node.
    /// </summary>
    public class EthereumNodeMetrics
    {
        /// <summary>
        /// Tracks version of a node.
        /// </summary>
        public readonly Gauge NodeVersion = Metrics.CreateGauge
        ("eth_client_version", "Client version used by node",
            new GaugeConfiguration
            {
                LabelNames = new []
                {
                    "client", "version", "version_short", "architecture", "language"
                }
            });
    }
}
