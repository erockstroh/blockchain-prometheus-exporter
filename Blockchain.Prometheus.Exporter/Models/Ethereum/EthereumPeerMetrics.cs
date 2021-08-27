using Prometheus;

namespace Blockchain.Prometheus.Exporter.Models.Ethereum
{
    /// <summary>
    /// Defines metrics for node peers.
    /// </summary>
    public class EthereumPeerMetrics
    {
        /// <summary>
        /// Tracks the number of currently connected peers.
        /// </summary>
        public readonly Gauge PeerCount = Metrics.CreateGauge
            ("eth_peers", "Number of currently connected peers");
    }
}
